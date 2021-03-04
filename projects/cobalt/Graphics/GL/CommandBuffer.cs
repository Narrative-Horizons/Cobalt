using Cobalt.Graphics.GL.Commands;
using Cobalt.Math;
using System;
using System.Collections.Generic;
using System.Text;

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
            List<Vector4> clearValues = new List<Vector4>(info.ClearValues);
            for(int i = 0; i < info.RenderPass.GetAttachments().Count; i++)
            {
                IRenderPass.AttachmentDescription attachmentInfo = info.RenderPass.GetAttachments()[i];
                if(attachmentInfo.LoadOp == EAttachmentLoad.Clear)
                {
                    clearValues[i] = null;
                }
            }

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

        public void Bind(IPipelineLayout layout, int firstSet)
        {
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
            commands.Add(new GL.Commands.DrawArraysCommand(baseVertex, vertexCount, baseInstance, instanceCount));
        }

        public void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount)
        {
        }

        public void DrawElements(int elementCount, int baseVertex, int baseInstance, int instanceCount, long indexOffset)
        {
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
    }
}
