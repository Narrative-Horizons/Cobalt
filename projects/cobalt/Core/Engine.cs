using Cobalt.Bindings.PhysX;
using Cobalt.Entities;
using Cobalt.Graphics;
using Cobalt.Graphics.API;
using Cobalt.Graphics.GL;
using Cobalt.Physics;

namespace Cobalt.Core
{
    public class Engine<Application>
    {
        #region Private Data
        private Application _app;
        private static Engine<Application> _instance = null;
        #endregion

        #region Properties
        public IGraphicsApplication GraphicsApplication { get; internal set; }
        public IPhysicalDevice GPU { get; internal set; }
        public IDevice Device { get; internal set; }
        public IRenderSurface Surface { get; internal set; }
        public ISwapchain SwapChain { get; internal set; }
        public GraphicsContext Context { get; internal set; }
        public Window Window { get; internal set; }
        public PhysicsSystem Physics { get; internal set; }
        public AssetManager Assets { get; internal set; }
        public Registry Registry { get; internal set; }
        public RenderableManager RenderableManager { get; internal set; }
        #endregion

        private Engine(Application app)
        {
            _app = app;
        }

        public static void Initialize(Application app)
        {
            _instance ??= new Engine<Application>(app);
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
                Assets = new AssetManager(Device);
                RenderableManager = new RenderableManager(Device);
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
                    SwapChain.Present(new ISwapchain.PresentInfo());
                }

                application.Cleanup();
            }

            PhysX.Destroy();
            Context.Dispose();
        }

        public IGraphicsApplication CreateGraphicsApplication(IGraphicsApplication.CreateInfo createInfo)
        {
            GraphicsApplication = Context.CreateApplication(createInfo);
            return GraphicsApplication;
        }

        public IPhysicalDevice CreatePhysicalDevice(IPhysicalDevice.CreateInfo createInfo)
        {
            GPU = GraphicsApplication.GetPhysicalDevices().Find(gpu => gpu.SupportsGraphics() && gpu.SupportsPresent() && gpu.SupportsCompute() && gpu.SupportsTransfer());
            return GPU;
        }

        public IDevice CreateDevice(IDevice.CreateInfo createInfo)
        {
            Device = GPU.Create(createInfo);
            Surface = Device.GetSurface(Window);

            return Device;
        }

        public Window CreateWindow(Window.CreateInfo createInfo)
        {
            Window = Context.CreateWindow(createInfo);
            return Window;
        }

        public GraphicsContext CreateGraphicsContext(GraphicsContext.API api)
        {
            Context = GraphicsContext.GetInstance(api);
            return Context;
        }

        public ISwapchain CreateSwapChain(ISwapchain.CreateInfo createInfo)
        {
            SwapChain = Surface.CreateSwapchain(createInfo);
            return SwapChain;
        }
    }
}
