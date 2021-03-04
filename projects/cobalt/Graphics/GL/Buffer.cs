using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;
using System;
using System.Runtime.InteropServices;

namespace Cobalt.Graphics.GL
{
    internal class Buffer : IBuffer
    {
        public uint Handle { get; private set; }

        private GCHandle _mappedHandle;

        public Buffer(IBuffer.MemoryInfo memoryInfo, IBuffer.CreateInfo createInfo)
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
                OpenGL.NamedBufferStorage(Handle, 0, IntPtr.Zero, flags);
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(createInfo.InitialPayload, GCHandleType.Pinned);
                OpenGL.NamedBufferStorage(Handle, (uint)size, handle.AddrOfPinnedObject(), flags);
            }
        }

        public void Dispose()
        {
            OpenGL.DeleteBuffers(Handle);
        }

        public object Map()
        {
            IntPtr ptr = OpenGL.MapNamedBuffer(Handle, EBufferAccess.ReadOnly);
            _mappedHandle = (GCHandle)ptr;

            return _mappedHandle.Target;
        }

        public object Map(int offset, int size)
        {
            IntPtr ptr = OpenGL.MapNamedBufferRange(Handle, offset, size, EBufferAccess.ReadOnly);
            _mappedHandle = (GCHandle)ptr;

            return _mappedHandle.Target;
        }

        public void Unmap()
        {
            OpenGL.UnmapNamedBuffer(Handle);
        }
    }
}
