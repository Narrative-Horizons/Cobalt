using System.Linq;
using Cobalt.Bindings.Vulkan;
using Cobalt.Graphics.Enums;

namespace Cobalt.Graphics
{
    public class CommandList
    {
        private readonly VK.CommandBuffer _handle;
        private readonly uint _frameInFlight;

        internal CommandList(VK.CommandBuffer buffer, uint frameInFlight)
        {
            _handle = buffer;
            _frameInFlight = frameInFlight;
        }

        public void Bind(Shader shader, uint firstSet, Descriptor[] sets,
            uint[] dynamicOffsets)
        {
            VK.BindDescriptorSets(_handle, _frameInFlight, (uint) shader.bindPoint, shader.handle, firstSet,
                (uint) sets.Length, sets.Select(set => set.handle).ToArray(), (uint) dynamicOffsets.Length, dynamicOffsets);
        }

        public void Bind(Shader shader)
        {
            VK.BindPipeline(_handle, (uint) shader.bindPoint, _frameInFlight, shader.handle);
        }

        public void Bind(uint firstBinding, Buffer[] vertexBuffers, ulong[] offsets)
        {
            VK.BindVertexBuffers(_handle, _frameInFlight, firstBinding, (uint) vertexBuffers.Length,
                vertexBuffers.Select(buffer => buffer.handle).ToArray(), offsets);
        }

        public void Bind(Buffer indexBuffer, ulong offset, IndexType indexType)
        {
            VK.BindIndexBuffer(_handle, _frameInFlight, indexBuffer.handle, offset, (uint) indexType);
        }

        public void DrawIndexedIndirect(Buffer indirectBuffer, ulong offset, uint drawCount, uint stride)
        {
            VK.DrawIndexedIndirect(_handle, _frameInFlight, indirectBuffer.handle, offset, drawCount, stride);
        }

        public void PipelineBarrier(PipelineStageFlagBits srcStageMask, PipelineStageFlagBits dstStageMask, DependencyFlagBits dependencyFlags,
            MemoryBarrier[] memoryBarriers,
            BufferMemoryBarrier[] bufferMemoryBarriers,
            ImageMemoryBarrier[] imageMemoryBarriers)
        {
            VK.PipelineBarrier(_handle, _frameInFlight, (uint) srcStageMask, (uint) dstStageMask,
                (uint) dependencyFlags,
                (uint)memoryBarriers.Length, memoryBarriers, (uint)bufferMemoryBarriers.Length, bufferMemoryBarriers,
                (uint)imageMemoryBarriers.Length, imageMemoryBarriers);
        }
    }
}
