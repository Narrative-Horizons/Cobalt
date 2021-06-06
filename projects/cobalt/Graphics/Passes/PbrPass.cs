using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics.Passes
{
    internal class PbrRenderPass : RenderPass
    {
        private Shader _pbrShader;
        private IRenderPass _pass;

        public DebugCameraComponent Camera { get; set; }

        public PbrRenderPass(IDevice device, IPipelineLayout layout) : base(device)
        {
            _pbrShader = new Shader(new Shader.CreateInfo.Builder().VertexSource(FileSystem.LoadFileToString("data/shaders/pbr/pbr_vertex.glsl"))
                .FragmentSource(FileSystem.LoadFileToString("data/shaders/pbr/pbr_fragment.glsl")).Build(), device, layout, true);

            _pass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder()
                .AddAttachment(new IRenderPass.AttachmentDescription.Builder()
                    .InitialLayout(EImageLayout.Undefined)
                    .FinalLayout(EImageLayout.PresentSource)
                    .LoadOp(EAttachmentLoad.Clear)
                    .StoreOp(EAttachmentStore.Store)
                    .Format(EDataFormat.BGRA8_SRGB)));
        }

        public override void Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw)
        {
            buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo
            {
                ClearValues = new List<ClearValue>() { new ClearValue(new ClearValue.ClearColor(0, 0, 0, 1)), new ClearValue(1) },
                Width = 1920,
                Height = 1080,
                FrameBuffer = info.frameBuffer,
                RenderPass = _pass
            });

            buffer.Bind(_pbrShader.Pipeline);
            buffer.Bind(_pbrShader.Layout, 0, draw.descriptorSets);
            Draw(buffer, draw);
        }
    }
}
