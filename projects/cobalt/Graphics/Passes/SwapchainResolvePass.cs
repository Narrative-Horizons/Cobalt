using Cobalt.Bindings.Vulkan;
using Cobalt.Graphics.Enums;

namespace Cobalt.Graphics.Passes
{
    public class SwapchainResolvePass : Pass
    {
        private readonly Shader _resolveShader;
        private readonly VK.RenderPass _renderPass;

        private readonly uint _framesInFlight;

        private readonly Descriptor[] _textureDescriptor;
        private readonly Framebuffer[] _framebuffers;


        public SwapchainResolvePass(Device device, Swapchain swapchain, uint framesInFlight, ImageView[] image, VK.Sampler sampler)
        {
            _framesInFlight = framesInFlight;

            AttachmentDescription colorDesc = new AttachmentDescription
            {
                initialLayout = (uint) ImageLayout.Undefined,
                finalLayout = (uint) ImageLayout.ShaderReadOnlyOptimal,
                format = (uint) Format.B8G8R8A8Srgb,
                samples = (uint) SampleCountFlagBits.Count1Bit,
                loadOp = (uint) AttachmentLoadOp.Load,
                storeOp = (uint) AttachmentStoreOp.Store,
                flags = 0
            };

            AttachmentReference colorBuffer = new AttachmentReference
            {
                attachment = 0,
                layout = (uint) ImageLayout.ShaderReadOnlyOptimal
            };

            SubpassDescription swapchainResolvePass = new SubpassDescription
            {
                attachments = new[] {colorBuffer},
                inputAttachmentCount = 1,
                pipelineBindPoint = (uint) PipelineBindPoint.Graphics,
                flags = 0
            };

            RenderPassCreateInfo renderPassInfo = new RenderPassCreateInfo
            {
                attachments = new[] {colorDesc},
                attachmentCount = 1,
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

            _textureDescriptor = new Descriptor[_framesInFlight];
            for (uint i = 0; i < _framesInFlight; i++)
            {
                VK.DescriptorSet set = VK.AllocateDescriptors(_resolveShader.handle);

                _textureDescriptor[i] = new Descriptor(set, 0, 0);

                DescriptorWriteInfo textureWriteInfo = new DescriptorWriteInfo
                {
                    sets = set,
                    set = 0,
                    binding = 0,
                    count = 1,
                    element = 0,
                    type = (uint) DescriptorType.InputAttachment
                };

                ImageWriteInfo textureSamplerInfo = new ImageWriteInfo
                {
                    view = image[i].handle, 
                    sampler = sampler, 
                    layout = (uint) ImageLayout.ShaderReadOnlyOptimal
                };

                TypedWriteInfo textureTypedInfo = new TypedWriteInfo
                {
                    images = textureSamplerInfo
                };

                textureWriteInfo.infos = new[]
                {
                    textureTypedInfo
                };

                VK.WriteDescriptors(device.handle, 1, new[] { textureWriteInfo });
            }

            _framebuffers = new Framebuffer[_framesInFlight];

            for (uint i = 0; i < _framesInFlight; i++)
            {
                FramebufferCreateInfo framebufferInfo = new FramebufferCreateInfo
                {
                    attachmentCount = 1,
                    attachments = new[] {VK.GetSwapChainImageView(swapchain.handle, i)},
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
            commandList.BeginRenderPass(_renderPass, _framebuffers[frameInFlight]);

            commandList.Bind(_resolveShader, 0, new[] { _textureDescriptor[frameInFlight] }, new[] { 0U });
            commandList.Bind(_resolveShader);

            commandList.Draw(6, 1, 0, 0);

            commandList.EndRenderPass();
        }

        public override void Dispose()
        {
            
        }
    }
}