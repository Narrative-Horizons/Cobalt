using Cobalt.Bindings.Vulkan;
using Cobalt.Graphics.Enums;

namespace Cobalt.Graphics.Passes
{
    public class UberPBRPass : Pass
    {
        private Shader _opaqueShader;
        private VK.RenderPass _renderPass;

        private uint _framesInFlight;

        private Framebuffer[] _framebuffers;
        private ImageView[] _frameBufferColorImageViews;
        private ImageView[] _frameBufferDepthImageViews;

        public UberPBRPass(Device device, uint framesInFlight)
        {
            _framesInFlight = framesInFlight;

            AttachmentDescription colorDesc = new AttachmentDescription();
            colorDesc.initialLayout = (uint)ImageLayout.Undefined;
            colorDesc.finalLayout = (uint)ImageLayout.ColorAttachmentOptimal;
            colorDesc.format = (uint) Format.B8G8R8A8Srgb;
            colorDesc.samples = (uint) SampleCountFlagBits.Count1Bit;
            colorDesc.loadOp = (uint) AttachmentLoadOp.Clear;
            colorDesc.storeOp = (uint) AttachmentStoreOp.Store;
            colorDesc.flags = 0;

            AttachmentDescription depthDesc = new AttachmentDescription();
            depthDesc.initialLayout = (uint)ImageLayout.Undefined;
            depthDesc.finalLayout = (uint)ImageLayout.DepthStencilAttachmentOptimal;
            depthDesc.format = (uint)Format.D24UnormS8Uint;
            depthDesc.samples = (uint)SampleCountFlagBits.Count1Bit;
            depthDesc.loadOp = (uint)AttachmentLoadOp.Clear;
            depthDesc.storeOp = (uint)AttachmentStoreOp.Store;
            depthDesc.flags = 0;

            AttachmentReference colorBuffer = new AttachmentReference();
            colorBuffer.layout = (uint) ImageLayout.ColorAttachmentOptimal;
            colorBuffer.attachment = 0;

            AttachmentReference depthBuffer = new AttachmentReference();
            depthBuffer.layout = (uint)ImageLayout.DepthStencilAttachmentOptimal;
            depthBuffer.attachment = 1;

            SubpassDescription opaquePass = new SubpassDescription();
            opaquePass.colorAttachments = new[] {colorBuffer};
            opaquePass.colorAttachmentCount = 1;
            opaquePass.depthStencilAttachments = new[] {depthBuffer};
            opaquePass.pipelineBindPoint = (uint) PipelineBindPoint.Graphics;
            opaquePass.flags = 0;

            RenderPassCreateInfo renderPassInfo = new RenderPassCreateInfo();
            renderPassInfo.attachments = new[] {colorDesc, depthDesc};
            renderPassInfo.attachmentCount = 2;
            renderPassInfo.subpasses = new[] {opaquePass};
            renderPassInfo.subpassCount = 1;

            _renderPass = device.CreateRenderPass(renderPassInfo);

            _framebuffers = new Framebuffer[_framesInFlight];
            _frameBufferColorImageViews = new ImageView[_framesInFlight];
            _frameBufferDepthImageViews = new ImageView[_framesInFlight];

            for (uint i = 0; i < _framesInFlight; i++)
            {
                ImageCreateInfo colorInfo = new ImageCreateInfo();
                colorInfo.format = (uint) Format.B8G8R8A8Srgb;
                colorInfo.width = 1280;
                colorInfo.height = 720;
                colorInfo.depth = 1;
                colorInfo.initialLayout = (uint) ImageLayout.Undefined;
                colorInfo.arrayLayers = 1;
                colorInfo.samples = (uint) SampleCountFlagBits.Count1Bit;
                colorInfo.imageType = (uint) ImageType.Type2D;
                colorInfo.mipLevels = 1;
                colorInfo.sharingMode = (uint) SharingMode.Exclusive;
                colorInfo.tiling = (uint) ImageTiling.Optimal;
                colorInfo.usage = (uint) ImageUsageFlagBits.ColorAttachmentBit;

                ImageMemoryCreateInfo colorMemory = new ImageMemoryCreateInfo();
                colorMemory.usage = (uint) MemoryUsage.GpuOnly;
                colorMemory.requiredFlags = (uint) MemoryPropertyFlagBits.DeviceLocalBit;

                Image colorImage = device.CreateImage(colorInfo, colorMemory, "color", i);
                _frameBufferColorImageViews[i] = colorImage.CreateImageView((Format) colorInfo.format, ImageAspectFlagBits.ColorBit);

                ImageCreateInfo depthInfo = new ImageCreateInfo();
                depthInfo.format = (uint)Format.D24UnormS8Uint;
                depthInfo.width = 1280;
                depthInfo.height = 720;
                depthInfo.depth = 1;
                depthInfo.initialLayout = (uint)ImageLayout.Undefined;
                depthInfo.arrayLayers = 1;
                depthInfo.samples = (uint)SampleCountFlagBits.Count1Bit;
                depthInfo.imageType = (uint)ImageType.Type2D;
                depthInfo.mipLevels = 1;
                depthInfo.sharingMode = (uint)SharingMode.Exclusive;
                depthInfo.tiling = (uint)ImageTiling.Optimal;
                depthInfo.usage = (uint)ImageUsageFlagBits.DepthStencilAttachmentBit;

                ImageMemoryCreateInfo depthMemory = new ImageMemoryCreateInfo();
                depthMemory.usage = (uint)MemoryUsage.GpuOnly;
                depthMemory.requiredFlags = (uint)MemoryPropertyFlagBits.DeviceLocalBit;

                Image depthImage = device.CreateImage(depthInfo, depthMemory, "depth", i);
                _frameBufferDepthImageViews[i] = depthImage.CreateImageView((Format)depthInfo.format, ImageAspectFlagBits.DepthBit);

                FramebufferCreateInfo framebufferInfo = new FramebufferCreateInfo();
                framebufferInfo.attachmentCount = 2;
                framebufferInfo.attachments = new[] {_frameBufferColorImageViews[i].handle, _frameBufferDepthImageViews[i].handle};
                framebufferInfo.height = 720;
                framebufferInfo.width = 1280;
                framebufferInfo.layers = 1;
                framebufferInfo.pass = _renderPass;

                _framebuffers[i] = device.CreateFramebuffer(framebufferInfo);
            }

            ShaderCreateInfo opaqueShaderInfo = new ShaderCreateInfo();
            opaqueShaderInfo.pass = _renderPass;
            opaqueShaderInfo.subPassIndex = 0;
            opaqueShaderInfo.vertexModulePath = "data/shaders/pbr/opaque_vert.spv";
            opaqueShaderInfo.fragmentModulePath = "data/shaders/pbr/opaque_frag.spv";

            ShaderLayoutCreateInfo opaqueLayoutInfo = new ShaderLayoutCreateInfo();
            ShaderLayoutSetCreateInfo opaqueSetSceneInfo = new ShaderLayoutSetCreateInfo();
            ShaderLayoutSetCreateInfo opaqueSetObjectInfo = new ShaderLayoutSetCreateInfo();

            ShaderLayoutBindingCreateInfo opaqueBindingObjectDataInfo = new ShaderLayoutBindingCreateInfo();
            opaqueBindingObjectDataInfo.bindingIndex = 0;
            opaqueBindingObjectDataInfo.type = (uint) DescriptorType.StorageBuffer;
            opaqueBindingObjectDataInfo.stageFlags = (uint) ShaderStageFlagBits.VertexBit | (uint) ShaderStageFlagBits.FragmentBit;
            opaqueBindingObjectDataInfo.descriptorCount = 1;

            ShaderLayoutBindingCreateInfo opaqueBindingMaterialDataInfo = new ShaderLayoutBindingCreateInfo();
            opaqueBindingMaterialDataInfo.bindingIndex = 1;
            opaqueBindingMaterialDataInfo.type = (uint)DescriptorType.StorageBuffer;
            opaqueBindingMaterialDataInfo.stageFlags = (uint)ShaderStageFlagBits.FragmentBit;
            opaqueBindingMaterialDataInfo.descriptorCount = 1;

            ShaderLayoutBindingCreateInfo opaqueBindingSceneDataInfo = new ShaderLayoutBindingCreateInfo();
            opaqueBindingSceneDataInfo.bindingIndex = 0;
            opaqueBindingSceneDataInfo.type = (uint)DescriptorType.UniformBuffer;
            opaqueBindingSceneDataInfo.stageFlags = (uint)ShaderStageFlagBits.VertexBit | (uint)ShaderStageFlagBits.FragmentBit;
            opaqueBindingSceneDataInfo.descriptorCount = 1;

            ShaderLayoutBindingCreateInfo opaqueBindingTextureDataInfo = new ShaderLayoutBindingCreateInfo();
            opaqueBindingTextureDataInfo.bindingIndex = 2;
            opaqueBindingTextureDataInfo.type = (uint)DescriptorType.CombinedImageSampler;
            opaqueBindingTextureDataInfo.stageFlags = (uint)ShaderStageFlagBits.FragmentBit;
            opaqueBindingTextureDataInfo.descriptorCount = 500;

            opaqueSetObjectInfo.bindingCount = 3;
            opaqueSetObjectInfo.bindingInfos = new[] {opaqueBindingObjectDataInfo, opaqueBindingMaterialDataInfo, opaqueBindingTextureDataInfo};

            opaqueSetSceneInfo.bindingCount = 1;
            opaqueSetSceneInfo.bindingInfos = new[] {opaqueBindingSceneDataInfo};

            opaqueLayoutInfo.setCount = 2;
            opaqueLayoutInfo.setInfos = new[] {opaqueSetSceneInfo, opaqueSetObjectInfo};

            opaqueShaderInfo.layoutInfo = opaqueLayoutInfo;

            _opaqueShader = device.CreateShader(opaqueShaderInfo);

            // subpasses
            //      zprepass (eventually)
            //      opaque
            //      translucent (eventually)

            // attachments
            //      depthBuffer
            //      colorBuffer
        }

        public override PassType GetPassType()
        {
            return PassType.Graphics;
        }

        public override void Start(CommandList commandList)
        {

        }

        public override void Execute(CommandList commandList)
        {
        }

        public override void Dispose()
        {
            
        }
    }
}
