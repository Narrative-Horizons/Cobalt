using System;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.Vulkan
{
    public static class VK
    {
		public struct ApiVersion
		{
			public uint major;
			public uint minor;
			public uint patch;
		};
		public struct InstanceCreateInfo
		{
			public ApiVersion appVersion;
			public string appName;
			public ApiVersion engineVersion;
			public string engineName;
			public ApiVersion requiredVersion;
			public ApiVersion desiredVersion;
			public ulong enabledLayerCount;
			public string[] enabledLayers;
			public ulong enabledExtensionCount;
			public string[] enabledExtensions;
			public bool requireValidationLayers;
			public bool useDefaultDebugger;
			public GLFW.GLFWWindow window;
			// TODO: custom debugger and layers
		};

        public struct SwapchainCreateInfo
        {

        }

        public struct RenderPassCreateInfo
        {

        }

		#region DLL Loading
#if COBALT_PLATFORM_WINDOWS
		public const string LIBRARY = "bin/vk-bootstrap-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/vk-bootstrap-native-bindings";
#else
        public const string LIBRARY = "bin/vk-bootstrap-native-bindings";
#endif
		#endregion

		[DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_device", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateInstance(InstanceCreateInfo info);

		[DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_device", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool DestroyInstance(IntPtr instance);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateSwapchain(IntPtr device, SwapchainCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_swapchain", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroySwapchain(IntPtr swapchain);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_create_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateRenderPass(IntPtr device, RenderPassCreateInfo info);

        [DllImport(LIBRARY, EntryPoint = "cobalt_vkb_destroy_renderpass", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyRenderPass(IntPtr device, IntPtr renderpass);
	}
}
