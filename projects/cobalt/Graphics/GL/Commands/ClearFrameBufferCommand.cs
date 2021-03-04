﻿using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Math;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace Cobalt.Graphics.GL.Commands
{
    internal class ClearFrameBufferCommand : ICommand
    {
        private FrameBuffer _fbo;
        private List<Vector4> _clearValues;

        public ClearFrameBufferCommand(FrameBuffer fbo, List<Vector4> clearValues)
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
                Vector4 clear = _clearValues[i];
                if (object.Equals(clear, null))
                    continue;

                GCHandle handle = GCHandle.Alloc(clear);
                IntPtr val = (IntPtr)handle;

                OpenGL.ClearNamedFramebufferfv(_fbo.Handle, Bindings.GL.EClearBuffer.Color, i, val);

                handle.Free();
            }
        }
    }
}
