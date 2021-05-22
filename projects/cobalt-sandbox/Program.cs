using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Graphics;
using Cobalt.Graphics.API;
using Cobalt.Math;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cobalt.Sandbox
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector3 position;
        public Vector2 uv;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UniformBufferData
    {
        public Matrix4 projection;
        public Matrix4 view;
        public Matrix4 model;
    }

    public class WindowResizeEvent : EventData
    {
        public uint width, height;

        public WindowResizeEvent(uint width, uint height)
        {
            this.width = width;
            this.height = height;
        }
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
            IQueue presentQueue = device.Queues().Find(queue => queue.GetProperties().Present);
            IQueue transferQueue = device.Queues().Find(queue => queue.GetProperties().Transfer);

            #region Framebuffer

            IFrameBuffer[] FrameBuffer = new IFrameBuffer[2];
            IImage[] colorAttachments = new IImage[2];
            IImage[] depthAttachments = new IImage[2];
            IImageView[] colorAttachmentViews = new IImageView[2];
            IImageView[] depthAttachmentViews = new IImageView[2];

            for (int i = 0; i < 2; i++)
            {
                colorAttachments[i] = device.CreateImage(new IImage.CreateInfo.Builder().AddUsage(EImageUsage.ColorAttachment).Width(1280).Height(720).Type(EImageType.Image2D)
                    .Format(EDataFormat.R8G8B8A8).MipCount(1).LayerCount(1),
                    new IImage.MemoryInfo.Builder().Usage(EMemoryUsage.GPUOnly).AddRequiredProperty(EMemoryProperty.DeviceLocal));

                colorAttachmentViews[i] = colorAttachments[i].CreateImageView(new IImageView.CreateInfo.Builder().ViewType(EImageViewType.ViewType2D).BaseArrayLayer(0)
                    .BaseMipLevel(0).ArrayLayerCount(1).MipLevelCount(1).Format(EDataFormat.R8G8B8A8));

                depthAttachments[i] = device.CreateImage(new IImage.CreateInfo.Builder().AddUsage(EImageUsage.ColorAttachment).Width(1280).Height(720).Type(EImageType.Image2D)
                    .Format(EDataFormat.D24_SFLOAT_S8_UINT).MipCount(1).LayerCount(1),
                    new IImage.MemoryInfo.Builder().Usage(EMemoryUsage.GPUOnly).AddRequiredProperty(EMemoryProperty.DeviceLocal));

                depthAttachmentViews[i] = depthAttachments[i].CreateImageView(new IImageView.CreateInfo.Builder().ViewType(EImageViewType.ViewType2D).BaseArrayLayer(0)
                    .BaseMipLevel(0).ArrayLayerCount(1).MipLevelCount(1).Format(EDataFormat.D24_SFLOAT_S8_UINT));

                FrameBuffer[i] = device.CreateFrameBuffer(new IFrameBuffer.CreateInfo.Builder()
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder().ImageView(colorAttachmentViews[i]).Usage(EImageUsage.ColorAttachment))
                    .AddAttachment(new IFrameBuffer.CreateInfo.Attachment.Builder().ImageView(depthAttachmentViews[i]).Usage(EImageUsage.DepthStencilAttachment)));
            }

            #endregion

            IRenderSurface surface = device.GetSurface(window);
            ISwapchain swapchain = surface.CreateSwapchain(new ISwapchain.CreateInfo.Builder().Width(1280).Height(720).ImageCount(2).Layers(1).Build());

            IRenderPass renderPass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder().AddAttachment(new IRenderPass.AttachmentDescription.Builder().InitialLayout(EImageLayout.Undefined)
                .FinalLayout(EImageLayout.PresentSource).LoadOp(EAttachmentLoad.Clear).StoreOp(EAttachmentStore.Store).Format(EDataFormat.BGRA8_SRGB)));

            IRenderPass screenPass = device.CreateRenderPass(new IRenderPass.CreateInfo.Builder().AddAttachment(new IRenderPass.AttachmentDescription.Builder().InitialLayout(EImageLayout.Undefined)
                .FinalLayout(EImageLayout.PresentSource).LoadOp(EAttachmentLoad.Clear).StoreOp(EAttachmentStore.Store).Format(EDataFormat.BGRA8_SRGB)));

            IPipelineLayout layout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder().AddDescriptorSetLayout(device.CreateDescriptorSetLayout(
                new IDescriptorSetLayout.CreateInfo.Builder().AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder().BindingIndex(0).Count(1)
                .DescriptorType(EDescriptorType.UniformBuffer)
                .AddAccessibleStage(EShaderType.Vertex).Build())
                .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                .AddAccessibleStage(EShaderType.Fragment).BindingIndex(1).DescriptorType(EDescriptorType.CombinedImageSampler).Count(1).Name("albedo").Build()).Build())).Build());

            ICommandPool commandPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().Queue(graphicsQueue).ResetAllocations(true).TransientAllocations(true));
            List<ICommandBuffer> commandBuffers = commandPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(swapchain.GetImageCount()).Level(ECommandBufferLevel.Primary).Build());

            ICommandPool transferPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().Queue(transferQueue).ResetAllocations(false).TransientAllocations(true));
            ICommandBuffer transferCmdBuffer = transferPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(1).Level(ECommandBufferLevel.Primary))[0];

            RenderableManager renderableManager = new RenderableManager(device);
            AssetManager assetManager = new AssetManager();

            ModelAsset asset = assetManager.LoadModel("data/SciFiHelmet/SciFiHelmet.gltf");
            renderableManager.QueueRenderable(asset);

            List<RenderableMesh> meshes = renderableManager.GetRenderables(asset);
            RenderableMesh box = meshes[0];

            Registry reg = new Registry();
            Entity e = reg.Create();
            reg.Assign(e, new MeshComponent(box));
            reg.Assign(e, new TransformComponent());

            #region Shawn Cube

            float[] vertices = {
                -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
                 0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
                 0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                 0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
                 0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
                -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

                -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

                 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                 0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                 0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                 0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                 0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                 0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
                 0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
                 0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
                 0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
            };


            IBuffer buf = device.CreateBuffer(
                IBuffer.FromPayload(vertices).AddUsage(EBufferUsage.ArrayBuffer),
                new IBuffer.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                    .AddRequiredProperty(EMemoryProperty.HostVisible)
                    .Usage(EMemoryUsage.CPUToGPU));

            string vsSource = FileSystem.LoadFileToString("data/standard_vertex.glsl");
            string fsSource = FileSystem.LoadFileToString("data/standard_fragment.glsl");

            Shader shader = device.CreateShader(new Shader.CreateInfo.Builder().VertexSource(vsSource).FragmentSource(fsSource).Build(), layout, true);

            IVertexAttributeArray vao = shader.Pipeline.CreateVertexAttributeArray(new List<IBuffer>() { buf });

            Core.ImageAsset shawnImage = assetManager.LoadImage("data/SciFiHelmet/SciFiHelmet_BaseColor.png");
            IImage logoImage = device.CreateImage(new IImage.CreateInfo.Builder()
                    .Depth(1).Format(EDataFormat.R8G8B8A8).Height((int) shawnImage.Height).Width((int) shawnImage.Width)
                    .InitialLayout(EImageLayout.Undefined).LayerCount(1).MipCount(1).SampleCount(ESampleCount.Samples1)
                    .Type(EImageType.Image2D),
                new IImage.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal).Usage(EMemoryUsage.GPUOnly));


            transferCmdBuffer.Copy(shawnImage.AsBytes, logoImage, new List<ICommandBuffer.BufferImageCopyRegion>(){new ICommandBuffer.BufferImageCopyRegion.Builder().ArrayLayer(0)
                .BufferOffset(0).ColorAspect(true).Depth(1).Height((int) shawnImage.Height).Width((int) shawnImage.Width).MipLevel(0).Build() });
            #endregion

            float[] screenVerts =
            {
                -1, -1, 0,    0, 0,
                1, -1, 0,    1, 0,
                1, 1, 0,    1, 1,

                1, 1, 0,    1, 1,
                -1, 1, 0,   0, 1,
                -1, -1, 0,  0, 0
            };

            IBuffer screenQuadBuf = device.CreateBuffer(
                IBuffer.FromPayload(screenVerts).AddUsage(EBufferUsage.ArrayBuffer),
                new IBuffer.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal)
                    .AddRequiredProperty(EMemoryProperty.HostVisible)
                    .Usage(EMemoryUsage.CPUToGPU));


            IQueue.SubmitInfo transferSubmission = new IQueue.SubmitInfo(transferCmdBuffer);
            transferQueue.Execute(transferSubmission);

            IImageView logoImageView = logoImage.CreateImageView(new IImageView.CreateInfo.Builder().ArrayLayerCount(1).BaseArrayLayer(0).BaseMipLevel(0).Format(EDataFormat.R8G8B8A8)
                .MipLevelCount(1).ViewType(EImageViewType.ViewType2D));

            ISampler logoImageSampler = device.CreateSampler(new ISampler.CreateInfo.Builder().AddressModeU(EAddressMode.Repeat)
                .AddressModeV(EAddressMode.Repeat).AddressModeW(EAddressMode.Repeat).MagFilter(EFilter.Linear).MinFilter(EFilter.Linear)
                .MipmapMode(EMipmapMode.Linear));

            IBuffer uniformBuffer = device.CreateBuffer(IBuffer.FromPayload(new UniformBufferData()).AddUsage(EBufferUsage.UniformBuffer),
                new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));

            IDescriptorPool descriptorPool = device.CreateDescriptorPool(new IDescriptorPool.CreateInfo.Builder().AddPoolSize(EDescriptorType.CombinedImageSampler, 2).MaxSetCount(2).Build());

            List<IDescriptorSet> descriptorSets = descriptorPool.Allocate(new IDescriptorSet.CreateInfo.Builder().AddLayout(layout.GetDescriptorSetLayouts()[0])
                .AddLayout(layout.GetDescriptorSetLayouts()[0]).Build());

            descriptorSets.ForEach(set =>
            {
                DescriptorWriteInfo writeInfo = new DescriptorWriteInfo.Builder()
                    .AddImageInfo(new DescriptorWriteInfo.DescriptorImageInfo.Builder().Layout(EImageLayout.ShaderReadOnly)
                    .Sampler(logoImageSampler).View(logoImageView)).ArrayElement(0).BindingIndex(1).DescriptorSet(set).Build();

                DescriptorWriteInfo writeInfo2 = new DescriptorWriteInfo.Builder()
                    .AddBufferInfo(new DescriptorWriteInfo.DescriptorBufferInfo.Builder().Buffer(uniformBuffer).Offset(0).Range(64).Build())
                    .ArrayElement(0).BindingIndex(0).DescriptorSet(set)
                    .Build();

                device.UpdateDescriptorSets(new List<DescriptorWriteInfo>() { writeInfo, writeInfo2 });
            });

            int frame = 0;
            double time = 0.0;
            double rotSpeed = 5.0;

            DebugCamera cam = new DebugCamera(new Vector3(2, 0, 2), new Vector3(0, 1, 0), 0);
            ScreenResolvePass screenResolve = new ScreenResolvePass(swapchain, device, 1280, 720);

            EventManager.Default.AddHandler((WindowResizeEvent e) =>
            {
                Console.WriteLine(e.width + ", " + e.height);
                return false;
            });

            EventManager.Default.Dispatch(new WindowResizeEvent(200, 200));

            while (window.IsOpen())
            {
                cam.Update();
                ICommandBuffer buffer = commandBuffers[frame];
                buffer.Record(new ICommandBuffer.RecordInfo());

                if(Input.IsKeyPressed(Bindings.GLFW.Keys.Escape))
                {
                    window.Close();
                }

                if(Input.IsKeyDown(Bindings.GLFW.Keys.NumpadAdd))
                {
                    rotSpeed += 0.1;
                }

                if(Input.IsKeyDown(Bindings.GLFW.Keys.NumpadSubtract))
                {
                    rotSpeed -= 0.1;
                }

                buffer.BeginRenderPass(new ICommandBuffer.RenderPassBeginInfo()
                {
                    ClearValues = new List<ClearValue>() { new ClearValue(new ClearValue.ClearColor(0, 0, 0, 1)), new ClearValue(1) },
                    Width = 1280,
                    Height = 720,
                    FrameBuffer = FrameBuffer[frame],
                    RenderPass = renderPass
                });

                MeshComponent meshC = reg.Get<MeshComponent>(e);
                RenderableMesh mesh = meshC.Mesh;

                buffer.Bind(shader.Pipeline);
                buffer.Bind(meshC.Mesh.VAO);
                //buffer.Bind(vao);
                buffer.Bind(layout, 0, new List<IDescriptorSet>() { descriptorSets[frame] });
                //buffer.Draw(0, 36, 0, 1);
                buffer.DrawElements((int) mesh.indexCount, (int) mesh.baseVertex, 0, 1, (int) mesh.baseIndex);

                // Screen Resolve
                var frameInfo = new RenderPass.FrameInfo { FrameInFlight = frame };
                screenResolve.SetInputTexture(new Cobalt.Graphics.Texture() { Image = colorAttachmentViews[frame], Sampler = logoImageSampler }, frameInfo);
                screenResolve.Record(buffer, frameInfo);

                buffer.End();

                Matrix4 model = Matrix4.Rotate(new Vector3(0, (float)time, 0));

                NativeBuffer<UniformBufferData> nativeData = new NativeBuffer<UniformBufferData>(uniformBuffer.Map());
                UniformBufferData data = nativeData.Get(0);
                data.projection = cam.projection;
                data.view = cam.view;
                data.model = model;
                nativeData.Set(data, 0);
                uniformBuffer.Unmap();

                graphicsQueue.Execute(new IQueue.SubmitInfo(commandBuffers[frame]));

                window.Poll();
                swapchain.Present(new ISwapchain.PresentInfo());

                frame = (frame + 1) % 2;
                time += rotSpeed;
            }

            gfxContext.Dispose();
        }
    }
}