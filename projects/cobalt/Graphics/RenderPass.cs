using Cobalt.Core;
using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics
{
    public abstract class RenderPass
    {
        public struct FrameInfo
        {
            public IFrameBuffer FrameBuffer;
            public int FrameInFlight;
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
            public Dictionary<IVertexAttributeArray, DrawCommand> payload;
            public IBuffer indirectDrawBuffer;
        }

        public IDevice Device { get; private set; }

        public string Name { get; protected set; }
        public IRenderPass Native { get; protected set; }

        public RenderPass(IDevice device)
        {
            Device = device;
        }

        public abstract void Record(ICommandBuffer buffer, FrameInfo info);

        protected void Draw(ICommandBuffer buffer, DrawInfo draw)
        {
            foreach (var (vao, command) in draw.payload)
            {
                buffer.Bind(vao);
                buffer.DrawElementsMultiIndirect(command.indirect, command.bufferOffset, draw.indirectDrawBuffer);
            }
        }
    }

}
