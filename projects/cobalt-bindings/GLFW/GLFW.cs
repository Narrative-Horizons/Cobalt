using Cobalt.Bindings.Utils;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Text;

namespace Cobalt.Bindings.GLFW
{
    [SuppressUnmanagedCodeSecurity]
    public static class GLFW
    {
        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "../x86_64/GLFW@3.3.3-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "../x86_64/GLFW@3.3.3-native-bindings";
#else
        public const string LIBRARY = "../x86_64/GLFW@3.3.3-native-bindings";
#endif
        #endregion

        #region Callbacks
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ErrorCallback(ErrorCode code, IntPtr message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SizeCallback(GLFWWindow window, int width, int height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MouseCallback(GLFWWindow window, double x, double y);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MouseButtonCallback(GLFWWindow window, MouseButton button, InputState state, ModifierKeys modifiers);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CharCallback(GLFWWindow window, uint codePoint);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void KeyCallback(GLFWWindow window, Keys key, int scanCode, InputState state, ModifierKeys mods);
        #endregion

        #region DLL Imports
        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_init", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Init();

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_terminate", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Terminate();

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_monitor_content_scale", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetMonitorContentScale(IntPtr monitor, out float xScale, out float yScale);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_monitor_user_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetMonitorUserPointer(IntPtr monitor);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_monitor_user_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMonitorUserPointer(IntPtr monitor, IntPtr pointer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_window_user_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetWindowUserPointer(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_window_user_pointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowUserPointer(GLFWWindow window, IntPtr userPointer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_current_context", CallingConvention = CallingConvention.Cdecl)]
        public static extern GLFWWindow GetCurrentContext();

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_primary_monitor", CallingConvention = CallingConvention.Cdecl)]
        public static extern GLFWMonitor GetPrimaryMonitor();

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_maximize_window", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MaximizeWindow(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_version_string", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetVersionString();

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetTime();

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetTime(double time);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_timer_frequency", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong GetTimerFrequency();

        [DllImport(LIBRARY, EntryPoint = "cobalt_get_time_value", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong GetTimerValue();

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_version", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetVersion(out int major, out int minor, out int revision);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_swap_interval", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SwapInterval(int interval);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_video_mode", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetVideoModeInternal(GLFWMonitor monitor);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_video_modes", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetVideoModes(GLFWMonitor monitor, out int count);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_window_monitor", CallingConvention = CallingConvention.Cdecl)]
        public static extern GLFWMonitor GetWindowMonitor(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_window_monitor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowMonitor(GLFWWindow window, GLFWMonitor monitor, int x, int y, int width, int height, int refreshRate);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_cursor_pos", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetCursorPosition(GLFWWindow window, out double x, out double y);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_cursor_pos", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCursorPosition(GLFWWindow window, double x, double y);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_window_hint", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WindowHint(Hint hint, int value);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_window_should_close", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool WindowShouldClose(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_window_should_close", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowShouldClose(GLFWWindow window, bool close);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_make_context_current", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MakeContextCurrent(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_window_opacity", CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetWindowOpacity(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_window_opacity", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowOpacity(GLFWWindow window, float opacity);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_monitor_workarea", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetMonitorWorkArea(GLFWMonitor monitor, out int x, out int y, out int width, out int height);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_create_window", CallingConvention = CallingConvention.Cdecl)]
        public static extern GLFWWindow CreateWindow(int width, int height, byte[] title, GLFWMonitor monitor, GLFWWindow share);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_window_title", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetWindowTitle(GLFWWindow window, byte[] title);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_show_window", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShowWindow(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_swap_buffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SwapBuffers(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_poll_events", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PollEvents();

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_proc_address", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetProcAddress(IntPtr procName);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_get_error", CallingConvention = CallingConvention.Cdecl)]
        private static extern ErrorCode GetErrorPrivate(out IntPtr description);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_error_callback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(ErrorCallback))]
        public static extern ErrorCallback SetErrorCallback(ErrorCallback errorHandler);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_window_size_callback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(SizeCallback))]
        public static extern SizeCallback SetWindowSizeCallback(GLFWWindow window, SizeCallback sizeCallback);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_cursor_pos_callback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(MouseCallback))]
        public static extern MouseCallback SetCursorPositionCallback(GLFWWindow window, MouseCallback mouseCallback);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_mouse_button_callback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(MouseButtonCallback))]
        public static extern MouseButtonCallback SetMouseButtonCallback(GLFWWindow window, MouseButtonCallback mouseCallback);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_char_callback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(CharCallback))]
        public static extern CharCallback SetCharCallback(GLFWWindow window, CharCallback charCallback);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glfw_set_key_callback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(KeyCallback))]
        public static extern KeyCallback SetKeyCallback(GLFWWindow window, KeyCallback keyCallback);
        #endregion

        #region Public API
        public static ErrorCode GetError(out string description)
        {
            var code = GetErrorPrivate(out var ptr);
            description = code == ErrorCode.None ? null : Util.PtrToStringUTF8(ptr);
            return code;
        }

        public static void SetWindowTitle(GLFWWindow window, string title)
        {
            SetWindowTitle(window, Encoding.UTF8.GetBytes(title));
        }

        public static GLFWWindow CreateWindow(int width, int height, [NotNull] string title, GLFWMonitor monitor, GLFWWindow share)
        {
            return CreateWindow(width, height, Encoding.UTF8.GetBytes(title), monitor, share);
        }

        public static void WindowHint(Hint hint, Profile value) 
        { 
            WindowHint(hint, (int)value); 
        }
        #endregion
    }
}