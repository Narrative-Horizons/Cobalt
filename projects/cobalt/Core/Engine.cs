using Cobalt.Bindings.PhysX;
using Cobalt.Entities;
using Cobalt.Graphics;
using Cobalt.Graphics.API;
using Cobalt.Graphics.GL;
using Cobalt.Physics;
using System;

namespace Cobalt.Core
{
    public class Engine<Application>
    {
        #region Private Data
        private Application _app;
        private static Engine<Application> _instance = null;
        #endregion

        #region Properties
        public GraphicsContext Context { get; internal set; }
        public Window Window { get; internal set; }
        public PhysicsSystem Physics { get; internal set; }
        public RenderSystem Render { get; internal set; }
        public Registry Registry { get; internal set; }
        #endregion

        private Engine(Application app)
        {
            _app = app;
        }

        public static void Initialize(Application app)
        {
            _instance ??= new Engine<Application>(app);
        }

        public void CreateGraphicsContext(Window window)
        {
            Context = new GraphicsContext(window);
        }

        public static Engine<Application> Instance()
        {
            return _instance;
        }

        public static void Destruct()
        {
            _instance = null;
        }

        public void Run()
        {
            if (_app is BaseApplication application)
            {
                application.Setup();

                Registry = new Registry();
                Render = new RenderSystem(Registry, Context);
                Physics = new PhysicsSystem(Registry);
                
                PhysX.Init();
                Physics.Simulate();

                application.Initialize();

                while (Window.IsOpen())
                {
                    Window.Poll();
                    if (Input.IsKeyPressed(Bindings.GLFW.Keys.Escape))
                    {
                        Window.Close();
                    }

                    Physics.Update();

                    application.Update();
                    application.Render();

                    Physics.Sync();

                    Physics.Simulate();
                }

                application.Cleanup();
            }

            PhysX.Destroy();
            Context.Dispose();
        }

        public Window CreateWindow(Window.CreateInfo createInfo)
        {
            Window = new Window(createInfo);
            return Window;
        }
    }
}
