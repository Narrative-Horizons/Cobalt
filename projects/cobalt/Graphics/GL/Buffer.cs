using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;
using Cobalt.Graphics.API;
using System;
using System.Runtime.InteropServices;

namespace Cobalt.Graphics.GL
{
    internal class Buffer<T> : IBuffer, IHandledType where T : unmanaged
    {
        public uint Handle { get; private set; }
        public EBufferAccessMask Flags { get; private set; } = 0;
        public EBufferAccessMask ReadWriteRangeFlags { get; private set; } = 0;
        public EBufferAccess ReadWriteFlag { get; private set; }
        public bool Persistent { get; private set; }
        public int Size { get; private set; }
        private IntPtr Mapping { get; set; } = default;

        private int _mapStart = 0;
        private int _mapRange = 0;

        public Buffer(IBuffer.MemoryInfo memoryInfo, IBuffer.CreateInfo<T> createInfo)
        {
            Handle = OpenGL.CreateBuffers();
            int size = createInfo.Size;
            Flags = 0;
            Size = size;

            if(memoryInfo.Usage != EMemoryUsage.GPUOnly && (memoryInfo.Required.Contains(EMemoryProperty.HostVisible) ||
                memoryInfo.Preferred.Contains(EMemoryProperty.HostVisible)))
            {
                Flags |= EBufferAccessMask.MapPersistentBit;
                Flags |= EBufferAccessMask.DynamicStorageBit;
                Flags |= EBufferAccessMask.MapReadBit;
                Flags |= EBufferAccessMask.MapWriteBit;
            }

            if(memoryInfo.Preferred.Contains(EMemoryProperty.HostCoherent) || memoryInfo.Required.Contains(EMemoryProperty.HostCoherent))
            {
                Flags |= EBufferAccessMask.MapCoherentBit;
            }

            if(memoryInfo.Usage == EMemoryUsage.CPUOnly)
            {
                Flags |= EBufferAccessMask.ClientStorageBit;
            }

            if(createInfo.InitialPayload == null)
            {
                OpenGL.NamedBufferStorage(Handle, size, IntPtr.Zero, Flags);
            }
            else
            {
                GCHandle h = GCHandle.Alloc(createInfo.InitialPayload, GCHandleType.Pinned);
                OpenGL.NamedBufferStorage(Handle, size, h.AddrOfPinnedObject(), Flags);
            }

            bool read = (Flags & EBufferAccessMask.MapReadBit) == EBufferAccessMask.MapReadBit;
            bool write = (Flags & EBufferAccessMask.MapWriteBit) == EBufferAccessMask.MapWriteBit;
            bool persistent = (Flags & EBufferAccessMask.MapPersistentBit) == EBufferAccessMask.MapPersistentBit;
            bool coherent = (Flags & EBufferAccessMask.MapCoherentBit) == EBufferAccessMask.MapCoherentBit;

            ReadWriteRangeFlags |= read ? EBufferAccessMask.MapReadBit : 0;
            ReadWriteRangeFlags |= write ? EBufferAccessMask.MapWriteBit : 0;
            ReadWriteRangeFlags |= persistent ? EBufferAccessMask.MapPersistentBit : 0;
            ReadWriteRangeFlags |= coherent ? EBufferAccessMask.MapCoherentBit : 0;

            if (read && write)
            {
                ReadWriteFlag = EBufferAccess.ReadWrite;
            }
            else if (read)
            {
                ReadWriteFlag = EBufferAccess.ReadOnly;
            } 
            else if (write)
            {
                ReadWriteFlag = EBufferAccess.WriteOnly;
            }

            Persistent = persistent;
        }

        public void Dispose()
        {
            Unmap();
            OpenGL.DeleteBuffers(Handle);
        }

        public IntPtr Map()
        {
            if (Mapping == default || (_mapStart != 0 && _mapRange != Size))
            {
                Mapping = OpenGL.MapNamedBuffer(Handle, ReadWriteFlag);
            }
            return Mapping;
        }

        public IntPtr Map(int offset, int size)
        {
            if (Mapping == default || offset != _mapStart || size != _mapRange)
            {
                Mapping = OpenGL.MapNamedBufferRange(Handle, offset, size, ReadWriteRangeFlags);
                _mapStart = offset;
                _mapRange = size;
            }
            return Mapping;
        }

        public void Unmap()
        {
            if (!Persistent && Handle != default)
            {
                OpenGL.UnmapNamedBuffer(Handle);
                Handle = default;
            }
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
