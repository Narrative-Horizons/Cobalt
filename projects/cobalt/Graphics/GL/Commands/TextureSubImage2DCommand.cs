using Cobalt.Bindings.GL;
using Cobalt.Graphics.API;

using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL.Commands
{
    internal class TextureSubImage2DCommand : ICommand
    {
        private Image image;
        private byte[] source;
        private int mipLevel;
        private int x;
        private int y;
        private int width;
        private int height;
        private EPixelInternalFormat ePixelInternalFormat;
        private EPixelType ePixelType;
        private int bufferOffset;

        public TextureSubImage2DCommand(Image image, byte[] source, int mipLevel, int x, int y, int width, int height, EPixelInternalFormat ePixelInternalFormat, EPixelType ePixelType, int bufferOffset)
        {
            this.image = image;
            this.source = source;
            this.mipLevel = mipLevel;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.ePixelInternalFormat = ePixelInternalFormat;
            this.ePixelType = ePixelType;
            this.bufferOffset = bufferOffset;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            byte[] offsetBuffer = null;
            if (bufferOffset > 0)
            {
                offsetBuffer = new byte[source.Length - bufferOffset];
                for (int i = 0; i < offsetBuffer.Length; i++)
                {
                    offsetBuffer[i] = source[bufferOffset + i];
                }
            }
            else
            {
                offsetBuffer = source;
            }

            OpenGL.TextureSubImage2D(image.Handle, mipLevel, x, y, width, height, ePixelInternalFormat, ePixelType, offsetBuffer);
        }
    }
}