using OpenGL = Cobalt.Bindings.GL.GL;

using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class GraphicsPipeline : IGraphicsPipeline
    {
        public List<IVertexAttributeArray> Arrays { get; private set; } = new List<IVertexAttributeArray>();
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

        public IVertexAttributeArray CreateVertexAttributeArray(List<IBuffer> vertexBuffers)
        {
            VertexAttributeArray array = new VertexAttributeArray(Info.VertexAttributeCreationInformation, vertexBuffers);
            Arrays.Add(array);

            return array;
        }

        public IVertexAttributeArray CreateVertexAttributeArray(List<IBuffer> vertexBuffers, IBuffer elementBuffer)
        {
            VertexAttributeArray array = new VertexAttributeArray(Info.VertexAttributeCreationInformation, vertexBuffers, elementBuffer);
            Arrays.Add(array);

            return array;
        }

        public void Dispose()
        {
            OpenGL.DeleteProgram(Handle);
        }

        public IPipelineLayout GetLayout()
        {
            return Info.PipelineLayout;
        }

        public Bindings.GL.EBeginMode DrawMode()
        {
            ETopology topo = Info.InputAssemblyCreationInformation.Topology;
            switch (topo)
            {
                case ETopology.PointList:
                    return Bindings.GL.EBeginMode.Points;
                case ETopology.LineList:
                    return Bindings.GL.EBeginMode.Lines;
                case ETopology.LineStrip:
                    return Bindings.GL.EBeginMode.LineStrip;
                case ETopology.TriangleList:
                    return Bindings.GL.EBeginMode.Triangles;
                case ETopology.TriangleStrip:
                    return Bindings.GL.EBeginMode.TriangleStrip;
                case ETopology.TriangleFan:
                    return Bindings.GL.EBeginMode.TriangleFan;
            }

            throw new InvalidOperationException("Unsupported data format");
        }
    }
}
