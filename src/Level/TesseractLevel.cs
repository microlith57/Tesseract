using Microsoft.Xna.Framework;
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

        public TesseractLevel(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Tag = (int)Tags.Global | (int)Tags.FrozenUpdate | (int)Tags.TransitionUpdate | (int)Tags.PauseUpdate;
            Camera = new TesseractCamera(this);
        }

        public override void Update() {
            base.Update();
            if (Scene is Level level) { Camera.Update(level, this); }
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
    }
}
