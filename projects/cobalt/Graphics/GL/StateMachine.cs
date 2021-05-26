using Cobalt.Bindings.GL;
using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL
{
    internal static class StateMachine
    {
        private static uint _currentProgram = uint.MaxValue;
        private static uint _currentVao = uint.MaxValue;
        private static EBeginMode _currentDrawMode = 0;
        private static HashSet<TextureSamplerHandleWrapper> _residentTextureSamplerHandles = new HashSet<TextureSamplerHandleWrapper>();

        private static HashSet<ulong> _residentTextureHandles = new HashSet<ulong>();

        private static Dictionary<TextureSamplerHandleWrapper, ulong> _cachedHandles = new Dictionary<TextureSamplerHandleWrapper, ulong>();

        private static bool DepthEnabled = false;

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

        internal static void SetDepthMask(bool v)
        {
            if (v != DepthEnabled)
            {
                OpenGL.DepthMask(v);
            }
            DepthEnabled = v;
        }

        internal static void BindBuffer(EBufferUsage usage, IBuffer buffer)
        {
            EBufferTarget target = EBufferTarget.ArrayBuffer;

            switch (usage)
            {
                case EBufferUsage.TransferSource:
                    break;
                case EBufferUsage.TransferDestination:
                    break;
                case EBufferUsage.UniformBuffer:
                    target = EBufferTarget.UniformBuffer;
                    break;
                case EBufferUsage.StorageBuffer:
                    target = EBufferTarget.ShaderStorageBuffer;
                    break;
                case EBufferUsage.ArrayBuffer:
                    break;
                case EBufferUsage.IndexBuffer:
                    break;
                case EBufferUsage.TextureBuffer:
                    break;
                case EBufferUsage.IndirectBuffer:
                    target = EBufferTarget.DrawIndirectBuffer;
                    break;
            }

            OpenGL.BindBuffer(target, ((IHandledType)buffer).GetHandle());
        }

        internal static void BindStorageBufferRange(uint index, IBuffer buffer, int offset, int range)
        {
            OpenGL.BindBufferRange(EBufferTarget.ShaderStorageBuffer, index, ((IHandledType)buffer).GetHandle(), offset, range);
        }

        internal static void BindUniformBufferRange(uint index, IBuffer buffer, int offset, int range)
        {
            OpenGL.BindBufferRange(EBufferTarget.UniformBuffer, index, ((IHandledType)buffer).GetHandle(), offset, range);
        }

        public static void BindFramebuffer(FrameBuffer FBO)
        {
            OpenGL.BindFramebuffer(EFramebufferTarget.Framebuffer, FBO.Handle);
        }

        public static void UniformHandleuivArb(int index, ulong[] handles)
        {
            OpenGL.UniformHandleui64vARB(index, handles.Length, handles);
        }

        public static void MultiDrawElementsIndirect(DrawElementsIndirectCommand payload, int offset, IBuffer indirectBuffer)
        {
            unsafe
            {
                OpenGL.BindBuffer(EBufferTarget.DrawIndirectBuffer, ((IHandledType)indirectBuffer).GetHandle());
                IntPtr offsetPtr = new IntPtr(offset);
                OpenGL.MultiDrawElementsIndirect(_currentDrawMode, EDrawElementsType.UnsignedInt, offsetPtr, payload.Data.Count, sizeof(DrawElementsIndirectCommandPayload));
                OpenGL.BindBuffer(EBufferTarget.DrawIndirectBuffer, 0);
            }
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
            if(_residentTextureHandles.Contains(handle) == false && handle != 0)
            {
                OpenGL.MakeTextureHandleResidentARB(handle);
                _residentTextureHandles.Add(handle);
            }
        }

        public static void DrawArraysInstancedBaseInstance(int baseVertex, int vertexCount, int baseInstance, int instanceCount)
        {
            OpenGL.DrawArraysInstancedBaseInstance(_currentDrawMode, baseVertex, vertexCount, instanceCount, (uint)baseInstance);
        }

        public static void DrawElementsInstancedBaseVertexBaseInstance(int elementCount, int baseVertex,
            int baseInstance, int instanceCount, long indexOffset)
        {
            IntPtr offsetPtr = new IntPtr(indexOffset);

            OpenGL.DrawElementsInstancedBaseVertexBaseInstance(_currentDrawMode, elementCount, Bindings.GL.EDrawElementsType.UnsignedInt, 
                offsetPtr, instanceCount, baseVertex, (uint)baseInstance);
        }

        internal static ulong GetTextureSamplerHandle(ImageView imageView, Sampler sampler)
        {
            uint tex = imageView.Handle;
            uint sam = sampler.Handle;

            TextureSamplerHandleWrapper wrapper = new TextureSamplerHandleWrapper
            {
                textureHandle = tex,
                samplerHandle = sam
            };

            if(_cachedHandles.ContainsKey(wrapper))
            {
                return _cachedHandles[wrapper];
            }

            ulong handle = OpenGL.GetTextureSamplerHandleARB(tex, sam);
            _cachedHandles.Add(wrapper, handle);

            return handle;
        }
    }
}
