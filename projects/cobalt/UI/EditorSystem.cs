using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Events;
using Cobalt.Graphics;
using Cobalt.Graphics.API;
using Cobalt.Graphics.GL;
using Cobalt.Graphics.Passes;
using Cobalt.Math;
using RenderPass = Cobalt.Graphics.RenderPass;

namespace Cobalt.UI
{
    internal class EditorSystem
    {
        #region Data Objects

        private struct UIEntityData
        {
            public Matrix4 Transformation; // [0, 64)
            public uint MaterialId; // [64, 68)

            public uint Identifier; // [68, 72)
            public uint Generation; // [72, 76)

            // padding
            private uint padding2; // [76, 80)
        }

        private struct UIMaterialPayload
        {
            public uint Texture;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct SceneData
        {
            [FieldOffset(0)] public Matrix4 View;

            [FieldOffset(64)] public Matrix4 Projection;
        }

        private class FrameData
        {
            public IDescriptorPool DescriptorPool;
            public IDescriptorSet DescriptorSet;
            public IBuffer EntityData;
            public IBuffer MaterialData;
            public IBuffer IndirectBuffer;
            public IBuffer SceneBuffer;
        }

        #endregion

        private readonly Registry _registry;
        private readonly IDevice _device;
        private readonly ISwapchain _swapchain;
        private readonly UIPass _uiPass;

        private readonly uint _framesInFlight;
        private int _currentFrame;

        private readonly ICommandPool _cmdPool;
        private readonly List<ICommandBuffer> _cmdBuffers;
        private readonly IQueue _submitQueue;

        private readonly List<IFence> _renderSync = new List<IFence>();

        private readonly Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<UIEntityData>>>
            _framePayload = new Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<UIEntityData>>>();

        private readonly List<FrameData> _frames = new List<FrameData>();
        private readonly HashSet<Entity> _renderables = new HashSet<Entity>();

        private readonly Dictionary<UIEditorMaterialComponent, uint> _materialIndices =
            new Dictionary<UIEditorMaterialComponent, uint>();

        private readonly UIMaterialPayload[] _materials = new UIMaterialPayload[1024];
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly Dictionary<Texture, uint> _textureIndices = new Dictionary<Texture, uint>();

        internal static readonly uint MAT_NOT_FOUND = uint.MaxValue;
        internal static readonly uint TEX_NOT_FOUND = uint.MaxValue;
        internal static readonly uint MAX_TEX_COUNT = 500;

        internal EditorSystem(Registry registry, IDevice device, ISwapchain swapchain)
        {
            _registry = registry;
            _device = device;
            _swapchain = swapchain;

            _framesInFlight = swapchain.GetImageCount();

            _submitQueue = device.Queues().Find(queue => queue.GetProperties().Graphics);
            _cmdPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().ResetAllocations(true));
            _cmdBuffers = _cmdPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(_framesInFlight)
                .Level(ECommandBufferLevel.Primary).Build());

            _uiPass = new UIPass(device);

            for (uint i = 0; i < swapchain.GetImageCount(); ++i)
            {
                _renderSync.Add(device.CreateFence(new IFence.CreateInfo.Builder()
                    .Signaled(true)
                    .Build()));
            }

