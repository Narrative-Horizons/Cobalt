using System;
using Cobalt.Graphics;
using Cobalt.Bindings.GLFW;

namespace Cobalt.Sandbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GraphicsContext gfxContext = GraphicsContext.GetInstance(GraphicsContext.API.OpenGL_4);

            Window window = gfxContext.CreateWindow(new Window.CreateInfo.Builder()
                .Width(1280)
                .Height(720)
                .Name("Cobalt Sandbox")
                .Build());
            IGraphicsApplication gfxApplication = gfxContext.CreateApplication(new IGraphicsApplication.CreateInfo.Builder()
                .Debug(true)
                .Name("Sandbox")
                .Build());

            gfxApplication.GetPhysicalDevices().ForEach(device => Console.WriteLine(device.Name()));

            while (window.IsOpen())
            {
                window.Refresh();
            }

            gfxContext.Dispose();
        }
    }
}