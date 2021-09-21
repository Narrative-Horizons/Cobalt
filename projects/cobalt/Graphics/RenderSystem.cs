using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Events;
using Cobalt.Graphics.API;
using Cobalt.Graphics.Passes;
using Cobalt.Math;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Cobalt.Graphics.RenderPass;

namespace Cobalt.Graphics
{
    public class RenderSystem
    {
        private Registry _registry;
        private ICommandPool _cmdPool;
        private List<ICommandBuffer> _cmdBuffers;
        private readonly uint _framesInFlight;
        private int _currentFrame = 0;
        private IQueue _submitQueue;
        private PbrPipeline _pipeline;
        private List<IFence> _renderSync = new List<IFence>();

        public RenderSystem(Registry registry, IDevice device, ISwapchain swapchain)
        {
            _registry = registry;
            _framesInFlight = swapchain.GetImageCount();
            _pipeline = new PbrPipeline(registry, device, swapchain);
            _submitQueue = device.Queues().Find(queue => queue.GetProperties().Graphics);
            _cmdPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().ResetAllocations(true));
            _cmdBuffers = _cmdPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(_framesInFlight).Level(ECommandBufferLevel.Primary).Build());

            for (uint i = 0; i < swapchain.GetImageCount(); ++i)
            {
                _renderSync.Add(device.CreateFence(new IFence.CreateInfo.Builder()
                    .Signaled(true)
                    .Build()));
            }
        }

        public void Render()
        {
            ICommandBuffer cmdBuffer = _cmdBuffers[_currentFrame];
            IFence renderSync = _renderSync[_currentFrame];

            renderSync.Wait();

            ComponentView<CameraComponent> cameraView = _registry.GetView<CameraComponent>();
            cameraView.ForEach((camera) =>
            {
                /// TODO: Move to ScriptableComponentSystem
                camera.OnUpdate();

                cmdBuffer.Record(new ICommandBuffer.RecordInfo());
                _pipeline.Render(cmdBuffer, _currentFrame, camera);

            });

            cmdBuffer.End();

            _submitQueue.Execute(new IQueue.SubmitInfo.Builder()
                .Buffer(cmdBuffer)
                .Signal(renderSync)
                .Build());

            // Compute visibility pass

            // Z Pass
            // Opaque Pass
            // Translucent Pass
            // Resolve Pass
            // Post Processing

            _currentFrame = (_currentFrame + 1) % (int) _framesInFlight;
        }
    }

    class PbrPipeline
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
            private uint padding0; // [68, 72)
            private uint padding1; // [72, 76)
            private uint padding2; // [76, 80)
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

        private class FrameImages
        {
            public IFrameBuffer visibilityFbo;
            public IFrameBuffer resolveFbo;
            public IImageView visibility;
            public IImageView depth;
            public IImageView color;
        }

        private FrameImages[] _visibility;

        private IFrameBuffer[] _frameBuffer;
        private IImageView[] _colorAttachmentViews;
        private ISampler _imageResolveSampler;

        private readonly ZPrePass _zPrePass;
        private readonly PbrRenderPass _pbrPass;
        private readonly ScreenResolvePass _screenResolvePass;

        private readonly VisibilityPass _visibilityPass;
        private readonly OpaquePbrUberPass _opaquePbrPass;

        private readonly Dictionary<EMaterialType, HashSet<Entity>> _renderables = new Dictionary<EMaterialType, HashSet<Entity>>();
        private readonly Dictionary<PbrMaterialComponent, uint> _materialIndices = new Dictionary<PbrMaterialComponent, uint>();
        private readonly MaterialPayload[] _materials = new MaterialPayload[1024];
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly Dictionary<Texture, uint> _textureIndices = new Dictionary<Texture, uint>();
        private readonly IDevice _device;
        private readonly List<FrameData> _frames = new List<FrameData>();
        private readonly Registry _registry;
        private readonly Dictionary<EMaterialType, Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>>> _framePayload = new Dictionary<EMaterialType, Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>>>();

        private static readonly uint MAT_NOT_FOUND = uint.MaxValue;
        private static readonly uint TEX_NOT_FOUND = uint.MaxValue;
        private static readonly uint MAX_TEX_COUNT = 500;

        public PbrPipeline(Registry registry, IDevice device, ISwapchain swapchain)
        {
            foreach (EMaterialType type in Enum.GetValues(typeof(EMaterialType)))
            {
                _framePayload.Add(type, new Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>>());
                _renderables.Add(type, new HashSet<Entity>());
            }

            int framesInFlight = (int) swapchain.GetImageCount();

            IPipelineLayout layout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder().AddDescriptorSetLayout(device.CreateDescriptorSetLayout(
                    new IDescriptorSetLayout.CreateInfo.Builder()
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
                        .Count(1)
                        .DescriptorType(EDescriptorType.CombinedImageSampler)
                        .Name("VisibilityBuffer")
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(4)
                        .Count(1)
                        .DescriptorType(EDescriptorType.UniformBuffer)
                        .Name("VisibilityBufferParameters")
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(5)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("VertexBuffer")
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(6)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("IndexBuffer")
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(7)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("DrawCommands")
                        .AddAccessibleStage(EShaderType.Fragment))
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(8)
                        .Count((int)MAX_TEX_COUNT)
                        .DescriptorType(EDescriptorType.CombinedImageSampler)
                        .Name("Textures")
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .Build()))
                .Build());

            _device = device;
            _registry = registry;
            _zPrePass = new ZPrePass(device);
            _pbrPass = new PbrRenderPass(device, layout, EMaterialType.Opaque);
            _screenResolvePass = new ScreenResolvePass(swapchain, device, 1280, 720);
            _visibilityPass = new VisibilityPass(device);
            _opaquePbrPass = new OpaquePbrUberPass(device, layout);

            registry.Events.AddHandler<ComponentAddEvent<PbrMaterialComponent>>(_AddComponent);
            registry.Events.AddHandler<ComponentAddEvent<PbrMaterialComponent>>(_AddComponent);
            registry.Events.AddHandler<ComponentAddEvent<MeshComponent>>(_AddComponent);
            registry.Events.AddHandler<ComponentAddEvent<TransformComponent>>(_AddComponent);
            registry.Events.AddHandler<EntityReleaseEvent>(_RemoveEntity);
            registry.Events.AddHandler<ComponentRemoveEvent<PbrMaterialComponent>>(_RemoveComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<MeshComponent>>(_RemoveComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<TransformComponent>>(_RemoveComponent);

            _BuildFrameData(framesInFlight, layout);
        }

        public void Render(ICommandBuffer buffer, int frameInFlight, CameraComponent camera)
        {
            _Build(frameInFlight, camera);

            int idx = 0;

            DrawInfo drawInfo = new DrawInfo();
            drawInfo.indirectDrawBuffer = _frames[frameInFlight].indirectBuffer;
            drawInfo.payload = new Dictionary<EMaterialType, Dictionary<IVertexAttributeArray, DrawCommand>>();
            drawInfo.descriptorSets = new List<IDescriptorSet>() { _frames[frameInFlight].descriptorSet };

            DrawElementsIndirectCommand drawData = new DrawElementsIndirectCommand();
            NativeBuffer<DrawElementsIndirectCommandPayload> nativeIndirectData = new NativeBuffer<DrawElementsIndirectCommandPayload>(_frames[frameInFlight].indirectBuffer.Map());
            
            foreach (var (type, payload) in _framePayload)
            {
                var items = new Dictionary<IVertexAttributeArray, DrawCommand>();
                drawInfo.payload.Add(type, items);
                foreach (var obj in payload)
                {
                    DrawCommand cmd = new DrawCommand();
                    cmd.bufferOffset = drawData.Data.Count * 20;
                    cmd.indirect = drawData;
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

            FrameInfo sceneRenderInfo = new FrameInfo
            {
                frameBuffer = _frameBuffer[frameInFlight],
                frameInFlight = frameInFlight,
                width = 1280,
                height = 720
            };

            FrameInfo pbrVisRenderInfo = new FrameInfo
            {
                frameBuffer = _visibility[frameInFlight].visibilityFbo,
                frameInFlight = frameInFlight,
                width = 1280,
                height = 720
            };

            // Handle scene draw
            buffer.Sync();

            _visibilityPass.Record(buffer, pbrVisRenderInfo, drawInfo);

            pbrVisRenderInfo.frameBuffer = _visibility[frameInFlight].resolveFbo;
            _opaquePbrPass.Record(buffer, pbrVisRenderInfo, drawInfo);

            _zPrePass.Record(buffer, sceneRenderInfo, drawInfo);
            _pbrPass.Record(buffer, sceneRenderInfo, drawInfo);

            // Handle screen resolution
            var frameInfo = new FrameInfo { frameInFlight = frameInFlight };
            _screenResolvePass.SetInputTexture(new Texture() { Image = _colorAttachmentViews[frameInFlight], Sampler = _imageResolveSampler }, frameInfo);
            _screenResolvePass.Record(buffer, frameInfo, new DrawInfo());
        }

        private void _Build(int frameInFlight, CameraComponent camera)
        {
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
                .BindingIndex(8)
                .Build());

            // Per-Material
            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo( 
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(_frames[frameInFlight].materialData)
                    .Range(1000000 * 16).Build())
                    .BindingIndex(1)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            // Per-Entity Data
            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(_frames[frameInFlight].entityData)
                    .Range(1000000 * 68).Build())
                    .BindingIndex(0)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            // Per-Scene Data
            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(_frames[frameInFlight].sceneBuffer)
                    .Range(256).Build())
                    .BindingIndex(2)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            _device.UpdateDescriptorSets(writeInfos);

            // Upload data for next frame

            foreach (var (vao, meshes) in _framePayload)
                foreach (var (mesh, entities) in _framePayload)
                    entities.Clear();

            foreach (var (type, renderables) in _renderables)
            {
                var payload = _framePayload[type];
                foreach (var renderable in renderables)
                {
                    PbrMaterialComponent matComponent = _registry.Get<PbrMaterialComponent>(renderable);
                    MeshComponent mesh = _registry.Get<MeshComponent>(renderable);
                    EntityData e = new EntityData { MaterialId = _GetOrInsert(matComponent), Transformation = _registry.Get<TransformComponent>(renderable).TransformMatrix };
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

        private bool _AddComponent<T>(ComponentAddEvent<T> data) 
        {
            _AddEntity(data.Entity, data.Registry);
            return false;
        }

        private void _AddEntity(Entity ent, Registry reg)
        {
            if (reg.Has<PbrMaterialComponent>(ent) && reg.Has<TransformComponent>(ent) && reg.Has<MeshComponent>(ent))
            {
                PbrMaterialComponent matComponent = reg.Get<PbrMaterialComponent>(ent);

                if (matComponent != default)
                {
                    _GetOrInsert(matComponent);
                    _renderables[matComponent.Type].Add(ent);
                }
            }
        }

        private bool _RemoveComponent<T>(ComponentRemoveEvent<T> data)
        {
            PbrMaterialComponent matComponent = data.Registry.Get<PbrMaterialComponent>(data.Entity);
            if (matComponent != default)
            {
                _renderables[matComponent.Type].Remove(data.Entity);
            }
            return false;
        }

        private bool _RemoveEntity(EntityReleaseEvent data)
        {
            PbrMaterialComponent matComponent = data.SpawningRegistry.Get<PbrMaterialComponent>(data.SpawnedEntity);
            if (matComponent != default)
            {
                _renderables[matComponent.Type].Remove(data.SpawnedEntity);
            }
            return false;
        }

        private uint _GetOrInsert(PbrMaterialComponent mat)
        {
            uint matId = _materialIndices.GetValueOrDefault(mat, MAT_NOT_FOUND);
            if (matId == MAT_NOT_FOUND)
            {
                _materialIndices.Add(mat, (uint)_materialIndices.Count);
                matId = (uint)_materialIndices.Count - 1;
                _materials[matId] = new MaterialPayload
                {
                    Albedo = _GetOrInsert(mat.Albedo),
                    Normal = _GetOrInsert(mat.Normal),
                    Emission = _GetOrInsert(mat.Emission),
                    OcclusionRoughnessMetallic = _GetOrInsert(mat.OcclusionRoughnessMetallic)
                };
            }
            return matId;
        }

        private uint _GetOrInsert(Texture tex)
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
    
        private void _BuildFrameData(int framesInFlight, IPipelineLayout layout)
        {
            for (int frame = 0; frame < framesInFlight; ++frame)
            {
                if (frame >= _frames.Count)
                {
                    _frames.Capacity = frame + 1;
                    _frames.Add(new FrameData());
                    _frames[frame].descriptorPool = _device.CreateDescriptorPool(new IDescriptorPool.CreateInfo.Builder()
                        .AddPoolSize(EDescriptorType.CombinedImageSampler, (int)MAX_TEX_COUNT * 4) // allow for 4 times the max number of textures per pool
                        .MaxSetCount(32)
                        .Build());

                    _frames[frame].descriptorSet = _frames[frame].descriptorPool.Allocate(new IDescriptorSet.CreateInfo.Builder()
                        .AddLayout(layout.GetDescriptorSetLayouts()[0])
                        .Build())[0];

                    /// TODO: Make this actual sizeof
                    _frames[frame].entityData = _device.CreateBuffer(new IBuffer.CreateInfo<EntityData>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(1000000 * 68),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));

                    _frames[frame].materialData = _device.CreateBuffer(new IBuffer.CreateInfo<MaterialPayload>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(1000000 * 16),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));

                    _frames[frame].indirectBuffer = _device.CreateBuffer(new IBuffer.CreateInfo<DrawElementsIndirectCommandPayload>.Builder().AddUsage(EBufferUsage.IndirectBuffer).Size(16 * 1024),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostVisible).AddRequiredProperty(EMemoryProperty.HostCoherent));

                    _frames[frame].sceneBuffer = _device.CreateBuffer(IBuffer.FromPayload(new SceneData()).AddUsage(EBufferUsage.UniformBuffer),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));

                    // TODO: Handle transparency information
                }
            }

            _frameBuffer = new IFrameBuffer[framesInFlight];
            _visibility = new FrameImages[framesInFlight];

            IImage[] colorAttachments = new IImage[framesInFlight];
            IImage[] depthAttachments = new IImage[framesInFlight];
            _colorAttachmentViews = new IImageView[framesInFlight];
            IImageView[] depthAttachmentViews = new IImageView[framesInFlight];

            for (int i = 0; i < framesInFlight; i++)
            {
                colorAttachments[i] = _device.CreateImage(new IImage.CreateInfo.Builder().AddUsage(EImageUsage.ColorAttachment).Width(1280).Height(720).Type(EImageType.Image2D)
                    .Format(EDataFormat.R8G8B8A8).MipCount(1).LayerCount(1),
                    new IImage.MemoryInfo.Builder().Usage(EMemoryUsage.GPUOnly).AddRequiredProperty(EMemoryProperty.DeviceLocal));

                _colorAttachmentViews[i] = colorAttachments[i].CreateImageView(new IImageView.CreateInfo.Builder().ViewType(EImageViewType.ViewType2D).BaseArrayLayer(0)
                    .BaseMipLevel(0).ArrayLayerCount(1).MipLevelCount(1).Format(EDataFormat.R8G8B8A8));

                depthAttachments[i] = _device.CreateImage(new IImage.CreateInfo.Builder().AddUsage(EImageUsage.DepthAttachment).Width(1280).Height(720).Type(EImageType.Image2D)
                    .Format(EDataFormat.D32_SFLOAT).MipCount(1).LayerCount(1),
                    new IImage.MemoryInfo.Builder().Usage(EMemoryUsage.GPUOnly).AddRequiredProperty(EMemoryProperty.DeviceLocal));

                depthAttachmentViews[i] = depthAttachments[i].CreateImageView(new IImageView.CreateInfo.Builder().ViewType(EImageViewType.ViewType2D).BaseArrayLayer(0)
                    .BaseMipLevel(0).ArrayLayerCount(1).MipLevelCount(1).Format(EDataFormat.D32_SFLOAT));

                _frameBuffer[i] = _device.CreateFrameBuffer(new IFrameBuffer.CreateInfo.Builder()
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder().ImageView(_colorAttachmentViews[i]).Usage(EImageUsage.ColorAttachment))
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder().ImageView(depthAttachmentViews[i]).Usage(EImageUsage.DepthAttachment)));

                var visibility = _device.CreateImage(new IImage.CreateInfo.Builder()
                    .AddUsage(EImageUsage.ColorAttachment)
                    .Width(1280)
                    .Height(720)
                    .Type(EImageType.Image2D)
                    .Format(EDataFormat.R32G32_UINT)
                    .SampleCount(ESampleCount.Samples1)
                    .MipCount(1)
                    .LayerCount(1),
                    new IImage.MemoryInfo.Builder()
                        .Usage(EMemoryUsage.GPUOnly)
                        .AddRequiredProperty(EMemoryProperty.DeviceLocal));
                _visibility[i].visibility = visibility.CreateImageView(new IImageView.CreateInfo.Builder()
                            .ViewType(EImageViewType.ViewType2D)
                            .BaseArrayLayer(0)
                            .BaseMipLevel(0)
                            .ArrayLayerCount(1)
                            .MipLevelCount(1)
                            .Format(EDataFormat.R32G32_UINT));


                var depthAttachment = _device.CreateImage(new IImage.CreateInfo.Builder()
                    .AddUsage(EImageUsage.DepthAttachment)
                    .Width(1280)
                    .Height(720)
                    .Type(EImageType.Image2D)
                    .Format(EDataFormat.D32_SFLOAT)
                    .MipCount(1)
                    .LayerCount(1),
                    new IImage.MemoryInfo.Builder()
                        .Usage(EMemoryUsage.GPUOnly)
                        .AddRequiredProperty(EMemoryProperty.DeviceLocal));
                _visibility[i].depth = depthAttachment.CreateImageView(new IImageView.CreateInfo.Builder()
                            .ViewType(EImageViewType.ViewType2D)
                            .BaseArrayLayer(0)
                            .BaseMipLevel(0)
                            .ArrayLayerCount(1)
                            .MipLevelCount(1)
                            .Format(EDataFormat.D32_SFLOAT));

                var colorAttachment = _device.CreateImage(new IImage.CreateInfo.Builder()
                    .AddUsage(EImageUsage.ColorAttachment)
                    .Width(1280)
                    .Height(720)
                    .Type(EImageType.Image2D)
                    .Format(EDataFormat.R8G8B8A8)
                    .MipCount(1)
                    .LayerCount(1),
                    new IImage.MemoryInfo.Builder()
                        .Usage(EMemoryUsage.GPUOnly)
                        .AddRequiredProperty(EMemoryProperty.DeviceLocal));
                _visibility[i].color = colorAttachment.CreateImageView(new IImageView.CreateInfo.Builder()
                    .ViewType(EImageViewType.ViewType2D)
                    .BaseArrayLayer(0)
                    .BaseMipLevel(0)
                    .ArrayLayerCount(1)
                    .MipLevelCount(1)
                    .Format(EDataFormat.R8G8B8A8));

                _visibility[i].visibilityFbo = _device.CreateFrameBuffer(new IFrameBuffer.CreateInfo.Builder()
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder()
                        .ImageView(_visibility[i].visibility)
                        .Usage(EImageUsage.ColorAttachment))
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder()
                        .ImageView(_visibility[i].depth)
                        .Usage(EImageUsage.DepthAttachment)));
                _visibility[i].resolveFbo = _device.CreateFrameBuffer(new IFrameBuffer.CreateInfo.Builder()
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder()
                        .ImageView(_visibility[i].color)
                        .Usage(EImageUsage.ColorAttachment))
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder()
                        .ImageView(_visibility[i].depth)
                        .Usage(EImageUsage.DepthAttachment)));
            }

            _imageResolveSampler = _device.CreateSampler(new ISampler.CreateInfo.Builder().AddressModeU(EAddressMode.Repeat)
                .AddressModeV(EAddressMode.Repeat).AddressModeW(EAddressMode.Repeat).MagFilter(EFilter.Linear).MinFilter(EFilter.Linear)
                .MipmapMode(EMipmapMode.Linear));
        }
    }
}
