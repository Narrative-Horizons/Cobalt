using Cobalt.Graphics.API;
using Cobalt.Graphics.GL.Commands;
using Cobalt.Math;
using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class CommandBuffer : ICommandBuffer
    {
        public List<ICommand> commands { get; private set; } = new List<ICommand>();

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

            commands.Add(new BindFramebufferCommand((FrameBuffer)info.FrameBuffer));
            commands.Add(new ClearFrameBufferCommand((FrameBuffer)info.FrameBuffer, clearValues));
        }

        public void Bind(IGraphicsPipeline pipeline)
        {
            commands.Add(new GL.Commands.BindGraphicsPipelineCommand(pipeline as GraphicsPipeline));
        }

        public void Bind(IVertexAttributeArray vao)
        {
            commands.Add(new GL.Commands.BindVertexArrayObjectCommand(vao as VertexAttributeArray));
        }

        public void Bind(IPipelineLayout layout, int firstSet, List<IDescriptorSet> sets)
        {
            commands.Add(new BindDescriptorSetsCommand(layout, sets));
        }

        public void Bind(IPipelineLayout layout, int firstSet, List<IDescriptorSet> sets, List<uint> offsets)
        {
            commands.Add(new BindDynamicDescriptorSetsCommand(sets, offsets));
        }

        public void Bind(EBufferUsage usage, IBuffer buffer)
        {
            commands.Add(new BindBufferCommand(usage, buffer));
        }

        public void Copy(IBuffer source, IBuffer destination, List<ICommandBuffer.BufferCopyRegion> regions)
        {

        }

        public void Dispose()
        {
            commands.Clear();
        }

        public void Draw(int baseVertex, int vertexCount, int baseInstance, int instanceCount)
        {
            commands.Add(new DrawArraysCommand(baseVertex, vertexCount, baseInstance, instanceCount));
        }

        public void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount)
        {
            DrawElements(elementCount, baseVertex, baseInstance, instanceCount, 0);
        }

        public void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount, long indexOffset)
        {
            commands.Add(new DrawElementsCommand(baseVertex, elementCount, baseInstance, instanceCount, indexOffset));
        }

        public void DrawElementsMultiIndirect(DrawElementsIndirectCommand indirect, int offset, IBuffer indirectBuffer)
        {
            commands.Add(new MultiDrawElementsIndirectCommand(indirect, offset, indirectBuffer));
        }

        public void End()
        {
            _recording = false;
        }

        public void Execute()
        {
            foreach(ICommand com in commands)
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
                commands.Clear();
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
                            commands.Add(new TextureSubImage2DCommand(image, source, region.MipLevel,
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
            commands.Add(new WaitSyncCommand());
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

        
    }
}
