using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
using Cobalt.Math;
using CobaltConverter.Core;
using Silk.NET.Assimp;
using static Cobalt.Bindings.STB.ImageLoader;
using static Cobalt.Bindings.Utils.Util;

namespace Cobalt.Core
{
    public class AssimpTextureData
    {
        public string path;
        public TextureMapping mapping;
        public uint uvindex;
        public float blend;
        public TextureOp op;
        public TextureMapMode mapMode;
        public uint flags;
    }

    public class ImageAsset
    {
        public uint Width { get; }
        public uint Height { get; }
        public uint Components { get; }
        public bool IsHDR { get; }
        public uint BitsPerPixel { get; }
        public byte[] AsBytes { get; }
        public ushort[] AsUnsignedShorts { get; }
        public float[] AsFloats { get; }

        internal ImageAsset(string path)
        {
            ImagePayload payload = LoadImage(path);
            Width = (uint)payload.width;
            Height = (uint)payload.height;
            Components = (uint)payload.channels;
            IsHDR = payload.hdr_f_image != IntPtr.Zero;

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

        internal ImageAsset(ImagePayload payload)
        {
            Width = (uint)payload.width;
            Height = (uint)payload.height;
            Components = (uint)payload.channels;
            IsHDR = payload.hdr_f_image != IntPtr.Zero;

            int count = (int)(Width * Height * Components);

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
        public List<MeshNode> meshes = new List<MeshNode>();
        public List<MaterialData> materials = new List<MaterialData>();
        public string Path { get; }
        public string RootPath { get; }
        internal AssetManager Manager { get; }

        public MeshNode rootNode;

        private void ProcessMeshNode(Entity parent, Registry registry, MeshNode node, List<RenderableMesh> renderableMeshes)
        {
            foreach(Mesh mesh in node.meshes)
            {
                Entity meshEntity = registry.Create();
                TransformComponent meshTransform = new TransformComponent
                {
                    Parent = registry.Get<TransformComponent>(parent),
                    TransformMatrix = Matrix4.Identity
                };

                registry.Assign(meshEntity, meshTransform);

                RenderableMesh rMesh = renderableMeshes.Find(m => m.localMesh.GUID == mesh.GUID);
                MeshComponent meshComp = new MeshComponent(rMesh);

                registry.Assign(meshEntity, meshComp);

                PbrMaterialComponent materialComponent = new PbrMaterialComponent
                {
                    Type = EMaterialType.Opaque,
                };

                if(mesh.material.albedo != null)
                {
                    materialComponent.Albedo = new Graphics.Texture { Image = mesh.material.albedo.view, Sampler = mesh.material.albedo.sampler };
                }

                if(mesh.material.normal != null)
                {
                    materialComponent.Normal = new Graphics.Texture { Image = mesh.material.normal.view, Sampler = mesh.material.normal.sampler };
                }

                if(mesh.material.ORM != null)
                {
                    materialComponent.OcclusionRoughnessMetallic = new Graphics.Texture { Image = mesh.material.ORM.view, Sampler = mesh.material.ORM.sampler };
                }

                if(mesh.material.emissive != null)
                {
                    materialComponent.Emission = new Graphics.Texture { Image = mesh.material.emissive.view, Sampler = mesh.material.emissive.sampler };
                }

                registry.Assign(meshEntity, materialComponent);
            }

            foreach (MeshNode child in node.children)
            {
                Entity childEntity = registry.Create();
                TransformComponent trans = new TransformComponent
                {
                    Parent = registry.TryGet<TransformComponent>(parent),
                    TransformMatrix = child.transform
                };

                registry.Assign(childEntity, trans);

                ProcessMeshNode(childEntity, registry, child, renderableMeshes);

                registry.Get<TransformComponent>(parent).Children.Add(registry.TryGet<TransformComponent>(childEntity));
            }
        }

        public Entity AsEntity(Registry registry, RenderableManager renderableManager)
        {
            renderableManager.QueueRenderable(this);
            List<RenderableMesh> renderables = renderableManager.GetRenderables(this);

            Entity rootEntity = registry.Create();
            registry.Assign(rootEntity, new TransformComponent() { TransformMatrix = Matrix4.Identity });
            ProcessMeshNode(rootEntity, registry, rootNode, renderables);

            return rootEntity;
        }

        internal unsafe void ProcessNode(Node* node, MeshNode meshNode, Scene* scene, Matrix4 parentMatrix)
        {
            Matrix4 mat = new Matrix4();

            MeshNode mNode;

            if (node == scene->MRootNode)
            {
                mNode = meshNode = rootNode = new MeshNode();
                meshNode.transform = parentMatrix;
                meshes.Add(mNode);
            }
            else
            { 
                mat= System.Numerics.Matrix4x4.Transpose(node->MTransformation);

                mNode = new MeshNode
                {
                    transform = mat
                };

                meshNode.children.Add(mNode);
                meshes.Add(mNode);
            }

            for (int i = 0; i < node->MNumMeshes; i++)
            {
                Silk.NET.Assimp.Mesh* assMesh = scene->MMeshes[node->MMeshes[i]];
                ProcessMesh(assMesh, scene, mNode);
            }

            for(int i = 0; i < node->MNumChildren; i++)
            {
                ProcessNode(node->MChildren[i], mNode, scene, mat);
            }
        }

        internal unsafe void ProcessMesh(Silk.NET.Assimp.Mesh* assMesh, Scene* scene, MeshNode meshNode)
        {
            Mesh mesh = new Mesh
            {
                positions = new Vector3[assMesh->MNumVertices],
                normals = new Vector3[assMesh->MNumVertices],
                tangents = new Vector3[assMesh->MNumVertices],
                binormals = new Vector3[assMesh->MNumVertices],
                texcoords = new Vector2[assMesh->MNumVertices],
                GUID = Guid.NewGuid()
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

            mesh.material = materials[(int)assMesh->MMaterialIndex];
            meshNode.meshes.Add(mesh);
        }

        internal unsafe AssimpTextureData GetTexture(Material* material, TextureType type, uint index)
        {
            Assimp assimp = Assimp.GetApi();
            AssimpTextureData data = new AssimpTextureData();
            AssimpString path = new AssimpString();

            assimp.GetMaterialTexture(material, type, 0, ref path, ref data.mapping, ref data.uvindex, ref data.blend, ref data.op, ref data.mapMode, ref data.flags);

            data.path = path.ToString();
            return data;
        }

        internal TextureData CreateCombinedOrmTextureData(AssimpTextureData rmData, AssimpTextureData oData)
        {
            ImagePayload combinedPayload = ImageConverter.ConvertToORM(RootPath + rmData.path, RootPath + oData.path);
            ImageAsset combinedAsset = new ImageAsset(combinedPayload);

            int mipCount = (int)MathF.Floor(MathF.Log2(MathF.Max(combinedAsset.Width, combinedAsset.Height))) + 1;

            IImage image = Manager.Device.CreateImage(new IImage.CreateInfo.Builder()
                .Depth(1).Format(EDataFormat.R8G8B8A8).Height((int)combinedAsset.Height).Width((int)combinedAsset.Width)
                .InitialLayout(EImageLayout.Undefined).LayerCount(1).MipCount(mipCount).SampleCount(ESampleCount.Samples1)
                .Type(EImageType.Image2D),
            new IImage.MemoryInfo.Builder()
                .AddRequiredProperty(EMemoryProperty.DeviceLocal).Usage(EMemoryUsage.GPUOnly));

            Manager.TransferBuffer.Record(new ICommandBuffer.RecordInfo());

            Manager.TransferBuffer.Copy(combinedAsset.AsBytes, image, new List<ICommandBuffer.BufferImageCopyRegion>(){new ICommandBuffer.BufferImageCopyRegion.Builder().ArrayLayer(0)
                        .BufferOffset(0).ColorAspect(true).Depth(1).Height((int) combinedAsset.Height).Width((int) combinedAsset.Width).MipLevel(0).Build() });

            Manager.TransferBuffer.End();

            IQueue.SubmitInfo transferSubmission = new IQueue.SubmitInfo.Builder().Buffer(Manager.TransferBuffer).Build();
            Manager.TransferQueue.Execute(transferSubmission);

            IImageView imageView = image.CreateImageView(new IImageView.CreateInfo.Builder().ArrayLayerCount(1).BaseArrayLayer(0).BaseMipLevel(0).Format(EDataFormat.R8G8B8A8)
                .MipLevelCount(mipCount).ViewType(EImageViewType.ViewType2D));

            // TODO: Get this data from the TextureData from Assimp
            ISampler imageSampler = Manager.Device.CreateSampler(new ISampler.CreateInfo.Builder().AddressModeU(EAddressMode.Repeat)
                .AddressModeV(EAddressMode.Repeat).AddressModeW(EAddressMode.Repeat).MagFilter(EFilter.Linear).MinFilter(EFilter.Linear)
                .MipmapMode(EMipmapMode.Linear));

            TextureData texData = new TextureData
            {
                asset = combinedAsset,
                image = image,
                data = rmData,
                sampler = imageSampler,
                view = imageView
            };

            return texData;
        }

        internal TextureData CreateTextureData(AssimpTextureData data)
        {
            ImageAsset asset = Manager.LoadImage(RootPath + data.path);

            int mipCount = (int)MathF.Floor(MathF.Log2(MathF.Max(asset.Width, asset.Height))) + 1;

            IImage image = Manager.Device.CreateImage(new IImage.CreateInfo.Builder()
                .Depth(1).Format(EDataFormat.R8G8B8A8).Height((int)asset.Height).Width((int)asset.Width)
                .InitialLayout(EImageLayout.Undefined).LayerCount(1).MipCount(mipCount).SampleCount(ESampleCount.Samples1)
                .Type(EImageType.Image2D),
            new IImage.MemoryInfo.Builder()
                .AddRequiredProperty(EMemoryProperty.DeviceLocal).Usage(EMemoryUsage.GPUOnly));

            Manager.TransferBuffer.Record(new ICommandBuffer.RecordInfo());

            Manager.TransferBuffer.Copy(asset.AsBytes, image, new List<ICommandBuffer.BufferImageCopyRegion>(){new ICommandBuffer.BufferImageCopyRegion.Builder().ArrayLayer(0)
                        .BufferOffset(0).ColorAspect(true).Depth(1).Height((int) asset.Height).Width((int) asset.Width).MipLevel(0).Build() });

            Manager.TransferBuffer.End();

            IQueue.SubmitInfo transferSubmission = new IQueue.SubmitInfo.Builder().Buffer(Manager.TransferBuffer).Build();
            Manager.TransferQueue.Execute(transferSubmission);

            IImageView imageView = image.CreateImageView(new IImageView.CreateInfo.Builder().ArrayLayerCount(1).BaseArrayLayer(0).BaseMipLevel(0).Format(EDataFormat.R8G8B8A8)
                .MipLevelCount(mipCount).ViewType(EImageViewType.ViewType2D));

            // TODO: Get this data from the TextureData from Assimp
            ISampler imageSampler = Manager.Device.CreateSampler(new ISampler.CreateInfo.Builder().AddressModeU(EAddressMode.Repeat)
                .AddressModeV(EAddressMode.Repeat).AddressModeW(EAddressMode.Repeat).MagFilter(EFilter.Linear).MinFilter(EFilter.Linear)
                .MipmapMode(EMipmapMode.Linear));

            TextureData texData = new TextureData
            {
                asset = asset,
                image = image,
                data = data,
                sampler = imageSampler,
                view = imageView
            };

            return texData;
        }

        internal unsafe void ProcessMaterials(Node* node, Scene* scene)
        {
            uint numMaterials = scene->MNumMaterials;

            if (numMaterials == 0)
                return;

            Assimp assimp = Assimp.GetApi();

            for (int i = 0; i < numMaterials; i++)
            {
                MaterialData mat = new MaterialData();
                Material* material = scene->MMaterials[i];

                bool hasAlbedo = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeDiffuse) > 0;
                bool hasNormal = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeNormals) > 0;
                bool hasEmissive = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeEmissive) > 0;
                bool hasORM = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeUnknown) > 0;
                bool hasAO = assimp.GetMaterialTextureCount(material, TextureType.TextureTypeLightmap) > 0;

                if(hasAlbedo)
                {
                    mat.albedo = CreateTextureData(GetTexture(material, TextureType.TextureTypeDiffuse, 0));
                }

                if(hasNormal)
                {
                    mat.normal = CreateTextureData(GetTexture(material, TextureType.TextureTypeNormals, 0));
                }

                if(hasEmissive)
                {
                    mat.emissive = CreateTextureData(GetTexture(material, TextureType.TextureTypeEmissive, 0));
                }
                
                if(hasAO && hasORM)
                {
                    // Combine
                    AssimpTextureData rmData = GetTexture(material, TextureType.TextureTypeUnknown, 0);
                    AssimpTextureData oData = GetTexture(material, TextureType.TextureTypeLightmap, 0);

                    mat.ORM = CreateCombinedOrmTextureData(rmData, oData);
                }

                if(hasORM && !hasAO)
                {
                    mat.ORM = CreateTextureData(GetTexture(material, TextureType.TextureTypeUnknown, 0));
                }

                if(hasAO && !hasORM)
                {
                    // TODO: Generate texture with Red values for AO
                }

                materials.Add(mat);
            }
        }

