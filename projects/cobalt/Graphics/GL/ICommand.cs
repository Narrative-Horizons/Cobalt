using System;

namespace Cobalt.Graphics.GL
{
    public interface ICommand : IDisposable
    {
        void Execute();
    }
}
