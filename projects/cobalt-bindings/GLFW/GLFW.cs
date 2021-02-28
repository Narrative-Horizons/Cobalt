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
        public const string LIBRARY = "../GLFW.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "../libglfw.3"; // mac
#else
        public const string LIBRARY = "../glfw";
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
        [DllImport(LIBRARY, EntryPoint = "glfwInit", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Init();

        [DllImport(LIBRARY, EntryPoint = "glfwTerminate", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Terminate();

        [DllImport(LIBRARY, EntryPoint = "glfwGetMonitorContentScale", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetMonitorContentScale(IntPtr monitor, out float xScale, out float yScale);

        [DllImport(LIBRARY, EntryPoint = "glfwGetMonitorUserPointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetMonitorUserPointer(IntPtr monitor);

        [DllImport(LIBRARY, EntryPoint = "glfwSetMonitorUserPointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMonitorUserPointer(IntPtr monitor, IntPtr pointer);

        [DllImport(LIBRARY, EntryPoint = "glfwSetWindowUserPointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowUserPointer(GLFWWindow window, IntPtr userPointer);

        [DllImport(LIBRARY, EntryPoint = "glfwGetWindowUserPointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetWindowUserPointer(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwGetCurrentContext", CallingConvention = CallingConvention.Cdecl)]
        private static extern GLFWWindow GetCurrentContext();

        [DllImport(LIBRARY, EntryPoint = "glfwGetPrimaryMonitor", CallingConvention = CallingConvention.Cdecl)]
        private static extern GLFWMonitor GetPrimaryMonitor();

        [DllImport(LIBRARY, EntryPoint = "glfwMaximizeWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MaximizeWindow(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwGetVersionString", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetVersionString();

        [DllImport(LIBRARY, EntryPoint = "glfwGetTime", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetTime();

        [DllImport(LIBRARY, EntryPoint = "glfwSetTime", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetTime(double time);

        [DllImport(LIBRARY, EntryPoint = "glfwGetTimerFrequency", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong GetTimerFrequency();

        [DllImport(LIBRARY, EntryPoint = "glfwGetTimerValue", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong GetTimerValue();

        [DllImport(LIBRARY, EntryPoint = "glfwGetVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetVersion(out int major, out int minor, out int revision);

        [DllImport(LIBRARY, EntryPoint = "glfwSwapInterval", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SwapInterval(int interval);

        [DllImport(LIBRARY, EntryPoint = "glfwGetVideoMode", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetVideoModeInternal(GLFWMonitor monitor);

        [DllImport(LIBRARY, EntryPoint = "glfwGetVideoModes", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetVideoModes(GLFWMonitor monitor, out int count);

        [DllImport(LIBRARY, EntryPoint = "glfwGetWindowMonitor", CallingConvention = CallingConvention.Cdecl)]
        public static extern GLFWMonitor GetWindowMonitor(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwSetWindowMonitor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowMonitor(GLFWWindow window, GLFWMonitor monitor, int x, int y, int width, int height, int refreshRate);

        [DllImport(LIBRARY, EntryPoint = "glfwGetCursorPos", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetCursorPosition(GLFWWindow window, out double x, out double y);

        [DllImport(LIBRARY, EntryPoint = "glfwSetCursorPos", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCursorPosition(GLFWWindow window, double x, double y);

        [DllImport(LIBRARY, EntryPoint = "glfwWindowHint", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WindowHint(Hint hint, int value);

        [DllImport(LIBRARY, EntryPoint = "glfwWindowShouldClose", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool WindowShouldClose(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwSetWindowShouldClose", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowShouldClose(GLFWWindow window, bool close);

        [DllImport(LIBRARY, EntryPoint = "glfwMakeContextCurrent", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MakeContextCurrent(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwGetWindowOpacity", CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetWindowOpacity(IntPtr window);

        [DllImport(LIBRARY, EntryPoint = "glfwSetWindowOpacity", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowOpacity(IntPtr window, float opacity);

        [DllImport(LIBRARY, EntryPoint = "glfwGetMonitorWorkarea", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetMonitorWorkArea(IntPtr monitor, out int x, out int y, out int width, out int height);

        [DllImport(LIBRARY, EntryPoint = "glfwCreateWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern GLFWWindow CreateWindow(int width, int height, byte[] title, GLFWMonitor monitor, GLFWWindow share);

        [DllImport(LIBRARY, EntryPoint = "glfwSetWindowTitle", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetWindowTitle(GLFWWindow window, byte[] title);

        [DllImport(LIBRARY, EntryPoint = "glfwShowWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShowWindow(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwSwapBuffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SwapBuffers(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwPollEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PollEvents();

        [DllImport(LIBRARY, EntryPoint = "glfwGetProcAddress", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetProcAddress(byte[] procName);

        [DllImport(LIBRARY, EntryPoint = "glfwGetError", CallingConvention = CallingConvention.Cdecl)]
        private static extern ErrorCode GetErrorPrivate(out IntPtr description);

        [DllImport(LIBRARY, EntryPoint = "glfwSetErrorCallback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(ErrorCallback))]
        public static extern ErrorCallback SetErrorCallback(ErrorCallback errorHandler);

        [DllImport(LIBRARY, EntryPoint = "glfwSetWindowSizeCallback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(SizeCallback))]
        public static extern SizeCallback SetWindowSizeCallback(GLFWWindow window, SizeCallback sizeCallback);

        [DllImport(LIBRARY, EntryPoint = "glfwSetCursorPosCallback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(MouseCallback))]
        public static extern MouseCallback SetCursorPositionCallback(GLFWWindow window, MouseCallback mouseCallback);

        [DllImport(LIBRARY, EntryPoint = "glfwSetMouseButtonCallback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(MouseButtonCallback))]
        public static extern MouseButtonCallback SetMouseButtonCallback(GLFWWindow window, MouseButtonCallback mouseCallback);

        [DllImport(LIBRARY, EntryPoint = "glfwSetCharCallback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.FunctionPtr, MarshalTypeRef = typeof(CharCallback))]
        public static extern CharCallback SetCharCallback(GLFWWindow window, CharCallback charCallback);

        [DllImport(LIBRARY, EntryPoint = "glfwSetKeyCallback", CallingConvention = CallingConvention.Cdecl)]
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