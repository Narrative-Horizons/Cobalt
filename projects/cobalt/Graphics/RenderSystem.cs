using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
using Cobalt.Graphics.GL.Commands;
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

            _pbrPass = new PbrRenderPass(device);
            /// TODO: Resize this
            _screenResolvePass = new ScreenResolvePass(swapchain, device, 1280, 720);

            passes.Add(_pbrPass);
            passes.Add(_screenResolvePass);

            FrameBuffer = new IFrameBuffer[framesInFlight];
            IImage[] colorAttachments = new IImage[framesInFlight];
            IImage[] depthAttachments = new IImage[framesInFlight];
            colorAttachmentViews = new IImageView[framesInFlight];
            IImageView[] depthAttachmentViews = new IImageView[framesInFlight];

            _submitQueue = device.Queues()[0];

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
            _prerender();
            ICommandBuffer cmdBuffer = cmdBuffers[currentFrame];

            _pbrPass.Record(cmdBuffer, new RenderPass.FrameInfo
            {
                FrameBuffer = FrameBuffer[currentFrame],
                FrameInFlight = currentFrame
            });

            _screenResolvePass.SetInputTexture(new Texture() { Image = colorAttachmentViews[currentFrame], Sampler = imageResolveSampler }, new RenderPass.FrameInfo
            {
                FrameInFlight = currentFrame
            });

            _screenResolvePass.Record(cmdBuffer, new RenderPass.FrameInfo
            {
                FrameBuffer = _swapChain.GetFrameBuffer(currentFrame),
                FrameInFlight = currentFrame
            });

            _submitQueue.Execute(new IQueue.SubmitInfo(cmdBuffer));

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
        private readonly Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>> framePayload = new Dictionary<IVertexAttributeArray, Dictionary<RenderableMesh, List<EntityData>>>();
        private readonly List<FrameData> frames = new List<FrameData>();
        private Shader _pbrShader;
        private IRenderPass _pass;

        public PbrRenderPass(IDevice device) : base(device)
        {
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
                    .Build()))
                .Build());

            _pbrShader = new Shader(new Shader.CreateInfo.Builder().VertexSource(FileSystem.LoadFileToString("data/pbr_vertex.glsl"))
                .FragmentSource(FileSystem.LoadFileToString("data/pbr_fragment.glsl")).Build(), device, layout, true);

            _pass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder().AddAttachment(new IRenderPass.AttachmentDescription.Builder().InitialLayout(EImageLayout.Undefined)
                .FinalLayout(EImageLayout.PresentSource).LoadOp(EAttachmentLoad.Clear).StoreOp(EAttachmentStore.Store).Format(EDataFormat.BGRA8_SRGB)));
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
                if (!framePayload.ContainsKey(renderMesh.VAO))
                {
                    framePayload.Add(renderMesh.VAO, new Dictionary<RenderableMesh, List<EntityData>>());
                }
                if (!framePayload[renderMesh.VAO].ContainsKey(renderMesh))
                {
                    framePayload[renderMesh.VAO].Add(renderMesh, new List<EntityData>());
                }
                framePayload[renderMesh.VAO][renderMesh].Add(e);
            }
        }

        public override void Record(ICommandBuffer buffer, FrameInfo info)
        {
            if (info.FrameInFlight >= frames.Count)
            {
                frames.Capacity = info.FrameInFlight + 1;
                frames.Add(new FrameData());
                frames[info.FrameInFlight].descriptorPool = Device.CreateDescriptorPool(new IDescriptorPool.CreateInfo.Builder()
                    .AddPoolSize(EDescriptorType.CombinedImageSampler, (int) MAX_TEX_COUNT * 4) // allow for 4 times the max number of textures per pool
                    .MaxSetCount(32)
                    .Build());
                frames[info.FrameInFlight].descriptorSet = frames[info.FrameInFlight].descriptorPool.Allocate(new IDescriptorSet.CreateInfo.Builder()
                    .AddLayout(_pbrShader.Pipeline.GetLayout().GetDescriptorSetLayouts())
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
            
            writeInfos.Add(texArrayBuilder.DescriptorSet(frames[info.FrameInFlight].descriptorSet).ArrayElement(0).BindingIndex(2)
                .Build());

            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(frames[info.FrameInFlight].materialData)
                    .Range(1000000 * 16).Build())
                    .BindingIndex(1)
                    .DescriptorSet(frames[info.FrameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

            writeInfos.Add(new DescriptorWriteInfo.Builder().AddBufferInfo(
                new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                    .Buffer(frames[info.FrameInFlight].entityData)
                    .Range(1000000 * 68).Build())
                    .BindingIndex(9)
                    .DescriptorSet(frames[info.FrameInFlight].descriptorSet)
                    .ArrayElement(0).Build());

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
                foreach(var child in obj.Value)
                {
                    List<EntityData> instances = child.Value;

                    foreach (EntityData instance in instances)
                    {
                        nativeEntityData.Set(instance);
                    }
                }
            }
            frames[info.FrameInFlight].entityData.Unmap();

            // Build the multidraw indirect buffers

            Device.UpdateDescriptorSets(writeInfos);

            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo
            {
                ClearValues = new List<ClearValue>() { new ClearValue(new ClearValue.ClearColor(0, 0, 0, 1)), new ClearValue(1) },
                Width = 1280,
                Height = 720,
                FrameBuffer = info.FrameBuffer,
                RenderPass = _pass
            });

            buffer.Bind(_pbrShader.Pipeline);
            buffer.Bind(_pbrShader.Layout, 0, new List<IDescriptorSet> { frames[info.FrameInFlight].descriptorSet });

            int idx = 0;

            foreach (var obj in framePayload)
            {
                buffer.Bind(obj.Key);
                DrawElementsIndirectCommand drawData = new DrawElementsIndirectCommand();

                foreach (var child in obj.Value)
                {
                    RenderableMesh mesh = child.Key;
                    List<EntityData> instances = child.Value;

                    drawData.Add(new DrawElementsIndirectCommandPayload
                    {
                        BaseVertex = mesh.baseVertex,
                        FirstIndex = mesh.baseIndex,
                        BaseInstance = (uint)idx,
                        Count = mesh.indexCount,
                        InstanceCount = (uint)instances.Count
                    });

                    idx += instances.Count;
                }

                // Submit draw to command buffer
                buffer.DrawElementsMultiIndirect(drawData);
            }
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
