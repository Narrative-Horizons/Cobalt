using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL.Commands
{
    internal class BindVertexArrayObjectCommand : ICommand
    {
        public VertexAttributeArray VAO { get; private set; }

        public BindVertexArrayObjectCommand(VertexAttributeArray vao)
        {
            VAO = vao;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            StateMachine.BindVertexArray(VAO);
        }
    }
}
