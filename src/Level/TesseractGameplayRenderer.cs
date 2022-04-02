using System;
using System.Reflection;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Celeste.Mod.Tesseract {
    public static class TesseractGameplayRenderer {
        public static bool Active => TesseractLevel.AnyActive;
        public static VirtualRenderTarget PlayerBuffer;
        static ObjModel Quad;
        private static bool DataLoaded = false;

        public static void Initialize() {
        }

        public static void LoadData() {
            if (!DataLoaded) {
                if (Everest.Content.TryGet("Assets/quad", out var modAsset)) {
                    Quad = ObjModel.CreateFromStream(modAsset.Stream, isExport: false);
                    DataLoaded = true;
                }
            }
        }

        public static void Render(Scene scene) {
            if (scene == null || scene is not Level level) { return; }

            TesseractLevel tesLevel = scene.Tracker?.GetEntity<TesseractLevel>();
            if (tesLevel == null) { return; }
            if (PlayerBuffer == null || PlayerBuffer.IsDisposed) { PlayerBuffer = VirtualContent.CreateRenderTarget("tesseract-buffer-player", 320, 180); }

            Engine.Instance.GraphicsDevice.SetRenderTarget(PlayerBuffer);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.Camera.Matrix);
            scene.Tracker.GetEntity<Player>()?.Render();
            Draw.SpriteBatch.End();

            Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

            foreach (var entity in scene.GetEntitiesByTagMask(TesseractTags.Tesseract)) {
                if (entity is not TesseractEntity tesEntity) { continue; }
                tesEntity.Render(scene, tesLevel);
            }
            RenderQuad(scene, tesLevel);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.Camera.Matrix);
            if (GameplayRenderer.RenderDebug || Engine.Commands.Open) {
                scene.Entities.DebugRender(level.Camera);
            }
            Draw.SpriteBatch.End();
        }

        public static void RenderQuad(Scene scene, TesseractLevel tesLevel) {
            if (!DataLoaded) { return; }

            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            var effect = tesLevel.Camera.EffectPlayer;
            effect.Projection = tesLevel.Camera.Projection;
            effect.View = tesLevel.Camera.View;
            effect.World = Matrix.CreateScale(TesseractCamera.Width, 1f, TesseractCamera.Height)
                         * Matrix.CreateRotationX((float)Math.PI / 2f)
                         * Matrix.CreateRotationZ((float)Math.PI)
                         * Matrix.CreateConstrainedBillboard(tesLevel.Camera.FocusPoint,
                                                             tesLevel.Camera.Position,
                                                             Vector3.Up,
                                                             tesLevel.Camera.ForwardVector,
                                                             Vector3.Up);
            effect.Texture = PlayerBuffer.Target;
            Quad.Draw(effect);
        }

        public static void Load() {
            On.Celeste.GameplayBuffers.Create += OnCreateBuffers;
            On.Celeste.GameplayBuffers.Unload += OnUnloadBuffers;
            IL.Celeste.Level.Render += ModLevelRender;
        }

        public static void Unload() {
            On.Celeste.GameplayBuffers.Create -= OnCreateBuffers;
            On.Celeste.GameplayBuffers.Unload -= OnUnloadBuffers;
            IL.Celeste.Level.Render -= ModLevelRender;
        }

        public static void OnCreateBuffers(On.Celeste.GameplayBuffers.orig_Create orig) {
            orig();

            if (PlayerBuffer != null && !PlayerBuffer.IsDisposed) { PlayerBuffer.Dispose(); PlayerBuffer = null; }
            PlayerBuffer = VirtualContent.CreateRenderTarget("tesseract-buffer-player", 320, 180);
        }

        public static void OnUnloadBuffers(On.Celeste.GameplayBuffers.orig_Unload orig) {
            orig();

            if (PlayerBuffer != null && !PlayerBuffer.IsDisposed) { PlayerBuffer.Dispose(); PlayerBuffer = null; }
        }

        public static void ModLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            FieldInfo gameplayRendererField = typeof(Level).GetField("GameplayRenderer", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo lightingRendererField = typeof(Level).GetField("Lighting", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo rendererRenderMethod = typeof(Renderer).GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);

            Logger.Log("Tesseract", "patching Level.Render call to GameplayRenderer");
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(gameplayRendererField),
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt(rendererRenderMethod))) { return; }
            Logger.Log("Tesseract", "-> found patch location");

            var skip_gameplay_label = cursor.DefineLabel();
            cursor.EmitDelegate<Func<bool>>(() => Active);
            cursor.Emit(OpCodes.Brtrue, skip_gameplay_label);
            cursor.GotoNext(MoveType.After, i => i.MatchCallOrCallvirt(rendererRenderMethod));
            cursor.MarkLabel(skip_gameplay_label);
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.EmitDelegate<Action<Level>>((level) => { TesseractGameplayRenderer.Render(level); });
            Logger.Log("Tesseract", "-> patched!");

            cursor.Goto(0);

            Logger.Log("Tesseract", "patching Level.Render to skip lighting");
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(lightingRendererField),
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt(rendererRenderMethod))) { return; }
            Logger.Log("Tesseract", "-> found patch location");

            var skip_lighting_label = cursor.DefineLabel();
            cursor.EmitDelegate<Func<bool>>(() => Active);
            cursor.Emit(OpCodes.Brtrue, skip_lighting_label);
            cursor.GotoNext(MoveType.After, i => i.MatchCallOrCallvirt(rendererRenderMethod));
            cursor.MarkLabel(skip_lighting_label);
            Logger.Log("Tesseract", "-> patched!");
        }
    }
}