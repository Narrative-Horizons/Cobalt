using Cobalt.Bindings.Vulkan;
using Cobalt.Graphics.Enums;

namespace Cobalt.Graphics.Passes
{
    public class SwapchainResolvePass : Pass
    {
        private readonly Shader _resolveShader;
        private readonly VK.RenderPass _renderPass;

        private readonly uint _framesInFlight;

        private readonly Framebuffer[] _framebuffers;


        public SwapchainResolvePass(Device device, Swapchain swapchain, uint framesInFlight, ImageView[] image, VK.Sampler sampler)
        {
            _framesInFlight = framesInFlight;

            AttachmentDescription inputDesc = new AttachmentDescription
            {
                initialLayout = (uint) ImageLayout.Undefined,
                finalLayout = (uint) ImageLayout.ShaderReadOnlyOptimal,
                format = (uint) Format.B8G8R8A8Srgb,
                samples = (uint) SampleCountFlagBits.Count1Bit,
                loadOp = (uint) AttachmentLoadOp.Load,
                storeOp = (uint) AttachmentStoreOp.Store,
                flags = 0
            };

            AttachmentReference inputBuffer = new AttachmentReference
            {
                attachment = 0,
                layout = (uint) ImageLayout.ShaderReadOnlyOptimal
            };

            AttachmentDescription outputDesc = new AttachmentDescription
            {
                initialLayout = (uint)ImageLayout.Undefined,
                finalLayout = (uint)ImageLayout.PresentSrcKHR,
                format = (uint)Format.B8G8R8A8Srgb,
                samples = (uint)SampleCountFlagBits.Count1Bit,
                loadOp = (uint)AttachmentLoadOp.Load,
                storeOp = (uint)AttachmentStoreOp.Store,
                flags = 0
            };

            AttachmentReference outputBuffer = new AttachmentReference
            {
                attachment = 1,
                layout = (uint)ImageLayout.ColorAttachmentOptimal
            };

            SubpassDescription swapchainResolvePass = new SubpassDescription
            {
                attachments = new[] {inputBuffer},
                inputAttachmentCount = 1,
                colorAttachments = new []{outputBuffer},
                colorAttachmentCount = 1,
                pipelineBindPoint = (uint) PipelineBindPoint.Graphics,
                flags = 0
            };

            RenderPassCreateInfo renderPassInfo = new RenderPassCreateInfo
            {
                attachments = new[] {inputDesc, outputDesc},
                attachmentCount = 2,
                subpasses = new[] {swapchainResolvePass},
                subpassCount = 1
            };

            _renderPass = device.CreateRenderPass(renderPassInfo);

            ShaderCreateInfo resolveShaderInfo = new ShaderCreateInfo
            {
                pass = _renderPass,
                subPassIndex = 0,
                vertexModulePath = "data/shaders/swapchain_resolve_pass/resolve_vert.spv",
                fragmentModulePath = "data/shaders/swapchain_resolve_pass/resolve_frag.spv"
            };

            ShaderLayoutCreateInfo resolveLayoutInfo = new ShaderLayoutCreateInfo();
            ShaderLayoutSetCreateInfo resolveSetInfo = new ShaderLayoutSetCreateInfo();

            ShaderLayoutBindingCreateInfo resolveBindingInfo = new ShaderLayoutBindingCreateInfo
            {
                bindingIndex = 0,
                type = (uint) DescriptorType.InputAttachment,
                stageFlags = (uint) ShaderStageFlagBits.FragmentBit,
                descriptorCount = 1
            };

            resolveSetInfo.bindingCount = 1;
            resolveSetInfo.bindingInfos = new[] { resolveBindingInfo };

            resolveLayoutInfo.setCount = 1;
            resolveLayoutInfo.setInfos = new[] { resolveSetInfo };

            resolveShaderInfo.layoutInfo = resolveLayoutInfo;

            _resolveShader = device.CreateShader(resolveShaderInfo);

            _framebuffers = new Framebuffer[_framesInFlight];

            for (uint i = 0; i < _framesInFlight; i++)
            {
                FramebufferCreateInfo framebufferInfo = new FramebufferCreateInfo
                {
                    attachmentCount = 2,
                    attachments = new[] { image[i].handle, VK.GetSwapChainImageView(swapchain.handle, i) },
                    height = 720,
                    width = 1280,
                    layers = 1,
                    pass = _renderPass
                };

                _framebuffers[i] = device.CreateFramebuffer(framebufferInfo);
            }
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
            ClearValue clear = new ClearValue();
            clear.color = new ClearColorValue();
            unsafe
            {
                clear.color.float32[0] = 0;
                clear.color.float32[1] = 0;
                clear.color.float32[2] = 0;
                clear.color.float32[3] = 1;
            }

            commandList.BeginRenderPass(_renderPass, _framebuffers[frameInFlight], new []{clear});

            commandList.Bind(_resolveShader);

            commandList.Draw(6, 1, 0, 0);

            commandList.EndRenderPass();
        }

        public override void Dispose()
        {
            
        }
    }
}