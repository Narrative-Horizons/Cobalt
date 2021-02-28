using System;
using Cobalt.Core;
using Cobalt.Graphics;
using Cobalt.Bindings.GLFW;

namespace Cobalt.Sandbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Window window = new Window();

            GraphicsContext gfxContext = GraphicsContext.GetInstance(GraphicsContext.API.OpenGL_4);
            IGraphicsApplication gfxApplication = gfxContext.CreateApplication(new IGraphicsApplication.CreateInfo.Builder()
                .Debug(true)
                .Name("Sandbox")
                .Build());
            gfxApplication.GetPhysicalDevices().ForEach(device => Console.WriteLine(device.Name()));

            while (window.IsOpen())
            {
                window.Refresh();
            }

            GLFW.Terminate();
        }
    }
}