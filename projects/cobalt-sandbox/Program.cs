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
            GraphicsContext gfxContext = GraphicsContext.GetInstance(GraphicsContext.API.OpenGL_4);
            IGraphicsApplication gfxApplication = gfxContext.CreateApplication(new IGraphicsApplication.CreateInfo.Builder()
                .Debug(true)
                .Name("Sandbox")
                .Build());

            Window window = new Window();

            while(window.IsOpen())
            {
                window.Refresh();
            }

            GLFW.Terminate();
        }
    }
}