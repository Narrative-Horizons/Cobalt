using static Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GL;

namespace Cobalt.Graphics.GL.Commands
{
    internal class MemoryBarrierCommand : ICommand
    {
        public EMemoryBarrier Barrier { get; private set; }

        public MemoryBarrierCommand(EMemoryBarrier barrier)
        {
            Barrier = barrier;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            MemoryBarrier(Barrier);
        }
    }
}