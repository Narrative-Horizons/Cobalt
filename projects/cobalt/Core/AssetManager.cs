using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cobalt.Graphics.API;
using Cobalt.Math;
using SharpGLTF.Schema2;

using static Cobalt.Bindings.STB.ImageLoader;
using static Cobalt.Bindings.Utils.Util;

namespace Cobalt.Core
{
    public class ImageAsset
    {
        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public uint Components { get; private set; }
        public bool IsHdr { get; private set; }
        public uint BitsPerPixel { get; private set; }
        public byte[] AsBytes { get; private set; } = null;
        public ushort[] AsUnsignedShorts { get; private set; } = null;
        public float[] AsFloats { get; private set; } = null;

        internal ImageAsset(string path)
        {
            ImagePayload payload = LoadImage(path);
            Width = (uint)payload.width;
            Height = (uint)payload.height;
            Components = (uint)payload.channels;
            IsHdr = payload.hdr_f_image != IntPtr.Zero;

            int count = (int) (Width * Height * Components);

            if (payload.hdr_f_image != IntPtr.Zero)
            {
                BitsPerPixel = sizeof(float) * 8;
                AsFloats = new float[count];
                Marshal.Copy(payload.hdr_f_image, AsFloats, 0, count);
            }
            else if (payload.sdr_us_image != IntPtr.Zero)
            {
                BitsPerPixel = sizeof(ushort) * 8;
                AsUnsignedShorts = new ushort[count];
                Copy(payload.sdr_us_image, AsUnsignedShorts, 0, count);
            }
            else if (payload.sdr_ub_image != IntPtr.Zero)
            {
                BitsPerPixel = sizeof(byte) * 8;
                AsBytes = new byte[count];
                Marshal.Copy(payload.sdr_ub_image, AsBytes, 0, count);
            }

            ReleaseImage(ref payload);
        }
    }

    public class ModelAsset
    {
        private static UInt64 _uniqueCount = 0;
        public List<MeshNode> meshes = new List<MeshNode>();
        public string Path { get; private set; }

        internal ModelAsset(string path)
        {
            Path = path;

            if (path.Contains(".gltf") || path.Contains(".glb"))
            {
                // GTLF model
                ModelRoot model = ModelRoot.Load(path);
                bool isAnimated = model.LogicalAnimations.Count > 0;

                uint nodeCount = (uint)model.LogicalNodes.Count;
                for (int i = 0; i < nodeCount; i++)
                {
                    var node = model.LogicalNodes[i];
                    var mesh = node.Mesh;

                    var worldMatrix = isAnimated ? node.GetWorldMatrix(null, 0) : node.WorldMatrix;

                    if (mesh != null)
                    {
                        // Process the mesh
                        uint primitiveCount = (uint)mesh.Primitives.Count;

                        for (int j = 0; j < primitiveCount; j++)
                        {
                            Mesh m = new Mesh();
                            Accessor accPosition = mesh.Primitives[j].GetVertexAccessor("POSITION");
                            var posArray = accPosition.AsVector3Array();

                            m.positions = new Vector3[posArray.Count];
                            for (int k = 0; k < posArray.Count; k++)
                            {
                                m.positions[k] = new Vector3(posArray[k].X, posArray[k].Y, posArray[k].Z);
                            }

                            Accessor accNormal = mesh.Primitives[j].GetVertexAccessor("NORMAL");
                            if (accNormal != null)
                            {
                                var normalArray = accNormal.AsVector3Array();

                                m.normals = new Vector3[normalArray.Count];
                                for (int k = 0; k < normalArray.Count; k++)
                                {
                                    m.normals[k] = new Vector3(normalArray[k].X, normalArray[k].Y, normalArray[k].Z);
                                }
                            }

                            Accessor accUVs = mesh.Primitives[j].GetVertexAccessor("TEXCOORD_0");
                            if (accUVs != null)
                            {
                                var uvArray = accUVs.AsVector2Array();

                                m.texcoords = new Vector2[uvArray.Count];
                                for (int k = 0; k < uvArray.Count; k++)
                                {
                                    m.texcoords[k] = new Vector2(uvArray[k].X, uvArray[k].Y);
                                }
                            }

                            Accessor accTangent = mesh.Primitives[j].GetVertexAccessor("TANGENT");
                            if (accTangent != null)
                            {
                                var tangentArray = accTangent.AsVector4Array();

                                m.tangents = new Vector3[tangentArray.Count];
                                for (int k = 0; k < tangentArray.Count; k++)
                                {
                                    m.tangents[k] = new Vector3(tangentArray[k].X, tangentArray[k].Y, tangentArray[k].Z);

                                    // TODO: Use dir to calculate binormal
                                    float dir = tangentArray[k].W;
                                }
                            }

                            if (isAnimated)
                            {
                                Accessor accJoints = mesh.Primitives[j].GetVertexAccessor("JOINTS_0");
                                Accessor accWeights = mesh.Primitives[j].GetVertexAccessor("WEIGHTS_0");
                            }

                            var accTriangles = mesh.Primitives[j].GetIndexAccessor();
                            if(accTriangles != null)
                            { 
                                var indexArray = accTriangles.AsIndicesArray();

                                m.triangles = new uint[indexArray.Count];
                                for(int k = 0; k < indexArray.Count; k++)
                                {
                                    m.triangles[k] = indexArray[k];
                                }
                            }

                            MeshNode meshNode = new MeshNode
                            {
                                mesh = m,
                                transform = worldMatrix
                            };

                            meshes.Add(meshNode);
                        }
                    }
                }
            }
        }
    }

