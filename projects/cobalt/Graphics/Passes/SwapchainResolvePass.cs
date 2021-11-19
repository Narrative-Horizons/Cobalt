using Cobalt.Bindings.Vulkan;
using Cobalt.Graphics.Enums;

namespace Cobalt.Graphics.Passes
{
    public class SwapchainResolvePass : Pass
    {
        private Shader _resolveShader;
        private VK.RenderPass _renderPass;
        private Device _device;

        private uint _framesInFlight;

        public SwapchainResolvePass(Device device, uint framesInFlight)
        {
            _framesInFlight = framesInFlight;
            _device = device;

            AttachmentDescription colorDesc = new AttachmentDescription();
            colorDesc.initialLayout = (uint)ImageLayout.Undefined;
            colorDesc.finalLayout = (uint)ImageLayout.ColorAttachmentOptimal;
            colorDesc.format = (uint)Format.B8G8R8A8Srgb;
            colorDesc.samples = (uint)SampleCountFlagBits.Count1Bit;
            colorDesc.loadOp = (uint)AttachmentLoadOp.Clear;
            colorDesc.storeOp = (uint)AttachmentStoreOp.Store;
            colorDesc.flags = 0;

            AttachmentReference colorBuffer = new AttachmentReference();
            colorBuffer.layout = (uint)ImageLayout.ColorAttachmentOptimal;
            colorBuffer.attachment = 0;

            SubpassDescription swapchainResolvePass = new SubpassDescription();
            swapchainResolvePass.attachments = new[] { colorBuffer };
            swapchainResolvePass.inputAttachmentCount = 1;
            swapchainResolvePass.pipelineBindPoint = (uint)PipelineBindPoint.Graphics;
            swapchainResolvePass.flags = 0;

            RenderPassCreateInfo renderPassInfo = new RenderPassCreateInfo();
            renderPassInfo.attachments = new[] { colorDesc };
            renderPassInfo.attachmentCount = 1;
            renderPassInfo.subpasses = new[] { swapchainResolvePass };
            renderPassInfo.subpassCount = 1;

            _renderPass = device.CreateRenderPass(renderPassInfo);
        }

        public override PassType GetPassType()
        {
            return PassType.Graphics;
        }

        public override void Start(CommandList commandList)
        {

        }

        public override void Execute(CommandList commandList, uint frameInFlight)
        {

        }

        public override void Dispose()
        {
            
        }
    }
}