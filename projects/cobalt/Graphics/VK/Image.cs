using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics.VK
{
    public class Image : IDisposable
    {
        internal readonly Bindings.Vulkan.VK.Image handle;
        internal readonly Bindings.Vulkan.VK.Device device;

        internal Image(Bindings.Vulkan.VK.Device device, ImageCreateInfo info)
        {
            this.device = device;
            handle = Bindings.Vulkan.VK.CreateImage(device, info);
        }

        public void Dispose()
        {
            Bindings.Vulkan.VK.DestroyImage(device, handle);
        }
    }
}
