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

        public Matrix Projection;
        public Matrix View;

        public BasicEffect EffectLit;
        public BasicEffect EffectPlayer;

        public TesseractCamera(TesseractLevel tesLevel) {
            TesLevel = tesLevel;

            EffectLit = new BasicEffect(Engine.Graphics.GraphicsDevice);
            EffectLit.EmissiveColor = new Vector3(0.8f, 0.8f, 0.8f);
            EffectLit.Alpha = 1.0f;

            EffectPlayer = new BasicEffect(Engine.Graphics.GraphicsDevice);
            EffectPlayer.EmissiveColor = new Vector3(1f, 1f, 1f);
            EffectPlayer.Alpha = 1.0f;
            EffectPlayer.TextureEnabled = true;

            UpdateMatrices();
        }

        public void Update(Level level, TesseractLevel tesLevel) {
            if (!level.FrozenOrPaused) {
                var levelX = MathHelper.Lerp(level.Camera.Left, level.Camera.Right, 0.5f) - tesLevel.Position.X;
                var levelY = tesLevel.Position.Y - MathHelper.Lerp(level.Camera.Bottom, level.Camera.Top, 0.5f);

                FocusPoint.X = levelX * (float)Math.Cos(Theta);
                FocusPoint.Y = levelY;
                FocusPoint.Z = levelX * -(float)Math.Sin(Theta);
            }
            Position = Vector3.Transform(FocusPoint, Matrix.CreateTranslation(0f, 0f, Separation) * Matrix.CreateRotationY(Theta));
            UpdateMatrices();
        }

        private void UpdateMatrices() {
            Projection = Matrix.CreateOrthographic(Width, Height, ClipNear, ClipFar);
            EffectLit.Projection = Projection;
            EffectPlayer.Projection = Projection;
            View = Matrix.CreateLookAt(Position, FocusPoint, Vector3.Up);
            EffectLit.View = View;
            EffectPlayer.View = View;
        }
    }
}
