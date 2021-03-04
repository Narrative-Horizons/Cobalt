using System;
using System.Collections.Generic;
using System.Text;

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
            // TODO Bind vao
        }
    }
}
