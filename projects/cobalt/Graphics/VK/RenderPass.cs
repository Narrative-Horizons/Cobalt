using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.VK
{
    public class RenderPass
    {
        internal readonly IntPtr handle;
        private readonly Device _device;
        public RenderPass(Device device, Bindings.Vulkan.VK.RenderPassCreateInfo info)
        {
            handle = Bindings.Vulkan.VK.CreateRenderPass(device.handle, info);
            _device = device;
        }

        public void Dispose()
        {
            Bindings.Vulkan.VK.DestroyRenderPass(_device.handle, handle);
        }
    }
}
