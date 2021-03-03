using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics
{
    public interface IRenderSurface : IDisposable
    {
        public ISwapchain CreateSwapchain(ISwapchain.CreateInfo info);
        public ISwapchain GetSwapchain();
    }
}
