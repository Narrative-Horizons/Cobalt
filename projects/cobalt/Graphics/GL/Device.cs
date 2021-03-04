using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class Device : IDevice
    {
        public bool Debug { get; private set; }

        private readonly List<IQueue> _queues = new List<IQueue>();
        private readonly Dictionary<Window, RenderSurface> _surfaces = new Dictionary<Window, RenderSurface>();

        public List<RenderPass> Passes { get; private set; } = new List<RenderPass>();
        public List<FrameBuffer> FrameBuffers { get; private set; } = new List<FrameBuffer>();
        public List<Buffer> Buffers { get; private set; } = new List<Buffer>();
        public List<Image> Images { get; private set; } = new List<Image>();
        public List<ShaderModule> Modules { get; private set; } = new List<ShaderModule>();
        public List<DescriptorSetLayout> DescSetLayouts { get; private set; } = new List<DescriptorSetLayout>();
        public List<PipelineLayout> Layouts { get; private set; } = new List<PipelineLayout>();
        public List<GraphicsPipeline> GraphicsPipelines { get; private set; } = new List<GraphicsPipeline>();
        public List<CommandPool> CommandPools { get; private set; } = new List<CommandPool>();

        public Device(IDevice.CreateInfo info)
        {
            info.QueueInformation.ForEach(info =>
            {
                _queues.Add(new Queue(info));
            });

            Debug = info.Debug;
        }

        public void Dispose()
        {
            _queues.ForEach(queue => queue.Dispose());
        }

        public List<IQueue> Queues()
        {
            return _queues;
        }

        public IRenderSurface GetSurface(Window window)
        {
            if (!_surfaces.ContainsKey(window))
            {
                _surfaces[window] = new RenderSurface(window);
            }
            return _surfaces[window];
        }

        public IRenderPass CreateRenderPass(IRenderPass.CreateInfo info)
        {
            RenderPass pass = new RenderPass(info);
            Passes.Add(pass);

            return pass;
        }

        public IFrameBuffer CreateFrameBuffer(IFrameBuffer.CreateInfo info)
        {
            FrameBuffer fb = new FrameBuffer(info);
            FrameBuffers.Add(fb);

            return fb;
        }

        public IBuffer CreateBuffer(IBuffer.CreateInfo info, IBuffer.MemoryInfo memory)
        {
            Buffer buffer = new Buffer(memory, info);
            Buffers.Add(buffer);

            return buffer;
        }

        public IImage CreateImage(IImage.CreateInfo info, IImage.MemoryInfo memory)
        {
            Image image = new Image(memory, info);
            Images.Add(image);

            return image;
        }

        public IShaderModule CreateShaderModule(IShaderModule.CreateInfo info)
        {
            ShaderModule module = new ShaderModule(info);
            Modules.Add(module);

            return module;
        }

        public IDescriptorSetLayout CreateDescriptorSetLayout(IDescriptorSetLayout.CreateInfo info)
        {
            DescriptorSetLayout layout = new DescriptorSetLayout(info);
            DescSetLayouts.Add(layout);

            return layout;
        }

        public IPipelineLayout CreatePipelineLayout(IPipelineLayout.CreateInfo info)
        {
            PipelineLayout layout = new PipelineLayout(info);
            Layouts.Add(layout);

            return layout;
        }

        public IGraphicsPipeline CreateGraphicsPipeline(IGraphicsPipeline.CreateInfo info)
        {
            GraphicsPipeline pipeline = new GraphicsPipeline(info);
            GraphicsPipelines.Add(pipeline);

            return pipeline;
        }

        public ICommandPool CreateCommandPool(ICommandPool.CreateInfo info)
        {
            CommandPool pool = new CommandPool(info);
            CommandPools.Add(pool);

            return pool;
        }
    }
}
