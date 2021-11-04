using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics
{
    public class ImageView : IDisposable
    {
        internal readonly VK.ImageView handle;
        internal readonly VK.Instance device;

        internal ImageView(VK.Instance device, ImageViewCreateInfo info, string name, uint frame)
        {
            this.device = device;
            handle = VK.CreateImageView(device, info, name, frame);
        }

        public void Dispose()
        {
            VK.DestroyImageView(device, handle);
        }
    }
}
