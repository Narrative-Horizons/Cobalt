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
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Image(IImage.MemoryInfo memoryInfo, IImage.CreateInfo createInfo)
        {
            int width = createInfo.Width;
            int height = createInfo.Height;

            Width = width;
            Height = height;

            int depth = createInfo.Depth;
            Levels = createInfo.MipCount;
            int layers = createInfo.LayerCount;
            ETextureTarget target = ToTarget(createInfo.Type, layers);
            EPixelInternalFormat format = ToSizedFormat(createInfo.Format);

            Format = createInfo.Format;
            Type = createInfo.Type;
            LayerCount = createInfo.LayerCount;

            int sampleCount = (int)createInfo.SampleCount;

            Handle = OpenGL.CreateTextures(ETextureTarget.Texture2D);

            if (target == ETextureTarget.Texture1D)
            {
                OpenGL.TextureStorage1D(Handle, Levels, format, width);
                if (sampleCount > 1)
                {
                    // Log that multisampling not supported for 1D images
                }
            }
            else if (target == ETextureTarget.Texture1DArray)
            {
                OpenGL.TextureStorage2D(Handle, Levels, format, width, layers);
                if (sampleCount > 1)
                {
                    // Log that multisampling not supported for 1D images
                }
            }
            else if (target == ETextureTarget.Texture2D)
            {
                if (sampleCount > 1)
                {
                    OpenGL.TextureStorage2DMS(Handle, sampleCount, format, width, height, true);
                }
                else
                {
                    OpenGL.TextureStorage2D(Handle, Levels, format, width, height);
                }
            }
            else if (target == ETextureTarget.Texture2DArray)
            {
                if (sampleCount > 1)
                {
                    OpenGL.TextureStorage3DMS(Handle, sampleCount, format, width, height, layers, true);
                }
                else
                {
                    OpenGL.TextureStorage3D(Handle, Levels, format, width, height, layers);
                }
            }
            else if(target == ETextureTarget.Texture3D)
            {
                if (sampleCount > 1)
                {
                    OpenGL.TextureStorage3DMS(Handle, sampleCount, format, width, height, depth, true);
                }
                else
                {
                    OpenGL.TextureStorage3D(Handle, Levels, format, width, height, depth);
                }
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

        internal static EPixelInternalFormat ToSizedFormat(EDataFormat format)
        {
            switch (format)
            {
                case EDataFormat.Unknown:
                    break;
                case EDataFormat.BGRA8_SRGB:
                    break;
                case EDataFormat.R8G8B8A8_SRGB:
                    break;
                case EDataFormat.R32_UINT:
                    return EPixelInternalFormat.R32ui;
                case EDataFormat.R8G8B8A8:
                    return EPixelInternalFormat.Rgba8;
                case EDataFormat.R32G32_SFLOAT:
                    return EPixelInternalFormat.Rg32f;
                case EDataFormat.R32G32_UINT:
                    return EPixelInternalFormat.Rg32ui;
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

        internal static EPixelFormat ToPixelFormat(EDataFormat format)
        {
            switch (format)
            {
                case EDataFormat.Unknown:
                    throw new ArgumentException("Unknown format");
                case EDataFormat.BGRA8_SRGB:
                    return EPixelFormat.Bgra;
                case EDataFormat.R8G8B8A8_SRGB:
                case EDataFormat.R8G8B8A8:
                    return EPixelFormat.Rgba;
                case EDataFormat.R32_UINT:
                    return EPixelFormat.RedInteger;
                case EDataFormat.R32G32_UINT:
                    return EPixelFormat.RgInteger;
                case EDataFormat.R32G32_SFLOAT:
                    return EPixelFormat.Rg;
                case EDataFormat.R32G32B32_SFLOAT:
                    return EPixelFormat.Rgb;
                case EDataFormat.R32G32B32A32_SFLOAT:
                    return EPixelFormat.Rgba;
                case EDataFormat.D24_SFLOAT_S8_UINT:
                    return EPixelFormat.DepthStencil;
                case EDataFormat.D32_SFLOAT:
                    return EPixelFormat.DepthComponent;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        internal static EPixelType ToPixelType(EDataFormat format)
        {
            switch (format)
            {
                case EDataFormat.Unknown:
                    throw new ArgumentException("Invalid format");
                case EDataFormat.BGRA8_SRGB:
                case EDataFormat.R8G8B8A8_SRGB:
                case EDataFormat.R8G8B8A8:
                    return EPixelType.Byte;
                case EDataFormat.R32_UINT:
                case EDataFormat.R32G32_UINT:
                    return EPixelType.UnsignedInt;
                case EDataFormat.R32G32_SFLOAT:
                case EDataFormat.R32G32B32_SFLOAT:
                case EDataFormat.R32G32B32A32_SFLOAT:
                case EDataFormat.D32_SFLOAT:
                    return EPixelType.Float;
                case EDataFormat.D24_SFLOAT_S8_UINT:
                    throw new ArgumentException("Invalid format");
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public IImageView CreateImageView(IImageView.CreateInfo info)
        {
            ImageView view = new ImageView(info, this);
            Views.Add(view);

            return view;
        }

        public byte[] GetPixels(int x, int y, uint width, uint height)
        {
            uint bpp = Format.GetBppFromFormat();
            byte[] imagePixels = new byte[Width * Height * bpp];

            OpenGL.GetTextureImage(Handle, 0, ToPixelFormat(Format), ToPixelType(Format),
                (uint)imagePixels.Length, imagePixels);

            byte[] pixels = new byte[width * height * bpp];

            int index = 0;
            for (int i = 0; i < width; i++)
            {
                int cursorx = x + i;
                
                for (int j = 0; j < height; j++)
                {
                    int cursory = y + j;

                    for (int b = 0; b < bpp; b++)
                    {
                        pixels[index++] = imagePixels[(cursorx + cursory * width) * bpp + b];
                    }
                }
            }

            return pixels;
        }
    }
}
