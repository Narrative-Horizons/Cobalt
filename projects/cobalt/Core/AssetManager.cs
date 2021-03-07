using Cobalt.Bindings.STB;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using static Cobalt.Bindings.STB.ImageLoader;
using static Cobalt.Bindings.Utils.Util;

namespace Cobalt.Core
{
    public class ImageAsset
    {
        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public uint Components { get; private set; }
        public bool IsHdr { get; private set; }
        public uint BitsPerPixel { get; private set; }
        public byte[] AsBytes { get; private set; } = null;
        public ushort[] AsUnsignedShorts { get; private set; } = null;
        public float[] AsFloats { get; private set; } = null;

        internal ImageAsset(string path)
        {
            ImagePayload payload = LoadImage(path);
            Width = (uint)payload.width;
            Height = (uint)payload.height;
            Components = (uint)payload.channels;
            IsHdr = payload.hdr_f_image != IntPtr.Zero;

            int count = (int) (Width * Height * Components);

            if (payload.hdr_f_image != IntPtr.Zero)
            {
                BitsPerPixel = sizeof(float) * 8;
                AsFloats = new float[count];
                Marshal.Copy(payload.hdr_f_image, AsFloats, 0, count);
            }
            else if (payload.sdr_us_image != IntPtr.Zero)
            {
                BitsPerPixel = sizeof(ushort) * 8;
                AsUnsignedShorts = new ushort[count];
                Copy(payload.sdr_us_image, AsUnsignedShorts, 0, count);
            }
            else if (payload.sdr_ub_image != IntPtr.Zero)
            {
                BitsPerPixel = sizeof(byte) * 8;
                AsBytes = new byte[count];
                Marshal.Copy(payload.sdr_ub_image, AsBytes, 0, count);
            }

            ReleaseImage(ref payload);
        }
    }

    public class AssetManager : IDisposable
    {
        private readonly Dictionary<string, ImageAsset> _images = new Dictionary<string, ImageAsset>();

        public AssetManager()
        {
        }

        public void Dispose()
        {
        }

        public ImageAsset LoadImage(string path)
        {
            var asset = new ImageAsset(path);
            _images[path] = asset;
            return asset;
        }
    }
}
