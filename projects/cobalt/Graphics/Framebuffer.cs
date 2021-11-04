using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics
{
    public class Framebuffer : IDisposable
    {
        internal readonly VK.Framebuffer handle;
        internal readonly VK.Instance device;

        public Framebuffer(VK.Instance device, FramebufferCreateInfo info)
        {
            this.device = device;
            handle = VK.CreateFramebuffer(device, info);
        }

        public void Dispose()
        {
            VK.DestroyFramebuffer(device, handle);
        }
    }
}
