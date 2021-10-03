using System;
using System.Collections.Generic;
using System.Text;
using Cobalt.Core;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
using Cobalt.UI;

namespace Cobalt.Graphics.Passes
{
    public class UIPass : RenderPass
    {
        private readonly Shader _shader;
        private readonly IRenderPass _pass;

        public UIPass(IDevice device) : base(device)
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
                            .Count(1)
                            .DescriptorType(EDescriptorType.UniformBuffer)
                            .Name("SceneData")
                            .AddAccessibleStage(EShaderType.Vertex)
                            .AddAccessibleStage(EShaderType.Fragment).Build())
                        .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                            .BindingIndex(3)
                            .Count((int)EditorSystem.MAX_TEX_COUNT)
                            .DescriptorType(EDescriptorType.CombinedImageSampler)
                            .Name("Textures")
                            .AddAccessibleStage(EShaderType.Fragment).Build())
                        .Build()))
                .Build());

            string vsSource = FileSystem.LoadFileToString("data/shaders/UI/uipass_vertex.glsl");
            string fsSource = FileSystem.LoadFileToString("data/shaders/UI/uipass_fragment.glsl");

            _shader = new Shader(new Shader.CreateInfo.Builder()
                .VertexSource(vsSource)
                .FragmentSource(fsSource)
                .DepthInfo(new IGraphicsPipeline.DepthStencilCreateInfo.Builder()
                    .DepthCompareOp(ECompareOp.Less)
                    .DepthWriteEnabled(false)
                    .DepthTestEnabled(false)
                    .Build())
                .Build(), device, layout);

            _pass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder()
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.Undefined)
                    .FinalLayout(EImageLayout.ColorAttachment)
                    .LoadOp(EAttachmentLoad.Load)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.BGRA8_SRGB)));
        }

        public override bool Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw)
        {
            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo
            {
                ClearValues = new List<ClearValue>() { new ClearValue(new ClearValue.ClearColor(0, 0, 0, 1)), null },
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
