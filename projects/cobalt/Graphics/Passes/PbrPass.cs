﻿using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Events;
using Cobalt.Graphics.API;
using Cobalt.Math;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalt.Graphics.Passes
{
    internal class PbrRenderPass : RenderPass
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

        private readonly Dictionary<PbrMaterialComponent, uint> MaterialIndices = new Dictionary<PbrMaterialComponent, uint>();
        private MaterialPayload[] materials = new MaterialPayload[1024];
        private List<Texture> textures = new List<Texture>();
        private readonly Dictionary<Texture, uint> textureIndices = new Dictionary<Texture, uint>();
        private static readonly uint MAT_NOT_FOUND = uint.MaxValue;
        private static readonly uint TEX_NOT_FOUND = uint.MaxValue;
        private static readonly uint MAX_TEX_COUNT = 500;
        private readonly Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>> framePayload = new Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>>();
        private readonly List<FrameData> frames = new List<FrameData>();
        private Shader _pbrShader;
        private IRenderPass _pass;
        private readonly List<Entity> _renderables = new List<Entity>(); // replace with sparse set at some point
        private readonly Registry _registry;

        public DebugCameraComponent Camera { get; set; }

        public PbrRenderPass(IDevice device, int framesInFlight, Registry registry) : base(device)
        {
            _registry = registry;
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

            _pbrShader = new Shader(new Shader.CreateInfo.Builder().VertexSource(FileSystem.LoadFileToString("data/shaders/pbr/pbr_vertex.glsl"))
                .FragmentSource(FileSystem.LoadFileToString("data/shaders/pbr/pbr_fragment.glsl")).Build(), device, layout, true);

            _pass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder().AddAttachment(new IRenderPass.AttachmentDescription.Builder().InitialLayout(EImageLayout.Undefined)
                .FinalLayout(EImageLayout.PresentSource).LoadOp(EAttachmentLoad.Clear).StoreOp(EAttachmentStore.Store).Format(EDataFormat.BGRA8_SRGB)));

            for (int frame = 0; frame < framesInFlight; ++frame)
            {
                if (frame >= frames.Count)
                {
                    frames.Capacity = frame + 1;
                    frames.Add(new FrameData());
                    frames[frame].descriptorPool = Device.CreateDescriptorPool(new IDescriptorPool.CreateInfo.Builder()
                        .AddPoolSize(EDescriptorType.CombinedImageSampler, (int)MAX_TEX_COUNT * 4) // allow for 4 times the max number of textures per pool
                        .MaxSetCount(32)
                        .Build());

                    frames[frame].descriptorSet = frames[frame].descriptorPool.Allocate(new IDescriptorSet.CreateInfo.Builder()
                        .AddLayout(_pbrShader.Pipeline.GetLayout().GetDescriptorSetLayouts()[0])
                        .Build())[0];

                    /// TODO: Make this actual sizeof
                    frames[frame].entityData = Device.CreateBuffer(new IBuffer.CreateInfo<EntityData>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(1000000 * 68),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));

                    frames[frame].materialData = Device.CreateBuffer(new IBuffer.CreateInfo<MaterialPayload>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(1000000 * 16),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));

                    frames[frame].indirectBuffer = Device.CreateBuffer(new IBuffer.CreateInfo<DrawElementsIndirectCommandPayload>.Builder().AddUsage(EBufferUsage.IndirectBuffer).Size(16 * 1024),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostVisible).AddRequiredProperty(EMemoryProperty.HostCoherent));

                    frames[frame].sceneBuffer = Device.CreateBuffer(IBuffer.FromPayload(new SceneData()).AddUsage(EBufferUsage.UniformBuffer),
                        new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));
                }
            }

            registry.Events.AddHandler<ComponentAddEvent<PbrMaterialComponent>>(_addComponent);
            registry.Events.AddHandler<ComponentAddEvent<MeshComponent>>(_addComponent);
            registry.Events.AddHandler<ComponentAddEvent<TransformComponent>>(_addComponent);
            registry.Events.AddHandler<EntityReleaseEvent>(_removeEntity);
            registry.Events.AddHandler<ComponentRemoveEvent<PbrMaterialComponent>>(_removeComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<MeshComponent>>(_removeComponent);
            registry.Events.AddHandler<ComponentRemoveEvent<TransformComponent>>(_removeComponent);
        }

        public override void Record(ICommandBuffer buffer, FrameInfo info)
        {
            _UploadData(info.FrameInFlight);

            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo
            {
                ClearValues = new List<ClearValue>() { new ClearValue(new ClearValue.ClearColor(0, 0, 0, 1)), new ClearValue(1) },
                Width = 1920,
                Height = 1080,
                FrameBuffer = info.FrameBuffer,
                RenderPass = _pass
            });

            buffer.Bind(_pbrShader.Pipeline);
            buffer.Bind(_pbrShader.Layout, 0, new List<IDescriptorSet> { frames[info.FrameInFlight].descriptorSet });

            int idx = 0;

            DrawInfo drawInfo = new DrawInfo();
            drawInfo.indirectDrawBuffer = frames[info.FrameInFlight].indirectBuffer;
            drawInfo.payload = new Dictionary<IVertexAttributeArray, DrawCommand>();

            DrawElementsIndirectCommand drawData = new DrawElementsIndirectCommand();
            NativeBuffer<DrawElementsIndirectCommandPayload> nativeIndirectData = new NativeBuffer<DrawElementsIndirectCommandPayload>(frames[info.FrameInFlight].indirectBuffer.Map());
            foreach (var obj in framePayload)
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

                // Submit draw to command buffer
                // buffer.Bind(obj.Key);
                // buffer.DrawElementsMultiIndirect(drawData, 0, frames[info.FrameInFlight].indirectBuffer);
            }
            frames[info.FrameInFlight].indirectBuffer.Unmap();
            Draw(buffer, drawInfo);
        }

        private void _UploadData(int frameInFlight)
        {
            List<DescriptorWriteInfo> writeInfos = new List<DescriptorWriteInfo>();
            DescriptorWriteInfo.Builder texArrayBuilder = new DescriptorWriteInfo.Builder();

            // Build texture array
            textures.ForEach(tex =>
            {
                texArrayBuilder.AddImageInfo(new DescriptorWriteInfo.DescriptorImageInfo.Builder()
                    .View(tex.Image)
                    .Sampler(tex.Sampler)
                    .Layout(EImageLayout.ShaderReadOnly)
                    .Build());
            });

            writeInfos.Add(texArrayBuilder.DescriptorSet(frames[frameInFlight].descriptorSet).ArrayElement(0).BindingIndex(2)
                .Build());

            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(frames[frameInFlight].materialData)
                    .Range(1000000 * 16).Build())
                    .BindingIndex(1)
                    .DescriptorSet(frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(frames[frameInFlight].entityData)
                    .Range(1000000 * 68).Build())
                    .BindingIndex(0)
                    .DescriptorSet(frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(frames[frameInFlight].sceneBuffer)
                    .Range(44 * 4).Build())
                    .BindingIndex(3)
                    .DescriptorSet(frames[frameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            Device.UpdateDescriptorSets(writeInfos);

            // Upload data for next frame

            foreach (var (vao, meshes) in framePayload)
                foreach (var (mesh, entities) in framePayload)
                    entities.Clear();
                

            _renderables.ForEach(renderable =>
            {
                PbrMaterialComponent matComponent = _registry.Get<PbrMaterialComponent>(renderable);
                MeshComponent mesh = _registry.Get<MeshComponent>(renderable);
                EntityData e = new EntityData { MaterialId = _GetOrInsert(matComponent), Transformation = _registry.Get<TransformComponent>(renderable).TransformMatrix };
                RenderableMesh renderMesh = mesh.Mesh;
                if (!framePayload.ContainsKey(renderMesh.VAO))
                {
                    framePayload.Add(renderMesh.VAO, new Dictionary<RenderableMesh, List<EntityData>>());
                }
                if (!framePayload[renderMesh.VAO].ContainsKey(renderMesh))
                {
                    framePayload[renderMesh.VAO].Add(renderMesh, new List<EntityData>());
                }
                framePayload[renderMesh.VAO][renderMesh].Add(e);
            });

            int writeFrame = (frameInFlight + 1) % frames.Count;

            // Build material array
            NativeBuffer<MaterialPayload> nativeMaterialData = new NativeBuffer<MaterialPayload>(frames[writeFrame].materialData.Map());
            foreach (MaterialPayload payload in materials)
            {
                nativeMaterialData.Set(payload);
            }

            // Build uniform/shader storage buffers
            NativeBuffer<EntityData> nativeEntityData = new NativeBuffer<EntityData>(frames[writeFrame].entityData.Map());
            foreach (var (vao, meshes) in framePayload)
            {
                foreach (var (mesh, instances) in meshes)
                {
                    instances.ForEach(instance => nativeEntityData.Set(instance));
                }
            }

            NativeBuffer<SceneData> nativeSceneData = new NativeBuffer<SceneData>(frames[writeFrame].sceneBuffer.Map());
            SceneData data = new SceneData
            {
                View = Camera.View,
                Projection = Camera.Projection,
                ViewProjection = Camera.View * Camera.Projection,

                CameraPosition = Camera.position,
                CameraDirection = Camera.front,

                SunDirection = new Vector3(-1, -1, -1),
                SunColor = new Vector3(1, 1, 1)
            };
            nativeSceneData.Set(data);
        }

        private uint _GetOrInsert(PbrMaterialComponent mat)
        {
            uint matId = MaterialIndices.GetValueOrDefault(mat, MAT_NOT_FOUND);
            if (matId == MAT_NOT_FOUND)
            {
                MaterialIndices.Add(mat, (uint)MaterialIndices.Count);
                matId = (uint)MaterialIndices.Count - 1;
                materials[matId] = new MaterialPayload
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

            if (textureIndices.ContainsKey(tex))
            {
                return textureIndices.GetValueOrDefault(tex);
            }
            textureIndices.Add(tex, (uint)textureIndices.Count);
            textures.Add(tex);
            return (uint)textureIndices.Count - 1;
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
    }
}
