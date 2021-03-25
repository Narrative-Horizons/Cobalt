using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL
{
    internal static class StateMachine
    {
        private static uint _currentProgram = uint.MaxValue;
        private static uint _currentVao = uint.MaxValue;
        private static Bindings.GL.EBeginMode _currentDrawMode = 0;
        private static HashSet<TextureSamplerHandleWrapper> _residentTextureSamplerHandles = new HashSet<TextureSamplerHandleWrapper>();
        private static HashSet<ulong> _residentTextureHandles = new HashSet<ulong>();

        private struct TextureSamplerHandleWrapper : IEquatable<TextureSamplerHandleWrapper>
        {
            public ulong textureHandle;
            public ulong samplerHandle;

            public override int GetHashCode()
            {
                return HashCode.Combine(textureHandle, samplerHandle);
            }

            public override bool Equals(object obj)
            {
                if (obj is TextureSamplerHandleWrapper wrapper)
                {
                    return Equals(wrapper);
                }
                return false;
            }
            public bool Equals(TextureSamplerHandleWrapper wrapper)
            {
                return textureHandle == wrapper.textureHandle && samplerHandle == wrapper.samplerHandle;
            }
        }

        public static void UniformHandleuivArb(int index, ulong[] handles)
        {
            OpenGL.UniformHandleui64vARB(index, handles.Length, handles);
        }

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

        public static void MakeTextureHandleResidentArb(ulong handle)
        {
            if(_residentTextureHandles.Contains(handle) == false)
            {
                OpenGL.MakeTextureHandleResidentARB(handle);
                _residentTextureHandles.Add(handle);
            }
        }

        /*public static void MakeTextureSamplerHandleResidentArb(long textureHandle, long samplerHandle)
        {
            TextureSamplerHandleWrapper wrapper = new TextureSamplerHandleWrapper
            {
                textureHandle = textureHandle,
                samplerHandle = samplerHandle
            };

            if (_residentTextureSamplerHandles.Contains(wrapper) == false)
            {
                OpenGL.MakeTextureSamplerHandleResidentArb(textureHandle, samplerHandle);
                _residentTextureSamplerHandles.Add(wrapper);
            }
        }*/

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

        internal static ulong GetTextureSamplerHandle(ImageView imageView, Sampler sampler)
        {
            uint tex = imageView.Handle;
            uint sam = sampler.Handle;

            return OpenGL.GetTextureSamplerHandleARB(tex, sam);
        }
    }
}
