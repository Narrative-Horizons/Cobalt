using System;
using System.Text;
using Cobalt.Bindings.GL;
using Cobalt.Bindings.GLFW;
using Cobalt.Bindings.Utils;

namespace Cobalt.Core
{
    public class Window
    {
        private readonly GLFWWindow _window;

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

            GLFW.WindowHint(Hint.ContextVersionMajor, 4);
            GLFW.WindowHint(Hint.ContextVersionMinor, 6);
            GLFW.WindowHint(Hint.OpenglProfile, Profile.Core);
            _window = GLFW.CreateWindow(1280, 720, "Cobalt Engine", GLFWMonitor.None, GLFWWindow.None);

            GLFW.MakeContextCurrent(_window);
            GLFW.ShowWindow(_window);

            GLFW.SetWindowSizeCallback(_window, Resize);

            if (GL.LoadGLProcAddress(GLFW.GetProcAddress))
            {
                Console.WriteLine("Successfully loaded GLAD.");
            }
            else
            {
                Console.WriteLine("Error on GLAD Init");
            }
            GL.ClearColor(1.0f, 0.0f, 1.0f, 1.0f);
        }

        public bool IsOpen()
        {
            return !GLFW.WindowShouldClose(_window);
        }

        public void Close()
        {
            GLFW.SetWindowShouldClose(_window, true);
        }

        private static void GlfwError(ErrorCode code, IntPtr message)
        {
            throw new Exception(Util.PtrToStringUTF8(message));
        }

        private static void Resize(GLFWWindow window, int width, int height)
        {

        }

        public void Refresh()
        {
            GL.Clear(EBufferBit.ColorBuffer);

            GLFW.PollEvents();
            GLFW.SwapBuffers(_window);
        }
    }
}