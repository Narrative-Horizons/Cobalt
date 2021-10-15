﻿using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics.VK
{
    public class ImageView : IDisposable
    {
        internal readonly Bindings.Vulkan.VK.ImageView handle;
        internal readonly Bindings.Vulkan.VK.Device device;
        internal ImageView(Bindings.Vulkan.VK.Device device, ImageViewCreateInfo info)
        {
            this.device = device;
            handle = Bindings.Vulkan.VK.CreateImageView(device, info);
        }

        public void Dispose()
        {
            Bindings.Vulkan.VK.DestroyImageView(device, handle);
        }
    }
}
