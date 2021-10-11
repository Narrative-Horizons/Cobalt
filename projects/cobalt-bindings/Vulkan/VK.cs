using System;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.Vulkan
{
    public static class VK
    {
        public struct Instance
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Instance window)
            {
                return window.handle;
            }

            public static explicit operator Instance(IntPtr handle) => new Instance(handle);

            public Instance(IntPtr handle)
            {
                this.handle = handle;
            }

            public void Destroy()
            {
                DestroyInstance(this);
            }
        }

        public struct Device
        {
            public IntPtr handle;

            public static implicit operator IntPtr(Device window)
            {
                return window.handle;
            }

            public static explicit operator Device(IntPtr handle) => new Device(handle);

            public Device(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct SwapChain
        {
            public IntPtr handle;

            public static implicit operator IntPtr(SwapChain window)
            {
                return window.handle;
            }

            public static explicit operator SwapChain(IntPtr handle) => new SwapChain(handle);

            public SwapChain(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct RenderPass
        {
            public IntPtr handle;

            public static implicit operator IntPtr(RenderPass window)
            {
                return window.handle;
            }

            public static explicit operator RenderPass(IntPtr handle) => new RenderPass(handle);

            public RenderPass(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        public struct CommandBuffer
        {
            public IntPtr handle;

            public static implicit operator IntPtr(CommandBuffer window)
            {
                return window.handle;
            }

            public static explicit operator CommandBuffer(IntPtr handle) => new CommandBuffer(handle);

            public CommandBuffer(IntPtr handle)
            {
                this.handle = handle;
            }
        }

        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "bin/gfx-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/gfx-native-bindings";
#else
        public const string LIBRARY = "bin/gfx-native-bindings";
#endif
		#endregion

		[DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_device", CallingConvention = CallingConvention.Cdecl)]
        public static extern Instance CreateInstance(InstanceCreateInfo info);

		[DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_device", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DestroyInstance(Instance instance);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern SwapChain CreateSwapchain(Device device, SwapchainCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroySwapchain(SwapChain swapchain);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern RenderPass CreateRenderPass(Device device, RenderPassCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyRenderPass(Device device, RenderPass renderpass);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern CommandBuffer CreateCommandBuffer(Device device, CommandBufferCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_commandbuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyCommandBuffer(Device device, CommandBuffer buffer, uint index);
    }
}
