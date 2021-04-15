using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;
using Cobalt.Graphics.API;
using System;

namespace Cobalt.Graphics.GL
{
    internal class ImageView : IImageView
    {
        public uint Handle { get; private set; }
        public Image Image{ get; private set; }

        public IImageView.CreateInfo Info { get; private set; }

        public ImageView(IImageView.CreateInfo createInfo, Image image)
        {
            Handle = OpenGL.GenTextures();
            Image = image;
            Info = createInfo;

            int target = ToTarget(createInfo.ViewType);
            int format = ToSizedFormat(createInfo.Format);

            OpenGL.TextureView(Handle, (uint)target, Image.Handle, (EPixelInternalFormat)format, (uint)createInfo.BaseMipLevel,
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
                    return (int)ETextureTarget.Texture1D;
                case EImageViewType.ViewType2D:
                    return (int)ETextureTarget.Texture2D;
                case EImageViewType.ViewType3D:
                    return (int)ETextureTarget.Texture3D;
                case EImageViewType.ViewTypeCube:
                    return (int)ETextureTarget.TextureCubeMap;
                case EImageViewType.ViewType1DArray:
                    return (int)ETextureTarget.Texture1DArray;
                case EImageViewType.ViewType2DArray:
                    return (int)ETextureTarget.Texture2DArray;
                case EImageViewType.ViewTypeCubeArray:
                    return (int)ETextureTarget.TextureCubeMapArray;
                default:
                    break;
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
                    return (int)EPixelInternalFormat.Srgb8Alpha8;
                case EDataFormat.R8G8B8A8:
                    return (int)EPixelInternalFormat.Rgba8;
                case EDataFormat.R32G32_SFLOAT:
                    break;
                case EDataFormat.R32G32B32_SFLOAT:
                    break;
                case EDataFormat.R32G32B32A32_SFLOAT:
                    return (int)EPixelInternalFormat.Rgba32f;
                case EDataFormat.D24_SFLOAT_S8_UINT:
                    return (int)EPixelInternalFormat.Depth24Stencil8;
            }

            throw new NotSupportedException("Unsupported format for image data.");
        }
    }
}
