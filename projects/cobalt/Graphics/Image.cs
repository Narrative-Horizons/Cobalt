using System;
using Cobalt.Bindings.Vulkan;
using Cobalt.Graphics.Enums;

namespace Cobalt.Graphics
{
    public class Image : IDisposable
    {
        internal readonly VK.Image handle;
        internal readonly Device device;
        
        private readonly string _name;

        internal Format imageFormat;

        internal Image(Device device, ImageCreateInfo info, ImageMemoryCreateInfo memoryInfo, string name, uint frame)
        {
            this._name = name;
            this.device = device;
            handle = VK.CreateImage(device.handle, info, memoryInfo, name, frame);
        }

        public ImageView CreateImageView(Format format, ImageAspectFlagBits aspectFlags)
        {
            ImageViewCreateInfo createInfo = new ImageViewCreateInfo
            {
                image = handle,
                format = (uint) format,
                layerCount = 1,
                levelCount = 1,
                baseArrayLayer = 0,
                baseMipLevel = 0,
                viewType = (uint) ImageViewType.Type2D,
                aspectMask = (uint) aspectFlags
            };

            imageFormat = format;

            return new ImageView(device.handle, createInfo, _name + "_view_" + format, 0);
        }

        public void Dispose()
        {
            VK.DestroyImage(device.handle, handle);
        }
    }
}
