using System;
using System.Runtime.InteropServices;
using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL
{
    internal static class StateMachine
    {
        private static uint _currentProgram = uint.MaxValue;
        private static uint _currentVao = uint.MaxValue;
        private static Bindings.GL.EBeginMode _currentDrawMode = 0;

        public static void UseProgram(GraphicsPipeline pipeline)
        {
            uint handle = pipeline.Handle;
            if (handle != _currentProgram)
            {
                _currentProgram = handle;
                _currentDrawMode = pipeline.DrawMode();
                OpenGL.UseProgram(handle);
            }
        }

        public static void BindVertexArray(VertexAttributeArray vao)
        {
            uint handle = vao.Handle;
            if (handle != _currentVao)
            {
                _currentVao = handle;
                OpenGL.BindVertexArray(handle);
            }
        }

        public static void DrawArraysInstancedBaseInstance(int baseVertex, int vertexCount, int baseInstance, int instanceCount)
        {
            OpenGL.DrawArraysInstancedBaseInstance(_currentDrawMode, baseVertex, vertexCount, instanceCount, (uint)baseInstance);
        }

        public static void DrawElementsInstancedBaseVertexBaseInstance(int elementCount, int baseVertex,
            int baseInstance, int instanceCount, long indexOffset)
        {
            IntPtr offsetPtr = Marshal.AllocHGlobal(sizeof(long));
            Marshal.WriteInt64(offsetPtr, 0, indexOffset << 2);

            OpenGL.DrawElementsInstancedBaseVertexBaseInstance(_currentDrawMode, elementCount, Bindings.GL.EDrawElementsType.UnsignedInt, 
                offsetPtr, instanceCount, baseVertex, (uint)baseInstance);

            Marshal.FreeHGlobal(offsetPtr);
        }
    }
}
