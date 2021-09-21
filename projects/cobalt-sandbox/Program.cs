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

    public class Sandbox : BaseApplication
    {
        public RenderSystem RenderSystem { get; internal set; }

        public override void Setup()
        {
            var engine = Engine<Sandbox>.Instance();

            engine.CreateGraphicsContext(GraphicsContext.API.OpenGL_4);
            engine.CreateWindow(new Window.CreateInfo.Builder()
                .Width(1280)
                .Height(720)
                .Name("Cobalt Sandbox")
                .Build());

            engine.CreateGraphicsApplication(new IGraphicsApplication.CreateInfo.Builder()
                .Debug(true)
                .Name("Sandbox")
                .Build());

            engine.CreatePhysicalDevice(null);

            if (engine.GPU == null)
                return;

            engine.CreateDevice(new IDevice.CreateInfo.Builder()
                .Debug(engine.GPU.Debug())
                .QueueInformation(engine.GPU.QueueInfos())
                .Build());

            engine.CreateSwapChain(new ISwapchain.CreateInfo.Builder().Width(1280).Height(720).ImageCount(2).Layers(1)
                .Build());
        }

        public override void Initialize()
        {
            var engine = Engine<Sandbox>.Instance();

            RenderSystem = new RenderSystem(engine.Registry, engine.Device, engine.SwapChain);
            ModelAsset asset = engine.Assets.LoadModel("data/Sponza/Sponza.gltf");
            Entity meshEntity = asset.AsEntity(engine.Registry, engine.RenderableManager);

            Entity cameraEntity = engine.Registry.Create();
            engine.Registry.Assign(cameraEntity, new TransformComponent());
            engine.Registry.Assign<CameraComponent>(cameraEntity, new FreeLookCamera(65.0f, 0.1f, 5000.0f, 16.0f / 9.0f));
            engine.Registry.Get<TransformComponent>(cameraEntity).Position = new Vector3(1, 2, 1);

            PhysX.MeshData pData = new PhysX.MeshData
            {
                UUID = 0,
                vertices = new PhysX.VertexData[3]
            };

            PhysX.VertexData v = new PhysX.VertexData { x = 0, y = 0, z = 0 };
            PhysX.VertexData v1 = new PhysX.VertexData { x = 1, y = 0, z = 0 };
            PhysX.VertexData v2 = new PhysX.VertexData { x = 1, y = 1, z = 0 };
            pData.vertices[0] = v;
            pData.vertices[1] = v1;
            pData.vertices[2] = v2;

            pData.vertexCount = 3;
            pData.indices = new uint[] { 0, 1, 2 };
            pData.indexCount = 3;

            PhysX.CreateMeshShape(pData);
            // PhysX.CreateMeshCollider(meshEntity.UUID, pData.UUID, 0, 0, 0);
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            RenderSystem.Render();
        }

        public override void Cleanup()
        {
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Engine<Sandbox>.Initialize(new Sandbox());
            Engine<Sandbox>.Instance().Run();
            Engine<Sandbox>.Destruct();
        }
    }
}