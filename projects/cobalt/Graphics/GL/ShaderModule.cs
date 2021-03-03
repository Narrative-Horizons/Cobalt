using OpenGL = Cobalt.Bindings.GL.GL;

using System;
using System.IO;
using System.Text;

namespace Cobalt.Graphics.GL
{
    public class ShaderModule : IShaderModule
    {
        public uint Handle { get; private set; }
        public EShaderType ShaderType { get; private set; }

        public ShaderModule(IShaderModule.CreateInfo info)
        {
            MemoryStream memoryStream = new MemoryStream();
            info.ResourceStream.CopyTo(memoryStream);

            string contents = Encoding.Default.GetString(memoryStream.ToArray());

            Handle = OpenGL.CreateShader(ToNativeType(info.Type));
            OpenGL.ShaderSource(Handle, contents);
            OpenGL.CompileShader(Handle);

            OpenGL.GetShaderiv(Handle, Bindings.GL.EShaderParameter.CompileStatus, out int compileStatus);
            if(compileStatus == 0)
            {
                string status = OpenGL.GetShaderInfoLog(Handle);
                throw new InvalidOperationException(status);
            }
        }

        public void Dispose()
        {
            OpenGL.DeleteShader(Handle);
        }

        public EShaderType Type()
        {
            return ShaderType;
        }

        private Bindings.GL.EShaderType ToNativeType(EShaderType type)
        {
            switch (type)
            {
                case EShaderType.Vertex:
                    return Bindings.GL.EShaderType.VertexShader;
                case EShaderType.TessellationControl:
                    return Bindings.GL.EShaderType.TessControlShader;
                case EShaderType.TessellationEvaluation:
                    return Bindings.GL.EShaderType.TessEvaluationShader;
                case EShaderType.Geometry:
                    return Bindings.GL.EShaderType.GeometryShader;
                case EShaderType.Fragment:
                    return Bindings.GL.EShaderType.FragmentShader;
                case EShaderType.Compute:
                    return Bindings.GL.EShaderType.ComputeShader;
            }

            throw new InvalidOperationException("Shader type is unknown");
        }
    }
}
