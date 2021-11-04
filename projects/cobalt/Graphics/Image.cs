using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics
{
    public class Image : IDisposable
    {
        internal readonly VK.Image handle;
        internal readonly VK.Instance device;

        internal Image(VK.Instance device, ImageCreateInfo info, string name, uint frame)
        {
            this.device = device;
            handle = VK.CreateImage(device, info, name, frame);
        }

        public void Dispose()
        {
            VK.DestroyImage(device, handle);
        }
    }
}
