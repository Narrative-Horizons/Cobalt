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
	}
}
