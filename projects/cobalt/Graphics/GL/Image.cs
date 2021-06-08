using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;
using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class Image : IImage
    {
        public List<ImageView> Views { get; private set; } = new List<ImageView>();
        public uint Handle { get; private set; }
        public EDataFormat Format { get; private set; }
        public EImageType Type { get; private set; }
        public int LayerCount { get; private set; }
        public int Levels { get; private set; }

        public Image(IImage.MemoryInfo memoryInfo, IImage.CreateInfo createInfo)
        {
            int width = createInfo.Width;
            int height = createInfo.Height;
            int depth = createInfo.Depth;
            this.Levels = createInfo.MipCount;
            int layers = createInfo.LayerCount;
            ETextureTarget target = ToTarget(createInfo.Type, layers);
            EPixelInternalFormat format = ToSizedFormat(createInfo.Format);

            this.Format = createInfo.Format;
            this.Type = createInfo.Type;
            this.LayerCount = createInfo.LayerCount;

            Handle = OpenGL.CreateTextures(ETextureTarget.Texture2D);

            if (target == ETextureTarget.Texture1D)
            {
                OpenGL.TextureStorage1D(Handle, Levels, format, width);
            }
            else if (target == ETextureTarget.Texture1DArray)
            {
                OpenGL.TextureStorage2D(Handle, Levels, format, width, layers);
            }
            else if (target == ETextureTarget.Texture2D)
            {
                OpenGL.TextureStorage2D(Handle, Levels, format, width, height);
            }
            else if (target == ETextureTarget.Texture2DArray)
            {
                OpenGL.TextureStorage3D(Handle, Levels, format, width, height, layers);
            }
            else if(target == ETextureTarget.Texture3D)
            {
                OpenGL.TextureStorage3D(Handle, Levels, format, width, height, depth);
            }
            else if(target == ETextureTarget.TextureCubeMap)
            {
                OpenGL.TextureStorage2D(Handle, Levels, format, width, height);
            }
            else if(target == ETextureTarget.TextureCubeMapArray)
            {
                OpenGL.TextureStorage3D(Handle, Levels, format, width, height, layers);
            }
            else
            {
                throw new InvalidOperationException("Unsupported data type");
            }
        }

        public void Dispose()
        {
            Views.ForEach(view => view.Dispose());
            Views.Clear();

            OpenGL.DeleteTextures(Handle);
        }

        private static ETextureTarget ToTarget(EImageType type, int layers)
        {
            switch (type)
            {
                case EImageType.Image1D:
                    return layers == 1 ? ETextureTarget.Texture1D : ETextureTarget.Texture1DArray;
                case EImageType.Image2D:
                    return layers == 1 ? ETextureTarget.Texture2D : ETextureTarget.Texture2DArray;
                case EImageType.Image3D:
                    return ETextureTarget.Texture3D;
                case EImageType.ImageCube:
                    return layers == 1 ? ETextureTarget.TextureCubeMap : ETextureTarget.TextureCubeMapArray;
            }

            throw new InvalidOperationException("Unsupported data format");
        }

        private static EPixelInternalFormat ToSizedFormat(EDataFormat format)
        {
            switch (format)
            {
                case EDataFormat.Unknown:
                    break;
                case EDataFormat.BGRA8_SRGB:
                    break;
                case EDataFormat.R8G8B8A8_SRGB:
                    break;
                case EDataFormat.R8G8B8A8:
                    return EPixelInternalFormat.Rgba8;
                case EDataFormat.R32G32_SFLOAT:
                    return EPixelInternalFormat.Rg32f;
                case EDataFormat.R32G32B32_SFLOAT:
                    return EPixelInternalFormat.Rgb32f;
                case EDataFormat.R32G32B32A32_SFLOAT:
                    return EPixelInternalFormat.Rgba32f;
                case EDataFormat.D24_SFLOAT_S8_UINT:
                    return EPixelInternalFormat.Depth24Stencil8;
                case EDataFormat.D32_SFLOAT:
                    return EPixelInternalFormat.DepthComponent32f;
            }

            throw new InvalidOperationException("Unsupported data format");
        }

        public IImageView CreateImageView(IImageView.CreateInfo info)
        {
            ImageView view = new ImageView(info, this);
            Views.Add(view);

            return view;
        }
    }
}
