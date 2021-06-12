using Cobalt.Core;
using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Cobalt.Graphics
{
    public class ComputeShader : IDisposable
    {
        public IComputePipeline _pipeline;
        public void Dispose()
        {

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ComputeBuffer
        {
            public int amount;
            public int result;
            public int number;
        }

        public IBuffer computeBuffer;

        private IDevice device;

        public ComputeShader(IDevice device, string computeSource)
        {
            this.device = device;

            IShaderModule computeModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.Compute)
                .ResourceStream(new MemoryStream(Encoding.UTF8.GetBytes(computeSource))));

            IPipelineLayout layout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder().AddDescriptorSetLayout(device.CreateDescriptorSetLayout(
                    new IDescriptorSetLayout.CreateInfo.Builder()
                    .AddBinding(new IDescriptorSetLayout.DescriptorSetLayoutBinding.Builder()
                        .BindingIndex(0)
                        .Count(1)
                        .DescriptorType(EDescriptorType.StorageBuffer)
                        .Name("ComputeData")
                        .AddAccessibleStage(EShaderType.Compute).Build())
                    .Build()))
                .Build());

            IDescriptorPool pool = device.CreateDescriptorPool(new IDescriptorPool.CreateInfo.Builder().AddPoolSize(EDescriptorType.StorageBuffer, 1).MaxSetCount(1).Build());
            IDescriptorSetLayout lay = layout.GetDescriptorSetLayouts()[0];
            IDescriptorSet set = pool.Allocate(new IDescriptorSet.CreateInfo.Builder().AddLayout(lay).Build())[0];

            _pipeline = device.CreateComputePipeline(new IComputePipeline.CreateInfo.Builder().Stage(new ShaderStageCreateInfo.Builder().EntryPoint("main").Module(computeModule).Build()).Layout(layout).Build());

            computeBuffer = device.CreateBuffer(new IBuffer.CreateInfo<ComputeBuffer>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(16), 
                new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));

            DescriptorWriteInfo writeInfo = new DescriptorWriteInfo.Builder().BindingIndex(0).DescriptorSet(set).ArrayElement(0).AddBufferInfo(new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                .Offset(0).Range(16).Buffer(computeBuffer).Build()).Build();

            device.UpdateDescriptorSets(new List<DescriptorWriteInfo>() { writeInfo });
        }

        public void Update()
        {
            ComputeBuffer data = new ComputeBuffer();
            data.amount = 1;
            data.result = 0;
            data.number = 244;

            NativeBuffer<ComputeBuffer> nativeData = new NativeBuffer<ComputeBuffer>(computeBuffer.Map());
            //ComputeBuffer compare = nativeData.Get();
            nativeData.Set(data);
            computeBuffer.Unmap();
        }
    }
}
