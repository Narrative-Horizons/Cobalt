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

            public static implicit operator IntPtr(Instance instance)
            {
                return instance.handle;
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

            public static implicit operator IntPtr(SwapChain swapchain)
            {
                return swapchain.handle;
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

            public static implicit operator IntPtr(RenderPass renderpass)
            {
                return renderpass.handle;
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

            public static implicit operator IntPtr(CommandBuffer commandbuffer)
            {
                return commandbuffer.handle;
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

            public static implicit operator IntPtr(Buffer buffer)
            {
                return buffer.handle;
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

            public static implicit operator IntPtr(ShaderModule shadermodule)
            {
                return shadermodule.handle;
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

            public static implicit operator IntPtr(Shader shader)
            {
                return shader.handle;
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

            public static implicit operator IntPtr(Framebuffer framebuffer)
            {
                return framebuffer.handle;
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

            public static implicit operator IntPtr(Image image)
            {
                return image.handle;
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

            public static implicit operator IntPtr(ImageView imageview)
            {
                return imageview.handle;
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

            public static implicit operator IntPtr(Semaphore semaphore)
            {
                return semaphore.handle;
            }

            public static explicit operator Semaphore(IntPtr handle) => new Semaphore(handle);

            public Semaphore(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Fence
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Fence fence)
            {
                return fence.handle;
            }

            public static explicit operator Fence(IntPtr handle) => new Fence(handle);

            public Fence(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Sampler
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Sampler sampler)
            {
                return sampler.handle;
            }

            public static explicit operator Sampler(IntPtr handle) => new Sampler(handle);

            public Sampler(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct Event
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Event @event)
            {
                return @event.handle;
            }

            public static explicit operator Event(IntPtr @event) => new Event(@event);

            public Event(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        #endregion

        #region DLL Loading

#if COBALT_PLATFORM_WINDOWS
        public const string Library = "bin/gfx-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string Library = "bin/gfx-native-bindings";
#else
        public const string Library = "bin/gfx-native-bindings";
#endif

        #endregion

        #region DLL Imports

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_device", CallingConvention = CallingConvention.Cdecl)]
        public static extern Instance CreateInstance(InstanceCreateInfo info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_device", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DestroyInstance(Instance instance);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern SwapChain CreateSwapchain(Instance device, SwapchainCreateInfo info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroySwapchain(SwapChain swapchain);

        [DllImport(Library, EntryPoint = "cobalt_vkb_get_swapchain_image_view", CallingConvention = CallingConvention.Cdecl)]
        public static extern ImageView GetSwapChainImageView(SwapChain swapchain, uint index);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_renderpass", CallingConvention = CallingConvention.Cdecl)]
        private static extern RenderPass CreateRenderPassImpl(Instance device, RenderPassCreateInfoImpl info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyRenderPass(Instance device, RenderPass renderpass);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern CommandBuffer CreateCommandBuffer(Instance device, CommandBufferCreateInfo info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyCommandBuffer(Instance device, CommandBuffer buffer, uint index);

        [DllImport(Library, EntryPoint = "cobalt_vkb_begin_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BeginCommandBuffer(Instance device, CommandBuffer buffer, uint index);

        [DllImport(Library, EntryPoint = "cobalt_vkb_command_begin_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BeginRenderPass(Instance device, CommandBuffer buffer, uint index, RenderPass pass, Framebuffer framebuffer);

        [DllImport(Library, EntryPoint = "cobalt_vkb_command_bind_pipeline", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BindPipeline(CommandBuffer buffer, uint bindpoint, uint index, Shader shader);

        [DllImport(Library, EntryPoint = "cobalt_vkb_command_draw", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Draw(Instance device, CommandBuffer buffer, uint index, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);

        [DllImport(Library, EntryPoint = "cobalt_vkb_command_end_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool EndRenderPass(Instance device, CommandBuffer buffer, uint index);

        [DllImport(Library, EntryPoint = "cobalt_vkb_commandbuffer_end", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool EndCommandBuffer(Instance device, CommandBuffer buffer, uint index);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Buffer CreateBuffer(Instance device, BufferCreateInfo info, BufferMemoryCreateInfo memoryInfo);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyBuffer(Instance device, Buffer buffer);

        [DllImport(Library, EntryPoint = "cobalt_vkb_map_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MapBuffer(Instance device, Buffer buffer);

        [DllImport(Library, EntryPoint = "cobalt_vkb_unmap_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnmapBuffer(Instance device, Buffer buffer);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_shadermodule", CallingConvention = CallingConvention.Cdecl)]
        public static extern ShaderModule CreateShaderModule(Instance device, ShaderModuleCreateInfo info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_shadermodule", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyShaderModule(Instance device, ShaderModule module);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_shader", CallingConvention = CallingConvention.Cdecl)]
        private static extern Shader CreateShaderImpl(Instance device, ShaderCreateInfoImpl info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_shader", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyShader(Instance device, Shader shader);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern Image CreateImage(Instance device, ImageCreateInfo info, ImageMemoryCreateInfo memoryInfo, string name, uint frame);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyImage(Instance device, Image image);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_imageview", CallingConvention = CallingConvention.Cdecl)]
        public static extern ImageView CreateImageView(Instance device, ImageViewCreateInfo info, string name, uint frame);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_imageview", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyImageView(Instance device, ImageView imageView);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_sampler", CallingConvention = CallingConvention.Cdecl)]
        public static extern Sampler CreateSampler(Instance device, SamplerCreateInfo info, string name);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_sampler", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroySampler(Instance device, Sampler sampler);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_framebuffer", CallingConvention = CallingConvention.Cdecl)]
        private static extern Framebuffer CreateFramebufferImpl(Instance device, FramebufferCreateInfoImpl info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_framebuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyFramebuffer(Instance device, Framebuffer imageView);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_semaphore", CallingConvention = CallingConvention.Cdecl)]
        public static extern Semaphore CreateSemaphore(Instance device, SemaphoreCreateInfo info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_acquire_next_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint AcquireNextImage(Instance device, SwapChain swapchain, ulong timeout, Semaphore semaphore);

        [DllImport(Library, EntryPoint = "cobalt_vkb_acquire_next_image_fenced", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint AcquireNextImage(Instance device, SwapChain swapchain, ulong timeout, Semaphore semaphore, Fence fence);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_semaphore", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroySemaphore(Instance device, Semaphore sempahore);

        [DllImport(Library, EntryPoint = "cobalt_vkb_submit_queue", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SubmitQueueImpl(Instance device, SubmitInfoImpl info, Fence fence);

        [DllImport(Library, EntryPoint = "cobalt_vkb_queue_present", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool PresentQueueImpl(Instance device, PresentInfoImpl info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_fence", CallingConvention = CallingConvention.Cdecl)]
        public static extern Fence CreateFence(Instance device, FenceCreateInfo info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_fence", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyFence(Instance device, Fence fence);

        [DllImport(Library, EntryPoint = "cobalt_vkb_wait_for_fences", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool WaitForFences(Instance device, uint count, Fence[] fences, bool waitAll, ulong timeout);

        [DllImport(Library, EntryPoint = "cobalt_vkb_reset_fences", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ResetFences(Instance device, uint count, Fence[] fences);

        [DllImport(Library, EntryPoint = "cobalt_vkb_copy_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CopyBuffer(Instance device, CommandBuffer buffer, uint index, 
            Buffer srcBuffer, Buffer dstBuffer, uint regionCount, BufferCopy[] regions);

        [DllImport(Library, EntryPoint = "cobalt_vkb_copy_buffer_to_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CopyBufferToImage(Instance device, CommandBuffer buffer, uint index, 
            Buffer srcBuffer, Image dstImage, uint dstImageLayout, uint regionCount, BufferImageCopy[] regions);

        [DllImport(Library, EntryPoint = "cobalt_vkb_pipeline_barrier", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PipelineBarrier(CommandBuffer buffer, uint index, uint srcStageMask, uint dstStageMask, uint dependencyFlags,
            uint memoryBarrierCount, MemoryBarrier[] memoryBarriers, uint bufferMemoryBarrierCount, BufferMemoryBarrier[] bufferMemoryBarriers,
            uint imageMemoryBarrierCount, ImageMemoryBarrier[] imageMemoryBarriers);

        [DllImport(Library, EntryPoint = "cobalt_vkb_draw_indirect", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DrawIndirect(CommandBuffer buffer, uint index, Buffer indirectBuffer, ulong offset, uint drawCount, uint stride);

        [DllImport(Library, EntryPoint = "cobalt_vkb_draw_indexed_indirect", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DrawIndexedIndirect(CommandBuffer buffer, uint index, Buffer indirectBuffer, ulong offset, 
            uint drawCount, uint stride);

        [DllImport(Library, EntryPoint = "cobalt_vkb_bind_descriptor_sets", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BindDescriptorSets(CommandBuffer buffer, uint index,
            uint pipelineBindPoint, Shader pipeline, uint firstSet, uint descriptorSetCount, DescriptorSet[] sets, uint dynamicOffsetCount, uint[] dynamicOffsets);

        [DllImport(Library, EntryPoint = "cobalt_vkb_command_bind_vertex_buffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BindVertexBuffers(CommandBuffer buffer, uint index, uint firstBinding, uint bindingCount, 
            Buffer[] buffers, ulong[] offsets);

        [DllImport(Library, EntryPoint = "cobalt_vkb_command_bind_index_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BindIndexBuffer(CommandBuffer buffer, uint index, Buffer indexBuffer, ulong offset, uint indexType);

        [DllImport(Library, EntryPoint = "cobalt_vkb_allocate_descriptors",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern DescriptorSet AllocateDescriptors(Shader shader);

        [DllImport(Library, EntryPoint = "cobalt_vkb_create_event",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern Event CreateEvent(Instance device, EventCreateInfo info);

        [DllImport(Library, EntryPoint = "cobalt_vkb_destroy_descriptors",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyDescriptors(Shader shader, DescriptorSet sets);

        [DllImport(Library, EntryPoint = "cobalt_vkb_write_descriptors",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void WriteDescriptorsImpl(Instance device, ulong count, DescriptorWriteInfoImpl[] infos);

        public static Shader CreateShader(Instance device, ShaderCreateInfo info)
        {
            unsafe
            {
                ShaderCreateInfoImpl infoImpl = new ShaderCreateInfoImpl();
                infoImpl.pass = info.pass;
                infoImpl.computeModulePath = info.computeModulePath;
                infoImpl.fragmentModulePath = info.fragmentModulePath;
                infoImpl.geometryModulePath = info.geometryModulePath;
                infoImpl.subPassIndex = info.subPassIndex;
                infoImpl.tesselationControlModulePath = info.tesselationControlModulePath;
                infoImpl.tesselationEvalModulePath = info.tesselationEvalModulePath;
                infoImpl.vertexModulePath = info.vertexModulePath;

                ShaderLayoutCreateInfoImpl layoutImpl = new ShaderLayoutCreateInfoImpl();
                layoutImpl.setCount = info.layoutInfo.setCount;

                ShaderLayoutSetCreateInfoImpl* setImpls =
                    stackalloc ShaderLayoutSetCreateInfoImpl[(int)info.layoutInfo.setCount];
                for (int i = 0; i < info.layoutInfo.setCount; i++)
                {
                    setImpls[i] = new ShaderLayoutSetCreateInfoImpl();
                    ShaderLayoutSetCreateInfo set = info.layoutInfo.setInfos[i];

                    setImpls[i].bindingCount = set.bindingCount;

                    ShaderLayoutBindingCreateInfo*
                        bindings = stackalloc ShaderLayoutBindingCreateInfo[(int)set.bindingCount];
                    for (int j = 0; j < set.bindingCount; j++)
                    {
                        bindings[j] = set.bindingInfos[j];
                    }

                    setImpls[i].bindingInfos = bindings;
                }

                layoutImpl.setInfos = setImpls;

                infoImpl.layoutInfo = layoutImpl;

                return CreateShaderImpl(device, infoImpl);
            }
        }

        public static void WriteDescriptors(Instance device, ulong count, DescriptorWriteInfo[] infos)
        {
            unsafe
            {
                DescriptorWriteInfoImpl[] infoImpls = new DescriptorWriteInfoImpl[infos.Length];
                uint infoIdx = 0;
                foreach (DescriptorWriteInfo info in infos)
                {
                    DescriptorWriteInfoImpl implInfo = new DescriptorWriteInfoImpl();
                    implInfo.set = info.set;
                    implInfo.count = info.count;
                    implInfo.type = info.type;
                    implInfo.binding = info.binding;
                    implInfo.element = info.element;

                    if (info.infos != null)
                    {
                        TypedWriteInfo* writeInfos = stackalloc TypedWriteInfo[info.infos.Length];
                        uint idx = 0;
                        foreach (TypedWriteInfo writeInfo in info.infos)
                        {
                            writeInfos[idx++] = writeInfo;
                        }

                        implInfo.infos = writeInfos;
                    }

                    implInfo.sets = info.sets;

                    infoImpls[infoIdx++] = implInfo;
                }

                WriteDescriptorsImpl(device, count, infoImpls);
            }
        }

        #endregion

        public static bool PresentQueue(Instance device, PresentInfo info)
        {
            PresentInfoImpl infoImpl = new PresentInfoImpl
            {
                swapchainCount = info.swapchainCount, waitSemaphoreCount = info.waitSemaphoreCount
            };

            unsafe
            {
                if (info.swapchains != null)
                {
                    SwapChain* swapchains = stackalloc SwapChain[info.swapchains.Length];
                    for (int i = 0; i < info.swapchains.Length; i++)
                    {
                        swapchains[i] = info.swapchains[i];
                    }

                    infoImpl.swapchains = swapchains;
                }

                if (info.waitSemaphores != null)
                {
                    Semaphore* waitSemaphores = stackalloc Semaphore[info.waitSemaphores.Length];
                    for (int i = 0; i < info.waitSemaphores.Length; i++)
                    {
                        waitSemaphores[i] = info.waitSemaphores[i];
                    }

                    infoImpl.waitSemaphores = waitSemaphores;
                }

                if (info.imageIndices != null)
                {
                    uint* imageIndices = stackalloc uint[info.imageIndices.Length];
                    for (int i = 0; i < info.imageIndices.Length; i++)
                    {
                        imageIndices[i] = info.imageIndices[i];
                    }

                    infoImpl.imageIndices = imageIndices;
                }

                return PresentQueueImpl(device, infoImpl);
            }
        }

        public static bool SubmitQueue(Instance device, SubmitInfo info, Fence fence)
        {
            SubmitInfoImpl infoImpl = new SubmitInfoImpl
            {
                commandBufferCount = info.commandBufferCount,
                signalSemaphoreCount = info.signalSemaphoreCount,
                waitSemaphoreCount = info.waitSemaphoreCount,
            };

            unsafe
            {
                if (info.waitSemaphores != null)
                {
                    Semaphore* waitSemaphores = stackalloc Semaphore[info.waitSemaphores.Length];
                    for (int i = 0; i < info.waitSemaphores.Length; i++)
                    {
                        waitSemaphores[i] = info.waitSemaphores[i];
                    }

                    infoImpl.waitSemaphores = waitSemaphores;
                }

                if (info.signalSemaphores != null)
                {
                    Semaphore* signalSemaphores = stackalloc Semaphore[info.signalSemaphores.Length];
                    for (int i = 0; i < info.signalSemaphores.Length; i++)
                    {
                        signalSemaphores[i] = info.signalSemaphores[i];
                    }

                    infoImpl.signalSemaphores = signalSemaphores;
                }

                if (info.waitDstStageMask != null)
                {
                    uint* waitDstStageMask = stackalloc uint[info.waitDstStageMask.Length];
                    for (int i = 0; i < info.waitDstStageMask.Length; i++)
                    {
                        waitDstStageMask[i] = info.waitDstStageMask[i];
                    }

                    infoImpl.waitDstStageMask = waitDstStageMask;
                }

                if (info.commandBuffer != null)
                {
                    IndexedCommandBuffersImpl* implBuffers =
                        stackalloc IndexedCommandBuffersImpl[info.commandBuffer.Length];

                    int idx = 0;
                    foreach (IndexedCommandBuffers b in info.commandBuffer)
                    {
                        IndexedCommandBuffersImpl* i = stackalloc IndexedCommandBuffersImpl[1];
                        i->amount = b.amount;
                        i->commandbuffer = b.commandbuffer;

                        if (b.bufferIndices != null)
                        {
                            uint* indices = stackalloc uint[b.bufferIndices.Length];
                            for (int j = 0; j < b.bufferIndices.Length; j++)
                            {
                                indices[j] = b.bufferIndices[j];
                            }

                            i->bufferIndices = indices;
                        }

                        implBuffers[idx++] = *i;
                    }

                    infoImpl.commandBuffer = implBuffers;
                }

                return SubmitQueueImpl(device, infoImpl, fence);
            }
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
                fixed (ImageView* attachments = info.attachments)
                {
                    infoImpl.attachments = attachments;
                    return CreateFramebufferImpl(device, infoImpl);
                }
            }
        }


        public static RenderPass CreateRenderPass(Instance device, RenderPassCreateInfo info)
        {
            RenderPassCreateInfoImpl infoImpl = new RenderPassCreateInfoImpl
            {
                attachmentCount = info.attachmentCount,
                subpassCount = info.subpassCount,
                dependencyCount = info.dependencyCount
            };

            unsafe
            {
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

                        if (sub.attachments != null)
                        {
                            AttachmentReference* attachments = stackalloc AttachmentReference[sub.attachments.Length];
                            for (int i = 0; i < sub.attachments.Length; i++)
                            {
                                attachments[i] = sub.attachments[i];
                            }

                            newSub.attachments = attachments;
                        }

                        if (sub.colorAttachments != null)
                        {
                            AttachmentReference* colorAttachments =
                                stackalloc AttachmentReference[sub.colorAttachments.Length];
                            for (int i = 0; i < sub.colorAttachments.Length; i++)
                            {
                                colorAttachments[i] = sub.colorAttachments[i];
                            }

                            newSub.colorAttachments = colorAttachments;
                        }

                        if (sub.resolveAttachments != null)
                        {
                            AttachmentReference* resolveAttachments =
                                stackalloc AttachmentReference[sub.resolveAttachments.Length];
                            for (int i = 0; i < sub.resolveAttachments.Length; i++)
                            {
                                resolveAttachments[i] = sub.resolveAttachments[i];
                            }

                            newSub.resolveAttachments = resolveAttachments;
                        }

                        if (sub.depthStencilAttachments != null)
                        {
                            AttachmentReference* depthStencilAttachments =
                                stackalloc AttachmentReference[sub.depthStencilAttachments.Length];
                            for (int i = 0; i < sub.depthStencilAttachments.Length; i++)
                            {
                                depthStencilAttachments[i] = sub.depthStencilAttachments[i];
                            }

                            newSub.depthStencilAttachments = depthStencilAttachments;
                        }

                        if (sub.preserveAttachments != null)
                        {
                            uint* preserveAttachments = stackalloc uint[sub.preserveAttachments.Length];
                            for (int i = 0; i < sub.preserveAttachments.Length; i++)
                            {
                                preserveAttachments[i] = sub.preserveAttachments[i];
                            }

                            newSub.preserveAttachments = preserveAttachments;
                        }

                        subs[idx++] = newSub;
                    }
                }

                if (info.attachments != null)
                {
                    AttachmentDescription* attachmentsDescs = stackalloc AttachmentDescription[info.attachments.Length];
                    for (int i = 0; i < info.attachments.Length; i++)
                    {
                        attachmentsDescs[i] = info.attachments[i];
                    }

                    infoImpl.attachments = attachmentsDescs;
                }

                if (subs != null)
                {
                    SubpassDescriptionImpl* subpasses = stackalloc SubpassDescriptionImpl[subs.Length];
                    for (int i = 0; i < subs.Length; i++)
                    {
                        subpasses[i] = subs[i];
                    }

                    infoImpl.subpasses = subpasses;
                }

                if (info.dependencies != null)
                {
                    SubpassDependency* dependencies = stackalloc SubpassDependency[info.dependencies.Length];
                    for (int i = 0; i < info.dependencies.Length; i++)
                    {
                        dependencies[i] = info.dependencies[i];
                    }

                    infoImpl.dependencies = dependencies;
                }

                return CreateRenderPassImpl(device, infoImpl);
            }
        }
    }
}
