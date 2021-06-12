using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Events;
using Cobalt.Graphics.API;
using Cobalt.Graphics.Passes;
using Cobalt.Math;
using System.Collections.Generic;
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

        public void render()
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

        private readonly ZPrePass _zPrePass;
        private readonly PbrRenderPass _pbrPass;
        private readonly ScreenResolvePass _screenResolvePass;
        private readonly HashSet<Entity> _renderables = new HashSet<Entity>();
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

        private ComputeShader cShader;

        public PbrPipeline(Registry registry, IDevice device, ISwapchain swapchain)
        {
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

            _device = device;
            _registry = registry;
            _zPrePass = new ZPrePass(device);
            _pbrPass = new PbrRenderPass(device, layout);
            _screenResolvePass = new ScreenResolvePass(swapchain, device, 1280, 720);

            cShader = device.CreateComputeShader(FileSystem.LoadFileToString("data/shaders/computetest.glsl"));

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

            cShader.Update();

            buffer.Bind(cShader._pipeline);
            buffer.Bind(cShader._layout, 0, new List<IDescriptorSet>() { cShader.set });
            buffer.Dispatch(1, 1, 1);

            buffer.Sync();

            int idx = 0;

            DrawInfo drawInfo = new DrawInfo();
            drawInfo.indirectDrawBuffer = _frames[frameInFlight].indirectBuffer;
            drawInfo.payload = new Dictionary<IVertexAttributeArray, DrawCommand>();
            drawInfo.descriptorSets = new List<IDescriptorSet>() { _frames[frameInFlight].descriptorSet };

            DrawElementsIndirectCommand drawData = new DrawElementsIndirectCommand();
            NativeBuffer<DrawElementsIndirectCommandPayload> nativeIndirectData = new NativeBuffer<DrawElementsIndirectCommandPayload>(_frames[frameInFlight].indirectBuffer.Map());
            foreach (var obj in _framePayload)
            {
                DrawCommand cmd = new DrawCommand();
                cmd.bufferOffset = drawData.Data.Count * 20;
                cmd.indirect = drawData;
                drawInfo.payload.Add(obj.Key, cmd);

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
            _frames[frameInFlight].indirectBuffer.Unmap();

            FrameInfo sceneRenderInfo = new FrameInfo
            {
                frameBuffer = _frameBuffer[frameInFlight],
                frameInFlight = frameInFlight
            };

            // Handle scene draw
            buffer.Sync();
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
                    .Range(240).Build())
                    .BindingIndex(3)
                    .DescriptorSet(_frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            _device.UpdateDescriptorSets(writeInfos);

            // Upload data for next frame

            foreach (var (vao, meshes) in _framePayload)
                foreach (var (mesh, entities) in _framePayload)
                    entities.Clear();

            foreach(var renderable in _renderables)
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
            foreach (var (vao, meshes) in _framePayload)
            {
                foreach (var (mesh, instances) in meshes)
                {
                    instances.ForEach(instance => nativeEntityData.Set(instance));
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
                 
                SunDirection = new Vector3(0, -1, 0), 
                SunColor = new Vector3(1, 1, 1)
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
                    _renderables.Add(ent);
                }
            }
        }

        private bool _RemoveComponent<T>(ComponentRemoveEvent<T> data)
        {
            _renderables.Remove(data.Entity);
            return false;
        }

        private bool _RemoveEntity(EntityReleaseEvent data)
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
                }
            }

            _frameBuffer = new IFrameBuffer[framesInFlight];
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
            }

            _imageResolveSampler = _device.CreateSampler(new ISampler.CreateInfo.Builder().AddressModeU(EAddressMode.Repeat)
                .AddressModeV(EAddressMode.Repeat).AddressModeW(EAddressMode.Repeat).MagFilter(EFilter.Linear).MinFilter(EFilter.Linear)
                .MipmapMode(EMipmapMode.Linear));
        }
    }
}
