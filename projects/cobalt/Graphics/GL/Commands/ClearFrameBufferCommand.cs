using OpenGL = Cobalt.Bindings.GL.GL;

using System.Collections.Generic;

namespace Cobalt.Graphics.GL.Commands
{
    internal class ClearFrameBufferCommand : ICommand
    {
        private FrameBuffer _fbo;
        private List<ClearValue> _clearValues;

        public ClearFrameBufferCommand(FrameBuffer fbo, List<ClearValue> clearValues)
        {
            _fbo = fbo;
            _clearValues = clearValues;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            for (int i = 0; i < _clearValues.Count; i++)
            {
                ClearValue clear = _clearValues[i];
                if (clear == null)
                    continue;

                if (clear.Color != null)
                {
                    OpenGL.ClearNamedFramebufferfv(_fbo.Handle, Bindings.GL.EClearBuffer.Color, i, new float[] { clear.Color.Red, clear.Color.Green, clear.Color.Blue, clear.Color.Alpha });
                }
                else if (clear.ColorUi != null)
                {
                    OpenGL.ClearNamedFramebufferfuiv(_fbo.Handle, Bindings.GL.EClearBuffer.Color, i, new uint[] { clear.ColorUi.Red, clear.ColorUi.Green, clear.ColorUi.Alpha });
                }
                else if (clear.Depth != null)
                {
                    StateMachine.SetDepthMask(true);
                    OpenGL.ClearNamedFramebufferfi(_fbo.Handle, Bindings.GL.EClearBuffer.DepthStencil, 0, clear.Depth ?? 1, 0);
                }
            }
        }
    }
}