    public class AssetManager : IDisposable
    {
        private readonly Dictionary<string, ImageAsset> _images = new Dictionary<string, ImageAsset>();
        private readonly Dictionary<string, ModelAsset> _models = new Dictionary<string, ModelAsset>();

        public AssetManager()
        {
        }

        public void Dispose()
        {
        }

        public ImageAsset LoadImage(string path)
        {
            var asset = new ImageAsset(path);
            _images[path] = asset;
            return asset;
        }

        public ImageAsset GetImage(string path)
        {
            return _images[path];
        }

        public ModelAsset LoadModel(string path)
        {
            var asset = new ModelAsset(path);
            _models[path] = asset;
            return asset;
        }

        public ModelAsset GetModel(string path)
        {
            return _models[path];
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RenderableMeshVertex
    {
        public Vector3 position;
        public Vector2 uv;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 binormal;
    }

    public class RenderableMesh
    {
        public Mesh localMesh;
        public RenderableMeshVertex[] localVertices;
        public uint baseVertex;
        public uint baseIndex;
        public uint indexCount;
        public IVertexAttributeArray VAO;

        public override int GetHashCode()
        {
            return HashCode.Combine(VAO.GetHashCode(), baseVertex.GetHashCode(), baseIndex.GetHashCode(), indexCount.GetHashCode());
        }
    }

    public class RenderableManager : IDisposable
    {
        private Dictionary<ModelAsset, List<RenderableMesh>> _renderableMeshes = new Dictionary<ModelAsset, List<RenderableMesh>>();
        private IDevice _device;

        public RenderableManager(IDevice device)
        {
            _device = device;
        }

        public List<RenderableMesh> GetRenderables(ModelAsset asset)
        {
            if(_renderableMeshes.ContainsKey(asset))
            {
                return _renderableMeshes[asset];
            }

            Logger.Log.Error("Asset " + asset.Path + " not loaded yet!");

            return null;
        }

        public void QueueRenderable(ModelAsset asset)
        {
            List<RenderableMesh> processingMeshes = new List<RenderableMesh>();

            var meshes = asset.meshes;

            List<RenderableMeshVertex> combinedVertices = new List<RenderableMeshVertex>();
            List<uint> combinedIndices = new List<uint>();

            foreach (MeshNode meshNode in meshes)
            {
                Mesh mesh = meshNode.mesh;

                RenderableMesh rMesh = new RenderableMesh
                {
                    localMesh = mesh
                };

                uint vertexCount = (uint)mesh.positions.Length;

                rMesh.baseVertex = (uint)combinedVertices.Count;
                rMesh.baseIndex = (uint)combinedIndices.Count;
                rMesh.indexCount = (uint)mesh.triangles.Length;

                rMesh.localVertices = new RenderableMeshVertex[vertexCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    RenderableMeshVertex vertex = new RenderableMeshVertex();
                    vertex.position = mesh.positions[i];
                    if (mesh.texcoords != null)
                        vertex.uv = mesh.texcoords[i];
                    else
                        vertex.uv = Vector2.Zero;

                    if (mesh.normals != null)
                        vertex.normal = mesh.normals[i];
                    else
                        vertex.normal = Vector3.UnitY;

                    if (mesh.tangents != null)
                        vertex.tangent = mesh.tangents[i];
                    else
                        vertex.tangent = Vector3.UnitX;

                    if (mesh.binormals != null)
                        vertex.binormal = mesh.binormals[i];
                    else
                        vertex.binormal = Vector3.UnitZ;

                    rMesh.localVertices[i] = vertex;

                    combinedVertices.Add(vertex);
                }

                combinedIndices.AddRange(mesh.triangles);

                processingMeshes.Add(rMesh);
            }

            IBuffer vertexBuffer = _device.CreateBuffer(
                    IBuffer.FromPayload(combinedVertices.ToArray())
                    .AddUsage(EBufferUsage.ArrayBuffer),
                    new IBuffer.MemoryInfo.Builder()
                        .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                        .AddRequiredProperty(EMemoryProperty.HostVisible)
                        .Usage(EMemoryUsage.CPUToGPU));

            IBuffer indexBuffer = _device.CreateBuffer(
                IBuffer.FromPayload(combinedIndices.ToArray())
                .AddUsage(EBufferUsage.IndexBuffer),
                new IBuffer.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                    .AddRequiredProperty(EMemoryProperty.HostVisible)
                    .Usage(EMemoryUsage.CPUToGPU));

            const int stride = 56;

            List<VertexAttribute> layout = new List<VertexAttribute>
            {
                new VertexAttribute.Builder().Location(0).Offset(0).Rate(EVertexInputRate.PerVertex).Format(EDataFormat.R32G32B32_SFLOAT).Stride(stride).Binding(0),  // 3 floats
                new VertexAttribute.Builder().Location(1).Offset(12).Rate(EVertexInputRate.PerVertex).Format(EDataFormat.R32G32_SFLOAT).Stride(stride).Binding(0),    // 2 floats
                new VertexAttribute.Builder().Location(2).Offset(20).Rate(EVertexInputRate.PerVertex).Format(EDataFormat.R32G32B32_SFLOAT).Stride(stride).Binding(0), // 3 floats
                new VertexAttribute.Builder().Location(3).Offset(32).Rate(EVertexInputRate.PerVertex).Format(EDataFormat.R32G32B32_SFLOAT).Stride(stride).Binding(0), // 3 floats
                new VertexAttribute.Builder().Location(4).Offset(44).Rate(EVertexInputRate.PerVertex).Format(EDataFormat.R32G32B32_SFLOAT).Stride(stride).Binding(0)  // 3 floats
            };

            IVertexAttributeArray vao = _device.CreateVertexAttributeArray(new List<IBuffer>() { vertexBuffer }, indexBuffer, layout);

            foreach(RenderableMesh rM in processingMeshes)
            {
                rM.VAO = vao;
            }

            _renderableMeshes.Add(asset, processingMeshes);
        }

        public void Dispose()
        {

        }
    }
}