            IPipelineLayout layout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder()
                .AddDescriptorSetLayout(device.CreateDescriptorSetLayout(
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
                            .Count((int) MAX_TEX_COUNT)
                            .DescriptorType(EDescriptorType.CombinedImageSampler)
                            .Name("Textures")
                            .AddAccessibleStage(EShaderType.Fragment).Build())
                        .Build()))
                .Build());

            registry.Events.AddHandler<ComponentAddEvent<UIEditorMaterialComponent>>(_AddComponent);
            registry.Events.AddHandler<ComponentAddEvent<MeshComponent>>(_AddComponent);
            registry.Events.AddHandler<ComponentAddEvent<TransformComponent>>(_AddComponent);
            registry.Events.AddHandler<EntityReleaseEvent>(_RemoveEntity);
            registry.Events.AddHandler<ComponentRemoveEvent<UIEditorMaterialComponent>>(_RemoveComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<MeshComponent>>(_RemoveComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<TransformComponent>>(_RemoveComponent);

            BuildFrameData((int)_framesInFlight, layout);
        }

        internal void Render()
        {
            ICommandBuffer cmdBuffer = _cmdBuffers[_currentFrame];
            IFence renderSync = _renderSync[_currentFrame];

            renderSync.Wait();

            cmdBuffer.Record(new ICommandBuffer.RecordInfo());

            // Render UI

            Build(_currentFrame);

            int idx = 0;

            RenderPass.DrawInfo drawInfo = new RenderPass.DrawInfo
            {
                indirectDrawBuffer = _frames[_currentFrame].IndirectBuffer,
                payload =
                    new Dictionary<EMaterialType, Dictionary<IVertexAttributeArray, RenderPass.DrawCommand>>(),
                descriptorSets = new List<IDescriptorSet>() {_frames[_currentFrame].DescriptorSet}
            };

            DrawElementsIndirectCommand drawData = new DrawElementsIndirectCommand();
            NativeBuffer<DrawElementsIndirectCommandPayload> nativeIndirectData = new NativeBuffer<DrawElementsIndirectCommandPayload>(_frames[_currentFrame].IndirectBuffer.Map());

            foreach (var (vao, payload) in _framePayload)
            {
                var items = new Dictionary<IVertexAttributeArray, RenderPass.DrawCommand>();
                drawInfo.payload.Add(EMaterialType.Opaque, items);
                foreach (var obj in payload)
                {
                    RenderPass.DrawCommand cmd = new RenderPass.DrawCommand();
                    cmd.bufferOffset = drawData.Data.Count * 20;
                    cmd.indirect = drawData;
                    items.Add(vao, cmd);

                    RenderableMesh mesh = obj.Key;
                    List<UIEntityData> instances = obj.Value;

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
            _frames[_currentFrame].IndirectBuffer.Unmap();

            RenderPass.FrameInfo sceneRenderInfo = new RenderPass.FrameInfo
            {
                frameBuffer = _swapchain.GetFrameBuffer(_currentFrame),
                frameInFlight = _currentFrame,
                width = 1280,
                height = 720
            };

            // Handle scene draw
            cmdBuffer.Sync();

            _uiPass.Record(cmdBuffer, sceneRenderInfo, drawInfo);

            cmdBuffer.End();

            _submitQueue.Execute(new IQueue.SubmitInfo.Builder()
                .Buffer(cmdBuffer)
                .Signal(renderSync)
                .Build());

            _currentFrame = (_currentFrame + 1) % (int) _framesInFlight;
        }

        private void BuildFrameData(int framesInFlight, IPipelineLayout layout)
        {
            for (int frame = 0; frame < framesInFlight; ++frame)
            {
                if (frame >= _frames.Count)
                {
                    _frames.Capacity = frame + 1;
                    _frames.Add(new FrameData());
                    _frames[frame].DescriptorPool = _device.CreateDescriptorPool(
                        new IDescriptorPool.CreateInfo.Builder()
                            .AddPoolSize(EDescriptorType.CombinedImageSampler,
                                (int) MAX_TEX_COUNT * 4) // allow for 4 times the max number of textures per pool
                            .MaxSetCount(32)
                            .Build());

                    _frames[frame].DescriptorSet = _frames[frame].DescriptorPool.Allocate(
                        new IDescriptorSet.CreateInfo.Builder()
                            .AddLayout(layout.GetDescriptorSetLayouts()[0])
                            .Build())[0];

                    /// TODO: Make this actual sizeof
                    _frames[frame].EntityData = _device.CreateBuffer(
                        new IBuffer.CreateInfo<UIEntityData>.Builder().AddUsage(EBufferUsage.StorageBuffer)
                            .Size(1000000 * 68),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU)
                            .AddRequiredProperty(EMemoryProperty.HostCoherent)
                            .AddRequiredProperty(EMemoryProperty.HostVisible));

                    _frames[frame].MaterialData = _device.CreateBuffer(
                        new IBuffer.CreateInfo<UIMaterialPayload>.Builder().AddUsage(EBufferUsage.StorageBuffer)
                            .Size(1000000 * 4),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU)
                            .AddRequiredProperty(EMemoryProperty.HostCoherent)
                            .AddRequiredProperty(EMemoryProperty.HostVisible));

                    _frames[frame].IndirectBuffer = _device.CreateBuffer(
                        new IBuffer.CreateInfo<DrawElementsIndirectCommandPayload>.Builder()
                            .AddUsage(EBufferUsage.IndirectBuffer).Size(16 * 1024),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU)
                            .AddRequiredProperty(EMemoryProperty.HostVisible)
                            .AddRequiredProperty(EMemoryProperty.HostCoherent));

                    _frames[frame].SceneBuffer = _device.CreateBuffer(
                        IBuffer.FromPayload(new SceneData()).AddUsage(EBufferUsage.UniformBuffer),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU)
                            .AddRequiredProperty(EMemoryProperty.HostCoherent)
                            .AddRequiredProperty(EMemoryProperty.HostVisible));
                }
            }
        }

        private void Build(int frameInFlight)
        {
            foreach (var (vao, meshes) in _framePayload)
                foreach (var (mesh, entities) in meshes)
                    entities.Clear();

            foreach (var renderable in _renderables)
            {
                var payload = _framePayload;
                UIEditorMaterialComponent matComponent = _registry.Get<UIEditorMaterialComponent>(renderable);
                MeshComponent mesh = _registry.Get<MeshComponent>(renderable);
                UIEntityData e = new UIEntityData
                {
                    MaterialId = _GetOrInsert(matComponent),
                    Transformation = _registry.Get<TransformComponent>(renderable).TransformMatrix,
                    Identifier = renderable.Identifier,
                    Generation = renderable.Generation
                };

                RenderableMesh renderMesh = mesh.Mesh;
                if (!payload.ContainsKey(renderMesh.vao))
                {
                    payload.Add(renderMesh.vao, new Dictionary<RenderableMesh, List<UIEntityData>>());
                }
                if (!payload[renderMesh.vao].ContainsKey(renderMesh))
                {
                    payload[renderMesh.vao].Add(renderMesh, new List<UIEntityData>());
                }
                payload[renderMesh.vao][renderMesh].Add(e);
            }

            int writeFrame = (frameInFlight + 1) % _frames.Count;

            // Build material array
            NativeBuffer<UIMaterialPayload> nativeMaterialData = new NativeBuffer<UIMaterialPayload>(_frames[writeFrame].MaterialData.Map());
            foreach (UIMaterialPayload payload in _materials)
            {
                nativeMaterialData.Set(payload);
            }
            _frames[writeFrame].MaterialData.Unmap();

            // Build uniform/shader storage buffers
            NativeBuffer<UIEntityData> nativeEntityData = new NativeBuffer<UIEntityData>(_frames[writeFrame].EntityData.Map());
            foreach (var (type, framePayload) in _framePayload)
            {
                foreach (var (_, instances) in framePayload)
                {
                    instances.ForEach(instance => nativeEntityData.Set(instance));
                }
            }
            _frames[writeFrame].EntityData.Unmap();

            // Build scene data
            // TODO : Create a editor view camera
            NativeBuffer<SceneData> nativeSceneData = new NativeBuffer<SceneData>(_frames[writeFrame].SceneBuffer.Map());
            SceneData data = new SceneData
            {
                View = Matrix4.Identity,
                Projection = Matrix4.Identity,
            };
            nativeSceneData.Set(data);
            _frames[writeFrame].SceneBuffer.Unmap();

            // Build texture array
            DescriptorWriteInfo.Builder texArrayBuilder = new DescriptorWriteInfo.Builder();
            _textures.ForEach(tex =>
            {
                texArrayBuilder.AddImageInfo(new DescriptorWriteInfo.DescriptorImageInfo.Builder()
                    .View(tex.Image)
                    .Sampler(tex.Sampler)
                    .Layout(EImageLayout.ShaderReadOnly)
                    .Build());
            });

            List<DescriptorWriteInfo> writeInfos = new List<DescriptorWriteInfo>
            {
                new DescriptorWriteInfo.Builder().AddBufferInfo(
                        new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                            .Buffer(_frames[frameInFlight].MaterialData)
                            .Range(1000000 * 4)
                            .Build())
                    .BindingIndex(1)
                    .DescriptorSet(_frames[frameInFlight].DescriptorSet)
                    .ArrayElement(0)
                    .Build(),
                new DescriptorWriteInfo.Builder().AddBufferInfo(
                        new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                            .Buffer(_frames[frameInFlight].EntityData)
                            .Range(1000000 * 68)
                            .Build())
                    .BindingIndex(0)
                    .DescriptorSet(_frames[frameInFlight].DescriptorSet)
                    .ArrayElement(0)
                    .Build(),
                new DescriptorWriteInfo.Builder().AddBufferInfo(
                        new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                            .Buffer(_frames[frameInFlight].SceneBuffer)
                            .Range(128)
                            .Build())
                    .BindingIndex(2)
                    .DescriptorSet(_frames[frameInFlight].DescriptorSet)
                    .ArrayElement(0)
                    .Build()
            };

            _device.UpdateDescriptorSets(writeInfos);
        }

        private bool _AddComponent<T>(ComponentAddEvent<T> data)
        {
            _AddEntity(data.Entity, data.Registry);
            return false;
        }

        private void _AddEntity(Entity ent, Registry reg)
        {
            if (reg.Has<UIEditorMaterialComponent>(ent) && reg.Has<TransformComponent>(ent) && reg.Has<MeshComponent>(ent))
            {
                UIEditorMaterialComponent matComponent = reg.Get<UIEditorMaterialComponent>(ent);

                if (matComponent != default)
                {
                    _GetOrInsert(matComponent);
                    _renderables.Add(ent);
                }
            }
        }

        private bool _RemoveComponent<T>(ComponentRemoveEvent<T> data)
        {
            UIEditorMaterialComponent matComponent = data.Registry.Get<UIEditorMaterialComponent>(data.Entity);
            if (matComponent != default)
            {
                _renderables.Remove(data.Entity);
            }
            return false;
        }

        private bool _RemoveEntity(EntityReleaseEvent data)
        {
            UIEditorMaterialComponent matComponent = data.SpawningRegistry.Get<UIEditorMaterialComponent>(data.SpawnedEntity);
            if (matComponent != default)
            {
                _renderables.Remove(data.SpawnedEntity);
            }
            return false;
        }

        private uint _GetOrInsert(UIEditorMaterialComponent mat)
        {
            uint matId = _materialIndices.GetValueOrDefault(mat, MAT_NOT_FOUND);
            if (matId == MAT_NOT_FOUND)
            {
                _materialIndices.Add(mat, (uint)_materialIndices.Count);
                matId = (uint)_materialIndices.Count - 1;
                _materials[matId] = new UIMaterialPayload
                {
                    Texture = _GetOrInsert(mat.Texture),
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
