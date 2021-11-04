using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics
{
    public class Swapchain : IDisposable
    {
        internal readonly VK.SwapChain handle;

        public Swapchain(Device device, SwapchainCreateInfo info)
        {
            handle = VK.CreateSwapchain(device.handle, info);
        }

        public void Dispose()
        { 
            VK.DestroySwapchain(handle);
        }
    }
}