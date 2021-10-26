using Cobalt.Graphics.VK;
using System;
using Cobalt.Bindings.Vulkan;
using Cobalt.Graphics.VK.Enums;

namespace Cobalt.Graphics
{
    public class GraphicsContext : IDisposable
    {
        #region Properties
        public Device ContextDevice { get; private set; }
        #endregion

        public Bindings.Vulkan.VK.Shader shader;
        public Cobalt.Bindings.Vulkan.VK.RenderPass pass;
        public Cobalt.Bindings.Vulkan.VK.SwapChain swapchain;
        public Cobalt.Bindings.Vulkan.VK.Framebuffer[] framebuffers = new Bindings.Vulkan.VK.Framebuffer[2];
        public Cobalt.Bindings.Vulkan.VK.CommandBuffer commandbuffer;
        public Cobalt.Bindings.Vulkan.VK.Semaphore[] imageAvailableSemaphore;
        public Cobalt.Bindings.Vulkan.VK.Semaphore[] renderFinishedSemaphore;

        public GraphicsContext(Window window)
        {
            ContextDevice = Device.Create(window);

            SwapchainCreateInfo swapchainInfo = new SwapchainCreateInfo();
            swapchain = Bindings.Vulkan.VK.CreateSwapchain(ContextDevice.handle, swapchainInfo);

            RenderPassCreateInfo renderpassInfo = new RenderPassCreateInfo();

            AttachmentReference colorAttachmentRef = new AttachmentReference
            {
                attachment = 0, layout = (uint) ImageLayout.ColorAttachmentOptimal
            };

            SubpassDescription subpass = new SubpassDescription
            {
                pipelineBindPoint = (uint) PipelineBindPoint.Graphics,
                colorAttachmentCount = 1,
                colorAttachments = new[] {colorAttachmentRef}
            };

            AttachmentDescription colorAttachment = new AttachmentDescription
            {
                format = (uint) Format.B8G8R8A8Srgb,
                samples = (uint) SampleCountFlagBits.Count1Bit,
                loadOp = (uint) AttachmentLoadOp.Clear,
                storeOp = (uint) AttachmentStoreOp.Store,
                stencilLoadOp = (uint) AttachmentLoadOp.DontCare,
                stencilStoreOp = (uint) AttachmentStoreOp.DontCare,
                initialLayout = (uint) ImageLayout.Undefined,
                finalLayout = (uint) ImageLayout.PresentSrcKHR
            };

            SubpassDependency dependency = new SubpassDependency();
            dependency.srcSubpass = (~0U);
            dependency.dstSubpass = 0;

            dependency.srcStageMask = (uint)PipelineStageFlagBits.ColorAttachmentOutputBit;
            dependency.srcAccessMask = 0;

            dependency.dstStageMask = (uint)PipelineStageFlagBits.ColorAttachmentOutputBit;
            dependency.dstAccessMask = (uint) AccessFlagBits.ColorAttachmentWriteBit;

            renderpassInfo.subpassCount = 1;
            renderpassInfo.subpasses = new[] {subpass};
            renderpassInfo.attachmentCount = 1;
            renderpassInfo.attachments = new[] {colorAttachment};

            renderpassInfo.dependencyCount = 1;
            renderpassInfo.dependencies = new[] {dependency};

            pass = Bindings.Vulkan.VK.CreateRenderPass(ContextDevice.handle, renderpassInfo);

            ShaderCreateInfo shaderInfo = new ShaderCreateInfo
            {
                vertexModulePath = "data/shaders/vulkantest/triangle_vert.spv",
                fragmentModulePath = "data/shaders/vulkantest/triangle_frag.spv",
                subPassIndex = 0,
                pass = pass
            };

            shader = Bindings.Vulkan.VK.CreateShader(ContextDevice.handle, shaderInfo);

            for (int i = 0; i < 2; i++)
            {

                FramebufferCreateInfo framebufferInfo = new FramebufferCreateInfo
                {
                    width = 1280,
                    height = 720,
                    pass = pass,
                    layers = 1,
                    attachmentCount = 1,
                    attachments = new[] {Bindings.Vulkan.VK.GetSwapChainImageView(swapchain, (uint) i)}
                };

                framebuffers[i] = Bindings.Vulkan.VK.CreateFramebuffer(ContextDevice.handle, framebufferInfo);
            }

            CommandBufferCreateInfo bufferInfo = new CommandBufferCreateInfo {amount = 2, pool = 1, primary = true};

            commandbuffer = Bindings.Vulkan.VK.CreateCommandBuffer(ContextDevice.handle, bufferInfo);

            imageAvailableSemaphore = new Bindings.Vulkan.VK.Semaphore[2];
            renderFinishedSemaphore = new Bindings.Vulkan.VK.Semaphore[2];

            for (int i = 0; i < 2; i++)
            {
                Bindings.Vulkan.VK.BeginCommandBuffer(ContextDevice.handle, commandbuffer, (uint)i);

                Bindings.Vulkan.VK.BeginRenderPass(ContextDevice.handle, commandbuffer, (uint)i, pass, framebuffers[i]);

                Bindings.Vulkan.VK.BindPipeline(ContextDevice.handle, commandbuffer, (uint) i, shader);

                Bindings.Vulkan.VK.Draw(ContextDevice.handle, commandbuffer, (uint) i);

                Bindings.Vulkan.VK.EndRenderPass(ContextDevice.handle, commandbuffer, (uint) i);

                Bindings.Vulkan.VK.EndCommandBuffer(ContextDevice.handle, commandbuffer, (uint) i);

                SemaphoreCreateInfo semInfo = new SemaphoreCreateInfo();
                imageAvailableSemaphore[i] = Bindings.Vulkan.VK.CreateSemaphore(ContextDevice.handle, semInfo);
                renderFinishedSemaphore[i] = Bindings.Vulkan.VK.CreateSemaphore(ContextDevice.handle, semInfo);
            }
        }

