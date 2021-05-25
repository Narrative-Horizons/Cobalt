using Cobalt.Graphics.API;
using System;
using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL.Commands
{
    internal class MultiDrawElementsIndirectCommand : ICommand
    {
        private DrawElementsIndirectCommand _payload;

        public MultiDrawElementsIndirectCommand(DrawElementsIndirectCommand indirect)
        {
            _payload = indirect;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            StateMachine.MultiDrawElementsIndirect(_payload);
        }
    }
}
