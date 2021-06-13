﻿using Cobalt.Bindings.GL;
using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;
using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL
{
    internal static class StateMachine
    {
        private static uint _currentProgram = uint.MaxValue;
        private static uint _currentVao = uint.MaxValue;
        private static EBeginMode _currentDrawMode = 0;
        private static HashSet<TextureSamplerHandleWrapper> _residentTextureSamplerHandles = new HashSet<TextureSamplerHandleWrapper>();
        private static Dictionary<EEnableCap, bool> _capabilities = new Dictionary<EEnableCap, bool>();

        private static HashSet<ulong> _residentTextureHandles = new HashSet<ulong>();

        private static Dictionary<TextureSamplerHandleWrapper, ulong> _cachedHandles = new Dictionary<TextureSamplerHandleWrapper, ulong>();

        private static bool DepthEnabled = false;

        private static ECullFaceMode cullFace = ECullFaceMode.Back;
        private static EFrontFaceDirection frontFaceDirection = EFrontFaceDirection.Ccw;
        private static Bindings.GL.EPolygonMode fillMode = Bindings.GL.EPolygonMode.Fill;

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

        internal static void Enable(EEnableCap capability, bool enabled)
        {
            if (_capabilities.GetValueOrDefault(capability, false) != enabled)
            {
                _capabilities[capability] = enabled;
                if (enabled)
                    OpenGL.Enable(capability);
                else
                    OpenGL.Disable(capability);
            }
        }

        internal static void DepthFunc(EDepthFunction func)
        {
            OpenGL.DepthFunc(func);
        }

        internal static void CullFace(ECullFaceMode mode)
        {
            if (cullFace != mode)
            {
                cullFace = mode;
                OpenGL.CullFace(cullFace);
            }
        }

        internal static void FrontFace(EFrontFaceDirection direction)
        {
            if (frontFaceDirection != direction)
            {
                frontFaceDirection = direction;
                OpenGL.FrontFace(frontFaceDirection);
            }
        }

        internal static void PolygonMode(Bindings.GL.EPolygonMode mode)
        {
            if (fillMode != mode)
            {
                fillMode = mode;
                OpenGL.PolygonMode(ECullFaceMode.FrontAndBack, fillMode);
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

        internal static void BlendFunc(uint i, EBlendFactor sourceColorFactor, EBlendFactor destinationColorFactor, EBlendFactor sourceAlphaFactor, EBlendFactor destinationAlphaFactor)
        {
            EBlendingFactorSrc colorSrc = _convertSrc(sourceColorFactor);
            EBlendingFactorDest colorDst = _convertDst(destinationColorFactor);
            EBlendingFactorSrc alphaSrc = _convertSrc(sourceAlphaFactor);
            EBlendingFactorDest alphaDst = _convertDst(destinationAlphaFactor);

            OpenGL.BlendFuncI(i, colorSrc, colorDst, alphaSrc, alphaDst);
        }

        internal static void BlendEquation(uint i, EBlendOp colorBlendOp, EBlendOp alphaBlendOp)
        {
            EBlendEquationMode alpha = _convert(alphaBlendOp);
            EBlendEquationMode color = _convert(colorBlendOp);

            OpenGL.BlendEquationI(i, color, alpha);
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

        public static void UseProgram(ComputePipeline pipeline)
        {
            uint handle = pipeline.Handle;
            if (handle != _currentProgram)
            {
                _currentProgram = handle;
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

        internal static EBlendEquationMode _convert(EBlendOp op)
        {
            switch (op)
            {
                case EBlendOp.Add:
                    return EBlendEquationMode.FuncAdd;
                case EBlendOp.Subtract:
                    return EBlendEquationMode.FuncSubtract;
                case EBlendOp.ReverseSubtract:
                    return EBlendEquationMode.FuncReverseSubtract;
                case EBlendOp.Minimum:
                    return EBlendEquationMode.Min;
                case EBlendOp.Maximum:
                    return EBlendEquationMode.Max;
            }

            throw new InvalidOperationException("Cannot determine op mode.");
        }

        internal static EBlendingFactorDest _convertDst(EBlendFactor factor)
        {
            switch (factor)
            {
                case EBlendFactor.Zero:
                    return EBlendingFactorDest.Zero;
                case EBlendFactor.One:
                    return EBlendingFactorDest.One;
                case EBlendFactor.SrcColor:
                    return EBlendingFactorDest.SrcColor;
                case EBlendFactor.OneMinusSrcColor:
                    return EBlendingFactorDest.OneMinusSrcColor;
                case EBlendFactor.DstColor:
                    return EBlendingFactorDest.DstColor;
                case EBlendFactor.OneMinusDstColor:
                    return EBlendingFactorDest.OneMinusDstColor;
                case EBlendFactor.SrcAlpha:
                    return EBlendingFactorDest.SrcAlpha;
                case EBlendFactor.OneMinusSrcAlpha:
                    return EBlendingFactorDest.OneMinusSrcAlpha;
                case EBlendFactor.DstAlpha:
                    return EBlendingFactorDest.DstAlpha;
                case EBlendFactor.OneMinusDstAlpha:
                    return EBlendingFactorDest.OneMinusDstAlpha;
                case EBlendFactor.ConstantColor:
                    return EBlendingFactorDest.ConstantColor;
                case EBlendFactor.OneMinusConstantColor:
                    return EBlendingFactorDest.OneMinusConstantColor;
                case EBlendFactor.ConstantAlpha:
                    return EBlendingFactorDest.ConstantAlpha;
                case EBlendFactor.OneMinusConstantAlpha:
                    return EBlendingFactorDest.OneMinusConstantAlpha;
                case EBlendFactor.AlphaSaturate:
                    break;
            }
            throw new InvalidOperationException("Cannot determine factor.");
        }

        internal static EBlendingFactorSrc _convertSrc(EBlendFactor factor)
        {
            switch (factor)
            {
                case EBlendFactor.Zero:
                    return EBlendingFactorSrc.Zero;
                case EBlendFactor.One:
                    return EBlendingFactorSrc.One;
                case EBlendFactor.SrcColor:
                    break;
                case EBlendFactor.OneMinusSrcColor:
                    break;
                case EBlendFactor.DstColor:
                    return EBlendingFactorSrc.DstColor;
                case EBlendFactor.OneMinusDstColor:
                    return EBlendingFactorSrc.OneMinusDstColor;
                case EBlendFactor.SrcAlpha:
                    return EBlendingFactorSrc.SrcAlpha;
                case EBlendFactor.OneMinusSrcAlpha:
                    return EBlendingFactorSrc.OneMinusSrcAlpha;
                case EBlendFactor.DstAlpha:
                    return EBlendingFactorSrc.DstAlpha;
                case EBlendFactor.OneMinusDstAlpha:
                    return EBlendingFactorSrc.OneMinusDstAlpha;
                case EBlendFactor.ConstantColor:
                    return EBlendingFactorSrc.ConstantColor;
                case EBlendFactor.OneMinusConstantColor:
                    return EBlendingFactorSrc.OneMinusConstantColor;
                case EBlendFactor.ConstantAlpha:
                    return EBlendingFactorSrc.ConstantAlpha;
                case EBlendFactor.OneMinusConstantAlpha:
                    return EBlendingFactorSrc.OneMinusConstantAlpha;
                case EBlendFactor.AlphaSaturate:
                    return EBlendingFactorSrc.SrcAlphaSaturate;
            }
            throw new InvalidOperationException("Cannot determine factor.");
        }
    }
}
