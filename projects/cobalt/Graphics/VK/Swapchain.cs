using System;

namespace Cobalt.Graphics.VK
{
    internal class Swapchain : IDisposable
    {
        internal readonly IntPtr handle;
        public Swapchain(Device device, Bindings.Vulkan.VK.SwapchainCreateInfo info)
        {
            handle = Bindings.Vulkan.VK.CreateSwapchain(device.handle, info);
        }

        public void Dispose()
        {
            Bindings.Vulkan.VK.DestroySwapchain(handle);
        }
    }
}