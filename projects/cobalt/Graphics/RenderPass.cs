using Cobalt.Core;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics
{
    public abstract class RenderPass
    {
        public struct FrameInfo
        {
            public IFrameBuffer frameBuffer;
            public int frameInFlight;
            public int width;
            public int height;
        }

        public struct DrawCommand
        {
            public DrawElementsIndirectCommand indirect;
            public int bufferOffset;
        }

        public struct DrawInfo
        {
            public List<IDescriptorSet> descriptorSets;
            public Dictionary<EMaterialType, Dictionary<IVertexAttributeArray, DrawCommand>> payload;
            public IBuffer indirectDrawBuffer;
        }

        public IDevice Device { get; private set; }

        public string Name { get; protected set; }
        public IRenderPass Native { get; protected set; }

        public RenderPass(IDevice device)
        {
            Device = device;
        }

        public abstract bool Record(ICommandBuffer buffer, FrameInfo info, DrawInfo draw);

        protected void Draw(ICommandBuffer buffer, DrawInfo draw, EMaterialType type)
        {
            foreach (var (vao, command) in draw.payload[type])
            {
                buffer.Bind(vao);
                buffer.DrawElementsMultiIndirect(command.indirect, command.bufferOffset, draw.indirectDrawBuffer);
            }
        }
    }

}
