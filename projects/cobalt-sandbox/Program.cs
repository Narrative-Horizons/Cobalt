using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Graphics;
using Cobalt.Graphics.API;
using Cobalt.Math;
using System.Runtime.InteropServices;
using Cobalt.Bindings.PhysX;
using Cobalt.Physics;

namespace Cobalt.Sandbox
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector3 position;
        public Vector2 uv;
    }

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

            IPhysicalDevice physicalDevice = gfxApplication.GetPhysicalDevices()
                .Find(gpu => gpu.SupportsGraphics() && gpu.SupportsPresent() && gpu.SupportsCompute() && gpu.SupportsTransfer());

            if (physicalDevice == null)
                return;

            IDevice device = physicalDevice.Create(new IDevice.CreateInfo.Builder()
                .Debug(physicalDevice.Debug())
                .QueueInformation(physicalDevice.QueueInfos())
                .Build());

            IRenderSurface surface = device.GetSurface(window);
            ISwapchain swapchain = surface.CreateSwapchain(new ISwapchain.CreateInfo.Builder().Width(1280).Height(720).ImageCount(2).Layers(1).Build());

            Registry reg = new Registry();
            RenderSystem renderSystem = new RenderSystem(reg, device, swapchain);

            PhysicsSystem physicsSystem = new PhysicsSystem(reg);

            AssetManager assetManager = new AssetManager(device);

            RenderableManager renderableManager = new RenderableManager(device);

            ModelAsset asset = assetManager.LoadModel("data/Sponza/Sponza.gltf");
            Entity meshEntity = asset.AsEntity(reg, renderableManager);

            Entity cameraEntity = reg.Create();
            reg.Assign(cameraEntity, new TransformComponent());
            reg.Assign<CameraComponent>(cameraEntity, new FreeLookCamera(65.0f, 0.1f, 5000.0f, 16.0f / 9.0f));
            reg.Get<TransformComponent>(cameraEntity).Position = new Vector3(1, 2, 1);

            PhysX.Init();
            PhysX.MeshData pData = new PhysX.MeshData
            {
                UUID = 0, 
                vertices = new PhysX.VertexData[3]
            };

            PhysX.VertexData v = new PhysX.VertexData {x = 0, y = 0, z = 0};
            PhysX.VertexData v1 = new PhysX.VertexData {x = 1, y = 0, z = 0};
            PhysX.VertexData v2 = new PhysX.VertexData {x = 1, y = 1, z = 0};
            pData.vertices[0] = v;
            pData.vertices[1] = v1;
            pData.vertices[2] = v2;

            pData.vertexCount = 3;
            pData.indices = new uint[] {0, 1, 2};
            pData.indexCount = 3;

            PhysX.CreateMeshShape(pData);
            // PhysX.CreateMeshCollider(meshEntity.UUID, pData.UUID, 0, 0, 0);

            physicsSystem.Simulate();

            while (window.IsOpen())
            {
                // reg.Get<TransformComponent>(meshEntity).TransformMatrix *= Matrix4.Rotate(new Vector3(0, 0.01f, 0));
                window.Poll();
                if (Input.IsKeyPressed(Bindings.GLFW.Keys.Escape))
                {
                    window.Close();
                }

                physicsSystem.Update();

                renderSystem.Render();
                physicsSystem.Simulate();
                swapchain.Present(new ISwapchain.PresentInfo());
            }

            PhysX.Destroy();

            gfxContext.Dispose();
        }
    }
}