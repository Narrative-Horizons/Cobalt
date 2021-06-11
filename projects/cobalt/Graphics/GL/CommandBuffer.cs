using static Cobalt.Bindings.GL.EMemoryBarrier;

using Cobalt.Graphics.API;
using Cobalt.Graphics.GL.Commands;
using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class CommandBuffer : ICommandBuffer
    {
        public List<ICommand> Commands { get; private set; } = new List<ICommand>();

        private bool _recording = false;
        private bool _reset = false;

        public CommandBuffer(ICommandBuffer.AllocateInfo info, bool resetFlag)
        {
            _reset = resetFlag;
        }

        public void BeginRenderPass(ICommandBuffer.RenderPassBeginInfo info)
        {
            List<ClearValue> clearValues = new List<ClearValue>(info.ClearValues);
            for(int i = 0; i < info.RenderPass.GetAttachments().Count; i++)
            {
                IRenderPass.AttachmentDescription attachmentInfo = info.RenderPass.GetAttachments()[i];
                if(attachmentInfo.LoadOp != EAttachmentLoad.Clear)
                {
                    clearValues[i] = null;
                }
            }

            Commands.Add(new BindFramebufferCommand((FrameBuffer)info.FrameBuffer));
            Commands.Add(new ClearFrameBufferCommand((FrameBuffer)info.FrameBuffer, clearValues));
        }

        public void Bind(IGraphicsPipeline pipeline)
        {
            Commands.Add(new GL.Commands.BindGraphicsPipelineCommand(pipeline as GraphicsPipeline));
        }

        public void Bind(IVertexAttributeArray vao)
        {
            Commands.Add(new GL.Commands.BindVertexArrayObjectCommand(vao as VertexAttributeArray));
        }

        public void Bind(IPipelineLayout layout, int firstSet, List<IDescriptorSet> sets)
        {
            Commands.Add(new BindDescriptorSetsCommand(layout, sets));
        }

        public void Bind(IPipelineLayout layout, int firstSet, List<IDescriptorSet> sets, List<uint> offsets)
        {
            Commands.Add(new BindDynamicDescriptorSetsCommand(sets, offsets));
        }

        public void Bind(EBufferUsage usage, IBuffer buffer)
        {
            Commands.Add(new BindBufferCommand(usage, buffer));
        }

        public void Copy(IBuffer source, IBuffer destination, List<ICommandBuffer.BufferCopyRegion> regions)
        {

        }

        public void Dispose()
        {
            Commands.Clear();
        }

        public void Draw(int baseVertex, int vertexCount, int baseInstance, int instanceCount)
        {
            Commands.Add(new DrawArraysCommand(baseVertex, vertexCount, baseInstance, instanceCount));
        }

        public void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount)
        {
            DrawElements(elementCount, baseVertex, baseInstance, instanceCount, 0);
        }

        public void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount, long indexOffset)
        {
            Commands.Add(new DrawElementsCommand(baseVertex, elementCount, baseInstance, instanceCount, indexOffset));
        }

        public void DrawElementsMultiIndirect(DrawElementsIndirectCommand indirect, int offset, IBuffer indirectBuffer)
        {
            Commands.Add(new MultiDrawElementsIndirectCommand(indirect, offset, indirectBuffer));
        }

        public void End()
        {
            _recording = false;
        }

        public void Execute()
        {
            foreach(ICommand com in Commands)
            {
                com.Execute();
            }
        }

        public void Record(ICommandBuffer.RecordInfo info)
        {
            Reset();
            _recording = true;
        }

        public void Reset()
        {
            if (_reset)
            {
                Commands.Clear();
            }
        }

        public void Copy(byte[] source, IImage destination, List<ICommandBuffer.BufferImageCopyRegion> regions)
        {
            regions.ForEach(region =>
            {
                Image image = (Image)destination;
                switch (image.Type)
                {
                    case EImageType.Image1D:
                        break;
                    case EImageType.Image2D:
                        if(image.LayerCount == 1)
                        {
                            Commands.Add(new TextureSubImage2DCommand(image, source, region.MipLevel,
                                region.X, region.Y, region.Width, region.Height, ToInternalFormat(image.Format), ToType(image.Format),
                                region.BufferOffset));
                        }
                        break;
                    case EImageType.Image3D:
                        break;
                    case EImageType.ImageCube:
                        break;
                }
            });
        }

        public void Sync()
        {
            Commands.Add(new WaitSyncCommand());
        }

        public void Barrier(List<ICommandBuffer.BufferMemoryBarrier> memoryBarriers, List<ICommandBuffer.ImageMemoryBarrier> imageBarriers)
        {
            Bindings.GL.EMemoryBarrier barrierFlags = 0;
            memoryBarriers.ForEach(barrier =>
            {
                barrierFlags |= ToBarrierBit(barrier.SrcAccess);
            });

            imageBarriers.ForEach(barrier =>
            {
                barrierFlags |= ToBarrierBit(barrier.SrcAccess);
            });

            Commands.Add(new MemoryBarrierCommand(barrierFlags));
        }

        private static Bindings.GL.EPixelInternalFormat ToInternalFormat(EDataFormat format)
        {
            switch (format)
            {
                case EDataFormat.Unknown:
                    break;
                case EDataFormat.BGRA8_SRGB:
                    break;
                case EDataFormat.R32G32_SFLOAT:
                    break;
                case EDataFormat.R32G32B32_SFLOAT:
                    return Bindings.GL.EPixelInternalFormat.Rgb;
                case EDataFormat.R8G8B8A8_SRGB:
                case EDataFormat.R8G8B8A8:
                case EDataFormat.R32G32B32A32_SFLOAT:
                    return Bindings.GL.EPixelInternalFormat.Rgba;
            }

            throw new InvalidOperationException("Format unsupported");
        }

        private static Bindings.GL.EPixelType ToType(EDataFormat format)
        {
            switch (format)
            {
                case EDataFormat.Unknown:
                    break;
                case EDataFormat.BGRA8_SRGB:
                    break;
                case EDataFormat.R8G8B8A8_SRGB:
                case EDataFormat.R8G8B8A8:
                    return Bindings.GL.EPixelType.UnsignedByte;
                case EDataFormat.R32G32_SFLOAT:
                case EDataFormat.R32G32B32_SFLOAT:
                case EDataFormat.R32G32B32A32_SFLOAT:
                    return Bindings.GL.EPixelType.Float;
            }

            throw new InvalidOperationException("Format unsupported");
        }
    
        private static Bindings.GL.EMemoryBarrier ToBarrierBit(EAccessFlag access)
        {
            switch (access)
            {
                case EAccessFlag.IndirectCommandReadBit:
                    return CommandBit;
                case EAccessFlag.IndexReadBit:
                    return ElementArrayBit;
                case EAccessFlag.VertexAttributeReadBit:
                    return VertexAttribArrayBit;
                case EAccessFlag.UniformReadBit:
                    return UniformBit;
                case EAccessFlag.InputAttachmentReadBit:
                    return TextureFetchBit | ShaderImageAccessBit | PixelBufferBit | FramebufferBit;
                case EAccessFlag.ShaderReadBit:
                    return UniformBit | ShaderStorageBit | TextureFetchBit | ShaderImageAccessBit | PixelBufferBit | FramebufferBit | TransformFeedbackBit | QueryBufferBit | AtomicCounterBit;
                case EAccessFlag.ShaderWriteBit:
                    return UniformBit | ShaderStorageBit | TextureFetchBit | ShaderImageAccessBit | PixelBufferBit | FramebufferBit | TransformFeedbackBit | QueryBufferBit | AtomicCounterBit;
                case EAccessFlag.ColorAttachmentReadBit:
                    return TextureFetchBit | ShaderImageAccessBit | PixelBufferBit | FramebufferBit;
                case EAccessFlag.ColorAttachmentWriteBit:
                    return TextureFetchBit | ShaderImageAccessBit | PixelBufferBit | FramebufferBit;
                case EAccessFlag.DepthStencilReadBit:
                    return TextureFetchBit | ShaderImageAccessBit | PixelBufferBit | FramebufferBit;
                case EAccessFlag.DepthStencilWriteBit:
                    return TextureFetchBit | ShaderImageAccessBit | PixelBufferBit | FramebufferBit;
                case EAccessFlag.TransferReadBit:
                    return TextureUpdateBit | BufferUpdateBit;
                case EAccessFlag.TransferWriteBit:
                    return TextureUpdateBit | BufferUpdateBit;
                case EAccessFlag.HostReadBit:
                    return ClientMappedBufferBit | TextureUpdateBit | BufferUpdateBit;
                case EAccessFlag.HostWriteBit:
                    return ClientMappedBufferBit | TextureUpdateBit | BufferUpdateBit;
                case EAccessFlag.MemoryReadBit:
                    return AllBits;
                case EAccessFlag.MemoryWriteBit:
                    return AllBits;
            }

            throw new InvalidOperationException("Access flag unsupported");
        }
    }
}
