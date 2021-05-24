using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cobalt.Graphics.API;
using Cobalt.Math;
using CobaltConverter.Core;
using Silk.NET.Assimp;
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
        private static ulong _uniqueCount = 0;
        public List<MeshNode> meshes = new List<MeshNode>();
        public string Path { get; private set; }

        internal unsafe void ProcessNode(Node* node, Scene* scene, Matrix4 parentMatrix)
        {
            for(int i = 0; i < node->MNumMeshes; i++)
            {
                Silk.NET.Assimp.Mesh* assMesh = scene->MMeshes[node->MMeshes[i]];
                MeshNode meshNode = new MeshNode();

                ProcessMesh(assMesh, scene, meshNode, parentMatrix);

                meshes.Add(meshNode);
            }

            for(int i = 0; i < node->MNumChildren; i++)
            {
                ProcessNode(node->MChildren[i], scene, parentMatrix);
            }
        }

        internal unsafe void ProcessMesh(Silk.NET.Assimp.Mesh* assMesh, Scene* scene, MeshNode meshNode, Matrix4 parentMatrix)
        {
            Mesh mesh = new Mesh
            {
                positions = new Vector3[assMesh->MNumVertices],
                normals = new Vector3[assMesh->MNumVertices],
                tangents = new Vector3[assMesh->MNumVertices],
                binormals = new Vector3[assMesh->MNumVertices],
                texcoords = new Vector2[assMesh->MNumVertices]
            };

            for (int i = 0; i < assMesh->MNumVertices; i++)
            {
                mesh.positions[i] = new Vector3(assMesh->MVertices[i].X, assMesh->MVertices[i].Y, assMesh->MVertices[i].Z);

                if(assMesh->MNormals != null)
                {
                    mesh.normals[i] = new Vector3(assMesh->MNormals[i].X, assMesh->MNormals[i].Y, assMesh->MNormals[i].Z);
                }
                else
                {
                    mesh.normals[i] = Vector3.UnitY;
                }

                if(assMesh->MTextureCoords.Element0 != null)
                {
                    mesh.texcoords[i] = new Vector2(assMesh->MTextureCoords.Element0[i].X, assMesh->MTextureCoords.Element0[i].Y);
                }
                else
                {
                    mesh.texcoords[i] = Vector2.Zero;
                }

                if(assMesh->MTangents != null)
                {
                    mesh.tangents[i] = new Vector3(assMesh->MTangents[i].X, assMesh->MTangents[i].Y, assMesh->MTangents[i].Z);
                }
                else
                {
                    mesh.tangents[i] = Vector3.UnitX;
                }

                if (assMesh->MBitangents != null)
                {
                    mesh.binormals[i] = new Vector3(assMesh->MBitangents[i].X, assMesh->MBitangents[i].Y, assMesh->MBitangents[i].Z);
                }
                else
                {
                    mesh.binormals[i] = Vector3.UnitZ;
                }
            }

            mesh.triangles = new uint[assMesh->MNumFaces * 3];
            int iIdx = 0;
            for(int i = 0; i < assMesh->MNumFaces; i++)
            {
                Face face = assMesh->MFaces[i];
                for(int j = 0; j < face.MNumIndices; j++)
                {
                    mesh.triangles[iIdx++] = face.MIndices[j];
                }
            }

            Array.Reverse(mesh.triangles);

            meshNode.mesh = mesh;
        }

        internal unsafe void ProcessMaterials(Node* node, Scene* scene)
        {
            uint numMaterials = scene->MNumMaterials;

            if (numMaterials == 0)
                return;

            Assimp assimp = Assimp.GetApi();

            for (int i = 0; i < numMaterials; i++)
            {
                Material* material = scene->MMaterials[i];

                uint albedoTexCount = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeDiffuse);
                uint baseColorTexCount = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeBaseColor);
                uint normalTexCount = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeNormals);
                uint emissiveTexCount = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeEmissive);
                uint ORMTexCount = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeUnknown);
            }
        }

        internal ModelAsset(string path)
        {
            Path = path;

            unsafe
            {
                Assimp assimp = Assimp.GetApi();
                Scene* assScene = assimp.ImportFile(path, (uint)PostProcessPreset.TargetRealTimeMaximumQuality);

                if(assScene != null)
                {
                    ProcessMaterials(assScene->MRootNode, assScene);
                    ProcessNode(assScene->MRootNode, assScene, assScene->MRootNode->MTransformation);
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
        public uint uuid;
        public Mesh localMesh;
        public RenderableMeshVertex[] localVertices;
        public uint baseVertex;
        public uint baseIndex;
        public uint indexCount;
        public IVertexAttributeArray VAO;

        public override int GetHashCode()
        {
            return HashCode.Combine(uuid);
        }

        public override bool Equals(object obj)
        {
            RenderableMesh other = obj as RenderableMesh;
            if (other != null)
            {
                return other.uuid == uuid;
            }
            return false;
        }
    }

    public class RenderableManager : IDisposable
    {
        private readonly Dictionary<ModelAsset, List<RenderableMesh>> _renderableMeshes = new Dictionary<ModelAsset, List<RenderableMesh>>();
        private IDevice _device;
        private uint _meshCount = 0;

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

            List<MeshNode> meshes = asset.meshes;

            List<RenderableMeshVertex> combinedVertices = new List<RenderableMeshVertex>();
            List<uint> combinedIndices = new List<uint>();

            foreach (MeshNode meshNode in meshes)
            {
                ++ _meshCount;
                Mesh mesh = meshNode.mesh;

                RenderableMesh rMesh = new RenderableMesh
                {
                    localMesh = mesh,
                    uuid = _meshCount
                };

                uint vertexCount = (uint)mesh.positions.Length;

                rMesh.baseVertex = (uint)combinedVertices.Count;
                rMesh.baseIndex = (uint)combinedIndices.Count;
                rMesh.indexCount = (uint)mesh.triangles.Length;

                rMesh.localVertices = new RenderableMeshVertex[vertexCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    RenderableMeshVertex vertex = new RenderableMeshVertex
                    {
                        position = mesh.positions[i]
                    };

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
