using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Tesseract {
    public class TesseractCamera {
        public static float ClipNear = 1f;
        public static float ClipFar = 10000f;
        public static float Separation = MathHelper.Lerp(ClipNear, ClipFar, 0.5f);
        public static float Width = 320f;
        public static float Height = 180f;

        public TesseractLevel TesLevel;

        public Vector3 FocusPoint = default(Vector3);
        public float Theta = 0f;
        public Vector3 Position = default(Vector3);
        public Vector3 ForwardVector => Vector3.Normalize(Position - FocusPoint);
        public bool Rotating = false;

        public float Cosθ => (float)Math.Cos(Theta);
        public float Sinθ => (float)Math.Sin(Theta);

        public Matrix Projection;
        public Matrix View;

        public BasicEffect EffectLit;
        public BasicEffect EffectPlain;

        public TesseractCamera(TesseractLevel tesLevel) {
            TesLevel = tesLevel;

            EffectLit = new BasicEffect(Engine.Graphics.GraphicsDevice);

            EffectPlain = new BasicEffect(Engine.Graphics.GraphicsDevice);
            EffectPlain.EmissiveColor = new Vector3(1f, 1f, 1f);
            EffectPlain.Alpha = 1.0f;

            UpdateMatrices();
        }

        public void Update(Level level, TesseractLevel tesLevel) {
            var player = level.Tracker.GetEntity<Player>();
            if (!Rotating && player != null) {
                FocusPoint = Make3DWithDefaults(new Vector2(player.CenterX - tesLevel.Position.X,
                                                            tesLevel.Position.Y - player.CenterY),
                                                FocusPoint);
            }
            Position = Vector3.Transform(FocusPoint, Matrix.CreateTranslation(0f, 0f, Separation) * Matrix.CreateRotationY(Theta));
            UpdateMatrices();
        }

        public Vector3 Make3DWithDefaults(Vector2 visual_pos, Vector3 defaults) {
            var result = new Vector3(defaults.X, visual_pos.Y, defaults.Z);

            if (Cosθ == 1f || Cosθ == -1f) {
                FocusPoint.X = visual_pos.X * Cosθ;
            } else if (Sinθ == 1f || Sinθ == -1f) {
                FocusPoint.Z = visual_pos.X * -Sinθ;
            }

            return result;
        }

        private void UpdateMatrices() {
            Projection = Matrix.CreateOrthographic(Width, Height, ClipNear, ClipFar);
            EffectLit.Projection = Projection;
            EffectPlain.Projection = Projection;
            View = Matrix.CreateLookAt(Position, FocusPoint, Vector3.Up);
            EffectLit.View = View;
            EffectPlain.View = View;
        }
    }
}
