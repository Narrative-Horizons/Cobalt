using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
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

        public RenderSystem(Registry registry, IDevice device, uint framesInFlight)
        {
            EntityRegistry = registry;
            cmdPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().ResetAllocations(true));
            cmdBuffers = cmdPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(framesInFlight).Level(ECommandBufferLevel.Primary).Build());
            this.device = device;
            this.framesInFlight = framesInFlight;
        }

        public void render()
        {
            _prerender();
            ICommandBuffer cmdBuffer = cmdBuffers[currentFrame];

            // Compute visibility pass

            // Z Pass
            // Opaque Pass
            // Translucent Pass
            // Resolve Pass
            // Post Processing

            currentFrame = (currentFrame + 1) % (int) framesInFlight;
        }

        private void _prerender()
        {
            // build the pass render lists
            EntityView view = EntityRegistry.GetView();
            view.ForEach((e, reg) =>
            {
                bool hasTransform = reg.Has<TransformComponent>(e);
                bool hasMesh = reg.Has<MeshComponent>(e);
                
                if (hasTransform && hasMesh)
                {
                    passes.ForEach(pass => pass.Preprocess(e, reg));
                }
            });
        }
    }

    internal class ZPass : RenderPass
    {
        public ZPass(IDevice device) : base(device)
        {

        }

        public override void Preprocess(Entity ent, Registry reg)
        {
            // Process Z data
        }

        public override void Record(ICommandBuffer buffer, FrameInfo info)
        {
        }
    }

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
            public Matrix4 Transformation;
            public uint MaterialId;
        }

        private class FrameData
        {
            public IDescriptorPool descriptorPool;
            public IDescriptorSet descriptorSet;
            public IBuffer entityData;
            public IBuffer materialData;
        }

        private readonly Dictionary<PbrMaterialComponent, uint> MaterialIndices = new Dictionary<PbrMaterialComponent, uint>();
        private MaterialPayload[] materials = new MaterialPayload[1024];
        private List<Texture> textures = new List<Texture>();
        private readonly Dictionary<Texture, uint> textureIndices = new Dictionary<Texture, uint>();
        private static readonly uint MAT_NOT_FOUND = uint.MaxValue;
        private static readonly uint TEX_NOT_FOUND = uint.MaxValue;
        private static readonly uint MAX_MAT_COUNT = 4096;
        private static readonly uint MAX_TEX_COUNT = 1024;
        private readonly Dictionary<RenderableMesh, List<EntityData>> framePayload = new Dictionary<RenderableMesh, List<EntityData>>();
        private readonly List<FrameData> frames = new List<FrameData>();

        public PbrRenderPass(IDevice device) : base(device)
        {
            
        }

        public override void Preprocess(Entity ent, Registry reg)
        {
            PbrMaterialComponent matComponent = reg.TryGet<PbrMaterialComponent>(ent);
            MeshComponent mesh = reg.Get<MeshComponent>(ent);

            if (matComponent != default)
            {
                // Process PBR data
                uint matId = _GetOrInsert(matComponent);
                EntityData e = new EntityData { MaterialId = matId, Transformation = reg.Get<TransformComponent>(ent).transformMatrix };
                RenderableMesh renderMesh = mesh.Mesh;
                if (!framePayload.ContainsKey(renderMesh))
                {
                    framePayload.Add(renderMesh, new List<EntityData>());
                }
                framePayload[renderMesh].Add(e);
            }
        }

        public override void Record(ICommandBuffer buffer, FrameInfo info)
        {
            if (info.FrameInFlight >= frames.Count)
            {
                frames.Capacity = info.FrameInFlight + 1;
                frames[info.FrameInFlight] = new FrameData();
                frames[info.FrameInFlight].descriptorPool = Device.CreateDescriptorPool(new IDescriptorPool.CreateInfo.Builder()
                    .AddPoolSize(EDescriptorType.CombinedImageSampler, (int) MAX_TEX_COUNT * 4) // allow for 4 times the max number of textures per pool
                    .MaxSetCount(32)
                    .Build());
                frames[info.FrameInFlight].descriptorSet = frames[info.FrameInFlight].descriptorPool.Allocate(new IDescriptorSet.CreateInfo.Builder()
                    .Build())[0];


                /// TODO: Make this actual sizeof
                frames[info.FrameInFlight].entityData = Device.CreateBuffer(new IBuffer.CreateInfo<EntityData>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(1000000 * 68),
                    new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));

                frames[info.FrameInFlight].materialData = Device.CreateBuffer(new IBuffer.CreateInfo<MaterialPayload>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(1000000 * 16),
                    new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));
            }

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
            
            writeInfos.Add(texArrayBuilder.DescriptorSet(frames[info.FrameInFlight].descriptorSet)
                .Build());

            // Build material array
            NativeBuffer<MaterialPayload> nativeMaterialData = new NativeBuffer<MaterialPayload>(frames[info.FrameInFlight].materialData.Map());
            foreach (MaterialPayload payload in materials)
            {
                nativeMaterialData.Set(payload);
            }
            frames[info.FrameInFlight].materialData.Unmap();

            // Build uniform/shader storage buffers
            NativeBuffer<EntityData> nativeEntityData = new NativeBuffer<EntityData>(frames[info.FrameInFlight].entityData.Map());
            foreach (var obj in framePayload)
            {
                List<EntityData> instances = obj.Value;

                foreach (EntityData instance in instances)
                {
                    nativeEntityData.Set(instance);
                }
            }
            frames[info.FrameInFlight].entityData.Unmap();
            
            new DrawElementsIndirectCommand()

            // Build the multidraw indirect buffers


            // Submit draw to command buffer
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
            return (uint) textureIndices.Count - 1;
        }
    }
}
