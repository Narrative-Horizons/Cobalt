using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics
{
    public class Swapchain : IDisposable
    {
        internal readonly VK.SwapChain handle;
        internal uint Width { get; private set; }
        internal uint Height { get; private set; }

        public Swapchain(Device device, SwapchainCreateInfo info)
        {
            handle = VK.CreateSwapchain(device.handle, info);

            Width = info.width;
            Height = info.height;
        }

        public void Dispose()
        { 
            VK.DestroySwapchain(handle);
        }
    }
}