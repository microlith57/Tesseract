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
        private static bool DataLoaded = false;

        public static void Initialize() {
        }

        public static void BeforeRender(Scene scene) {
            if (scene == null || scene is not Level level) { return; }

            TesseractLevel tesLevel = scene.Tracker?.GetEntity<TesseractLevel>();
        }

        public static void Render(Scene scene) {
            if (scene == null || scene is not Level level) { return; }

            TesseractLevel tesLevel = scene.Tracker?.GetEntity<TesseractLevel>();
            if (tesLevel == null) { return; }

            tesLevel.BeforeRender();
            foreach (var entity in scene.GetEntitiesByTagMask(TesseractTags.Tesseract)) {
                if (entity is not TesseractEntity tesEntity) { continue; }
                tesEntity.BeforeRender(scene, tesLevel);
            }

            Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

            tesLevel.Render();
            BillboardRenderer.EntityBillboard64px.RenderAll(scene, tesLevel, GameplayBuffers.Gameplay);
            foreach (var entity in scene.GetEntitiesByTagMask(TesseractTags.Tesseract)) {
                if (entity is not TesseractEntity tesEntity) { continue; }
                tesEntity.Render(scene, tesLevel);
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.Camera.Matrix);
            if (GameplayRenderer.RenderDebug || Engine.Commands.Open) {
                scene.Entities.DebugRender(level.Camera);
            }
            Draw.SpriteBatch.End();
        }

        public static void Load() {
            IL.Celeste.Level.Render += ModLevelRender;
        }

        public static void Unload() {
            IL.Celeste.Level.Render -= ModLevelRender;
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