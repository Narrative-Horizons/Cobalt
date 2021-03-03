using Cobalt.Graphics;
using Cobalt.Math;
using System;
using System.IO;
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

            Cobalt.Graphics.GL.Buffer glBuf = new Graphics.GL.Buffer(new IBuffer.MemoryInfo.Builder().AddRequiredProperty(EMemoryProperty.DeviceLocal)
                .AddRequiredProperty(EMemoryProperty.HostVisible).Usage(EMemoryUsage.CPUToGPU),
                new IBuffer.CreateInfo.Builder().AddUsage(EBufferUsage.ArrayBuffer).InitialPayload(objectData).Size(Marshal.SizeOf(objectData[0]) * objectData.Length));

            string vsSource = "#version 460 \nvoid main()\n{\ngl_Position = vec4(0, 0, 0, 1);\n}";
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(vsSource);
            writer.Flush();
            stream.Position = 0;

            Cobalt.Graphics.GL.ShaderModule glShader = new Graphics.GL.ShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.Vertex).ResourceStream(stream));

            Graphics.GL.GraphicsPipeline glPipeline = new Graphics.GL.GraphicsPipeline(new IGraphicsPipeline.CreateInfo.Builder().AddStageCreationInformation(
                new IGraphicsPipeline.ShaderStageCreateInfo.Builder().Module(glShader).EntryPoint("main")));

            while (window.IsOpen())
            {
                window.Refresh();
            }

            gfxContext.Dispose();
        }
    }
}