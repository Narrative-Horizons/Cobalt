using Cobalt.Graphics;
using Cobalt.Math;
using System;
using System.Runtime.InteropServices;
using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Sandbox
{
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
                .Find(device => device.SupportsGraphics() && device.SupportsPresent() && device.SupportsCompute() && device.SupportsTransfer());
            IDevice device = physicalDevice.Create(new IDevice.CreateInfo.Builder()
                .Debug(physicalDevice.Debug())
                .QueueInformation(physicalDevice.QueueInfos())
                .Build());
            IQueue graphicsQueue = device.Queues().Find(queue => queue.GetProperties().Graphics);
            IQueue presentQueue  = device.Queues().Find(queue => queue.GetProperties().Present);
            IQueue transferQueue = device.Queues().Find(queue => queue.GetProperties().Transfer);


            VertexData[] objectData = new VertexData[3];
            objectData[0].position = new Vector3(-.5f, -.5f, 0f);
            objectData[0].uv = new Vector2(0, 0);

            objectData[1].position = new Vector3(.5f, -.5f, 0f);
            objectData[1].uv = new Vector2(1, 0);
            
            objectData[2].position = new Vector3(0f, .5f, 0f);
            objectData[2].uv = new Vector2(.5f, 1);

            GCHandle handle1 = GCHandle.Alloc(objectData);
            IntPtr param = (IntPtr)handle1;

            uint id = OpenGL.CreateBuffers();
            OpenGL.NamedBufferStorage(id, sizeof(float) * 5 * 3, param, Bindings.GL.EMapBit.MapReadBit);

            handle1.Free();

            IntPtr map = OpenGL.MapNamedBuffer(id, Bindings.GL.EAccessType.ReadOnly);
            GCHandle handle2 = (GCHandle)map;
            VertexData[] objectData2 = (handle2.Target as VertexData[]);

            /*Cobalt.Graphics.GL.Buffer glBuf = new Graphics.GL.Buffer(new IBuffer.MemoryInfo.Builder().AddRequiredProperty(EMemoryProperty.DeviceLocal)
                .AddRequiredProperty(EMemoryProperty.HostVisible).Usage(EMemoryUsage.CPUToGPU),
                new IBuffer.CreateInfo.Builder().AddUsage(EBufferUsage.ArrayBuffer).InitialPayload(objectData).Size(sizeof(float) * 5 * 3));

            VertexData[] mapData = (VertexData[])glBuf.Map();*/

            while (window.IsOpen())
            {
                window.Refresh();
            }

            gfxContext.Dispose();
        }
    }
}