        internal ModelAsset(AssetManager manager, string path)
        {
            Path = path;
            Manager = manager;
            RootPath = path[..(path.LastIndexOf('/') + 1)];

            unsafe
            {
                Assimp assimp = Assimp.GetApi();
                Scene* assScene = assimp.ImportFile(path, (uint)PostProcessPreset.TargetRealTimeMaximumQuality);

                //Matrix4 offset = Matrix4.Rotate(new Vector3(0, 90, 0));

                if (assScene == null) 
                    return;

                ProcessMaterials(assScene->MRootNode, assScene);
                ProcessNode(assScene->MRootNode, rootNode, assScene, System.Numerics.Matrix4x4.Transpose(assScene->MRootNode->MTransformation));
            }
        }
    }

    public class AssetManager : IDisposable
    {
        private readonly Dictionary<string, ImageAsset> _images = new Dictionary<string, ImageAsset>();
        private readonly Dictionary<string, ModelAsset> _models = new Dictionary<string, ModelAsset>();

        internal IDevice Device { get; }

        internal ICommandPool TransferPool { get; }
        internal ICommandBuffer TransferBuffer { get; }
        internal IQueue TransferQueue { get; }

        public AssetManager(IDevice device)
        {
            Device = device;

            TransferQueue = device.Queues().Find(queue => queue.GetProperties().Transfer);

            TransferPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().Queue(TransferQueue)
                .ResetAllocations(true).TransientAllocations(true));

            TransferBuffer = TransferPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(1).Level(ECommandBufferLevel.Primary))[0];
        }

        public void Dispose()
        {
        }

        public ImageAsset LoadImage(string path)
        { 
            if (_images.ContainsKey(path))
                return _images[path];

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
            if (_models.ContainsKey(path))
                return _models[path];

            var asset = new ModelAsset(this, path);
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
        public IVertexAttributeArray vao;

        public override int GetHashCode()
        {
            return HashCode.Combine(uuid);
        }

        public override bool Equals(object obj)
        {
            if (obj is RenderableMesh other)
            {
                return other.uuid == uuid;
            }

            return false;
        }
    }

    public class RenderableManager : IDisposable
    {
        private readonly Dictionary<ModelAsset, List<RenderableMesh>> _renderableMeshes = new Dictionary<ModelAsset, List<RenderableMesh>>();
        private readonly IDevice _device;
        private uint _meshCount;

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
            if (_renderableMeshes.ContainsKey(asset))
                return;

            List<RenderableMesh> processingMeshes = new List<RenderableMesh>();

            List<MeshNode> meshes = asset.meshes;

            List<RenderableMeshVertex> combinedVertices = new List<RenderableMeshVertex>();
            List<uint> combinedIndices = new List<uint>();

            foreach (var mesh in meshes.SelectMany(meshNode => meshNode.meshes))
            {
                ++_meshCount;
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
                        position = mesh.positions[i],
                        uv = mesh.texcoords?[i] ?? Vector2.Zero,
                        normal = mesh.normals?[i] ?? Vector3.UnitY,
                        tangent = mesh.tangents?[i] ?? Vector3.UnitX,
                        binormal = mesh.binormals?[i] ?? Vector3.UnitZ
                    };

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
                rM.vao = vao;
            }

            _renderableMeshes.Add(asset, processingMeshes);
        }

        public void Dispose()
        {

        }
    }
}
