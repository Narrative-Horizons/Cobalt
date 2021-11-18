using System;
using Cobalt.Bindings.Vulkan;
using Cobalt.Graphics.Enums;

namespace Cobalt.Graphics
{
    public class Shader : IDisposable
    {
        internal VK.Shader handle;
        internal readonly VK.Instance device;
        internal PipelineBindPoint bindPoint;

        internal Shader()
        {

        }

        internal Shader(VK.Instance device, ShaderCreateInfo info)
        {
            this.device = device;

            bindPoint = info.computeModulePath != null ? PipelineBindPoint.Compute : PipelineBindPoint.Graphics;
            handle = VK.CreateShader(device, info);
        }

        public void Dispose()
        {
            VK.DestroyShader(device, handle);
        }
    }
}