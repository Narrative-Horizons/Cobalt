using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics.Passes
{
    internal class ZPrePass : RenderPass
    {
        private Shader _shader;
        private IRenderPass _pass;

        public ZPrePass(IDevice device) : base(device)
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
                        .BindingIndex(3)
                        .Count(1)
                        .DescriptorType(EDescriptorType.UniformBuffer)
                        .Name("SceneData")
                        .AddAccessibleStage(EShaderType.Vertex)
                        .AddAccessibleStage(EShaderType.Fragment).Build())
                    .Build()))
                .Build());

            string vsSource = FileSystem.LoadFileToString("data/shaders/zpass/zpass_vertex.glsl");
            string fsSource = FileSystem.LoadFileToString("data/shaders/zpass/zpass_fragment.glsl");

            _shader = new Shader(new Shader.CreateInfo.Builder()
                .VertexSource(vsSource)
                .FragmentSource(fsSource)
                .DepthInfo(new IGraphicsPipeline.DepthStencilCreateInfo.Builder()
                    .DepthCompareOp(ECompareOp.Less)
                    .DepthWriteEnabled(true)
                    .DepthTestEnabled(true)
                    .Build())
                .Build(), device, layout);

            _pass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder()
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.Undefined)
                    .FinalLayout(EImageLayout.ColorAttachment)
                    .LoadOp(EAttachmentLoad.Load)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.BGRA8_SRGB))
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.DepthAttachment)
                    .FinalLayout(EImageLayout.DepthAttachment)
                    .LoadOp(EAttachmentLoad.Clear)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.D32_SFLOAT)));
        }

        public override bool Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw)
        {
            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo
            {
                ClearValues = new List<ClearValue>() { new ClearValue(new ClearValue.ClearColor(0, 0, 0, 1)), new ClearValue(1) },
                Width = 1280, 
                Height = 720,
                FrameBuffer = info.frameBuffer,
                RenderPass = _pass
            });

            buffer.Bind(_shader.Pipeline);
            buffer.Bind(_shader.Layout, 0, draw.descriptorSets);
            Draw(buffer, draw);

            return false;
        }
    }
}
