namespace Cobalt.Graphics.GL.Commands
{
    internal class WaitSyncCommand : ICommand
    {
        public WaitSyncCommand()
        {
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            var fence = Bindings.GL.GL.FenceSync();
            Bindings.GL.GL.WaitSync(fence);
            Bindings.GL.GL.DeleteSync(fence);
        }
    }
}
