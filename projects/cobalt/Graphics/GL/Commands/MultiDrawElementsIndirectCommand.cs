using Cobalt.Graphics.API;
using System;
using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL.Commands
{
    internal class MultiDrawElementsIndirectCommand : ICommand
    {
        private DrawElementsIndirectCommand _payload;
        private int _offset;
        private IBuffer _indirectBuffer;

        public MultiDrawElementsIndirectCommand(DrawElementsIndirectCommand indirect, int offset, IBuffer indirectBuffer)
        {
            _payload = indirect;
            _offset = offset;
            _indirectBuffer = indirectBuffer;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            StateMachine.MultiDrawElementsIndirect(_payload, _offset, _indirectBuffer);
        }
    }
}
