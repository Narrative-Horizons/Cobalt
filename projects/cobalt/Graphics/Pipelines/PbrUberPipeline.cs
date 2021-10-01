using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Events;
using Cobalt.Graphics.API;
using Cobalt.Graphics.Passes;
using Cobalt.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static Cobalt.Graphics.RenderPass;

namespace Cobalt.Graphics.Pipelines
{
    public class PbrUberPipeline : RenderPipeline
    {
        private struct MaterialPayload
        {
            public uint Albedo;
            public uint Normal;
            public uint Emission;
            public uint OcclusionRoughnessMetallic;
        }

        private struct EntityData
        {
            public Matrix4 Transformation; // [0, 64)
            public uint MaterialId; // [64, 68)

            // padding
#pragma warning disable IDE0051 // Remove unused private members
            private readonly uint padding0; // [68, 72)
            private readonly uint padding1; // [72, 76)
            private readonly uint padding2; // [76, 80)
#pragma warning restore IDE0051 // Remove unused private members
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct SceneData
        {
            [FieldOffset(0)]
            public Matrix4 View;

            [FieldOffset(64)]
            public Matrix4 Projection;

            [FieldOffset(128)]
            public Matrix4 ViewProjection;

            [FieldOffset(192)]
            public Vector3 CameraPosition;

            [FieldOffset(208)]
            public Vector3 CameraDirection;

            [FieldOffset(224)]
            public DirectionalLightComponent Sun;
        }

        private class FrameData
        {
            public IDescriptorPool descriptorPool;
            public IDescriptorSet descriptorSet;
            public IBuffer entityData;
            public IBuffer materialData;
            public IBuffer indirectBuffer;
            public IBuffer sceneBuffer;
        }

        public static readonly string VisibilityBufferResourceName = "VisibilityBuffer";
        public static readonly string DepthBufferResourceName = "DepthBuffer";
        public static readonly string ColorResolveBufferResourceName = "ColorResolveBuffer";
        public static readonly string VisibilityPassFrameBufferResourceName = "VisibilityFrameBuffer";

        private static readonly uint MAT_NOT_FOUND = uint.MaxValue;
        private static readonly uint TEX_NOT_FOUND = uint.MaxValue;
        private static readonly uint MAX_TEX_COUNT = 500;

        private static readonly int INDIRECT_DRAW_STRUCT_SIZE = 20;

       private readonly PbrVisibilityPass _visibility;

