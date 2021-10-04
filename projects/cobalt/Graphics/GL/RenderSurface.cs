using Cobalt.Graphics.API;
using System;

namespace Cobalt.Graphics.GL
{
    internal class RenderSurface : IRenderSurface
    {
        public GLSwapchain SwapChain { get; private set; }

        public Window Window { get; private set; }
        public RenderSurface(Window window)
        {
            Window = window;
        }

        public ISwapchain CreateSwapchain(ISwapchain.CreateInfo info)
        {
            if (SwapChain != null)
                throw new InvalidOperationException("GLSwapchain already exists");

            SwapChain = new GLSwapchain(Window, info);

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
