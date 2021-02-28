using System;
using Cobalt.Bindings.GL;
using Cobalt.Bindings.GLFW;

namespace Cobalt.Core
{
    public class Window
    {
        private readonly GLFWWindow window;

        static Window()
        {
            
        }

        public Window()
        {
            if (!GLFW.Init())
            {
                Console.WriteLine("Error on GLFW Init");
            }
            else
            {
                Console.WriteLine("Successfully initialized GLFW");
            }

            GLFW.SetErrorCallback(GlfwError);

            window = GLFW.CreateWindow(1280, 720, "Cobalt Engine", GLFWMonitor.None, GLFWWindow.None);

            GLFW.MakeContextCurrent(window);
            GLFW.ShowWindow(window);

            GL.glInit(GLFW.GetProcAddress);
            GL.ClearColor(1.0f, 0.0f, 1.0f, 1.0f);
        }

        public bool IsOpen()
        {
            return !GLFW.WindowShouldClose(window);
        }

        public void Close()
        {
            GLFW.SetWindowShouldClose(window, true);
        }

        private static void GlfwError(GLFWErrorCode code, IntPtr message)
        {
            throw new Exception(GLFWUtil.PtrToStringUTF8(message));
        }

        public void Refresh()
        {
            GL.Clear(EBufferBit.ColorBuffer);
            GLFW.PollEvents();
            GLFW.SwapBuffers(window);
        }
    }
}