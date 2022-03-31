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
                    Model = ObjModel.CreateFromStream(modAsset.Stream, isExport: false);
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
            effect.Alpha = 1f;
            effect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.SpecularPower = 2.0f;
            effect.AmbientLightColor = new Vector3(0.75f, 0.75f, 0.75f);
            // effect.DirectionalLight0.Enabled = true;
            // effect.DirectionalLight0.DiffuseColor = Vector3.One;
            // effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1.0f, -1.0f, -1.0f));
            // effect.DirectionalLight0.SpecularColor = Vector3.One;
            // effect.DirectionalLight1.Enabled = true;
            // effect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
            // effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.0f, 1.0f));
            // effect.DirectionalLight1.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
            effect.LightingEnabled = true;
            effect.EnableDefaultLighting();
            // effect.PreferPerPixelLighting = true;
            Model.Draw(effect);
        }
    }
}
