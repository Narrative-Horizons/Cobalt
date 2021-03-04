using Cobalt.Graphics;
using Cobalt.Math;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

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

            ICommandPool commandPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().Queue(graphicsQueue).ResetAllocations(true).TransientAllocations(true));
            List<ICommandBuffer> commandBuffers = commandPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(swapchain.GetImageCount()).Level(ECommandBufferLevel.Primary).Build());

            VertexData[] objectData = new VertexData[3];
            objectData[0].position = new Vector3(-.5f, -.5f, 0f);
            objectData[0].uv = new Vector2(0, 0);

            objectData[1].position = new Vector3(.5f, -.5f, 0f);
            objectData[1].uv = new Vector2(1, 0);
            
            objectData[2].position = new Vector3(0f, .5f, 0f);
            objectData[2].uv = new Vector2(.5f, 1);

            IBuffer buf = device.CreateBuffer(
                IBuffer.FromPayload(objectData).AddUsage(EBufferUsage.ArrayBuffer),
                new IBuffer.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                    .Usage(EMemoryUsage.GPUOnly));

            string vsSource = "#version 460\nlayout (location=0) in vec3 position;\nlayout (location=1) in vec2 uv;\nvoid main()\n{\ngl_Position = vec4(position, 1);\n}";
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(vsSource);
            writer.Flush();
            stream.Position = 0;

            IShaderModule vsshaderModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.Vertex).ResourceStream(stream));

            string fsSource = "#version 460 \nvoid main()\n{\ngl_FragColor = vec4(1, 0, 1, 1);\n}";
            MemoryStream fstream = new MemoryStream();
            StreamWriter fwriter = new StreamWriter(fstream);
            fwriter.Write(fsSource);
            fwriter.Flush();
            fstream.Position = 0;

            IShaderModule fsshaderModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.Fragment).ResourceStream(fstream));

            IGraphicsPipeline pipeline = device.CreateGraphicsPipeline(new IGraphicsPipeline.CreateInfo.Builder()
                .AddStageCreationInformation(
                    new IGraphicsPipeline.ShaderStageCreateInfo.Builder()
                    .Module(vsshaderModule)
                    .EntryPoint("main").Build())
                .AddStageCreationInformation(
                    new IGraphicsPipeline.ShaderStageCreateInfo.Builder()
                    .Module(fsshaderModule)
                    .EntryPoint("main").Build())
                .VertexAttributeCreationInformation(
                    new IGraphicsPipeline.VertexAttributeCreateInfo.Builder()
                    .AddAttribute(
                        new VertexAttribute.Builder()
                            .Binding(0)
                            .Format(EDataFormat.R32G32B32_SFLOAT)
                            .Location(0)
                            .Offset(0)
                            .Rate(EVertexInputRate.PerVertex)
                            .Stride(Marshal.SizeOf(new VertexData())))
                    .AddAttribute(
                        new VertexAttribute.Builder()
                            .Binding(0)
                            .Format(EDataFormat.R32G32_SFLOAT)
                            .Location(1)
                            .Offset(sizeof(float) * 3)
                            .Rate(EVertexInputRate.PerVertex)
                            .Stride(Marshal.SizeOf(new VertexData()))).Build())
                .InputAssemblyCreationInformation(
                    new IGraphicsPipeline.InputAssemblyCreateInfo.Builder()
                        .RestartEnabled(false)
                        .Topology(ETopology.TriangleList))
                .ViewportCreationInformation(
                    new IGraphicsPipeline.ViewportCreateInfo.Builder()
                        .Viewport(new Viewport()
                        {
                            LeftX = 0,
                            UpperY = 0,
                            Width = 1280,
                            Height = 720,
                            MinDepth = 0,
                            MaxDepth = 1
                        })
                        .ScissorRegion(new Scissor()
                        {
                            ExtentX = 1280,
                            ExtentY = 720,
                            OffsetX = 0,
                            OffsetY = 0
                        }))
                .RasterizerCreationInformation(
                    new IGraphicsPipeline.RasterizerCreateInfo.Builder()
                    .DepthClampEnabled(false)
                    .PolygonMode(EPolygonMode.Fill)
                    .WindingOrder(EVertexWindingOrder.Clockwise)
                    .CullFaces(EPolgyonFace.Back)
                    .RasterizerDiscardEnabled(true).Build())
                .MultisamplingCreationInformation(
                    new IGraphicsPipeline.MultisampleCreateInfo.Builder()
                    .AlphaToOneEnabled(false)
                    .AlphaToCoverageEnabled(false)
                    .Samples(ESampleCount.Samples1).Build())
                .PipelineLayout(layout).Build());

            IVertexAttributeArray vao = pipeline.CreateVertexAttributeArray(new List<IBuffer>() { buf });

            int frame = 0;

            foreach (ICommandBuffer buffer in commandBuffers)
            {
                buffer.Record(new ICommandBuffer.RecordInfo());

                buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo()
                {
                    ClearValues = new List<Vector4?>() { new Vector4(0, 0, 0, 1)},
                    Width = 1280,
                    Height = 720,
                    FrameBuffer = swapchain.GetFrameBuffer(frame),
                    RenderPass = renderPass
                });

                buffer.Bind(pipeline);
                buffer.Bind(vao);
                buffer.Draw(0, 3, 0, 1);

                buffer.End();

                frame++;
            }

            frame = 0;

            while (window.IsOpen())
            {
                graphicsQueue.Execute(new IQueue.SubmitInfo(commandBuffers[frame]));

                window.Poll();
                swapchain.Present(new ISwapchain.PresentInfo());

                frame = (frame + 1) % 2;
            }

            gfxContext.Dispose();
        }
    }
}