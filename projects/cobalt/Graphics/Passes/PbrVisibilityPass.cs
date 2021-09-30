using Cobalt.Graphics.API;
using Cobalt.Graphics.Pipelines;
using System.Collections.Generic;

namespace Cobalt.Graphics.Passes
{
    public class PbrVisibilityPass : RenderPass
    {
        private IRenderPipeline Pipeline { get; set; }
        private IRenderPass Pass { get; set; }
        public PbrVisibilityPass(IDevice device, IRenderPipeline pipeline) : base(device)
        {
            Pipeline = pipeline;
            Pass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder()
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .FinalLayout(EImageLayout.ColorAttachment)
                    .Format(EDataFormat.R32G32_UINT)
                    .InitialLayout(EImageLayout.Undefined)
                    .LoadOp(EAttachmentLoad.Clear)
                    .Samples(ESampleCount.Samples1)
                    .StencilLoadOp(EAttachmentLoad.DontCare)
                    .StencilStoreOp(EAttachmentStore.DontCare)
                    .StoreOp(EAttachmentStore.Store))
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .FinalLayout(EImageLayout.DepthAttachment)
                    .Format(EDataFormat.D32_SFLOAT)
                    .InitialLayout(EImageLayout.Undefined)
                    .LoadOp(EAttachmentLoad.Clear)
                    .Samples(ESampleCount.Samples1)
                    .StencilLoadOp(EAttachmentLoad.DontCare)
                    .StencilStoreOp(EAttachmentStore.DontCare)
                    .StoreOp(EAttachmentStore.Store)));
        }

        public override bool Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw)
        {
            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo()
            {
                ClearValues = new List<ClearValue>()
                {
                    new ClearValue(uint.MaxValue),
                    new ClearValue(uint.MaxValue),
                },
                FrameBuffer = Pipeline.GetFrameBuffer(PbrUberPipeline.VisibilityPassFrameBufferResourceName, info.frameInFlight),
                Width = info.width,
                Height = info.height,
                RenderPass = Pass
            });
            return true;
        }
    }
}
