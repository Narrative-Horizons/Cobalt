using OpenGL = Cobalt.Bindings.GL.GL;

using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    public class GraphicsPipeline : IGraphicsPipeline
    {
        public List<IVertexAttributeArray> Arrays { get; private set; }
        public IGraphicsPipeline.CreateInfo Info { get; private set; }
        public uint Handle { get; private set; }

        public GraphicsPipeline(IGraphicsPipeline.CreateInfo info)
        {
            Info = info;

            Handle = OpenGL.CreateProgram();

            Info.StageCreateInformation.ForEach(stage =>
            {
                ShaderModule module = stage.Module as ShaderModule;
                OpenGL.AttachShader(Handle, module.Handle);
            });

            OpenGL.LinkProgram(Handle);
            OpenGL.GetProgramiv(Handle, Bindings.GL.EProgramParameter.LinkStatus, out int linkStatus);
            if(linkStatus != 1)
            {
                string log = OpenGL.GetProgramInfoLog(Handle);
                throw new InvalidOperationException(log);
            }

            OpenGL.ValidateProgram(Handle);
            OpenGL.GetProgramiv(Handle, Bindings.GL.EProgramParameter.ValidateStatus, out int validateStatus);
            if(validateStatus != 1)
            {
                string log = OpenGL.GetProgramInfoLog(Handle);
                throw new InvalidOperationException(log);
            }

            Info.StageCreateInformation.ForEach(stage =>
            {
                ShaderModule module = stage.Module as ShaderModule;
                OpenGL.DetachShader(Handle, module.Handle);
            });
        }

        public IVertexAttributeArray createVertexAttributeArray(List<IBuffer> vertexBuffers)
        {
            throw new NotImplementedException();
        }

        public IVertexAttributeArray createVertexAttributeArray(List<IBuffer> vertexBuffers, IBuffer elementBuffer)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            OpenGL.DeleteProgram(Handle);
        }

        public IPipelineLayout GetLayout()
        {
            throw new NotImplementedException();
        }
    }
}
