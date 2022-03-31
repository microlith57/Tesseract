using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Tesseract {
    public class ObjModel : IDisposable {
        public class Mesh {
            public string Name = "";
            public ObjModel Model;
            public int VertexStart;
            public int VertexCount;
        }

        public List<Mesh> Meshes = new List<Mesh>();

        public VertexBuffer Vertices;
        private VertexPositionNormalTexture[] verts;
        private object _Vertices_QueuedLoadLock;
        private MaybeAwaitable<VertexBuffer> _Vertices_QueuedLoad;

        private bool ResetVertexBuffer() {
            if (Vertices != null && !Vertices.IsDisposed && !Vertices.GraphicsDevice.IsDisposed) {
                return false;
            }
            object queuedLoadLock = _Vertices_QueuedLoadLock;
            if (queuedLoadLock != null) {
                lock (queuedLoadLock) {
                    if (_Vertices_QueuedLoadLock == null) {
                        return true;
                    }
                    if (MainThreadHelper.IsMainThread) {
                        _Vertices_QueuedLoadLock = null;
                    }
                }
                if (!MainThreadHelper.IsMainThread) {
                    while (!_Vertices_QueuedLoad.IsValid) {
                        Thread.Yield();
                    }
                    _Vertices_QueuedLoad.GetResult();
                    return true;
                }
            }
            if (!(CoreModule.Settings.ThreadedGL ?? Everest.Flags.PreferThreadedGL) && !MainThreadHelper.IsMainThread && queuedLoadLock == null) {
                lock (queuedLoadLock = new object()) {
                    _Vertices_QueuedLoadLock = queuedLoadLock;
                    _Vertices_QueuedLoad = MainThreadHelper.Get(delegate {
                        lock (queuedLoadLock) {
                            if (_Vertices_QueuedLoadLock == null) {
                                return Vertices;
                            }
                            Vertices?.Dispose();
                            Vertices = new VertexBuffer(Engine.Graphics.GraphicsDevice, typeof(VertexPositionNormalTexture), verts.Length, BufferUsage.None);
                            Vertices.SetData(verts);
                            _Vertices_QueuedLoadLock = null;
                            return Vertices;
                        }
                    });
                }
                return true;
            }
            if (Vertices == null || Vertices.IsDisposed || Vertices.GraphicsDevice.IsDisposed) {
                Vertices = new VertexBuffer(Engine.Graphics.GraphicsDevice, typeof(VertexPositionNormalTexture), verts.Length, BufferUsage.None);
                Vertices.SetData(verts);
                return true;
            }
            return false;
        }

        public void ReassignVertices() {
            if (!ResetVertexBuffer()) {
                Vertices.SetData(verts);
            }
        }

        public void Draw(Effect effect) {
            ResetVertexBuffer();
            Engine.Graphics.GraphicsDevice.SetVertexBuffer(Vertices);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                Engine.Graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Vertices.VertexCount / 3);
            }
        }

        public void Dispose() {
            Vertices.Dispose();
            Meshes = null;
        }

        private static float Float(string data) {
            return float.Parse(data, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///       Create a new ObjModel from a stream
        ///       </summary>
        public static ObjModel CreateFromStream(Stream stream, bool isExport) {
            ObjModel objModel = new ObjModel();
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var texcoords = new List<Vector2>();
            var vertices = new List<VertexPositionNormalTexture>();
            Mesh mesh = null;

            if (isExport) {
                // using BinaryReader binaryReader = new BinaryReader(stream);
                // int num = binaryReader.ReadInt32();
                // for (int i = 0; i < num; i++) {
                //     if (mesh != null) {
                //         mesh.VertexCount = faces.Count - mesh.VertexStart;
                //     }
                //     mesh = new Mesh();
                //     mesh.Name = binaryReader.ReadString();
                //     mesh.VertexStart = faces.Count;
                //     objModel.Meshes.Add(mesh);
                //     int num2 = binaryReader.ReadInt32();
                //     for (int j = 0; j < num2; j++) {
                //         float x = binaryReader.ReadSingle();
                //         float y = binaryReader.ReadSingle();
                //         float z = binaryReader.ReadSingle();
                //         verts.Add(new Vector3(x, y, z));
                //     }
                //     int num3 = binaryReader.ReadInt32();
                //     for (int k = 0; k < num3; k++) {
                //         float x2 = binaryReader.ReadSingle();
                //         float y2 = binaryReader.ReadSingle();
                //         texcoords.Add(new Vector2(x2, y2));
                //     }
                //     int num4 = binaryReader.ReadInt32();
                //     for (int l = 0; l < num4; l++) {
                //         int index = binaryReader.ReadInt32() - 1;
                //         int index2 = binaryReader.ReadInt32() - 1;
                //         faces.Add(new VertexPositionNormalTexture {
                //             Position = verts[index],
                //             TextureCoordinate = texcoords[index2]
                //         });
                //     }
                // }
                throw new NotImplementedException();
            } else {
                using StreamReader streamReader = new StreamReader(stream);
                string line;
                while ((line = streamReader.ReadLine()) != null) {
                    string[] parts = line.Split(' ');
                    if (parts.Length == 0) { continue; }
                    switch (parts[0]) {
                        case "o":
                            if (mesh != null) { mesh.VertexCount = vertices.Count - mesh.VertexStart; }
                            mesh = new Mesh();
                            mesh.Name = parts[1];
                            mesh.VertexStart = vertices.Count;
                            objModel.Meshes.Add(mesh);
                            break;
                        case "v":
                            verts.Add(new Vector3(Float(parts[1]), Float(parts[2]), Float(parts[3])));
                            break;
                        case "vn":
                            normals.Add(new Vector3(Float(parts[1]), Float(parts[2]), Float(parts[3])));
                            break;
                        case "vt":
                            texcoords.Add(new Vector2(Float(parts[1]), Float(parts[2])));
                            break;
                        case "f":
                            for (int i = 1; i < Math.Min(4, parts.Length); i++) {
                                VertexPositionNormalTexture vert = default(VertexPositionNormalTexture);
                                string[] subparts = parts[i].Split('/');
                                if (subparts[0].Length > 0) {
                                    vert.Position = verts[int.Parse(subparts[0]) - 1];
                                    if (normals.Count > 0) { vert.Normal = normals[int.Parse(subparts[0]) - 1]; }
                                }
                                if (subparts[1].Length > 0) {
                                    vert.TextureCoordinate = texcoords[int.Parse(subparts[1]) - 1];
                                }
                                vertices.Add(vert);
                            }
                            break;
                    }
                }
            }
            if (mesh != null) { mesh.VertexCount = vertices.Count - mesh.VertexStart; }
            objModel.verts = vertices.ToArray();
            objModel.ResetVertexBuffer();
            return objModel;
        }
    }
}
