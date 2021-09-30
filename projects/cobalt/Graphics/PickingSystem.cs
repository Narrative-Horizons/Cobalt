using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cobalt.Core;
using Cobalt.Entities;
using Cobalt.Graphics.API;
using Cobalt.Graphics.GL;

namespace Cobalt.Graphics
{
    internal class PickingSystem
    {
        internal struct PickingData
        {
            public int mx;
            public int my;

            public uint identifier;
            public uint generation;
        }

        internal IPipelineLayout _layout;
        internal IDescriptorSet _set;
        internal IComputePipeline _pipeline;
        internal IBuffer _computeBuffer;
        internal PickingSystem(IDevice device)
        {
            IShaderModule computeModule = device.CreateShaderModule(new IShaderModule.CreateInfo.Builder().Type(EShaderType.Compute)
                .ResourceStream(new MemoryStream(Encoding.UTF8.GetBytes(FileSystem.LoadFileToString("data/shaders/picking/compute.glsl")))));

            _layout = device.CreatePipelineLayout(new IPipelineLayout.CreateInfo.Builder().AddDescriptorSetLayout(device.CreateDescriptorSetLayout(
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
            IDescriptorSetLayout lay = _layout.GetDescriptorSetLayouts()[0];
            _set = pool.Allocate(new IDescriptorSet.CreateInfo.Builder().AddLayout(lay).Build())[0];

            _pipeline = device.CreateComputePipeline(new IComputePipeline.CreateInfo.Builder().Stage(new ShaderStageCreateInfo.Builder().EntryPoint("main").Module(computeModule).Build()).Layout(_layout).Build());

            _computeBuffer = device.CreateBuffer(new IBuffer.CreateInfo<PickingData>.Builder().AddUsage(EBufferUsage.StorageBuffer).Size(16),
                new IBuffer.MemoryInfo.Builder().Usage(EMemoryUsage.CPUToGPU).AddRequiredProperty(EMemoryProperty.HostCoherent).AddRequiredProperty(EMemoryProperty.HostVisible));

            DescriptorWriteInfo writeInfo = new DescriptorWriteInfo.Builder().BindingIndex(0).DescriptorSet(_set).ArrayElement(0).AddBufferInfo(new DescriptorWriteInfo.DescriptorBufferInfo.Builder()
                .Offset(0).Range(16).Buffer(_computeBuffer).Build()).Build();

            device.UpdateDescriptorSets(new List<DescriptorWriteInfo>() { writeInfo });
        }

        internal Entity GetEntity(IImageView view, int mouseX, int mouseY)
        {
            byte[] pixels = view.GetImage().GetPixels(mouseX, mouseY, 1, 1);

            uint identifier = BitConverter.ToUInt32(pixels, 0);
            uint generation = BitConverter.ToUInt32(pixels, 4);

            Entity e = new Entity()
            {
                Generation = generation,
                Identifier = identifier
            };

            return e;

            /*PickingData data = new PickingData {mx = mouseX, my = mouseY};

            NativeBuffer<PickingData> nativeData = new NativeBuffer<PickingData>(_computeBuffer.Map());
            PickingData result = nativeData.Get();
            nativeData.Set(data, 0);
            _computeBuffer.Unmap();

            // submit shader
            // insert fence
            // wait on fence
            // read back shader buffer

            return new Entity();*/
        }
    }
}