        private ICommandPool _commandPool;
        private List<ICommandBuffer> _commandBuffers;
        private readonly Dictionary<EMaterialType, HashSet<Entity>> _renderables = new Dictionary<EMaterialType, HashSet<Entity>>();
        private readonly Dictionary<PbrMaterialComponent, uint> _materialIndices = new Dictionary<PbrMaterialComponent, uint>();
        private readonly MaterialPayload[] _materials = new MaterialPayload[1024];
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly Dictionary<Texture, uint> _textureIndices = new Dictionary<Texture, uint>();
        private readonly List<FrameData> _frames = new List<FrameData>();
        private readonly Registry _registry;
        private readonly Dictionary<EMaterialType, Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>>> _framePayload = new Dictionary<EMaterialType, Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>>>();
        public PbrUberPipeline(Registry registry, IDevice device, int width, int height, int frames) : base(device)
        {
            _registry = registry;
            _visibility = new PbrVisibilityPass(device, this);
            BuildResources(width, height, frames);

            registry.Events.AddHandler<ComponentAddEvent<PbrMaterialComponent>>(AddComponent);
            registry.Events.AddHandler<ComponentAddEvent<PbrMaterialComponent>>(AddComponent);
            registry.Events.AddHandler<ComponentAddEvent<MeshComponent>>(AddComponent);
            registry.Events.AddHandler<ComponentAddEvent<TransformComponent>>(AddComponent);
            registry.Events.AddHandler<EntityReleaseEvent>(RemoveEntity);
            registry.Events.AddHandler<ComponentRemoveEvent<PbrMaterialComponent>>(RemoveComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<MeshComponent>>(RemoveComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<TransformComponent>>(RemoveComponent);
        }
        public override void Render(FrameInfo frame, CameraComponent camera)
        {
            var frameInFlight = frame.frameInFlight;
            Build(frameInFlight, camera);

            DrawInfo drawInfo = new DrawInfo
            {
                indirectDrawBuffer = _frames[frameInFlight].indirectBuffer,
                payload = new Dictionary<EMaterialType, Dictionary<IVertexAttributeArray, DrawCommand>>(),
                descriptorSets = new List<IDescriptorSet>() { _frames[frameInFlight].descriptorSet }
            };

            DrawElementsIndirectCommand drawData = new DrawElementsIndirectCommand();
            NativeBuffer<DrawElementsIndirectCommandPayload> nativeIndirectData = new NativeBuffer<DrawElementsIndirectCommandPayload>(_frames[frameInFlight].indirectBuffer.Map());

            int idx = 0;

            foreach (var (type, payload) in _framePayload)
            {
                var items = new Dictionary<IVertexAttributeArray, DrawCommand>();
                drawInfo.payload.Add(type, items);
                foreach (var obj in payload)
                {
                    DrawCommand cmd = new DrawCommand
                    {
                        bufferOffset = drawData.Data.Count * INDIRECT_DRAW_STRUCT_SIZE,
                        indirect = drawData
                    };
                    items.Add(obj.Key, cmd);

                    foreach (var child in obj.Value)
                    {
                        RenderableMesh mesh = child.Key;
                        List<EntityData> instances = child.Value;

                        DrawElementsIndirectCommandPayload pay = new DrawElementsIndirectCommandPayload
                        {
                            BaseVertex = mesh.baseVertex,
                            FirstIndex = mesh.baseIndex,
                            BaseInstance = (uint)idx,
                            Count = mesh.indexCount,
                            InstanceCount = (uint)instances.Count
                        };

                        drawData.Add(pay);

                        nativeIndirectData.Set(pay);

                        idx += instances.Count;
                    }
                }
            }
            _frames[frameInFlight].indirectBuffer.Unmap();

            var cmdBuffer = _commandBuffers[frameInFlight % _commandBuffers.Count];
            _visibility.Record(cmdBuffer, frame, drawInfo);
        }

        public override void OnFrameStart(FrameInfo frame)
        {
            int frameInFlight = frame.frameInFlight;

            foreach (var (materialType, vaos) in _framePayload)
                foreach (var (vao, meshes) in vaos)
                    foreach (var (mesh, entities) in _framePayload)
                        entities.Clear();

            foreach (var (type, renderables) in _renderables)
            {
                var payload = _framePayload[type];
                foreach (var renderable in renderables)
                {
                    PbrMaterialComponent matComponent = _registry.Get<PbrMaterialComponent>(renderable);
                    MeshComponent mesh = _registry.Get<MeshComponent>(renderable);
                    EntityData e = new EntityData { MaterialId = GetOrInsert(matComponent), Transformation = _registry.Get<TransformComponent>(renderable).TransformMatrix };
                    RenderableMesh renderMesh = mesh.Mesh;
                    if (!payload.ContainsKey(renderMesh.vao))
                    {
                        payload.Add(renderMesh.vao, new Dictionary<RenderableMesh, List<EntityData>>());
                    }
                    if (!payload[renderMesh.vao].ContainsKey(renderMesh))
                    {
                        payload[renderMesh.vao].Add(renderMesh, new List<EntityData>());
                    }
                    payload[renderMesh.vao][renderMesh].Add(e);
                }
            }

            int writeFrame = (frameInFlight + 1) % _frames.Count;

            // Build material array
            NativeBuffer<MaterialPayload> nativeMaterialData = new NativeBuffer<MaterialPayload>(_frames[writeFrame].materialData.Map());
            foreach (MaterialPayload payload in _materials)
            {
                nativeMaterialData.Set(payload);
            }
            _frames[writeFrame].materialData.Unmap();

            // Build uniform/shader storage buffers
            NativeBuffer<EntityData> nativeEntityData = new NativeBuffer<EntityData>(_frames[writeFrame].entityData.Map());
            foreach (var (type, framePayload) in _framePayload)
            {
                foreach (var (vao, meshes) in framePayload)
                {
                    foreach (var (mesh, instances) in meshes)
                    {
                        instances.ForEach(instance => nativeEntityData.Set(instance));
                    }
                }
            }
            _frames[writeFrame].entityData.Unmap();

            List<DescriptorWriteInfo> writeInfos = new List<DescriptorWriteInfo>();
            DescriptorWriteInfo.Builder texArrayBuilder = new DescriptorWriteInfo.Builder();

            // Build texture array
            _textures.ForEach(tex =>
            {
                texArrayBuilder.AddImageInfo(new DescriptorWriteInfo.DescriptorImageInfo.Builder()
                    .View(tex.Image)
                    .Sampler(tex.Sampler)
                    .Layout(EImageLayout.ShaderReadOnly)
                    .Build());
            });

            writeInfos.Add(texArrayBuilder.DescriptorSet(_frames[frameInFlight].descriptorSet)
                .ArrayElement(0)
                .BindingIndex(3)
                .Build());

            // Per-Material
            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                        .Buffer(_frames[frameInFlight].materialData)
                        .Range(1000000 * 16)
                        .Build())
                    .BindingIndex(1)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0)
                    .Build());

