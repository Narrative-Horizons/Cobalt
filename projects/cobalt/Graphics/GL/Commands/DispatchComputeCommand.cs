using OpenGL = Cobalt.Bindings.GL.GL;
using System;

namespace Cobalt.Graphics.GL.Commands
{
    public class DispatchComputeCommand : ICommand
    {
        public uint X { get; private set; }
        public uint Y { get; private set; }
        public uint Z { get; private set; }

        public DispatchComputeCommand(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            OpenGL.DispatchCompute(X, Y, Z);
        }
    }
}
