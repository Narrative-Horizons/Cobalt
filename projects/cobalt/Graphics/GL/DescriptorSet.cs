using Cobalt.Graphics.API;
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
            int Copy(int offset, int count, IBinding dst, int dstOffset, int dstCount);
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

            public int Copy(int offset, int count, IBinding dst, int dstOffset, int dstCount)
            {
                CombinedImageSamplerBinding d = dst as CombinedImageSamplerBinding;

                int copyCount = 0;
                for (int i = 0; i < count && i + dstOffset < d.Images.Length && i + offset < Images.Length; ++i)
                {
                    d.Images[i + offset] = Images[i + dstOffset];
                    d.Samplers[i + offset] = Samplers[i + dstOffset];
                    d.Handles[i + offset] = Handles[i + dstOffset];
                    ++copyCount;
                }

                return copyCount;
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
                if (Buffer == null)
                    return;

                StateMachine.BindStorageBufferRange(Index, Buffer, Offset, Range);
            }

            public void Bind(uint offset)
            {
                if (Buffer == null)
                    return;

                StateMachine.BindStorageBufferRange(Index, Buffer, (int)offset, Range);
            }

            public bool IsDynamic()
            {
                return true;
            }

            public int Copy(int offset, int count, IBinding dst, int dstOffset, int dstCount)
            {
                StorageBufferBinding d = dst as StorageBufferBinding;
                d.Index = Index;
                d.Buffer = Buffer;
                return 1;
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
                if (Buffer == null)
                    return;

                StateMachine.BindUniformBufferRange(Index, Buffer, Offset, Range);
            }

            public void Bind(uint offset)
            {
                if (Buffer == null)
                    return;

                StateMachine.BindUniformBufferRange(Index, Buffer, (int) offset, Range);
            }

            public bool IsDynamic()
            {
                return true;
            }

            public int Copy(int offset, int count, IBinding dst, int dstOffset, int dstCount)
            {
                UniformBufferBinding d = dst as UniformBufferBinding;
                d.Index = Index;
                d.Buffer = Buffer;
                return 1;
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

        public void Copy(DescriptorCopyInfo copyInfo)
        {
            int srcBindingStart = copyInfo.srcBinding;
            int srcOffsetStart = copyInfo.srcArrayElement;
            int dstBindingStart = copyInfo.dstBinding;
            int dstOffsetStart = copyInfo.dstArrayElement;
            DescriptorSet dstSet = copyInfo.dst as DescriptorSet;

            for (int i = 0; i < copyInfo.count; )
            {
                IBinding src = _bindings.GetValueOrDefault(srcBindingStart, null);
                IBinding dst = dstSet._bindings.GetValueOrDefault(dstBindingStart, null);

                if (src != null && dst != null)
                {
                    int numCopied = src.Copy(srcOffsetStart, copyInfo.count, dst, dstOffsetStart, copyInfo.count);
                    srcBindingStart += 1;
                    dstBindingStart += 1;

                    srcOffsetStart = 0;
                    dstOffsetStart = 0;

                    i += numCopied;
                }
                else
                {
                    Console.WriteLine("Invalid copy info.  Source and destination do not both have binding.");
                }
            }
        }
    }
}