            // Per-Entity Data
            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                        .Buffer(_frames[frameInFlight].entityData)
                        .Range(1000000 * 68)
                        .Build())
                    .BindingIndex(0)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0)
                    .Build());

            // Per-Scene Data
            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                        .Buffer(_frames[frameInFlight].sceneBuffer)
                        .Range(256)
                        .Build())
                    .BindingIndex(2)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0)
                    .Build());

            Device.UpdateDescriptorSets(writeInfos);
        }

        private void Build(int frameInFlight, CameraComponent camera)
        {
            // Upload data for next frame
            int writeFrame = (frameInFlight + 1) % _frames.Count;

            NativeBuffer<SceneData> nativeSceneData = new NativeBuffer<SceneData>(_frames[writeFrame].sceneBuffer.Map());
            SceneData data = new SceneData
            {
                View = camera.ViewMatrix,
                Projection = camera.ProjectionMatrix,
                ViewProjection = camera.ViewMatrix * camera.ProjectionMatrix,

                CameraPosition = camera.Transform.Position,
                CameraDirection = camera.Transform.Forward,

                Sun = new DirectionalLightComponent
                {
                    Direction = new Vector3(0, -1, 0),
                    Ambient = new Vector3(1),
                    Diffuse = new Vector3(1),
                    Specular = new Vector3(1),
                    Intensity = 1
                }
            };
            nativeSceneData.Set(data);
            _frames[writeFrame].sceneBuffer.Unmap();
        }

        private void BuildResources(int width, int height, int frames)
        {
            foreach (EMaterialType type in Enum.GetValues(typeof(EMaterialType)))
            {
                _framePayload.Add(type, new Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>>());
                _renderables.Add(type, new HashSet<Entity>());
            }

            // Build command buffers
            _commandPool = Device.CreateCommandPool(new ICommandPool.CreateInfo.Builder()
                .ResetAllocations(true)
                .TransientAllocations(false));
            _commandBuffers = _commandPool.Allocate(new ICommandBuffer.AllocateInfo.Builder()
                .Level(ECommandBufferLevel.Primary)
                .Count((uint)frames)
                .Build());

            List<IDescriptorSetLayout> descLayouts = new List<IDescriptorSetLayout>()
            {
                Device.CreateDescriptorSetLayout(new IDescriptorSetLayout.CreateInfo.Builder()
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(0)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("ObjectData")
                        .AddAccessibleStage(EShaderType.Vertex)
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(1)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("Materials")
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(2)
                        .Count(1)
                        .DescriptorType(EDescriptorType.UniformBuffer)
                        .Name("SceneData")
                        .AddAccessibleStage(EShaderType.Vertex)
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(3)
                        .Count((int)MAX_TEX_COUNT)
                        .DescriptorType(EDescriptorType.CombinedImageSampler)
                        .Name("Textures")
                        .AddAccessibleStage(EShaderType.Fragment).Build()))
            };

            _frames.AddRange(Enumerable.Range(0, frames).Select((frame) =>
            {
                FrameData data = new FrameData
                {
                    descriptorPool = Device.CreateDescriptorPool(new IDescriptorPool.CreateInfo.Builder()
                        .AddPoolSize(EDescriptorType.CombinedImageSampler, (int)MAX_TEX_COUNT * 4) // allow for 4 times the max number of textures per pool
                        .MaxSetCount(32)
                        .Build()),

                    entityData = Device.CreateBuffer(new IBuffer.CreateInfo<EntityData>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(1000000 * 68),
                    new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible)),

                    materialData = Device.CreateBuffer(new IBuffer.CreateInfo<MaterialPayload>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(1000000 * 16),
                    new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible)),

                    indirectBuffer = Device.CreateBuffer(new IBuffer.CreateInfo<DrawElementsIndirectCommandPayload>.Builder().AddUsage(EBufferUsage.IndirectBuffer).Size(16 * 1024),
                    new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostVisible).AddRequiredProperty(EMemoryProperty.HostCoherent)),

                    sceneBuffer = Device.CreateBuffer(IBuffer.FromPayload(new SceneData()).AddUsage(EBufferUsage.UniformBuffer),
                    new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible))
                };

                data.descriptorSet = data.descriptorPool.Allocate(new IDescriptorSet.CreateInfo.Builder()
                    .AddLayout(descLayouts)
                    .Build())[0];

                return data;
            }));

            // Build visibility images
            var visibility = Enumerable.Range(0, frames).Select((frame) =>
            {
                return Device.CreateImage(
                        new IImage.CreateInfo.Builder()
                            .Type(EImageType.Image2D)
                            .SampleCount(ESampleCount.Samples1)
                            .AddUsage(EImageUsage.ColorAttachment)
                            .AddUsage(EImageUsage.Sampled)
                            .Width(width)
                            .Height(height)
                            .Depth(1)
                            .Format(EDataFormat.R32G32_UINT)
                            .InitialLayout(EImageLayout.Undefined)
                            .MipCount(1)
                            .LayerCount(1),
                        new IImage.MemoryInfo.Builder()
                            .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                            .Usage(EMemoryUsage.GPUOnly))
                    .CreateImageView(new IImageView.CreateInfo.Builder()
                        .ViewType(EImageViewType.ViewType2D)
                        .MipLevelCount(1)
                        .ArrayLayerCount(1)
                        .BaseMipLevel(0)
                        .BaseArrayLayer(0)
                        .Format(EDataFormat.R32G32_UINT));
            }).ToList();
            Register(VisibilityBufferResourceName, visibility);

            // Build depth images
            var depth = Enumerable.Range(0, frames).Select((frame) =>
            {
                return Device.CreateImage(
                    new IImage.CreateInfo.Builder()
                        .Type(EImageType.Image2D)
                        .SampleCount(ESampleCount.Samples1)
                        .AddUsage(EImageUsage.DepthAttachment)
                        .Width(width)
                        .Height(height)
                        .Depth(1)
                        .Format(EDataFormat.D32_SFLOAT)
                        .InitialLayout(EImageLayout.Undefined)
                        .MipCount(1)
                        .LayerCount(1),
                    new IImage.MemoryInfo.Builder()
                        .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                        .Usage(EMemoryUsage.GPUOnly))
                .CreateImageView(new IImageView.CreateInfo.Builder()
                    .ViewType(EImageViewType.ViewType2D)
                    .MipLevelCount(1)
                    .ArrayLayerCount(1)
                    .BaseMipLevel(0)
                    .BaseArrayLayer(0)
                    .Format(EDataFormat.D32_SFLOAT));
            }).ToList();
            Register(DepthBufferResourceName, depth);

            // Build resolve images
            var resolve = Enumerable.Range(0, frames).Select((frame) =>
            {
                return Device.CreateImage(
                    new IImage.CreateInfo.Builder()
                        .Type(EImageType.Image2D)
                        .SampleCount(ESampleCount.Samples1)
                        .AddUsage(EImageUsage.ColorAttachment)
                        .Width(width)
                        .Height(height)
                        .Depth(1)
                        .Format(EDataFormat.R8G8B8A8)
                        .InitialLayout(EImageLayout.Undefined)
                        .MipCount(1)
                        .LayerCount(1),
                    new IImage.MemoryInfo.Builder()
                        .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                        .Usage(EMemoryUsage.GPUOnly))
                .CreateImageView(new IImageView.CreateInfo.Builder()
                    .ViewType(EImageViewType.ViewType2D)
                    .MipLevelCount(1)
                    .ArrayLayerCount(1)
                    .BaseMipLevel(0)
                    .BaseArrayLayer(0)
                    .Format(EDataFormat.R8G8B8A8));
            }).ToList();
            Register(ColorResolveBufferResourceName, resolve);

            // Build visibility framebuffer
            /*var visFbo = Enumerable.Range(0, frames).Select((frame) =>
            {
                return Device.CreateFrameBuffer(new IFrameBuffer.CreateInfo.Builder()
                    .Layers(1)
                    .Width(width)
                    .Height(height)
                    .RenderPass(_visibility.Native)
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder()
                        .ImageView(visibility[frame])
                        .Usage(EImageUsage.ColorAttachment))
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder()
                        .ImageView(depth[frame])
                        .Usage(EImageUsage.DepthAttachment)));
            }).ToList();
            Register(VisibilityPassFrameBufferResourceName, visFbo);*/
        }

        private bool AddComponent<T>(ComponentAddEvent<T> data)
        {
            AddEntity(data.Entity, data.Registry);
            return false;
        }

        private void AddEntity(Entity ent, Registry reg)
        {
            if (reg.Has<PbrMaterialComponent>(ent) && reg.Has<TransformComponent>(ent) && reg.Has<MeshComponent>(ent))
            {
                PbrMaterialComponent matComponent = reg.Get<PbrMaterialComponent>(ent);

                if (matComponent != default)
                {
                    GetOrInsert(matComponent);
                    _renderables[matComponent.Type].Add(ent);
                }
            }
        }

        private bool RemoveComponent<T>(ComponentRemoveEvent<T> data)
        {
            PbrMaterialComponent matComponent = data.Registry.Get<PbrMaterialComponent>(data.Entity);
            if (matComponent != default)
            {
                _renderables[matComponent.Type].Remove(data.Entity);
            }
            return false;
        }

        private bool RemoveEntity(EntityReleaseEvent data)
        {
            PbrMaterialComponent matComponent = data.SpawningRegistry.Get<PbrMaterialComponent>(data.SpawnedEntity);
            if (matComponent != default)
            {
                _renderables[matComponent.Type].Remove(data.SpawnedEntity);
            }
            return false;
        }

        private uint GetOrInsert(PbrMaterialComponent mat)
        {
            uint matId = _materialIndices.GetValueOrDefault(mat, MAT_NOT_FOUND);
            if (matId == MAT_NOT_FOUND)
            {
                _materialIndices.Add(mat, (uint)_materialIndices.Count);
                matId = (uint)_materialIndices.Count - 1;
                _materials[matId] = new MaterialPayload
                {
                    Albedo = GetOrInsert(mat.Albedo),
                    Normal = GetOrInsert(mat.Normal),
                    Emission = GetOrInsert(mat.Emission),
                    OcclusionRoughnessMetallic = GetOrInsert(mat.OcclusionRoughnessMetallic)
                };
            }
            return matId;
        }

        private uint GetOrInsert(Texture tex)
        {
            if (tex == default)
            {
                return TEX_NOT_FOUND;
            }

            if (_textureIndices.ContainsKey(tex))
            {
                return _textureIndices.GetValueOrDefault(tex);
            }
            _textureIndices.Add(tex, (uint)_textureIndices.Count);
            _textures.Add(tex);
            return (uint)_textureIndices.Count - 1;
        }
    }
}
