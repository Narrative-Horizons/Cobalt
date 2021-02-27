using System;
using System.Text;
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
            GLFW.ShowWindow(window);
        }

        public void Refresh()
        {
            GLFW.PollEvents();
            GLFW.SwapBuffers(window);
        }
    }
}