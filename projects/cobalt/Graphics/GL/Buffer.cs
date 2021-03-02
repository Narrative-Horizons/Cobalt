using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;

namespace Cobalt.Graphics.GL
{
    public class Buffer : IBuffer
    {
        public uint Id { get; private set; }

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
                //OpenGL.NamedBufferStorage(Id, 0, null, flags);
            }
            else
            {
                //OpenGL.NamedBufferStorage(Id, size, createInfo.InitialPayload, flags);
            }
        }

        public void Dispose()
        {
            OpenGL.DeleteBuffers(Id);
        }

        public object Map()
        {
            return OpenGL.MapNamedBuffer(Id, EAccessType.ReadWrite);
        }

        public object Map(int offset, int size)
        {
            return OpenGL.MapNamedBufferRange(Id, offset, size, EAccessType.ReadWrite);
        }

        public void Unmap()
        {
            OpenGL.UnmapNamedBuffer(Id);
        }
    }
}
