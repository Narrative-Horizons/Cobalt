using OpenGL = Cobalt.Bindings.GL.GL;

using Cobalt.Graphics.API;
using System;

namespace Cobalt.Graphics.GL
{
    internal class ComputePipeline : IComputePipeline
    {
        public IComputePipeline.CreateInfo CreateInfo { get; private set; }
        public uint Handle { get; private set; }

        public ComputePipeline(IComputePipeline.CreateInfo info)
        {
            CreateInfo = info;

            // Build compute shader
            Handle = OpenGL.CreateProgram();
            ShaderModule module = CreateInfo.Stage.Module as ShaderModule;
            OpenGL.AttachShader(Handle, module.Handle);

            OpenGL.LinkProgram(Handle);
            OpenGL.GetProgramiv(Handle, Bindings.GL.EProgramParameter.LinkStatus, out int linkStatus);
            if (linkStatus != 1)
            {
                string log = OpenGL.GetProgramInfoLog(Handle);
                throw new InvalidOperationException(log);
            }

            OpenGL.ValidateProgram(Handle);
            OpenGL.GetProgramiv(Handle, Bindings.GL.EProgramParameter.ValidateStatus, out int validateStatus);
            if (validateStatus != 1)
            {
                string log = OpenGL.GetProgramInfoLog(Handle);
                throw new InvalidOperationException(log);
            }

            OpenGL.DetachShader(Handle, module.Handle);
        }

        public void Dispose()
        {
            OpenGL.DeleteProgram(Handle);
        }

        public IPipelineLayout GetLayout()
        {
            return CreateInfo.Layout;
        }
    }
}
