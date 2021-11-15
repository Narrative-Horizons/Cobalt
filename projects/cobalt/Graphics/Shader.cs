using System;
using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics
{
    public class Shader : IDisposable
    {
        internal VK.Shader handle;
        internal readonly VK.Instance device;

        internal Shader()
        {

        }

        internal Shader(VK.Instance device, ShaderCreateInfo info)
        {
            this.device = device;
            handle = VK.CreateShader(device, info);
        }

        public void Dispose()
        {
            VK.DestroyShader(device, handle);
        }
    }
}