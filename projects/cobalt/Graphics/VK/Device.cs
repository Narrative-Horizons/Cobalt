using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.VK
{
    public class Device : IDisposable
    {
        private readonly IntPtr _handle;

        private Device(IntPtr handle)
        {
            _handle = handle;
        }

        public static Device Create(Window window)
        {
            Bindings.Vulkan.VK.InstanceCreateInfo createInfo = new Bindings.Vulkan.VK.InstanceCreateInfo
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
            IntPtr handle = Bindings.Vulkan.VK.CreateInstance(createInfo);
            return handle != IntPtr.Zero ? new Device(handle) : null;
        }

        public void Dispose()
        {
            Bindings.Vulkan.VK.DestroyInstance(_handle);
        }
    }
}
