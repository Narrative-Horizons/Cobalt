﻿using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics.VK
{
    public class Framebuffer : IDisposable
    {
        internal readonly Bindings.Vulkan.VK.Framebuffer handle;
        internal readonly Bindings.Vulkan.VK.Device device;

        public Framebuffer(Bindings.Vulkan.VK.Device device, FramebufferCreateInfo info)
        {
            this.device = device;
            handle = Bindings.Vulkan.VK.CreateFramebuffer(device, info);
        }

        public void Dispose()
        {
            Bindings.Vulkan.VK.DestroyFramebuffer(device, handle);
        }
    }
}