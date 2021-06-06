using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Events;
using Cobalt.Graphics.API;
using Cobalt.Graphics.Passes;
using Cobalt.Math;
using System.Collections.Generic;

namespace Cobalt.Graphics
{
    public class RenderSystem
    {
        public Registry EntityRegistry { get; private set; }
        private ICommandPool cmdPool;
        private List<ICommandBuffer> cmdBuffers;
        private IDevice device;
        private readonly uint framesInFlight;
        private int currentFrame = 0;

        private readonly List<RenderPass> passes = new List<RenderPass>();

        private PbrRenderPass _pbrPass;
        private ScreenResolvePass _screenResolvePass;
        private ZPrePass _zPrePass;

        private IFrameBuffer[] FrameBuffer;
        private ISwapchain _swapChain;

        private IImageView[] colorAttachmentViews;

        private ISampler imageResolveSampler;
        private IQueue _submitQueue;

        public RenderSystem(Registry registry, IDevice device, ISwapchain swapchain)
        {
            this.framesInFlight = swapchain.GetImageCount();
            this.device = device;
            _swapChain = swapchain;

            EntityRegistry = registry;
            cmdPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().ResetAllocations(true));
            cmdBuffers = cmdPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(framesInFlight).Level(ECommandBufferLevel.Primary).Build());

            _zPrePass = new ZPrePass(device, (int)swapchain.GetImageCount(), registry);
            _pbrPass = new PbrRenderPass(device, (int) swapchain.GetImageCount(), registry);
            _screenResolvePass = new ScreenResolvePass(swapchain, device, 1280, 720);

            passes.Add(_zPrePass);
            passes.Add(_pbrPass);
            passes.Add(_screenResolvePass);

            FrameBuffer = new IFrameBuffer[framesInFlight];
            IImage[] colorAttachments = new IImage[framesInFlight];
            IImage[] depthAttachments = new IImage[framesInFlight];
            colorAttachmentViews = new IImageView[framesInFlight];
            IImageView[] depthAttachmentViews = new IImageView[framesInFlight];

            _submitQueue = device.Queues().Find(queue => queue.GetProperties().Graphics);