        uint currentFrame = 0;

        public void Render()
        {
            uint imageIndex =
                Bindings.Vulkan.VK.AcquireNextImage(ContextDevice.handle, swapchain, imageAvailableSemaphore[currentFrame]);

            SubmitInfo submitInfo = new SubmitInfo();
            submitInfo.waitSemaphoreCount = 1;
            submitInfo.waitSemaphores = new[] {imageAvailableSemaphore[currentFrame] };

            submitInfo.signalSemaphoreCount = 1;
            submitInfo.signalSemaphores = new[] {renderFinishedSemaphore[currentFrame] };

            submitInfo.waitSemaphoreCount = 1;
            submitInfo.waitDstStageMask = new[] {(uint)PipelineStageFlagBits.ColorAttachmentOutputBit};

            submitInfo.commandBufferCount = 1;
            IndexedCommandBuffers indexedBuffer = new IndexedCommandBuffers
            {
                amount = 1, bufferIndices = new[] { currentFrame }, commandbuffer = commandbuffer
            };

            submitInfo.commandBuffer = new []{ indexedBuffer};

            Bindings.Vulkan.VK.SubmitQueue(ContextDevice.handle, submitInfo);

            PresentInfo presentInfo = new PresentInfo();
            presentInfo.waitSemaphoreCount = 1;
            presentInfo.waitSemaphores = new[] {renderFinishedSemaphore[currentFrame] };
            presentInfo.swapchainCount = 1;
            presentInfo.swapchains = new[] {swapchain};
            presentInfo.imageIndices = new[] {imageIndex};

            Bindings.Vulkan.VK.PresentQueue(ContextDevice.handle, presentInfo);

            currentFrame++;

            if (currentFrame == 2)
                currentFrame = 0;
        }

        public void Dispose()
        {
            ContextDevice.Dispose();
        }
    }
}
