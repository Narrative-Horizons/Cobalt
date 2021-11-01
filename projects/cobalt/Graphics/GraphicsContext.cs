﻿using Cobalt.Graphics.VK;
using System;
using Cobalt.Bindings.Vulkan;
using Cobalt.Core;
using Cobalt.Graphics.VK.Enums;
using Cobalt.Math;

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
        public Cobalt.Bindings.Vulkan.VK.Framebuffer[] framebuffers;
        public Cobalt.Bindings.Vulkan.VK.CommandBuffer commandbuffer;
        public Cobalt.Bindings.Vulkan.VK.Semaphore[] imageAvailableSemaphore;
        public Cobalt.Bindings.Vulkan.VK.Semaphore[] renderFinishedSemaphore;
        public Cobalt.Bindings.Vulkan.VK.Fence[] inFlightFences;
        public Cobalt.Bindings.Vulkan.VK.Fence[] imagesInFlight;
        public Cobalt.Bindings.Vulkan.VK.Buffer[] indirectBuffer;
        public Bindings.Vulkan.VK.DescriptorSet[] set;
        public Bindings.Vulkan.VK.Buffer[] uniformBuffer;
        public uint frameCount = 3;

        public struct UniformBufferTest
        {
            public Vector4 color;
        }

        public GraphicsContext(Window window)
        {
            ContextDevice = Device.Create(window);

            SwapchainCreateInfo swapchainInfo = new SwapchainCreateInfo();
            swapchain = Bindings.Vulkan.VK.CreateSwapchain(ContextDevice.handle, swapchainInfo);

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

            SubpassDependency dependency = new SubpassDependency
            {
                srcSubpass = (~0U),
                dstSubpass = 0,
                srcStageMask = (uint) PipelineStageFlagBits.ColorAttachmentOutputBit,
                srcAccessMask = 0,
                dstStageMask = (uint) PipelineStageFlagBits.ColorAttachmentOutputBit,
                dstAccessMask = (uint) AccessFlagBits.ColorAttachmentWriteBit
            };

            RenderPassCreateInfo renderpassInfo = new RenderPassCreateInfo
            {
                subpassCount = 1,
                subpasses = new[] {subpass},
                attachmentCount = 1,
                attachments = new[] {colorAttachment},
                dependencyCount = 1,
                dependencies = new[] {dependency}
            };

            pass = Bindings.Vulkan.VK.CreateRenderPass(ContextDevice.handle, renderpassInfo);

            ShaderLayoutCreateInfo shaderLayout = new ShaderLayoutCreateInfo();
            shaderLayout.setCount = 1;
            shaderLayout.setInfos = new[]
            {
                new ShaderLayoutSetCreateInfo()
                {
                    bindingCount = 1,
                    bindingInfos = new[]
                    {
                        new ShaderLayoutBindingCreateInfo()
                        {
                            bindingIndex = 0,
                            descriptorCount = 1,
                            stageFlags = (uint) ShaderStageFlagBits.FragmentBit,
                            type = (uint) DescriptorType.UniformBuffer
                        }
                    }
                }
            };

            ShaderCreateInfo shaderInfo = new ShaderCreateInfo
            {
                vertexModulePath = "data/shaders/vulkantest/triangle_vert.spv",
                fragmentModulePath = "data/shaders/vulkantest/triangle_frag.spv",
                subPassIndex = 0,
                pass = pass,
                layoutInfo = shaderLayout
            };

            shader = Bindings.Vulkan.VK.CreateShader(ContextDevice.handle, shaderInfo);
            indirectBuffer = new Bindings.Vulkan.VK.Buffer[frameCount];
            framebuffers = new Bindings.Vulkan.VK.Framebuffer[frameCount];
            uniformBuffer = new Bindings.Vulkan.VK.Buffer[frameCount];

            set = new Bindings.Vulkan.VK.DescriptorSet[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                BufferCreateInfo uniformBufferCreateInfo = new BufferCreateInfo();
                uniformBufferCreateInfo.size = 16;
                uniformBufferCreateInfo.usage = (uint) BufferUsageFlagBits.UniformBufferBit;
                uniformBufferCreateInfo.sharingMode = (uint) SharingMode.Exclusive;

                BufferMemoryCreateInfo uniformMemoryInfo = new BufferMemoryCreateInfo();
                uniformMemoryInfo.usage = (uint)MemoryUsage.CpuToGpu;
                uniformMemoryInfo.requiredFlags =
                    (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit);
                uniformMemoryInfo.preferredFlags = 0;

                uniformBuffer[i] =
                    Bindings.Vulkan.VK.CreateBuffer(ContextDevice.handle, uniformBufferCreateInfo, uniformMemoryInfo);

                NativeBuffer<UniformBufferTest> nativeUniformBuffer =
                    new NativeBuffer<UniformBufferTest>(Bindings.Vulkan.VK.MapBuffer(ContextDevice.handle,
                        uniformBuffer[i]));
                UniformBufferTest test = nativeUniformBuffer.Get();
                test.color = new Vector4(1, 0, 1, 1);
                nativeUniformBuffer.Set(test, 0);
                Bindings.Vulkan.VK.UnmapBuffer(ContextDevice.handle, uniformBuffer[i]);

                set[i] = Bindings.Vulkan.VK.AllocateDescriptors(shader);

                DescriptorWriteInfo writeInfo = new DescriptorWriteInfo();
                writeInfo.sets = set[i];
                writeInfo.binding = 0;
                writeInfo.count = 1;
                writeInfo.element = 0;
                writeInfo.type = (uint) DescriptorType.UniformBuffer;

                BufferWriteInfo uniformBufferInfo = new BufferWriteInfo();
                uniformBufferInfo.buffer = uniformBuffer[i];
                uniformBufferInfo.offset = 0;
                uniformBufferInfo.range = 16;

                TypedWriteInfo typedInfo = new TypedWriteInfo();
                typedInfo.buffers = uniformBufferInfo;

                writeInfo.infos = new[] {typedInfo};

                Bindings.Vulkan.VK.WriteDescriptors(ContextDevice.handle, 1, new []{writeInfo});

                BufferCreateInfo indirectInfo = new BufferCreateInfo();
                indirectInfo.usage = (uint)(BufferUsageFlagBits.StorageBufferBit | BufferUsageFlagBits.IndirectBufferBit);
                indirectInfo.sharingMode = (uint)SharingMode.Exclusive;
                indirectInfo.size = 16;

                BufferMemoryCreateInfo indirectMemoryInfo = new BufferMemoryCreateInfo
                {
                    requiredFlags =
                        (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit),
                    preferredFlags = 0,
                    usage = (uint) MemoryUsage.CpuToGpu
                };

                indirectBuffer[i] =
                    Bindings.Vulkan.VK.CreateBuffer(ContextDevice.handle, indirectInfo, indirectMemoryInfo);

                DrawIndirectCommand drawCommand = new DrawIndirectCommand()
                {
                    firstInstance = 0,
                    firstVertex = 0,
                    instanceCount = 1,
                    vertexCount = 3
                };

                NativeBuffer<DrawIndirectCommand> nativeBuffer = new NativeBuffer<DrawIndirectCommand>(Bindings.Vulkan.VK.MapBuffer(ContextDevice.handle, indirectBuffer[i]));
                nativeBuffer.Set(drawCommand);
                Bindings.Vulkan.VK.UnmapBuffer(ContextDevice.handle, indirectBuffer[i]);

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

            CommandBufferCreateInfo bufferInfo = new CommandBufferCreateInfo {amount = frameCount, pool = (uint)CommandBufferPoolType.Graphics, primary = true};
            commandbuffer = Bindings.Vulkan.VK.CreateCommandBuffer(ContextDevice.handle, bufferInfo);

            imageAvailableSemaphore = new Bindings.Vulkan.VK.Semaphore[frameCount];
            renderFinishedSemaphore = new Bindings.Vulkan.VK.Semaphore[frameCount];

            inFlightFences = new Bindings.Vulkan.VK.Fence[frameCount];
            imagesInFlight = new Bindings.Vulkan.VK.Fence[frameCount];


            for (int i = 0; i < frameCount; i++)
            {
                Bindings.Vulkan.VK.BeginCommandBuffer(ContextDevice.handle, commandbuffer, (uint)i);
                Bindings.Vulkan.VK.BeginRenderPass(ContextDevice.handle, commandbuffer, (uint)i, pass, framebuffers[i]);
                Bindings.Vulkan.VK.BindDescriptorSets(ContextDevice.handle, commandbuffer, (uint) i,
                    (uint) PipelineBindPoint.Graphics, shader, 0,
                    1, new[] {set[i]}, 0, null);
                Bindings.Vulkan.VK.BindPipeline(ContextDevice.handle, commandbuffer, (uint)PipelineBindPoint.Graphics, (uint) i, shader);
                Bindings.Vulkan.VK.DrawIndirect(ContextDevice.handle, commandbuffer, (uint) i, indirectBuffer[i], 0, 1, 16);
                Bindings.Vulkan.VK.EndRenderPass(ContextDevice.handle, commandbuffer, (uint) i);
                Bindings.Vulkan.VK.EndCommandBuffer(ContextDevice.handle, commandbuffer, (uint) i);

                SemaphoreCreateInfo semInfo = new SemaphoreCreateInfo();
                imageAvailableSemaphore[i] = Bindings.Vulkan.VK.CreateSemaphore(ContextDevice.handle, semInfo);
                renderFinishedSemaphore[i] = Bindings.Vulkan.VK.CreateSemaphore(ContextDevice.handle, semInfo);

                FenceCreateInfo fenceInfo = new FenceCreateInfo {flags = (uint) FenceCreateFlagBits.SignaledBit};

                inFlightFences[i] = Bindings.Vulkan.VK.CreateFence(ContextDevice.handle, fenceInfo);
                imagesInFlight[i] = new Bindings.Vulkan.VK.Fence(IntPtr.Zero);
            }
        }

        public uint currentFrame;

        public void Render()
        {
            Bindings.Vulkan.VK.WaitForFences(ContextDevice.handle, 1, new []{inFlightFences[currentFrame]}, true, ulong.MaxValue);

            uint imageIndex =
                Bindings.Vulkan.VK.AcquireNextImage(ContextDevice.handle, swapchain, ulong.MaxValue, imageAvailableSemaphore[currentFrame]);

            if (imagesInFlight[imageIndex].handle != IntPtr.Zero)
            {
                Bindings.Vulkan.VK.WaitForFences(ContextDevice.handle, 1, new[] {imagesInFlight[imageIndex]}, true,
                    ulong.MaxValue);
            }
            imagesInFlight[imageIndex] = inFlightFences[currentFrame];

            SubmitInfo submitInfo = new SubmitInfo
            {
                waitSemaphoreCount = 1,
                waitSemaphores = new[] {imageAvailableSemaphore[currentFrame]},
                signalSemaphoreCount = 1,
                signalSemaphores = new[] {renderFinishedSemaphore[currentFrame]}
            };

            submitInfo.waitSemaphoreCount = 1;
            submitInfo.waitDstStageMask = new[] {(uint)PipelineStageFlagBits.ColorAttachmentOutputBit};

            submitInfo.commandBufferCount = 1;
            IndexedCommandBuffers indexedBuffer = new IndexedCommandBuffers
            {
                amount = 1, bufferIndices = new[] { currentFrame }, commandbuffer = commandbuffer
            };

            submitInfo.commandBuffer = new []{ indexedBuffer};

            Bindings.Vulkan.VK.ResetFences(ContextDevice.handle, 1, new[]{inFlightFences[currentFrame]});

            Bindings.Vulkan.VK.SubmitQueue(ContextDevice.handle, submitInfo, inFlightFences[currentFrame]);

            PresentInfo presentInfo = new PresentInfo
            {
                waitSemaphoreCount = 1,
                waitSemaphores = new[] {renderFinishedSemaphore[currentFrame]},
                swapchainCount = 1,
                swapchains = new[] {swapchain},
                imageIndices = new[] {imageIndex}
            };

            Bindings.Vulkan.VK.PresentQueue(ContextDevice.handle, presentInfo);

            currentFrame = (currentFrame + 1) % frameCount;
        }

        public void Dispose()
        {
            ContextDevice.Dispose();
        }
    }
}