            for (int i = 0; i < framesInFlight; i++)
            {
                colorAttachments[i] = device.CreateImage(new IImage.CreateInfo.Builder().AddUsage(EImageUsage.ColorAttachment).Width(1280).Height(720).Type(EImageType.Image2D)
                    .Format(EDataFormat.R8G8B8A8).MipCount(1).LayerCount(1),
                    new IImage.MemoryInfo.Builder().Usage(EMemoryUsage.GPUOnly).AddRequiredProperty(EMemoryProperty.DeviceLocal));

                colorAttachmentViews[i] = colorAttachments[i].CreateImageView(new IImageView.CreateInfo.Builder().ViewType(EImageViewType.ViewType2D).BaseArrayLayer(0)
                    .BaseMipLevel(0).ArrayLayerCount(1).MipLevelCount(1).Format(EDataFormat.R8G8B8A8));

                depthAttachments[i] = device.CreateImage(new IImage.CreateInfo.Builder().AddUsage(EImageUsage.ColorAttachment).Width(1280).Height(720).Type(EImageType.Image2D)
                    .Format(EDataFormat.D24_SFLOAT_S8_UINT).MipCount(1).LayerCount(1),
                    new IImage.MemoryInfo.Builder().Usage(EMemoryUsage.GPUOnly).AddRequiredProperty(EMemoryProperty.DeviceLocal));

                depthAttachmentViews[i] = depthAttachments[i].CreateImageView(new IImageView.CreateInfo.Builder().ViewType(EImageViewType.ViewType2D).BaseArrayLayer(0)
                    .BaseMipLevel(0).ArrayLayerCount(1).MipLevelCount(1).Format(EDataFormat.D24_SFLOAT_S8_UINT));

                FrameBuffer[i] = device.CreateFrameBuffer(new IFrameBuffer.CreateInfo.Builder()
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder().ImageView(colorAttachmentViews[i]).Usage(EImageUsage.ColorAttachment))
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder().ImageView(depthAttachmentViews[i]).Usage(EImageUsage.DepthStencilAttachment)));
            }

            imageResolveSampler = device.CreateSampler(new ISampler.CreateInfo.Builder().AddressModeU(EAddressMode.Repeat)
                .AddressModeV(EAddressMode.Repeat).AddressModeW(EAddressMode.Repeat).MagFilter(EFilter.Linear).MinFilter(EFilter.Linear)
                .MipmapMode(EMipmapMode.Linear));
        }

        public void render()
        {
            ICommandBuffer cmdBuffer = cmdBuffers[currentFrame];

            ComponentView<DebugCameraComponent> cameraView = EntityRegistry.GetView<DebugCameraComponent>();
            cameraView.ForEach((camera) =>
            {
                cmdBuffer.Record(new ICommandBuffer.RecordInfo());

                _pbrPass.Camera = camera;
                _pbrPass.Record(cmdBuffer, new RenderPass.FrameInfo
                {
                    FrameBuffer = FrameBuffer[currentFrame],
                    FrameInFlight = currentFrame
                });

                var frameInfo = new RenderPass.FrameInfo { FrameInFlight = currentFrame };
                _screenResolvePass.SetInputTexture(new Cobalt.Graphics.Texture() { Image = colorAttachmentViews[currentFrame], Sampler = imageResolveSampler }, frameInfo);
                _screenResolvePass.Record(cmdBuffer, frameInfo);

                camera.Update();
            });

            cmdBuffer.End();
            _submitQueue.Execute(new IQueue.SubmitInfo(cmdBuffer));

            // Compute visibility pass

            // Z Pass
            // Opaque Pass
            // Translucent Pass
            // Resolve Pass
            // Post Processing

            currentFrame = (currentFrame + 1) % (int) framesInFlight;
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

        private struct SceneData
        {
            public Matrix4 View;
            public Matrix4 Projection;
            public Matrix4 ViewProjection;

            public Vector3 CameraPosition;
            public Vector3 CameraDirection;

            public Vector3 SunDirection;
            public Vector3 SunColor;
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

        private IFrameBuffer[] _frameBuffer;
        private IImageView[] _colorAttachmentViews;
        private ISampler _imageResolveSampler;
        private IQueue _submitQueue;

        private readonly ZPrePass _zPrePass;
        private readonly PbrRenderPass _pbrPass;
        private readonly ScreenResolvePass _screenResolvePass;
        private readonly List<Entity> _renderables = new List<Entity>();
        private readonly Dictionary<PbrMaterialComponent, uint> _materialIndices = new Dictionary<PbrMaterialComponent, uint>();
        private readonly MaterialPayload[] _materials = new MaterialPayload[1024];
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly Dictionary<Texture, uint> _textureIndices = new Dictionary<Texture, uint>();
        private readonly IDevice _device;
        private readonly List<FrameData> _frames = new List<FrameData>();
        private readonly Registry _registry;
        private readonly Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>> _framePayload = new Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>>();

        private static readonly uint MAT_NOT_FOUND = uint.MaxValue;
        private static readonly uint TEX_NOT_FOUND = uint.MaxValue;
        private static readonly uint MAX_TEX_COUNT = 500;

        public PbrPipeline(Registry registry, IDevice device, ISwapchain swapchain)
        {
            int framesInFlight = (int) swapchain.GetImageCount();

            _device = device;
            _registry = registry;
            _zPrePass = new ZPrePass(device, (int)swapchain.GetImageCount(), registry);
            _pbrPass = new PbrRenderPass(device, (int)swapchain.GetImageCount(), registry);
            _screenResolvePass = new ScreenResolvePass(swapchain, device, 1280, 720);
            
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
                        .Count((int)MAX_TEX_COUNT)
                        .DescriptorType(EDescriptorType.CombinedImageSampler)
                        .Name("Textures")
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(3)
                        .Count(1)
                        .DescriptorType(EDescriptorType.UniformBuffer)
                        .Name("SceneData")
                        .AddAccessibleStage(EShaderType.Vertex)
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .Build()))
                .Build());

            registry.Events.AddHandler<ComponentAddEvent<PbrMaterialComponent>>(_addComponent);
            registry.Events.AddHandler<ComponentAddEvent<MeshComponent>>(_addComponent);
            registry.Events.AddHandler<ComponentAddEvent<TransformComponent>>(_addComponent);
            registry.Events.AddHandler<EntityReleaseEvent>(_removeEntity);
            registry.Events.AddHandler<ComponentRemoveEvent<PbrMaterialComponent>>(_removeComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<MeshComponent>>(_removeComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<TransformComponent>>(_removeComponent);

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
                }
            }

            _frameBuffer = new IFrameBuffer[framesInFlight];
            IImage[] colorAttachments = new IImage[framesInFlight];
            IImage[] depthAttachments = new IImage[framesInFlight];
            _colorAttachmentViews = new IImageView[framesInFlight];
            IImageView[] depthAttachmentViews = new IImageView[framesInFlight];

            _submitQueue = device.Queues().Find(queue => queue.GetProperties().Graphics);

            for (int i = 0; i < framesInFlight; i++)
            {
                colorAttachments[i] = device.CreateImage(new IImage.CreateInfo.Builder().AddUsage(EImageUsage.ColorAttachment).Width(1280).Height(720).Type(EImageType.Image2D)
                    .Format(EDataFormat.R8G8B8A8).MipCount(1).LayerCount(1),
                    new IImage.MemoryInfo.Builder().Usage(EMemoryUsage.GPUOnly).AddRequiredProperty(EMemoryProperty.DeviceLocal));

                _colorAttachmentViews[i] = colorAttachments[i].CreateImageView(new IImageView.CreateInfo.Builder().ViewType(EImageViewType.ViewType2D).BaseArrayLayer(0)
                    .BaseMipLevel(0).ArrayLayerCount(1).MipLevelCount(1).Format(EDataFormat.R8G8B8A8));

                depthAttachments[i] = device.CreateImage(new IImage.CreateInfo.Builder().AddUsage(EImageUsage.ColorAttachment).Width(1280).Height(720).Type(EImageType.Image2D)
                    .Format(EDataFormat.D24_SFLOAT_S8_UINT).MipCount(1).LayerCount(1),
                    new IImage.MemoryInfo.Builder().Usage(EMemoryUsage.GPUOnly).AddRequiredProperty(EMemoryProperty.DeviceLocal));

                depthAttachmentViews[i] = depthAttachments[i].CreateImageView(new IImageView.CreateInfo.Builder().ViewType(EImageViewType.ViewType2D).BaseArrayLayer(0)
                    .BaseMipLevel(0).ArrayLayerCount(1).MipLevelCount(1).Format(EDataFormat.D24_SFLOAT_S8_UINT));

                _frameBuffer[i] = device.CreateFrameBuffer(new IFrameBuffer.CreateInfo.Builder()
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder().ImageView(_colorAttachmentViews[i]).Usage(EImageUsage.ColorAttachment))
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder().ImageView(depthAttachmentViews[i]).Usage(EImageUsage.DepthStencilAttachment)));
            }

            _imageResolveSampler = device.CreateSampler(new ISampler.CreateInfo.Builder().AddressModeU(EAddressMode.Repeat)
                .AddressModeV(EAddressMode.Repeat).AddressModeW(EAddressMode.Repeat).MagFilter(EFilter.Linear).MinFilter(EFilter.Linear)
                .MipmapMode(EMipmapMode.Linear));
        }

        public void render(ICommandBuffer buffer, int frameInFlight, DebugCameraComponent camera)
        {
            _build(frameInFlight, camera);

            RenderPass.DrawInfo draw = new RenderPass.DrawInfo()
            {
                indirectDrawBuffer = _frames[frameInFlight].indirectBuffer,
                payload = new Dictionary<IVertexAttributeArray, RenderPass.DrawCommand>()
            };

            RenderPass.FrameInfo sceneRenderInfo = new RenderPass.FrameInfo
            {
                FrameBuffer = _frameBuffer[frameInFlight],
                FrameInFlight = frameInFlight
            };

            // Handle scene draw
            _pbrPass.Camera = camera;
            _pbrPass.Record(buffer, sceneRenderInfo);

            // Handle screen resolution
            var frameInfo = new RenderPass.FrameInfo { FrameInFlight = frameInFlight };
            _screenResolvePass.SetInputTexture(new Texture() { Image = _colorAttachmentViews[frameInFlight], Sampler = _imageResolveSampler }, frameInfo);
            _screenResolvePass.Record(buffer, frameInfo);

            camera.Update();
        }

        private void _build(int frameInFlight, DebugCameraComponent camera)
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

            writeInfos.Add(texArrayBuilder.DescriptorSet(_frames[frameInFlight].descriptorSet).ArrayElement(0).BindingIndex(2)
                .Build());

            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(_frames[frameInFlight].materialData)
                    .Range(1000000 * 16).Build())
                    .BindingIndex(1)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(_frames[frameInFlight].entityData)
                    .Range(1000000 * 68).Build())
                    .BindingIndex(0)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(_frames[frameInFlight].sceneBuffer)
                    .Range(44 * 4).Build())
                    .BindingIndex(3)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            _device.UpdateDescriptorSets(writeInfos);

            // Upload data for next frame

            foreach (var (vao, meshes) in _framePayload)
                foreach (var (mesh, entities) in _framePayload)
                    entities.Clear();


            _renderables.ForEach(renderable =>
            {
                PbrMaterialComponent matComponent = _registry.Get<PbrMaterialComponent>(renderable);
                MeshComponent mesh = _registry.Get<MeshComponent>(renderable);
                EntityData e = new EntityData { MaterialId = _GetOrInsert(matComponent), Transformation = _registry.Get<TransformComponent>(renderable).TransformMatrix };
                RenderableMesh renderMesh = mesh.Mesh;
                if (!_framePayload.ContainsKey(renderMesh.VAO))
                {
                    _framePayload.Add(renderMesh.VAO, new Dictionary<RenderableMesh, List<EntityData>>());
                }
                if (!_framePayload[renderMesh.VAO].ContainsKey(renderMesh))
                {
                    _framePayload[renderMesh.VAO].Add(renderMesh, new List<EntityData>());
                }
                _framePayload[renderMesh.VAO][renderMesh].Add(e);
            });

            int writeFrame = (frameInFlight + 1) % _frames.Count;

            // Build material array
            NativeBuffer<MaterialPayload> nativeMaterialData = new NativeBuffer<MaterialPayload>(_frames[writeFrame].materialData.Map());
            foreach (MaterialPayload payload in _materials)
            {
                nativeMaterialData.Set(payload);
            }

            // Build uniform/shader storage buffers
            NativeBuffer<EntityData> nativeEntityData = new NativeBuffer<EntityData>(_frames[writeFrame].entityData.Map());
            foreach (var (vao, meshes) in _framePayload)
            {
                foreach (var (mesh, instances) in meshes)
                {
                    instances.ForEach(instance => nativeEntityData.Set(instance));
                }
            }

            NativeBuffer<SceneData> nativeSceneData = new NativeBuffer<SceneData>(_frames[writeFrame].sceneBuffer.Map());
            SceneData data = new SceneData
            {
                View = camera.View,
                Projection = camera.Projection,
                ViewProjection = camera.View * camera.Projection,

                CameraPosition = camera.position,
                CameraDirection = camera.front,

                SunDirection = new Vector3(-1, -1, -1),
                SunColor = new Vector3(1, 1, 1)
            };
            nativeSceneData.Set(data);
        }

        private bool _addComponent<T>(ComponentAddEvent<T> data)
        {
            _addEntity(data.Entity, data.Registry);
            return false;
        }

        private void _addEntity(Entity ent, Registry reg)
        {
            if (reg.Has<PbrMaterialComponent>(ent) && reg.Has<TransformComponent>(ent) && reg.Has<MeshComponent>(ent))
            {
                PbrMaterialComponent matComponent = reg.Get<PbrMaterialComponent>(ent);

                if (matComponent != default)
                {
                    _GetOrInsert(matComponent);
                    _renderables.Add(ent);
                }
            }
        }

        private bool _removeComponent<T>(ComponentRemoveEvent<T> data)
        {
            _renderables.Remove(data.Entity);
            return false;
        }

        private bool _removeEntity(EntityReleaseEvent data)
        {
            _renderables.Remove(data.SpawnedEntity);
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
    }
}
