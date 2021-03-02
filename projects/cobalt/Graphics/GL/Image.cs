using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class Image : IImage
    {
        private List<ImageView> _views = new List<ImageView>();
        private uint _handle;
        private EDataFormat _format;
        private EImageType _type;
        private int _layerCount;


        public Image(IImage.MemoryInfo memoryInfo, IImage.CreateInfo createInfo)
        {
            _handle = OpenGL.CreateTextures(ETextureTarget.Texture2D);

        }

        public void Dispose()
        {
            // TODO
        }
    }
}
