using System;
using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL.Commands
{
    internal class DrawElementsCommand : ICommand
    {
        public int BaseVertex { get; private set; }
        public int ElementCount { get; private set; }
        public int BaseInstance { get; private set; }
        public int InstanceCount { get; private set; }
        public long IndexOffset { get; private set; }

        public DrawElementsCommand(int baseVertex, int elementCount, int baseInstance, int instanceCount, long indexOffset)
        {
            BaseVertex = baseVertex;
            ElementCount = elementCount;
            BaseInstance = baseInstance;
            InstanceCount = instanceCount;
            IndexOffset = indexOffset;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            StateMachine.DrawElementsInstancedBaseVertexBaseInstance(ElementCount, BaseVertex, BaseInstance, InstanceCount, IndexOffset);
        }
    }
}
