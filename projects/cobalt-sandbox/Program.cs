using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Graphics;
using Cobalt.Graphics.API;
using Cobalt.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            IRenderSurface surface = device.GetSurface(window);
            ISwapchain swapchain = surface.CreateSwapchain(new ISwapchain.CreateInfo.Builder().Width(1280).Height(720).ImageCount(2).Layers(1).Build());

            Registry reg = new Registry();
            RenderSystem renderSystem = new RenderSystem(reg, device, swapchain);

            AssetManager assetManager = new AssetManager();

            ICommandPool commandPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().Queue(graphicsQueue).ResetAllocations(true).TransientAllocations(true));
            List<ICommandBuffer> commandBuffers = commandPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(swapchain.GetImageCount()).Level(ECommandBufferLevel.Primary).Build());

            ICommandPool transferPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().Queue(transferQueue).ResetAllocations(false).TransientAllocations(true));
            ICommandBuffer transferCmdBuffer = transferPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(1).Level(ECommandBufferLevel.Primary))[0];

            Core.ImageAsset assetAlbedo = assetManager.LoadImage("data/Dragon/T_MountainDragon_BaseColor.jpg");
            IImage albedoImage = device.CreateImage(new IImage.CreateInfo.Builder()
                    .Depth(1).Format(EDataFormat.R8G8B8A8).Height((int)assetAlbedo.Height).Width((int)assetAlbedo.Width)
                    .InitialLayout(EImageLayout.Undefined).LayerCount(1).MipCount(1).SampleCount(ESampleCount.Samples1)
                    .Type(EImageType.Image2D),
                new IImage.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal).Usage(EMemoryUsage.GPUOnly));

            Core.ImageAsset assetNormal = assetManager.LoadImage("data/Dragon/T_MountainDragon_Normal.jpg");
            IImage normalImage = device.CreateImage(new IImage.CreateInfo.Builder()
                    .Depth(1).Format(EDataFormat.R8G8B8A8).Height((int)assetNormal.Height).Width((int)assetNormal.Width)
                    .InitialLayout(EImageLayout.Undefined).LayerCount(1).MipCount(1).SampleCount(ESampleCount.Samples1)
                    .Type(EImageType.Image2D),
                new IImage.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal).Usage(EMemoryUsage.GPUOnly));

            Core.ImageAsset assetORM = assetManager.LoadImage("data/Dragon/T_MountainDragon_MetallicSmoothness.tga");
            IImage ORMImage = device.CreateImage(new IImage.CreateInfo.Builder()
                    .Depth(1).Format(EDataFormat.R8G8B8A8).Height((int)assetORM.Height).Width((int)assetORM.Width)
                    .InitialLayout(EImageLayout.Undefined).LayerCount(1).MipCount(1).SampleCount(ESampleCount.Samples1)
                    .Type(EImageType.Image2D),
                new IImage.MemoryInfo.Builder()
                    .AddRequiredProperty(EMemoryProperty.DeviceLocal).Usage(EMemoryUsage.GPUOnly));

            transferCmdBuffer.Copy(assetAlbedo.AsBytes, albedoImage, new List<ICommandBuffer.BufferImageCopyRegion>(){new ICommandBuffer.BufferImageCopyRegion.Builder().ArrayLayer(0)
                .BufferOffset(0).ColorAspect(true).Depth(1).Height((int) assetAlbedo.Height).Width((int) assetAlbedo.Width).MipLevel(0).Build() });

            transferCmdBuffer.Copy(assetNormal.AsBytes, normalImage, new List<ICommandBuffer.BufferImageCopyRegion>(){new ICommandBuffer.BufferImageCopyRegion.Builder().ArrayLayer(0)
                .BufferOffset(0).ColorAspect(true).Depth(1).Height((int) assetNormal.Height).Width((int) assetNormal.Width).MipLevel(0).Build() });

            transferCmdBuffer.Copy(assetORM.AsBytes, ORMImage, new List<ICommandBuffer.BufferImageCopyRegion>(){new ICommandBuffer.BufferImageCopyRegion.Builder().ArrayLayer(0)
                .BufferOffset(0).ColorAspect(true).Depth(1).Height((int) assetORM.Height).Width((int) assetORM.Width).MipLevel(0).Build() });

            IQueue.SubmitInfo transferSubmission = new IQueue.SubmitInfo(transferCmdBuffer);
            transferQueue.Execute(transferSubmission);

            IImageView albedoImageView = albedoImage.CreateImageView(new IImageView.CreateInfo.Builder().ArrayLayerCount(1).BaseArrayLayer(0).BaseMipLevel(0).Format(EDataFormat.R8G8B8A8)
                .MipLevelCount(1).ViewType(EImageViewType.ViewType2D));

            IImageView normalImageView = normalImage.CreateImageView(new IImageView.CreateInfo.Builder().ArrayLayerCount(1).BaseArrayLayer(0).BaseMipLevel(0).Format(EDataFormat.R8G8B8A8)
                .MipLevelCount(1).ViewType(EImageViewType.ViewType2D));

            IImageView ORMImageView = ORMImage.CreateImageView(new IImageView.CreateInfo.Builder().ArrayLayerCount(1).BaseArrayLayer(0).BaseMipLevel(0).Format(EDataFormat.R8G8B8A8)
                .MipLevelCount(1).ViewType(EImageViewType.ViewType2D));

            ISampler albedoImageSampler = device.CreateSampler(new ISampler.CreateInfo.Builder().AddressModeU(EAddressMode.Repeat)
                .AddressModeV(EAddressMode.Repeat).AddressModeW(EAddressMode.Repeat).MagFilter(EFilter.Linear).MinFilter(EFilter.Linear)
                .MipmapMode(EMipmapMode.Linear));

            RenderableManager renderableManager = new RenderableManager(device);

            ModelAsset asset = assetManager.LoadModel("data/Dragon/Dragon.FBX");


            //Entity meshEntity = asset.AsEntity(reg, renderableManager);

            renderableManager.QueueRenderable(asset);

            List<RenderableMesh> meshes = renderableManager.GetRenderables(asset);
            RenderableMesh box = meshes[0];

            Matrix4 trans = Matrix4.Identity;
            trans *= Matrix4.Scale(new Vector3(0.005f));
            trans *= Matrix4.Rotate(new Vector3(90, 0, 0)); 

            Entity helmetEntity = reg.Create();
            reg.Assign(helmetEntity, new MeshComponent(box));
            reg.Assign(helmetEntity, new TransformComponent
            {
                TransformMatrix = trans
            });
            reg.Assign(helmetEntity, new PbrMaterialComponent
            {
                Albedo = new Texture
                {
                    Image = albedoImageView,
                    Sampler = albedoImageSampler
                },
                Normal = new Texture
                {
                    Image = normalImageView,
                    Sampler = albedoImageSampler
                },
                OcclusionRoughnessMetallic = new Texture
                {
                    Image = ORMImageView,
                    Sampler = albedoImageSampler
                }
            });

            Entity cameraEntity = reg.Create();
            reg.Assign(cameraEntity, new TransformComponent());
            reg.Assign(cameraEntity, new DebugCameraComponent(new Vector3(4, 0, 4), Vector3.UnitY));

            Stopwatch sw = new Stopwatch();

            Bindings.GL.GL.Finish();

            float angle = 0.0f;

            while (window.IsOpen())
            {
                trans = Matrix4.Identity;
                trans *= Matrix4.Scale(new Vector3(0.005f));
                trans *= Matrix4.Rotate(new Vector3(90, angle, 0));
                reg.Get<TransformComponent>(helmetEntity).TransformMatrix = trans;

                angle = (angle + 0.01f) % 360.0f;

                window.Poll();
                if(Input.IsKeyPressed(Bindings.GLFW.Keys.Escape))
                {
                    window.Close();
                }

                sw.Restart();
                renderSystem.render();
                sw.Stop();

                swapchain.Present(new ISwapchain.PresentInfo());
                Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            }

            gfxContext.Dispose();
        }
    }
}