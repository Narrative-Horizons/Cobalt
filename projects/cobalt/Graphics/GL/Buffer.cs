using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;
using System;
using System.Runtime.InteropServices;

namespace Cobalt.Graphics.GL
{
    public class Buffer : IBuffer
    {
        public uint Id { get; private set; }

        private GCHandle _mappedHandle;

        public Buffer(IBuffer.MemoryInfo memoryInfo, IBuffer.CreateInfo createInfo)
        {
            Id = OpenGL.CreateBuffers();
            int size = createInfo.Size;
            EMapBit flags = 0;

            if(memoryInfo.Usage != EMemoryUsage.GPUOnly && (memoryInfo.Required.Contains(EMemoryProperty.HostVisible) ||
                memoryInfo.Preferred.Contains(EMemoryProperty.HostVisible)))
            {
                flags |= EMapBit.MapPersistentBit;
                flags |= EMapBit.DynamicStorageBit;
                flags |= EMapBit.MapReadBit;
                flags |= EMapBit.MapWriteBit;
            }

            if(memoryInfo.Preferred.Contains(EMemoryProperty.HostCoherent) || memoryInfo.Required.Contains(EMemoryProperty.HostCoherent))
            {
                flags |= EMapBit.MapCoherentBit;
            }

            if(memoryInfo.Usage == EMemoryUsage.CPUOnly)
            {
                flags |= EMapBit.ClientStorageBit;
            }

            if(createInfo.InitialPayload == null)
            {
                OpenGL.NamedBufferStorage(Id, 0, IntPtr.Zero, flags);
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(createInfo.InitialPayload);
                IntPtr ptr = (IntPtr)handle;

                OpenGL.NamedBufferStorage(Id, size, ptr, flags);

                handle.Free();
            }
        }

        public void Dispose()
        {
            OpenGL.DeleteBuffers(Id);
        }

        public object Map()
        {
            IntPtr ptr = OpenGL.MapNamedBuffer(Id, EAccessType.ReadWrite);
            _mappedHandle = (GCHandle)ptr;

            return _mappedHandle.Target;
        }

        public object Map(int offset, int size)
        {
            IntPtr ptr = OpenGL.MapNamedBufferRange(Id, offset, size, EAccessType.ReadWrite);
            _mappedHandle = (GCHandle)ptr;

            return _mappedHandle.Target;
        }

        public void Unmap()
        {
            OpenGL.UnmapNamedBuffer(Id);
        }
    }
}
