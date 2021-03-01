using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;
using System;
using System.Runtime.InteropServices;

namespace Cobalt.Graphics.GL
{
    internal class Image : IImage
    {
        uint handle;

        public Image(IImage.MemoryInfo memoryInfo, IImage.CreateInfo createInfo)
        {
            handle = OpenGL.CreateTextures(ETextureTarget.Texture2D);
        }

        public void Dispose()
        {
            // TODO
        }
    }
}
