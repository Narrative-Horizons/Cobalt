using Cobalt.Graphics.API;

namespace Cobalt.Graphics.GL.Commands
{
    internal class BindBufferCommand : ICommand
    {
        public IBuffer Buffer { get; set; }
        public EBufferUsage Usage { get; set; }
        public BindBufferCommand(EBufferUsage usage, IBuffer buffer)
        {
            Usage = usage;
            Buffer = buffer;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            StateMachine.BindBuffer(Usage, Buffer);
        }
    }
}
