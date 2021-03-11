using Cobalt.Graphics.API;
using System;

namespace Cobalt.Graphics.GL
{
    internal class RenderSurface : IRenderSurface
    {
        public Swapchain SwapChain { get; private set; }

        public Window Window { get; private set; }
        public RenderSurface(Window window)
        {
            Window = window;
        }

        public ISwapchain CreateSwapchain(ISwapchain.CreateInfo info)
        {
            if (SwapChain != null)
                throw new InvalidOperationException("Swapchain already exists");

            SwapChain = new Swapchain(Window, info);

            return SwapChain;
        }

        public void Dispose()
        {
            SwapChain?.Dispose();
        }

        public ISwapchain GetSwapchain()
        {
            return SwapChain;
        }
    }
}
