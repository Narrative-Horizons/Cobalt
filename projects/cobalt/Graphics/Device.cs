using System;
using Cobalt.Bindings.Vulkan;
using static Cobalt.Bindings.Vulkan.VK;

namespace Cobalt.Graphics
{
    public class Device : IDisposable
    {
        internal readonly Instance handle;

        private Device(Instance handle)
        {
            this.handle = handle;
        }

        public static Device Create(Window window)
        {
            InstanceCreateInfo createInfo = new InstanceCreateInfo
            {
                appName = "Hello World",
                appVersion =
                {
                    major = 1,
                    minor = 0,
                    patch = 0
                },
                desiredVersion =
                {
                    major = 1,
                    minor = 2,
                    patch = 0
                },
                enabledExtensionCount = 0,
                enabledExtensions = { },
                enabledLayerCount = 0,
                enabledLayers = { },
                engineName = "Cobalt",
                engineVersion =
                {
                    major = 0,
                    minor = 0,
                    patch = 1
                },
                requiredVersion =
                {
                    major = 1,
                    minor = 2,
                    patch = 0
                },
                requireValidationLayers = true,
                useDefaultDebugger = true,
                window = window.Native()
            };

            Instance handle = CreateInstance(createInfo);
            return handle != IntPtr.Zero ? new Device(handle) : null;
        }

        public Shader CreateShader(ShaderCreateInfo createInfo)
        {
            return new Shader(handle, createInfo);
        }

        public RenderPass CreateRenderPass(RenderPassCreateInfo createInfo)
        {
            return VK.CreateRenderPass(handle, createInfo);
        }

        public Framebuffer CreateFramebuffer(FramebufferCreateInfo createInfo)
        {
            return new Framebuffer(handle, createInfo);
        }

        public Image CreateImage(ImageCreateInfo createInfo, ImageMemoryCreateInfo memoryInfo, string name, uint frame)
        {
            return new Image(this, createInfo, memoryInfo, name, frame);
        }

        public void Dispose()
        {
            
        }
    }
}
