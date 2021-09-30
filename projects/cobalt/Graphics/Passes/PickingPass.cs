using System.Collections.Generic;
using Cobalt.Core;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;

namespace Cobalt.Graphics.Passes
{
    internal class PickingPass : RenderPass
    {
        private readonly Shader _shader;
        private readonly IRenderPass _pass;

        public PickingPass(IDevice device) : base(device)
        {
            var layout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder()
                .AddDescriptorSetLayout(device.CreateDescriptorSetLayout(new IDescriptorSetLayout.CreateInfo.Builder()
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .AddAccessibleStage(EShaderType.Vertex)
                        .BindingIndex(0)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("ObjectData")
                        .Build())
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .AddAccessibleStage(EShaderType.Vertex)
                        .BindingIndex(2)
                        .Count(1)
                        .DescriptorType(EDescriptorType.UniformBuffer)
                        .Name("SceneData")
                        .Build())
                    .Build()))
                .Build());

            _shader = new Shader(new Shader.CreateInfo.Builder()
                .VertexSource(FileSystem.LoadFileToString("data/shaders/picking/vertex.glsl"))
                .FragmentSource(FileSystem.LoadFileToString("data/shaders/picking/fragment.glsl"))
                .DepthInfo(new IGraphicsPipeline.DepthStencilCreateInfo.Builder()
                    .DepthCompareOp(ECompareOp.LessOrEqual)
                    .DepthWriteEnabled(false)
                    .DepthTestEnabled(true)
                    .Build())
                .Build(), device, layout);

            _pass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder()
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.Undefined)
                    .FinalLayout(EImageLayout.ColorAttachment)
                    .LoadOp(EAttachmentLoad.Clear)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.R32G32_UINT)));
        }

        public override bool Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw)
        {
            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo
            {
                ClearValues = new List<ClearValue>() { new ClearValue(new ClearValue.ClearColor(0, 0, 0, 1)), new ClearValue(1) },
                Width = info.width,
                Height = info.height,
                FrameBuffer = info.frameBuffer,
                RenderPass = _pass
            });

            buffer.Bind(_shader.Pipeline);
            buffer.Bind(_shader.Layout, 0, draw.descriptorSets);
            Draw(buffer, draw, EMaterialType.Opaque);

            return true;
        }
    }
}
