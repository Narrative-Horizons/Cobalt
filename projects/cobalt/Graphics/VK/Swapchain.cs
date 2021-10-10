using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics.VK
{
    internal class Swapchain : IDisposable
    {
        internal readonly Bindings.Vulkan.VK.SwapChain handle;
        public Swapchain(Bindings.Vulkan.VK.Device device, SwapchainCreateInfo info)
        {
            handle = Bindings.Vulkan.VK.CreateSwapchain(device, info);
        }

        public void Dispose()
        {
            Bindings.Vulkan.VK.DestroySwapchain(handle);
        }
    }
}