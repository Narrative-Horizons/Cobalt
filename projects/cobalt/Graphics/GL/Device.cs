using Cobalt.Bindings.Utils;
using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenGL = Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL
{
    internal class Device : IDevice
    {
        public bool Debug { get; private set; }

        private readonly List<IQueue> _queues = new List<IQueue>();
        private readonly Dictionary<Window, RenderSurface> _surfaces = new Dictionary<Window, RenderSurface>();

        public List<RenderPass> Passes { get; private set; } = new List<RenderPass>();
        public List<FrameBuffer> FrameBuffers { get; private set; } = new List<FrameBuffer>();
        public List<IBuffer> Buffers { get; private set; } = new List<IBuffer>();
        public List<Image> Images { get; private set; } = new List<Image>();
        public List<ShaderModule> Modules { get; private set; } = new List<ShaderModule>();
        public List<DescriptorPool> DescriptorPools { get; private set; } = new List<DescriptorPool>();
        public List<DescriptorSetLayout> DescSetLayouts { get; private set; } = new List<DescriptorSetLayout>();
        public List<PipelineLayout> Layouts { get; private set; } = new List<PipelineLayout>();
        public List<GraphicsPipeline> GraphicsPipelines { get; private set; } = new List<GraphicsPipeline>();
        public List<CommandPool> CommandPools { get; private set; } = new List<CommandPool>();
        public List<Sampler> Samplers { get; private set; } = new List<Sampler>();

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
            foreach (var pair in _surfaces)
            {
                pair.Value.Dispose();
            }
            
            _queues.ForEach(queue => queue.Dispose());
            Passes.ForEach(pass => pass.Dispose());
            FrameBuffers.ForEach(fb => fb.Dispose());
            Buffers.ForEach(buf => buf.Dispose());
            Images.ForEach(img => img.Dispose());
            Modules.ForEach(mod => mod.Dispose());
            DescSetLayouts.ForEach(set => set.Dispose());
            Layouts.ForEach(layout => layout.Dispose());
            GraphicsPipelines.ForEach(pipe => pipe.Dispose());
            CommandPools.ForEach(cp => cp.Dispose());
            DescriptorPools.ForEach(dp => dp.Dispose());
            Samplers.ForEach(samp => samp.Dispose());

            _surfaces.Clear();
            Passes.Clear();
            FrameBuffers.Clear();
            Buffers.Clear();
            Images.Clear();
            Modules.Clear(); ;
            DescSetLayouts.Clear();
            Layouts.Clear();
            GraphicsPipelines.Clear();
            CommandPools.Clear();
            DescriptorPools.Clear();
            Samplers.Clear();
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

        public IBuffer CreateBuffer<T>(IBuffer.CreateInfo<T> info, IBuffer.MemoryInfo memory) where T : unmanaged
        {
            Buffer<T> buffer = new Buffer<T>(memory, info);
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

        public IDescriptorPool CreateDescriptorPool(IDescriptorPool.CreateInfo info)
        {
            DescriptorPool pool = new DescriptorPool(info);
            DescriptorPools.Add(pool);

            return pool;
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

        public ISampler CreateSampler(ISampler.CreateInfo info)
        {
            Sampler sampler = new Sampler(info);
            Samplers.Add(sampler);

            return sampler;
        }

        public Shader CreateShader(Shader.CreateInfo info, IPipelineLayout layout)
        {
            Shader shader = new Shader(info, this, layout);

            return shader;
        }

        public void UpdateDescriptorSets(List<DescriptorWriteInfo> writeInformation)
        {
            List<IDescriptorSet> sets = new List<IDescriptorSet>();
            writeInformation.ForEach(writeInfo =>
            {
                DescriptorSet s = (DescriptorSet)writeInfo.DescriptorSet;

                s.Write(writeInformation);
            });
        }

        public IVertexAttributeArray CreateVertexAttributeArray(List<IBuffer> vertexBuffers, IBuffer indexBuffer, List<VertexAttribute> layout)
        {
            VertexAttributeArray vao = new VertexAttributeArray(new IGraphicsPipeline.VertexAttributeCreateInfo.Builder()
                .Attributes(layout).Build(), vertexBuffers, indexBuffer);

            return vao;
        }
    }
}
