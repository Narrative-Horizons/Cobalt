using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics.VK
{
    public class ImageView : IDisposable
    {
        internal readonly Bindings.Vulkan.VK.ImageView handle;
        internal readonly Bindings.Vulkan.VK.Instance device;

        internal ImageView(Bindings.Vulkan.VK.Instance device, ImageViewCreateInfo info, string name, uint frame)
        {
            this.device = device;
            handle = Bindings.Vulkan.VK.CreateImageView(device, info, name, frame);
        }

        public void Dispose()
        {
            Bindings.Vulkan.VK.DestroyImageView(device, handle);
        }
    }
}
