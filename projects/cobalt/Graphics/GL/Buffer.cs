using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;
using Cobalt.Graphics.API;
using System;
using System.Runtime.InteropServices;
using Cobalt.Core;

namespace Cobalt.Graphics.GL
{
    internal class Buffer<T> : IBuffer, IHandledType where T : unmanaged
    {
        public uint Handle { get; private set; }

        private GCHandle _mappedHandle;

        public Buffer(IBuffer.MemoryInfo memoryInfo, IBuffer.CreateInfo<T> createInfo)
        {
            Handle = OpenGL.CreateBuffers();
            int size = createInfo.Size;
            EBufferAccessMask flags = 0;

            if(memoryInfo.Usage != EMemoryUsage.GPUOnly && (memoryInfo.Required.Contains(EMemoryProperty.HostVisible) ||
                memoryInfo.Preferred.Contains(EMemoryProperty.HostVisible)))
            {
                flags |= EBufferAccessMask.MapPersistentBit;
                flags |= EBufferAccessMask.DynamicStorageBit;
                flags |= EBufferAccessMask.MapReadBit;
                flags |= EBufferAccessMask.MapWriteBit;
            }

            if(memoryInfo.Preferred.Contains(EMemoryProperty.HostCoherent) || memoryInfo.Required.Contains(EMemoryProperty.HostCoherent))
            {
                flags |= EBufferAccessMask.MapCoherentBit;
            }

            if(memoryInfo.Usage == EMemoryUsage.CPUOnly)
            {
                flags |= EBufferAccessMask.ClientStorageBit;
            }

            if(createInfo.InitialPayload == null)
            {
                OpenGL.NamedBufferStorage(Handle, size, IntPtr.Zero, flags);
            }
            else
            {
                T[] payload = createInfo.InitialPayload;
                OpenGL.NamedBufferStorageTyped(Handle, (uint)size, payload, flags);
            }
        }

        public void Dispose()
        {
            OpenGL.DeleteBuffers(Handle);
        }

        public IntPtr Map()
        {
            IntPtr ptr = OpenGL.MapNamedBuffer(Handle, EBufferAccess.ReadWrite);
            return ptr;
        }

        public IntPtr Map(int offset, int size)
        {
            IntPtr ptr = OpenGL.MapNamedBufferRange(Handle, offset, size, EBufferAccess.ReadWrite);
            return ptr;
        }

        public void Unmap()
        {
            OpenGL.UnmapNamedBuffer(Handle);
        }

        public uint GetHandle()
        {
            return Handle;
        }
    }

    internal static class BufferHelper
    {
        public static uint GetHandle(IBuffer buffer)
        {
            return (buffer as IHandledType).GetHandle();
        }
    }
}
