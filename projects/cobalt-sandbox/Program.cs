using System;
using Cobalt.Core;
using Cobalt.Bindings.GLFW;

namespace Cobalt.Sandbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Window window = new Window();

            while(window.IsOpen())
            {
                window.Refresh();
            }

            GLFW.Terminate();
        }
    }
}