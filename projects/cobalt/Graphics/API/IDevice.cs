using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface IDevice : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder Debug(bool debug)
                {
                    base.Debug = debug;
                    return this;
                }

                public Builder AddQueueInformation(IQueue.CreateInfo info)
                {
                    base.QueueInformation.Add(info);
                    return this;
                }

                public new Builder QueueInformation(List<IQueue.CreateInfo> infos)
                {
                    base.QueueInformation.Clear();
                    base.QueueInformation.AddRange(infos);
                    return this;
                }
                
                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo
                    {
                        Debug = base.Debug,
                        QueueInformation = new List<IQueue.CreateInfo>(base.QueueInformation)
                    };
                    return info;
                }
            }

            public List<IQueue.CreateInfo> QueueInformation { get; private set; } = new List<IQueue.CreateInfo>();

            public bool Debug { get; private set; }
        }

        public List<IQueue> Queues();

        public IRenderSurface GetSurface(Window window);
        public IRenderPass CreateRenderPass(IRenderPass.CreateInfo info);
        public IFrameBuffer CreateFrameBuffer(IFrameBuffer.CreateInfo info);
        public IBuffer CreateBuffer<T>(IBuffer.CreateInfo<T> info, IBuffer.MemoryInfo memory) where T : unmanaged;
        public IImage CreateImage(IImage.CreateInfo info, IImage.MemoryInfo memory);
        public IShaderModule CreateShaderModule(IShaderModule.CreateInfo info);
        public IVertexAttributeArray CreateVertexAttributeArray(List<IBuffer> vertexBuffers, IBuffer indexBuffer, List<VertexAttribute> layout);
        public IDescriptorPool CreateDescriptorPool(IDescriptorPool.CreateInfo info);
        public IDescriptorSetLayout CreateDescriptorSetLayout(IDescriptorSetLayout.CreateInfo info);
        public IPipelineLayout CreatePipelineLayout(IPipelineLayout.CreateInfo info);
        public IGraphicsPipeline CreateGraphicsPipeline(IGraphicsPipeline.CreateInfo info);
        public ICommandPool CreateCommandPool(ICommandPool.CreateInfo info);
        public ISampler CreateSampler(ISampler.CreateInfo info);
        public Shader CreateShader(Shader.CreateInfo info, IPipelineLayout layout);
        public void UpdateDescriptorSets(List<DescriptorWriteInfo> writeInformation);
    }
}
