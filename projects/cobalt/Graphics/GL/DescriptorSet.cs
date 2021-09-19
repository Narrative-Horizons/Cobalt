﻿using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cobalt.Graphics.GL
{
    internal class DescriptorSet : IDescriptorSet
    {
        private interface IBinding
        {
            void Bind();
            void Bind(uint offset);
            bool IsDynamic();
        }

        private class CombinedImageSamplerBinding : IBinding
        {
            public int Index { get; set; }
            public Sampler[] Samplers { get; set; }
            public ImageView[] Images { get; set; }
            public ulong[] Handles { get; set; }

            public CombinedImageSamplerBinding(int index, int count)
            {
                Index = index;
                Samplers = new Sampler[count];
                Images = new ImageView[count];
                Handles = new ulong[count];
            }

            public void Bind()
            {
                foreach(ulong handle in Handles)
                {
                    StateMachine.MakeTextureHandleResidentArb(handle);
                }
                
                StateMachine.UniformHandleuivArb(Index, Handles);
            }

            public void Bind(uint offset)
            {
                throw new NotImplementedException();
            }

            public bool IsDynamic()
            {
                return false;
            }
        }

        private class StorageBufferBinding : IBinding
        {
            public uint Index { get; set; }
            public IBuffer Buffer { get; set; }
            public int Offset { get; set; }
            public int Range { get; set; }

            public StorageBufferBinding(uint index)
            {
                Index = index;
            }

            public void Bind()
            {
                StateMachine.BindStorageBufferRange(Index, Buffer, Offset, Range);
            }

            public void Bind(uint offset)
            {
                StateMachine.BindStorageBufferRange(Index, Buffer, (int)offset, Range);
            }

            public bool IsDynamic()
            {
                return true;
            }
        }

        private class UniformBufferBinding : IBinding
        {
            public uint Index { get; set; }
            public IBuffer Buffer { get; set; }
            public int Offset { get; set; }
            public int Range { get; set; }

            public UniformBufferBinding(uint index)
            {
                Index = index;
            }

            public void Bind()
            {
                StateMachine.BindUniformBufferRange(Index, Buffer, Offset, Range);
            }

            public void Bind(uint offset)
            {
                StateMachine.BindUniformBufferRange(Index, Buffer, (int) offset, Range);
            }

            public bool IsDynamic()
            {
                return true;
            }
        }

        private IDescriptorSetLayout _layout;
        private Dictionary<int, IBinding> _bindings = new Dictionary<int, IBinding>();

        public DescriptorSet(IDescriptorSetLayout layout)
        {
            _layout = layout;

            DescriptorSetLayout lay = (DescriptorSetLayout)layout;
            lay.Bindings.Sort((left, right) => left.BindingIndex - right.BindingIndex);
            lay.Bindings.ForEach(binding =>
            {
                switch (binding.DescriptorType)
                {
                    case EDescriptorType.Sampler:
                        break;
                    case EDescriptorType.SampledImage:
                        break;
                    case EDescriptorType.CombinedImageSampler:
                        _bindings.Add(binding.BindingIndex, new CombinedImageSamplerBinding(binding.BindingIndex, binding.Count));
                        break;
                    case EDescriptorType.TextureBuffer:
                        break;
                    case EDescriptorType.UniformBuffer:
                        _bindings.Add(binding.BindingIndex, new UniformBufferBinding((uint) binding.BindingIndex));
                        break;
                    case EDescriptorType.StorageBuffer:
                        _bindings.Add(binding.BindingIndex, new StorageBufferBinding((uint)binding.BindingIndex));
                        break;
                }
            });
        }

        public void Write(List<DescriptorWriteInfo> writeInfo)
        {
            writeInfo.ForEach(info =>
            {
                int bindingIndex = info.BindingIndex;
                DescriptorSetLayout lay = (DescriptorSetLayout)_layout;

                IDescriptorSetLayout.DescriptorSetLayoutBinding binding = lay.Bindings.First(bind => bind.BindingIndex == bindingIndex);
                switch (binding.DescriptorType)
                {
                    case EDescriptorType.Sampler:
                        break;
                    case EDescriptorType.SampledImage:
                        break;
                    case EDescriptorType.CombinedImageSampler:
                        CombinedImageSamplerBinding cis = (CombinedImageSamplerBinding)_bindings[bindingIndex];
                        for (int i = 0; i < info.ImageInfo.Count; i++)
                        {
                            DescriptorWriteInfo.DescriptorImageInfo imageInfo = info.ImageInfo[i];
                            cis.Images[info.ArrayElement + i] = (ImageView)imageInfo.View;
                            cis.Samplers[info.ArrayElement + i] = (Sampler)imageInfo.Sampler;
                            ulong handle = StateMachine.GetTextureSamplerHandle(
                                cis.Images[info.ArrayElement + i], cis.Samplers[info.ArrayElement + i]);
                            cis.Handles[info.ArrayElement + i] = handle;
                        }
                        break;
                    case EDescriptorType.TextureBuffer:
                        break;
                    case EDescriptorType.UniformBuffer:
                        UniformBufferBinding ubb = (UniformBufferBinding)_bindings[bindingIndex];
                        for (int i = 0; i < info.BufferInfo.Count; i++)
                        {
                            DescriptorWriteInfo.DescriptorBufferInfo bufferInfo = info.BufferInfo[i];
                            IBuffer buf = bufferInfo.Buffer;
                            int offset = bufferInfo.Offset;
                            int range = bufferInfo.Range;

                            ubb.Buffer = buf;
                            ubb.Offset = offset;
                            ubb.Range = range;
                        }
                        break;
                    case EDescriptorType.StorageBuffer:
                        StorageBufferBinding sbb = (StorageBufferBinding)_bindings[bindingIndex];
                        for(int i = 0; i < info.BufferInfo.Count; i++)
                        {
                            DescriptorWriteInfo.DescriptorBufferInfo bufferInfo = info.BufferInfo[i];
                            IBuffer buf = bufferInfo.Buffer;
                            int offset = bufferInfo.Offset;
                            int range = bufferInfo.Range;

                            sbb.Buffer = buf;
                            sbb.Offset = offset;
                            sbb.Range = range;
                        }
                        break;
                }
            });
        }

        public void Bind(IPipelineLayout layout)
        {
            DescriptorSetLayout lay = layout.GetDescriptorSetLayouts()[0] as DescriptorSetLayout;

            foreach (var (index, binding) in _bindings)
            {
                foreach (var bnd in lay.Bindings)
                {
                    if (bnd.BindingIndex == index)
                    {
                        binding.Bind();
                    }
                }
            }
        }

        public void Bind(List<uint> offsets)
        {
            int offsetIdx = 0;
            foreach (var (idx, binding) in _bindings)
            {
                if (binding.IsDynamic())
                {
                    binding.Bind(offsets[offsetIdx++]);
                }
                else
                {
                    binding.Bind();
                }
            }
        }

        public void Dispose()
        {

        }

        public IDescriptorSetLayout GetLayout()
        {
            return _layout;
        }
    }
}
