using GLAD = Cobalt.Bindings.GL.GL;

using Cobalt.Bindings.GLFW;
using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;


namespace Cobalt.Graphics
{
    public class GraphicsContext : IDisposable
    {
        /// <summary>
        /// Supported graphics APIs.
        /// </summary>
        public enum API
        {
            /// <summary>
            /// Load an OpenGL 4 context
            /// </summary>
            OpenGL_4
        }

        public API ContextAPI { get; private set; }

        private static GraphicsContext _context;
        private readonly List<IGraphicsApplication> _applications = new List<IGraphicsApplication>();
        private readonly List<Window> _windows = new List<Window>();

        static GraphicsContext()
        {
            if (!GLFW.Init())
            {
                Console.WriteLine("Error on GLFW Init");
            }
            else
            {
                Console.WriteLine("Successfully initialized GLFW");
            }
        }

        private GraphicsContext(API api)
        {
            
        }

        public static GraphicsContext GetInstance()
        {
            return GetInstance(API.OpenGL_4);
        }

        public static GraphicsContext GetInstance(API api)
        {
            if (_context == null)
            {
                _context = new GraphicsContext(api);
            }
            else if (_context.ContextAPI != api)
            {
                _context._applications.ForEach(application =>
                {
                    application.Dispose();
                });

                _context = new GraphicsContext(api);
            }

            return _context;
        }

        public IGraphicsApplication CreateApplication(IGraphicsApplication.CreateInfo info)
        {
            switch (ContextAPI)
            {
                case API.OpenGL_4:
                    {
                        IGraphicsApplication application = new GL.GraphicsApplication(info);
                        _applications.Add(application);
                        return application;
                    }
            }

            throw new InvalidOperationException("Selected API does not support function.");
        }

        public Window CreateWindow(Window.CreateInfo info)
        {
            Window window = new Window(info);
            _windows.Add(window);

            if (GLAD.LoadGLProcAddress(GLFW.GetProcAddress))
            {
                Console.WriteLine("Successfully loaded GLAD.");
            }
            else
            {
                Console.WriteLine("Error on GLAD Init");
            }

            return window;
        }

        public void Dispose()
        {
            _applications.ForEach(application => application.Dispose());
            _windows.ForEach(window => window.Close());

            _applications.Clear();
            _windows.Clear();

            GLFW.Terminate();
        }
    }
}
