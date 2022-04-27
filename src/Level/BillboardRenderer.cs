using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Tesseract {
    public static class BillboardRenderer {
        public abstract class Billboard {
            public Vector3 Position = default(Vector3);
            public Vector2 Size = default(Vector2);

            public Billboard(Vector3 position, Vector2 size) {
                Position = position;
                Size = size;
            }

            public abstract void Render(Scene scene, TesseractLevel tesLevel, RenderTarget2D target);
        }

        public class EntityBillboard64px : Billboard {
            private static VirtualRenderTarget buffer;
            public Entity Entity;

            public static List<EntityBillboard64px> Billboards = default(List<EntityBillboard64px>);

            public EntityBillboard64px(Vector3 position, Vector2 size, Entity entity) : base(position, size) {
                Entity = entity;
                if (Billboards == null) { Billboards = new List<EntityBillboard64px>(); }
                Billboards.Add(this);
            }

            public EntityBillboard64px(Vector3 position, Entity entity) : this(position, new Vector2(64, 64), entity) { }

            public override void Render(Scene scene, TesseractLevel tesLevel, RenderTarget2D target) {
                if (Entity is null || !Entity.Visible) { return; }

                var m = Matrix.Identity
                        * Matrix.CreateTranslation(new Vector3(-(int)Math.Floor(Entity.Center.X), -(int)Math.Floor(Entity.Center.Y), 0f))
                        * Matrix.CreateTranslation(new Vector3(-32f, -32f, 0f));

                Engine.Instance.GraphicsDevice.SetRenderTarget(buffer);
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, m);
                Entity.Render();
                Draw.SpriteBatch.End();

                Engine.Instance.GraphicsDevice.SetRenderTarget(target);

                var effect = tesLevel.Camera.EffectPlain;
                effect.Projection = tesLevel.Camera.Projection;
                effect.View = tesLevel.Camera.View;
                effect.World = Matrix.CreateScale(Size.X, 1f, Size.Y)
                             * Matrix.CreateRotationX((float)Math.PI / 2f)
                             * Matrix.CreateRotationZ((float)Math.PI)
                             * Matrix.CreateConstrainedBillboard(Position,
                                                                 tesLevel.Camera.Position,
                                                                 Vector3.Up,
                                                                 tesLevel.Camera.ForwardVector,
                                                                 Vector3.Up);
                effect.TextureEnabled = true;
                effect.Texture = buffer.Target;

                ResetVertexBuffer();
                Engine.Graphics.GraphicsDevice.SetVertexBuffer(quad_buf);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                    pass.Apply();
                    Engine.Graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                }
            }

            public static void RenderAll(Scene scene, TesseractLevel tesLevel, RenderTarget2D target) {
                if (Billboards == null) { Billboards = new List<EntityBillboard64px>(); }
                foreach (var billboard in Billboards) {
                    billboard.Render(scene, tesLevel, target);
                }
            }

            internal static void CreateBuffers() {
                if (buffer == null || buffer.IsDisposed) {
                    buffer = VirtualContent.CreateRenderTarget("tesseract-entitybillboards64px", 64, 64);
                }
            }

            internal static void UnloadBuffers() {
                if (buffer != null && !buffer.IsDisposed) { buffer.Dispose(); }
                buffer = null;
            }
        }

        public static List<Billboard> Billboards = default(List<Billboard>);

        private static VertexPositionNormalTexture[] quad_verts = new VertexPositionNormalTexture[6];
        private static VertexBuffer quad_buf;

        public static void LoadData() {
            var v1 = new Vector3(-0.5f, 0.0f, 0.5f);
            var v2 = new Vector3(0.5f, 0.0f, 0.5f);
            var v3 = new Vector3(-0.5f, 0.0f, -0.5f);
            var v4 = new Vector3(0.5f, 0.0f, -0.5f);

            var n = new Vector3(0.0f, 1.0f, 0.0f);

            var t1 = new Vector2(0f, 0f);
            var t2 = new Vector2(1f, 0f);
            var t3 = new Vector2(0f, 1f);
            var t4 = new Vector2(1f, 1f);

            quad_verts[0] = new VertexPositionNormalTexture(v2, n, t2);
            quad_verts[1] = new VertexPositionNormalTexture(v3, n, t3);
            quad_verts[2] = new VertexPositionNormalTexture(v1, n, t1);
            quad_verts[3] = new VertexPositionNormalTexture(v2, n, t2);
            quad_verts[4] = new VertexPositionNormalTexture(v4, n, t4);
            quad_verts[5] = new VertexPositionNormalTexture(v3, n, t3);
        }

        public static void CreateBuffers() { EntityBillboard64px.CreateBuffers(); }
        public static void UnloadBuffers() { EntityBillboard64px.UnloadBuffers(); }

        private static bool ResetVertexBuffer() {
            if (quad_buf == null || quad_buf.IsDisposed || quad_buf.GraphicsDevice.IsDisposed) {
                quad_buf = new VertexBuffer(Engine.Graphics.GraphicsDevice, typeof(VertexPositionNormalTexture), quad_verts.Length, BufferUsage.None);
                quad_buf.SetData(quad_verts);
                return true;
            }
            return false;
        }
    }
}
