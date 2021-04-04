using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL.Commands
{
    internal class BindFramebufferCommand : ICommand
    {
        public FrameBuffer FBO { get; private set; }
        public BindFramebufferCommand(FrameBuffer buffer)
        {
            FBO = buffer;
        }

        public void Dispose()
        {

        }

        public void Execute()
        {
            StateMachine.BindFramebuffer(FBO);
        }
    }
}
