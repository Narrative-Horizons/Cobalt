using System;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.Vulkan
{
    public static class VK
    {
        #region Named Pointers
        public struct Instance
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Instance window)
            {
                return window.handle;
            }

            public static explicit operator Instance(IntPtr handle) => new Instance(handle);

            public Instance(IntPtr handle)
            {
                this.handle = handle;
            }

            public void Destroy()
            {
                DestroyInstance(this);
            }
        }

        public struct SwapChain
        {
            public IntPtr handle;

            public static implicit operator IntPtr(SwapChain window)
            {
                return window.handle;
            }

            public static explicit operator SwapChain(IntPtr handle) => new SwapChain(handle);

            public SwapChain(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct RenderPass
        {
            public IntPtr handle;

            public static implicit operator IntPtr(RenderPass window)
            {
                return window.handle;
            }

            public static explicit operator RenderPass(IntPtr handle) => new RenderPass(handle);

            public RenderPass(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct CommandBuffer
        {
            public IntPtr handle;

            public static implicit operator IntPtr(CommandBuffer window)
            {
                return window.handle;
            }

            public static explicit operator CommandBuffer(IntPtr handle) => new CommandBuffer(handle);

            public CommandBuffer(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Buffer
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Buffer window)
            {
                return window.handle;
            }

            public static explicit operator Buffer(IntPtr handle) => new Buffer(handle);

            public Buffer(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct ShaderModule
        {
            public IntPtr handle;

            public static implicit operator IntPtr(ShaderModule window)
            {
                return window.handle;
            }

            public static explicit operator ShaderModule(IntPtr handle) => new ShaderModule(handle);

            public ShaderModule(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Shader
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Shader window)
            {
                return window.handle;
            }

            public static explicit operator Shader(IntPtr handle) => new Shader(handle);

            public Shader(IntPtr handle)
            {
                this.handle = handle;
            }
        }
        public struct Framebuffer
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Framebuffer window)
            {
                return window.handle;
            }

            public static explicit operator Framebuffer(IntPtr handle) => new Framebuffer(handle);

            public Framebuffer(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Image
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Image window)
            {
                return window.handle;
            }

            public static explicit operator Image(IntPtr handle) => new Image(handle);

            public Image(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct ImageView
        {
            public IntPtr handle;

            public static implicit operator IntPtr(ImageView window)
            {
                return window.handle;
            }

            public static explicit operator ImageView(IntPtr handle) => new ImageView(handle);

            public ImageView(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct DescriptorSet
        {
            public IntPtr handle;

            public static implicit operator IntPtr(DescriptorSet set)
            {
                return set.handle;
            }

            public static explicit operator DescriptorSet(IntPtr handle) => new DescriptorSet(handle);

            public DescriptorSet(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Semaphore
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Semaphore set)
            {
                return set.handle;
            }

            public static explicit operator Semaphore(IntPtr handle) => new Semaphore(handle);

            public Semaphore(IntPtr handle)
            {
                this.handle = handle;
            }
        }
        #endregion

        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "bin/gfx-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/gfx-native-bindings";
#else
        public const string LIBRARY = "bin/gfx-native-bindings";
#endif
        #endregion

        #region DLL Imports
        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_device", CallingConvention = CallingConvention.Cdecl)]
        public static extern Instance CreateInstance(InstanceCreateInfo info);

		[DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_device", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DestroyInstance(Instance instance);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern SwapChain CreateSwapchain(Instance device, SwapchainCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroySwapchain(SwapChain swapchain);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_get_swapchain_image_view", CallingConvention = CallingConvention.Cdecl)]
        public static extern ImageView GetSwapChainImageView(SwapChain swapchain, uint index);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_renderpass", CallingConvention = CallingConvention.Cdecl)]
        private static extern RenderPass CreateRenderPassImpl(Instance device, RenderPassCreateInfoImpl info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyRenderPass(Instance device, RenderPass renderpass);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern CommandBuffer CreateCommandBuffer(Instance device, CommandBufferCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyCommandBuffer(Instance device, CommandBuffer buffer, uint index);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_begin_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BeginCommandBuffer(Instance device, CommandBuffer buffer, uint index);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_command_begin_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BeginRenderPass(Instance device, CommandBuffer buffer, uint index, RenderPass pass, Framebuffer framebuffer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_command_bind_pipeline", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BindPipeline(Instance device, CommandBuffer buffer, uint index, Shader shader);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_command_draw", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Draw(Instance device, CommandBuffer buffer, uint index);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_command_end_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool EndRenderPass(Instance device, CommandBuffer buffer, uint index);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_commandbuffer_end", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool EndCommandBuffer(Instance device, CommandBuffer buffer, uint index);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Buffer CreateBuffer(Instance device, BufferCreateInfo info, BufferMemoryCreateInfo memoryInfo);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyBuffer(Instance device, Buffer buffer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_map_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MapBuffer(Instance device, Buffer buffer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_unmap_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnmapBuffer(Instance device, Buffer buffer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_shadermodule", CallingConvention = CallingConvention.Cdecl)]
        public static extern ShaderModule CreateShaderModule(Instance device, ShaderModuleCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_shadermodule", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyShaderModule(Instance device, ShaderModule module);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_shader", CallingConvention = CallingConvention.Cdecl)]
        public static extern Shader CreateShader(Instance device, ShaderCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern Image CreateImage(Instance device, ImageCreateInfo info, string name, uint frame);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyImage(Instance device, Image image);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_imageview", CallingConvention = CallingConvention.Cdecl)]
        public static extern ImageView CreateImageView(Instance device, ImageViewCreateInfo info, string name, uint frame);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_imageview", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyImageView(Instance device, ImageView imageView);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_framebuffer", CallingConvention = CallingConvention.Cdecl)]
        private static extern Framebuffer CreateFramebufferImpl(Instance device, FramebufferCreateInfoImpl info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_framebuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyFramebuffer(Instance device, Framebuffer imageView);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_semaphore", CallingConvention = CallingConvention.Cdecl)]
        public static extern Semaphore CreateSemaphore(Instance device, SemaphoreCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_acquire_next_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint AcquireNextImage(Instance device, SwapChain swapchain, Semaphore semaphore);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_semaphore", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroySemaphore(Instance device, Semaphore sempahore);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_submit_queue", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SubmitQueueImpl(Instance device, SubmitInfoImpl info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_queue_present", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool PresentQueueImpl(Instance device, PresentInfoImpl info);
        #endregion

        public static bool PresentQueue(Instance device, PresentInfo info)
        {
            PresentInfoImpl infoImpl = new PresentInfoImpl
            {
                swapchainCount = info.swapchainCount, waitSemaphoreCount = info.waitSemaphoreCount
            };

            unsafe
            {
                fixed (SwapChain* swapchains = &info.swapchains[0])
                {
                    infoImpl.swapchains = swapchains;
                }

                fixed (Semaphore* waitSemaphores = &info.waitSemaphores[0])
                {
                    infoImpl.waitSemaphores = waitSemaphores;
                }

                fixed (uint* imageIndices = &info.imageIndices[0])
                {
                    infoImpl.imageIndices = imageIndices;
                }
            }

            return PresentQueueImpl(device, infoImpl);
        }

        public static bool SubmitQueue(Instance device, SubmitInfo info)
        {
            SubmitInfoImpl infoImpl = new SubmitInfoImpl
            {
                commandBufferCount = info.commandBufferCount,
                signalSemaphoreCount = info.signalSemaphoreCount,
                waitSemaphoreCount = info.waitSemaphoreCount,
            };

            unsafe
            {
                fixed (Semaphore* waitSemaphores = &info.waitSemaphores[0])
                {
                    infoImpl.waitSemaphores = waitSemaphores;
                }

                fixed (Semaphore* signalSemaphores = &info.signalSemaphores[0])
                {
                    infoImpl.signalSemaphores = signalSemaphores;
                }

                fixed (uint* waitDstStageMask = &info.waitDstStageMask[0])
                {
                    infoImpl.waitDstStageMask = waitDstStageMask;
                }
            }

            unsafe
            {
                IndexedCommandBuffersImpl* implBuffers = stackalloc IndexedCommandBuffersImpl[(int)info.commandBufferCount];
                int idx = 0;
                foreach (IndexedCommandBuffers b in info.commandBuffer)
                {
                    IndexedCommandBuffersImpl i = new IndexedCommandBuffersImpl {commandbuffer = b.commandbuffer, amount = b.amount};

                    fixed (uint* indices = &b.bufferIndices[0])
                    {
                        i.bufferIndices = indices;
                    }

                    implBuffers[idx++] = i;
                }

                infoImpl.commandBuffer = implBuffers;
            }

            return SubmitQueueImpl(device, infoImpl);
        }


        public static Framebuffer CreateFramebuffer(Instance device, FramebufferCreateInfo info)
        {
            FramebufferCreateInfoImpl infoImpl = new FramebufferCreateInfoImpl
            {
                width = info.width,
                height = info.height,
                attachmentCount = info.attachmentCount,
                layers = info.layers,
                pass = info.pass
            };

            unsafe
            {
                fixed (ImageView* attachments = &info.attachments[0])
                {
                    infoImpl.attachments = attachments;
                }
            }

            return CreateFramebufferImpl(device, infoImpl);
        }


        public static RenderPass CreateRenderPass(Instance device, RenderPassCreateInfo info)
        {
            RenderPassCreateInfoImpl infoImpl = new RenderPassCreateInfoImpl
            {
                attachmentCount = info.attachmentCount,
                subpassCount = info.subpassCount,
                dependencyCount = info.dependencyCount
            };

            int idx = 0;
            SubpassDescriptionImpl[] subs = null;
            if (info.subpasses != null)
            {
                subs = new SubpassDescriptionImpl[info.subpassCount];

                foreach (SubpassDescription sub in info.subpasses)
                {
                    SubpassDescriptionImpl newSub = new SubpassDescriptionImpl
                    {
                        colorAttachmentCount = sub.colorAttachmentCount,
                        flags = sub.flags,
                        inputAttachmentCount = sub.inputAttachmentCount,
                        pipelineBindPoint = sub.pipelineBindPoint,
                        preserveAttachmentCount = sub.preserveAttachmentCount
                    };

                    unsafe
                    {
                        if (sub.attachments != null)
                        {
                            fixed (AttachmentReference* attachments = &sub.attachments[0])
                            {
                                newSub.attachments = attachments;
                            }
                        }

                        if (sub.colorAttachments != null)
                        {
                            fixed (AttachmentReference* colorAttachments = &sub.colorAttachments[0])
                            {
                                newSub.colorAttachments = colorAttachments;
                            }
                        }

                        if (sub.resolveAttachments != null)
                        {
                            fixed (AttachmentReference* resolveAttachments = &sub.resolveAttachments[0])
                            {
                                newSub.resolveAttachments = resolveAttachments;
                            }
                        }

                        if (sub.depthStencilAttachments != null)
                        {
                            fixed (AttachmentReference* depthStencilAttachments = &sub.depthStencilAttachments[0])
                            {
                                newSub.depthStencilAttachments = depthStencilAttachments;
                            }
                        }

                        if (sub.preserveAttachments != null)
                        {
                            fixed (uint* preserveAttachments = &sub.preserveAttachments[0])
                            {
                                newSub.preserveAttachments = preserveAttachments;
                            }
                        }
                    }

                    subs[idx++] = newSub;
                }
            }

            unsafe
            {
                if (info.attachments != null)
                {
                    fixed (AttachmentDescription* attachments = &info.attachments[0])
                    {
                        infoImpl.attachments = attachments;
                    }
                }

                if (subs != null)
                {
                    fixed (SubpassDescriptionImpl* subpasses = &subs[0])
                    {
                        infoImpl.subpasses = subpasses;
                    }
                }

                if (info.dependencies != null)
                {
                    fixed (SubpassDependency* dependencies = &info.dependencies[0])
                    {
                        infoImpl.dependencies = dependencies;
                    }
                }
            }

            return CreateRenderPassImpl(device, infoImpl);
        }
    }
}
