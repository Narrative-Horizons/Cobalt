using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics.Passes
{
    internal class ScreenResolvePass : RenderPass
    {
        private ISwapchain swapchain;
        public int Width { get; set; }
        public int Height { get; set; }
        public Shader Shader { get; private set; }
        public IVertexAttributeArray VAO { get; private set; }
        public IDescriptorPool DescriptorPool { get; private set; }
        public List<IDescriptorSet> DescriptorSets { get; private set; }

        public ScreenResolvePass(ISwapchain resolveTo, IDevice device, int width, int height) : base(device)
        {
            swapchain = resolveTo;
            Width = width;
            Height = height;

            IPipelineLayout screenLayout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder().AddDescriptorSetLayout(device.CreateDescriptorSetLayout(
                new IDescriptorSetLayout.CreateInfo.Builder()
                .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                .AddAccessibleStage(EShaderType.Fragment).BindingIndex(0).DescriptorType(EDescriptorType.CombinedImageSampler).Count(1).Name("source_image").Build()).Build())).Build());

            string vsScreenSource = FileSystem.LoadFileToString("data/shaders/screenresolve/screen_vertex.glsl");
            string fsScreenSource = FileSystem.LoadFileToString("data/shaders/screenresolve/screen_fragment.glsl");

            Shader = new Shader(new Shader.CreateInfo.Builder().VertexSource(vsScreenSource).FragmentSource(fsScreenSource).Build(), device, screenLayout, false);

            float[] screenVerts =
            {
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,

                1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            };

            IBuffer screenQuadBuf = device.CreateBuffer(
                IBuffer.FromPayload(screenVerts).AddUsage(EBufferUsage.ArrayBuffer),
                new IBuffer.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                    .Usage(EMemoryUsage.GPUOnly));

            VAO = Shader.Pipeline.CreateVertexAttributeArray(new List<IBuffer>() { screenQuadBuf });

            DescriptorPool = device.CreateDescriptorPool(new IDescriptorPool.CreateInfo.Builder()
                .AddPoolSize(EDescriptorType.CombinedImageSampler, (int)swapchain.GetImageCount())
                .MaxSetCount((int)swapchain.GetImageCount())
                .Build());

            DescriptorSets = DescriptorPool.Allocate(new IDescriptorSet.CreateInfo.Builder().AddLayout(screenLayout.GetDescriptorSetLayouts()[0])
                .AddLayout(screenLayout.GetDescriptorSetLayouts()[0]).Build());

            Native = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder().AddAttachment(new IRenderPass.AttachmentDescription.Builder().InitialLayout(EImageLayout.Undefined)
                .FinalLayout(EImageLayout.PresentSource).LoadOp(EAttachmentLoad.Clear).StoreOp(EAttachmentStore.Store).Format(EDataFormat.BGRA8_SRGB)));
        }

        public void SetInputTexture(Texture input, FrameInfo info)
        {
            IDescriptorSet dest = DescriptorSets[info.FrameInFlight];
            DescriptorWriteInfo writeInfo = new DescriptorWriteInfo.Builder()
                   .AddImageInfo(new DescriptorWriteInfo.DescriptorImageInfo.Builder().Layout(EImageLayout.ShaderReadOnly)
                   .Sampler(input.Sampler).View(input.Image)).ArrayElement(0).BindingIndex(0).DescriptorSet(dest).Build();

            Device.UpdateDescriptorSets(new List<DescriptorWriteInfo>() { writeInfo });
        }

        public override void Record(ICommandBuffer buffer, FrameInfo info)
        {
            IFrameBuffer renderTo = swapchain.GetFrameBuffer(info.FrameInFlight);

            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo()
            {
                ClearValues = { new ClearValue(new ClearValue.ClearColor(0.0f, 0.0f, 0.0f, 1.0f)) },
                FrameBuffer = renderTo,
                Height = Height,
                Width = Width,
                RenderPass = Native
            });

            buffer.Bind(Shader.Pipeline);
            buffer.Bind(VAO);
            buffer.Bind(Shader.Layout, 0, new List<IDescriptorSet>() { DescriptorSets[info.FrameInFlight] });
            buffer.Draw(0, 6, 0, 1);
        }
    }
}
