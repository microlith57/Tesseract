using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.Tesseract {
    [Tracked]
    [CustomEntity("Tesseract/TesseractLevel")]
    public class TesseractLevel : Entity {
        private static uint count = 0;
        private static bool counted = false;
        public static bool AnyActive => count > 0;

        public static TesseractLevel Instance => Engine.Scene?.Tracker?.GetEntity<TesseractLevel>();
        public TesseractCamera Camera;

        public BillboardRenderer.EntityBillboard64px PlayerBillboard;

        public TesseractLevel(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Tag = (int)Tags.Global | (int)Tags.FrozenUpdate | (int)Tags.TransitionUpdate | (int)Tags.PauseUpdate;
            Camera = new TesseractCamera(this);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            CoalesceBuffers();

            if (scene.Tracker.GetEntity<Player>() is Player player && player != null) {
                PlayerBillboard = new BillboardRenderer.EntityBillboard64px(Camera.Make3DWithDefaults(player.Position, Vector3.Zero), player);
            }
        }

        public override void Update() {
            base.Update();
            if (Scene is Level level) {
                Camera.Update(level, this);
                if (Scene.Tracker.GetEntity<Player>() is Player player && player != null) {
                    if (PlayerBillboard == null) {
                        PlayerBillboard = new BillboardRenderer.EntityBillboard64px(Camera.Make3DWithDefaults(player.Position, Vector3.Zero), player);
                    }
                    PlayerBillboard.Position = Camera.Make3DWithDefaults(player.Position, PlayerBillboard.Position);
                }
            }
        }

        public override void Added(Scene scene) {
            if (!counted) { count++; counted = true; }
            Logger.Log("Tesseract", "entered a tesseract level!");
            base.Added(scene);
        }

        public override void Removed(Scene scene) {
            if (counted) { count--; counted = false; }
            Logger.Log("Tesseract", "left a tesseract level!");
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene) {
            if (counted) { count--; counted = false; }
            Logger.Log("Tesseract", "ended a tesseract level!");
            base.SceneEnd(scene);
        }

        public void BeforeRender() {
            // if (PlayerTarget != null && !PlayerTarget.IsDisposed) {
            //     Engine.Instance.GraphicsDevice.SetRenderTarget(PlayerTarget);
            //     Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

            //     Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.Camera.Matrix);
            //     Scene.Tracker.GetEntities<Player>()?.ForEach(e => e.Render());
            //     Scene.Tracker.GetEntities<PlayerDeadBody>()?.ForEach(e => e.Render());
            //     Draw.SpriteBatch.End();
            // }
        }

        public override void Render() {
        }

        internal static void CreateBuffers() {
            // if (PlayerTarget != null && !PlayerTarget.IsDisposed) { PlayerTarget.Dispose(); PlayerTarget = null; }
            // PlayerTarget = VirtualContent.CreateRenderTarget("tesseract-buffer-player", 320, 180);
        }

        internal static void CoalesceBuffers() {
            // if (PlayerTarget == null || PlayerTarget.IsDisposed) { PlayerTarget = VirtualContent.CreateRenderTarget("tesseract-buffer-player", 320, 180); }
        }

        internal static void UnloadBuffers() {
            // if (PlayerTarget != null && !PlayerTarget.IsDisposed) { PlayerTarget.Dispose(); }
            // PlayerTarget = null;
        }
    }
}
