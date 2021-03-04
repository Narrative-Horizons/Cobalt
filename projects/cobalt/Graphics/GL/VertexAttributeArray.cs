using OpenGL = Cobalt.Bindings.GL.GL;

using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class VertexAttributeArray : IVertexAttributeArray
    {
        public uint Handle { get; private set; }

        public VertexAttributeArray(IGraphicsPipeline.VertexAttributeCreateInfo info, List<IBuffer> buffers)
        {
            Handle = OpenGL.CreateVertexArrays();

            info.Attributes.ForEach(attribute =>
            {
                Buffer buffer = buffers[attribute.Binding] as Buffer;
                OpenGL.VertexArrayVertexBuffer(Handle, (uint)attribute.Binding, buffer.Handle, IntPtr.Zero, attribute.Stride);
            });

            info.Attributes.ForEach(attribute =>
            {
                OpenGL.EnableVertexArrayAttrib(Handle, (uint)attribute.Location);
            });

            info.Attributes.ForEach(attribute =>
            {
                int count = GetCount(attribute.Format);
                Bindings.GL.EVertexAttribFormat format = GetFormat(attribute.Format);
                OpenGL.VertexArrayAttribFormat(Handle, (uint)attribute.Location, count, format, false, (uint)attribute.Offset);
            });

            info.Attributes.ForEach(attribute =>
            {
                OpenGL.VertexArrayAttribBinding(Handle, (uint)attribute.Location, (uint)attribute.Binding);
            });
        }

        public VertexAttributeArray(IGraphicsPipeline.VertexAttributeCreateInfo info, List<IBuffer> vertexBuffers, IBuffer elementBuffer) : this(info, vertexBuffers)
        {
            OpenGL.VertexArrayElementBuffer(Handle, ((Buffer)elementBuffer).Handle);
        }

        public void Dispose()
        {
            OpenGL.DeleteVertexArrays(Handle);
        }

        private static int GetCount(EDataFormat format)
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
                    break;
                case EDataFormat.R32G32_SFLOAT:
                    return 2;
                case EDataFormat.R32G32B32_SFLOAT:
                    return 3;
                case EDataFormat.R32G32B32A32_SFLOAT:
                    return 4;
            }

            throw new InvalidOperationException("Unsupported data format");
        }

        private static Bindings.GL.EVertexAttribFormat GetFormat(EDataFormat format)
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
                    break;
                case EDataFormat.R32G32_SFLOAT:
                case EDataFormat.R32G32B32_SFLOAT:
                case EDataFormat.R32G32B32A32_SFLOAT:
                    return Bindings.GL.EVertexAttribFormat.Float;
            }

            throw new InvalidOperationException("Unsupported data format");
        }
    }
}
