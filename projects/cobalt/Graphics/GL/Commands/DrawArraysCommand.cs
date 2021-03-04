namespace Cobalt.Graphics.GL.Commands
{
    internal class DrawArraysCommand : ICommand
    {
        public int BaseVertex { get; private set; }
        public int VertexCount { get; private set; }
        public int BaseInstance { get; private set; }
        public int InstanceCount { get; private set; }

        public DrawArraysCommand(int baseVertex, int vertexCount, int baseInstance, int instanceCount)
        {
            BaseVertex = baseVertex;
            VertexCount = vertexCount;
            BaseInstance = baseInstance;
            InstanceCount = instanceCount;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            StateMachine.DrawArraysInstancedBaseInstance(BaseVertex, VertexCount, BaseInstance, InstanceCount);
        }
    }
}
