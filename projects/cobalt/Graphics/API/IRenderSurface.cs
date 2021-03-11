using System;

namespace Cobalt.Graphics.API
{
    public interface IRenderSurface : IDisposable
    {
        public ISwapchain CreateSwapchain(ISwapchain.CreateInfo info);
        public ISwapchain GetSwapchain();
    }
}
