using Cobalt.Entities;
using Cobalt.Entities.Components;
using Cobalt.Graphics.API;
using Cobalt.Graphics.Passes;
using System.Collections.Generic;

namespace Cobalt.Graphics
{
    public class RenderSystem
    {
        public Registry EntityRegistry { get; private set; }
        private ICommandPool cmdPool;
        private List<ICommandBuffer> cmdBuffers;
        private IDevice device;
        private readonly uint framesInFlight;
        private int currentFrame = 0;

        private readonly List<RenderPass> passes = new List<RenderPass>();

        private PbrRenderPass _pbrPass;
        private ScreenResolvePass _screenResolvePass;
        private IFrameBuffer[] FrameBuffer;
        private ISwapchain _swapChain;

        private IImageView[] colorAttachmentViews;

        private ISampler imageResolveSampler;
        private IQueue _submitQueue;

        public RenderSystem(Registry registry, IDevice device, ISwapchain swapchain)
        {
            this.framesInFlight = swapchain.GetImageCount();
            this.device = device;
            _swapChain = swapchain;

            EntityRegistry = registry;
            cmdPool = device.CreateCommandPool(new ICommandPool.CreateInfo.Builder().ResetAllocations(true));
            cmdBuffers = cmdPool.Allocate(new ICommandBuffer.AllocateInfo.Builder().Count(framesInFlight).Level(ECommandBufferLevel.Primary).Build());

            _pbrPass = new PbrRenderPass(device, (int) swapchain.GetImageCount(), registry);
            /// TODO: Resize this
            _screenResolvePass = new ScreenResolvePass(swapchain, device, 1280, 720);

            passes.Add(_pbrPass);
            passes.Add(_screenResolvePass);

            FrameBuffer = new IFrameBuffer[framesInFlight];
            IImage[] colorAttachments = new IImage[framesInFlight];
            IImage[] depthAttachments = new IImage[framesInFlight];
            colorAttachmentViews = new IImageView[framesInFlight];
            IImageView[] depthAttachmentViews = new IImageView[framesInFlight];

            _submitQueue = device.Queues().Find(queue => queue.GetProperties().Graphics);

            for (int i = 0; i < framesInFlight; i++)
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

            imageResolveSampler = device.CreateSampler(new ISampler.CreateInfo.Builder().AddressModeU(EAddressMode.Repeat)
                .AddressModeV(EAddressMode.Repeat).AddressModeW(EAddressMode.Repeat).MagFilter(EFilter.Linear).MinFilter(EFilter.Linear)
                .MipmapMode(EMipmapMode.Linear));
        }

        public void render()
        {
            ICommandBuffer cmdBuffer = cmdBuffers[currentFrame];

            ComponentView<DebugCameraComponent> cameraView = EntityRegistry.GetView<DebugCameraComponent>();
            cameraView.ForEach((camera) =>
            {
                cmdBuffer.Record(new ICommandBuffer.RecordInfo());

                _pbrPass.Camera = camera;
                _pbrPass.Record(cmdBuffer, new RenderPass.FrameInfo
                {
                    FrameBuffer = FrameBuffer[currentFrame],
                    FrameInFlight = currentFrame
                });

                var frameInfo = new RenderPass.FrameInfo { FrameInFlight = currentFrame };
                _screenResolvePass.SetInputTexture(new Cobalt.Graphics.Texture() { Image = colorAttachmentViews[currentFrame], Sampler = imageResolveSampler }, frameInfo);
                _screenResolvePass.Record(cmdBuffer, frameInfo);

                camera.Update();
            });

            cmdBuffer.End();
            _submitQueue.Execute(new IQueue.SubmitInfo(cmdBuffer));

            // Compute visibility pass

            // Z Pass
            // Opaque Pass
            // Translucent Pass
            // Resolve Pass
            // Post Processing

            currentFrame = (currentFrame + 1) % (int) framesInFlight;
        }
    }

    internal class ZPass : RenderPass
    {
        public ZPass(IDevice device) : base(device)
        {

        }

        public override void Preprocess(Entity ent, Registry reg)
        {
            // Process Z data
        }

        public override void Record(ICommandBuffer buffer, FrameInfo info)
        {
        }
    }
}
