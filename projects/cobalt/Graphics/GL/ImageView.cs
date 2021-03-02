using OpenGL = Cobalt.Bindings.GL.GL;

using System;
using System.Collections.Generic;
using System.Text;
using Cobalt.Bindings.GL;

namespace Cobalt.Graphics.GL
{
    internal class ImageView : IImageView
    {
        public uint Handle { get; private set; }

        public ImageView(IImageView.CreateInfo createInfo, uint imageHandle)
        {
            Handle = OpenGL.GenTextures();

            int target = ToTarget(createInfo.ViewType);
            int format = ToSizedFormat(createInfo.Format);

            OpenGL.TextureView(Handle, (uint)target, imageHandle, (EInternalFormat)format, (uint)createInfo.BaseMipLevel,
                (uint)createInfo.MipLevelCount, (uint)createInfo.BaseArrayLayer, (uint)createInfo.ArrayLayerCount);
        }

        public void Dispose()
        {
            OpenGL.DeleteTextures(Handle);
        }

        private static int ToTarget(EImageViewType target)
        {
            switch (target)
            {
                case EImageViewType.ViewType1D:
                    return 0x0DE0;
                case EImageViewType.ViewType2D:
                    return 0x0DE1;
                case EImageViewType.ViewType3D:
                    return 0x806F;
                case EImageViewType.ViewTypeCube:
                    return 0x8513;
                case EImageViewType.ViewType1DArray:
                    return 0x8C18;
                case EImageViewType.ViewType2DArray:
                    return 0x8C1A;
                case EImageViewType.ViewTypeCubeArray:
                    return 0x9009;
            }

            return -1;
        }

        private static int ToSizedFormat(EDataFormat format)
        {
            switch (format)
            {
                case EDataFormat.Unknown:
                    break;
                case EDataFormat.BGRA8_SRGB:
                    break;
                case EDataFormat.R8G8B8A8_SRGB:
                    return 0x8C42;
                case EDataFormat.R8G8B8A8:
                    return 0x8058;
                case EDataFormat.R32G32_SFLOAT:
                    break;
                case EDataFormat.R32G32B32_SFLOAT:
                    break;
                case EDataFormat.R32G32B32A32_SFLOAT:
                    return 0x8814;
            }

            throw new NotSupportedException("Unsupported format for image data.");
        }
    }
}
