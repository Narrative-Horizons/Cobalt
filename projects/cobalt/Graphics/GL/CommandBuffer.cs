using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class CommandBuffer : ICommandBuffer
    {
        public List<ICommand> commands { get; private set; } = new List<ICommand>();

        public void BeginRenderPass(ICommandBuffer.RenderPassBeginInfo info)
        {
            throw new NotImplementedException();
        }

        public void Bind(IGraphicsPipeline pipeline)
        {
            throw new NotImplementedException();
        }

        public void Bind(IVertexAttributeArray vao)
        {
            throw new NotImplementedException();
        }

        public void Bind(IPipelineLayout layout, int firstSet)
        {
            throw new NotImplementedException();
        }

        public void Copy(IBuffer source, IBuffer destination, List<ICommandBuffer.BufferCopyRegion> regions)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Draw(int baseVertex, int vertexCount, int baseInstance, int instanceCount)
        {
            throw new NotImplementedException();
        }

        public void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount)
        {
            throw new NotImplementedException();
        }

        public void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount, long indexOffset)
        {
            throw new NotImplementedException();
        }

        public void End()
        {
            throw new NotImplementedException();
        }

        public void Record(ICommandBuffer.RecordInfo info)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
