using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Text;

#pragma warning disable 0419

namespace Cobalt.Bindings.GLFW
{
    [SuppressUnmanagedCodeSecurity]
    public static class GLFW
    {
#if true
        public const string LIBRARY = "../GLFW.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "libglfw.3"; // mac
#else
        public const string LIBRARY = "glfw";
#endif

        public static readonly int True = 1;
        public static readonly int False = 0;

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

        [DllImport(LIBRARY, EntryPoint = "glfwWindowHint", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WindowHint(GLFWHint hint, int value);

        [DllImport(LIBRARY, EntryPoint = "glfwMakeContextCurrent", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MakeContextCurrent(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwGetWindowOpacity", CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetWindowOpacity(IntPtr window);

        [DllImport(LIBRARY, EntryPoint = "glfwSetWindowOpacity", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowOpacity(IntPtr window, float opacity);

        [DllImport(LIBRARY, EntryPoint = "glfwWindowHintString", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WindowHintString(GLFWHint hint, byte[] value);

        [DllImport(LIBRARY, EntryPoint = "glfwGetWindowContentScale", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetWindowContentScale(IntPtr window, out float xScale, out float yScale);

        [DllImport(LIBRARY, EntryPoint = "glfwGetMonitorWorkarea", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetMonitorWorkArea(IntPtr monitor, out int x, out int y, out int width,
            out int height);

        [DllImport(LIBRARY, EntryPoint = "glfwCreateWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern GLFWWindow CreateWindow(int width, int height, byte[] title, GLFWMonitor monitor, GLFWWindow share);

        public static GLFWWindow CreateWindow(int width, int height, [NotNull] string title, GLFWMonitor monitor, GLFWWindow share)
        {
            return CreateWindow(width, height, Encoding.UTF8.GetBytes(title), monitor, share);
        }

        [DllImport(LIBRARY, EntryPoint = "glfwShowWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShowWindow(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwSwapBuffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SwapBuffers(GLFWWindow window);

        [DllImport(LIBRARY, EntryPoint = "glfwPollEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PollEvents();

        [DllImport(LIBRARY, EntryPoint = "glfwGetProcAddress", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetProcAddress(byte[] procName);
    }
}