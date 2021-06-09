using Cobalt.Graphics.API;
using System;

namespace Cobalt.Graphics.GL
{
    internal class Fence : IFence
    {
        private IntPtr _sync;

        public Fence(IFence.CreateInfo info)
        {
            if (info.Signaled)
            {
                PlaceSync();
            }
        }
        public void PlaceSync()
        {
            Dispose();
            _sync = Bindings.GL.GL.FenceSync();
        }

        public void Dispose()
        {
            if (_sync != default)
            {
                Bindings.GL.GL.DeleteSync(_sync);
            }
        }

        public void Reset()
        {
            // NO OP intentional
        }

        public void Wait()
        {
            Bindings.GL.GL.ClientWaitSync(_sync);
        }
    }
}
