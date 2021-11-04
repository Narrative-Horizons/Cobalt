using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics
{
    public class GraphicsContext : IDisposable
    {
        #region Properties
        public Device ContextDevice { get; private set; }
        public Swapchain Swapchain { get; private set; }
        #endregion

        public GraphicsContext(Window window)
        {
            ContextDevice = Device.Create(window);

            SwapchainCreateInfo swapchainInfo = new SwapchainCreateInfo();
            Swapchain = new Swapchain(ContextDevice, swapchainInfo);
        }

        public void Render()
        {

        }

        public void Dispose()
        {
            Swapchain.Dispose();
            ContextDevice.Dispose();
        }
    }
}
