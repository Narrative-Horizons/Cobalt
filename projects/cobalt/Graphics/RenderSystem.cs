using System;
using Cobalt.Bindings.Vulkan;
using Cobalt.Entities;
using Cobalt.Graphics.Enums;
using Cobalt.Graphics.Passes;

namespace Cobalt.Graphics
{
    public class RenderSystem
    {
        private readonly GraphicsContext _context;
        private Registry _registry;

        private readonly VK.CommandBuffer _graphicsBuffer;
        //private readonly VK.CommandBuffer _computeBuffer;
        private readonly CommandList[] _computeList;
        private readonly CommandList[] _graphicsList;

        private readonly VK.Semaphore[] _imageAvailableSemaphore;
        private readonly VK.Semaphore[] _renderFinishedSemaphore;
        private readonly VK.Fence[] _inFlightFences;
        private readonly VK.Fence[] _imagesInFlight;

        private uint _currentFrame;

        private readonly uint _framesInFlight = 3;

        public RenderSystem(Registry registry, GraphicsContext context)
        {
            _context = context;
            _registry = registry;

            CommandBufferCreateInfo commandBufferInfo = new CommandBufferCreateInfo
            {
                amount = _framesInFlight, 
                pool = (uint) CommandBufferPoolType.Graphics, 
                primary = true
            };

            _graphicsBuffer = VK.CreateCommandBuffer(context.ContextDevice.handle, commandBufferInfo);

            commandBufferInfo.pool = (uint) CommandBufferPoolType.Compute;

            //_computeBuffer = VK.CreateCommandBuffer(context.ContextDevice.handle, commandBufferInfo);

            _computeList = new CommandList[_framesInFlight];
            _graphicsList = new CommandList[_framesInFlight];

            for (uint i = 0; i < _framesInFlight; i++)
            {
                //_computeList[i] = new CommandList(_computeBuffer, i);
                _graphicsList[i] = new CommandList(_graphicsBuffer, i);
            }

            
            UberPBRPass uberPass = new UberPBRPass(_context.ContextDevice, _framesInFlight);

            SamplerCreateInfo resolveSamplerInfo = new SamplerCreateInfo();
            VK.Sampler resolveSampler =
                VK.CreateSampler(context.ContextDevice.handle, resolveSamplerInfo, "resolvePassSampler");
            SwapchainResolvePass resolvePass = new SwapchainResolvePass(_context.ContextDevice, context.Swapchain, _framesInFlight, uberPass.GetOutputImages(), resolveSampler);

            _imageAvailableSemaphore = new VK.Semaphore[_framesInFlight];
            _renderFinishedSemaphore = new VK.Semaphore[_framesInFlight];

            _inFlightFences = new VK.Fence[_framesInFlight];
            _imagesInFlight = new VK.Fence[_framesInFlight];

            for (uint i = 0; i < _framesInFlight; i++)
            {
                _graphicsList[i].Begin();
                uberPass.Execute(_graphicsList[i], i);
                resolvePass.Execute(_graphicsList[i], i);
                _graphicsList[i].End();

                SemaphoreCreateInfo semInfo = new SemaphoreCreateInfo();
                _imageAvailableSemaphore[i] = VK.CreateSemaphore(context.ContextDevice.handle, semInfo);
                _renderFinishedSemaphore[i] = VK.CreateSemaphore(context.ContextDevice.handle, semInfo);

                FenceCreateInfo fenceInfo = new FenceCreateInfo { flags = (uint)FenceCreateFlagBits.SignaledBit };

                _inFlightFences[i] = VK.CreateFence(context.ContextDevice.handle, fenceInfo);
                _imagesInFlight[i] = new VK.Fence(IntPtr.Zero);
            }

            int jonathan = 0;
        }

        public void PreRender()
        {

        }

        public void Render()
        {
            _context.Render();

            VK.WaitForFences(_context.ContextDevice.handle, 1, new[] { _inFlightFences[_currentFrame] }, true, ulong.MaxValue);

            uint imageIndex =
                VK.AcquireNextImage(_context.ContextDevice.handle, _context.Swapchain.handle, ulong.MaxValue, _imageAvailableSemaphore[_currentFrame]);

            if (_imagesInFlight[imageIndex].handle != IntPtr.Zero)
            {
                VK.WaitForFences(_context.ContextDevice.handle, 1, new[] { _imagesInFlight[imageIndex] }, true,
                    ulong.MaxValue);
            }
            _imagesInFlight[imageIndex] = _inFlightFences[_currentFrame];

            SubmitInfo submitInfo = new SubmitInfo
            {
                waitSemaphoreCount = 1,
                waitSemaphores = new[] { _imageAvailableSemaphore[_currentFrame] },
                signalSemaphoreCount = 1,
                signalSemaphores = new[] { _renderFinishedSemaphore[_currentFrame] }
            };

            submitInfo.waitSemaphoreCount = 1;
            submitInfo.waitDstStageMask = new[] { (uint)PipelineStageFlagBits.ColorAttachmentOutputBit };

            submitInfo.commandBufferCount = 1;
            IndexedCommandBuffers indexedBuffer = new IndexedCommandBuffers
            {
                amount = 1,
                bufferIndices = new[] { _currentFrame },
                commandbuffer = _graphicsList[_currentFrame]._handle
            };

            submitInfo.commandBuffer = new[] { indexedBuffer };

            VK.ResetFences(_context.ContextDevice.handle, 1, new[] { _inFlightFences[_currentFrame] });

            VK.SubmitQueue(_context.ContextDevice.handle, submitInfo, _inFlightFences[_currentFrame]);

            PresentInfo presentInfo = new PresentInfo
            {
                waitSemaphoreCount = 1,
                waitSemaphores = new[] { _renderFinishedSemaphore[_currentFrame] },
                swapchainCount = 1,
                swapchains = new[]
                {
                    _context.Swapchain.handle
                },
                imageIndices = new[] { imageIndex }
            };

            VK.PresentQueue(_context.ContextDevice.handle, presentInfo);

            _currentFrame = (_currentFrame + 1) % _framesInFlight;
        }

        public void PostRender()
        {
            
        }
    }
}
