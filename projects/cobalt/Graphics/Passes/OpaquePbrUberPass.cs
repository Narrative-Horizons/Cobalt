using Cobalt.Core;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics.Passes
{
    public class OpaquePbrUberPass : RenderPass
    {
        private Shader _shader;
        private IRenderPass _pass;

        public OpaquePbrUberPass(IDevice device, IPipelineLayout layout) : base(device)
        {
            // Depth testing is not needed, performed in screen space
            _shader = new Shader(new Shader.CreateInfo.Builder()
                .VertexSource(FileSystem.LoadFileToString("data/shaders/pbr_vis/vertex.glsl"))
                .FragmentSource(FileSystem.LoadFileToString("data/shaders/pbr_vis/fragment.glsl"))
                .DepthInfo(new IGraphicsPipeline.DepthStencilCreateInfo.Builder()
                                .DepthCompareOp(ECompareOp.Always)
                                .DepthWriteEnabled(false)
                                .DepthTestEnabled(false)
                                .Build())
                .Build(), device, layout);

            _pass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder()
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.Undefined)
                    .FinalLayout(EImageLayout.ColorAttachment)
                    .LoadOp(EAttachmentLoad.Clear)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.BGRA8_SRGB))
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.DepthAttachment)
                    .FinalLayout(EImageLayout.DepthAttachment)
                    .LoadOp(EAttachmentLoad.Load)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.D32_SFLOAT)));
        }

        public override bool Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw)
        {
            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo
            {
                ClearValues = new List<ClearValue>() { new ClearValue(new ClearValue.ClearColor(100.0f / 255.0f, 149.0f / 255.0f, 237.0f / 255.0f, 1)), null },
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
