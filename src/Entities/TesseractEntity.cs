using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Tesseract {
    public abstract class TesseractEntity : Entity {
        public TesseractLevel TesLevel => Scene?.Tracker?.GetEntity<TesseractLevel>();

        public Vector3 WorldPos;

        public TesseractEntity(Vector3 position) : base(Vector2.Zero) {
            Tag = TesseractTags.Tesseract;
        }

        public TesseractEntity() : this(Vector3.Zero) {
        }

        public virtual void BeforeRender(Scene scene, TesseractLevel tesLevel) {
        }

        public virtual void Render(Scene scene, TesseractLevel tesLevel) {
        }
    }
}
