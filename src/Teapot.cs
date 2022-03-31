using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.Tesseract {
    [CustomEntity("Tesseract/Teapot")]
    public class Teapot : TesseractEntity {
        static ObjModel Model;
        private static bool DataLoaded = false;
        public float rotation = 0f;

        public Teapot(EntityData data, Vector2 offset) : base(data.Vec3("worldpos")) {
        }

        public override void Update() {
            base.Update();
            rotation += Engine.DeltaTime;
        }

        public static void LoadData() {
            if (!DataLoaded) {
                if (Everest.Content.TryGet("Assets/teapot", out var modAsset)) {
                    Logger.Log("Tesseract", modAsset.ToString());
                    Model = ObjModel.CreateFromStream(modAsset.Stream, "Assets/teapot");
                    DataLoaded = true;
                }
            }
        }

        public override void Render(Scene scene, TesseractLevel tesLevel) {
            if (!DataLoaded) { return; }

            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            var effect = tesLevel.Camera.EffectLit;
            effect.Projection = tesLevel.Camera.Projection;
            effect.View = tesLevel.Camera.View;
            effect.World = Matrix.CreateRotationY(rotation) * Matrix.CreateScale(20f, 20f, 20f) * Matrix.CreateTranslation(WorldPos);
            Model.Draw(effect);
        }
    }
}
