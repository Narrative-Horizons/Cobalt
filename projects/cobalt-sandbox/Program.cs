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

            IRenderSurface surface = device.GetSurface(window);
            ISwapchain swapchain = surface.CreateSwapchain(new ISwapchain.CreateInfo.Builder().Width(1280).Height(720).ImageCount(2).Layers(1).Build());

            IRenderPass renderPass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder().AddAttachment(new IRenderPass.AttachmentDescription.Builder().InitialLayout(EImageLayout.Undefined)
                .FinalLayout(EImageLayout.PresentSource).LoadOp(EAttachmentLoad.Clear).StoreOp(EAttachmentStore.Store).Format(EDataFormat.BGRA8_SRGB)));

            IPipelineLayout layout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder().AddDescriptorSetLayout(device.CreateDescriptorSetLayout(
                new IDescriptorSetLayout.CreateInfo.Builder().AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                .AddAccessibleStage(EShaderType.Fragment).BindingIndex(0).DescriptorType(EDescriptorType.CombinedImageSampler).Count(1).Name("albedo").Build()).Build())).Build());

            VertexData[] objectData = new VertexData[3];
            objectData[0].position = new Vector3(-.5f, -.5f, 0f);
            objectData[0].uv = new Vector2(0, 0);

            objectData[1].position = new Vector3(.5f, -.5f, 0f);
            objectData[1].uv = new Vector2(1, 0);
            
            objectData[2].position = new Vector3(0f, .5f, 0f);
            objectData[2].uv = new Vector2(.5f, 1);

           

            IBuffer buf = device.CreateBuffer(new IBuffer.CreateInfo.Builder().AddUsage(EBufferUsage.ArrayBuffer).InitialPayload(objectData).Size(Marshal.SizeOf(objectData[0]) * objectData.Length),
                new IBuffer.MemoryInfo.Builder().AddRequiredProperty(EMemoryProperty.DeviceLocal)
                .AddRequiredProperty(EMemoryProperty.HostVisible).Usage(EMemoryUsage.CPUToGPU));

            string vsSource = "#version 460 \nvoid main()\n{\ngl_Position = vec4(0, 0, 0, 1);\n}";
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(vsSource);
            writer.Flush();
            stream.Position = 0;

            IShaderModule shaderModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.Vertex).ResourceStream(stream));

            IGraphicsPipeline pipeline = device.CreateGraphicsPipeline(new IGraphicsPipeline.CreateInfo.Builder().AddStageCreationInformation(
                new IGraphicsPipeline.ShaderStageCreateInfo.Builder().Module(shaderModule).EntryPoint("main")));

            while (window.IsOpen())
            {

            }

            gfxContext.Dispose();
        }
    }
}