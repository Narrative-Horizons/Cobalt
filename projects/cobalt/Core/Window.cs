using System;
using Cobalt.Bindings.GL;
using Cobalt.Bindings.GLFW;

namespace Cobalt.Core
{
    public class Window
    {
        private GLFWWindow window;

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

            window = GLFW.CreateWindow(800, 600, "Cobalt Engine", GLFWMonitor.None, GLFWWindow.None);

            GLFW.MakeContextCurrent(window);
            GLFW.ShowWindow(window);

            GL.glInit(GLFW.GetProcAddress);
            GL.ClearColor(1.0f, 0.0f, 1.0f, 1.0f);
        }

        public void Refresh()
        {
            GL.Clear(EBufferBit.ColorBuffer);
            GLFW.PollEvents();
            GLFW.SwapBuffers(window);
        }
    }
}