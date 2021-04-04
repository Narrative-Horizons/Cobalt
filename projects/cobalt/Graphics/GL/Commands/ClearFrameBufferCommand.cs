using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Math;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

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

                OpenGL.ClearNamedFramebufferfv(_fbo.Handle, Bindings.GL.EClearBuffer.Color, i, new float[] { clear.Color.Red, clear.Color.Green, clear.Color.Blue, clear.Color.Alpha });
            }
        }
    }
}
