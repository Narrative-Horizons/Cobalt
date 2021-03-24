using Cobalt.Graphics.API;
using System.Collections.Generic;
using System.Linq;

namespace Cobalt.Graphics.GL
{
    internal class DescriptorSet : IDescriptorSet
    {
        private interface IBinding
        {
            void Bind();
        }

        private class CombinedImageSamplerBinding : IBinding
        {
            public int Index { get; set; }
            public Sampler[] Samplers { get; set; }
            public ImageView[] Images { get; set; }
            public ulong[] Handles { get; set; }

            public CombinedImageSamplerBinding(int index, int count)
            {
                this.Index = index;
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
        }

        private IDescriptorSetLayout _layout;
        private List<IBinding> _bindings;

        public DescriptorSet(IDescriptorSetLayout layout)
        {
            this._layout = layout;

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
                        _bindings.Add(new CombinedImageSamplerBinding(binding.BindingIndex, binding.Count));
                        break;
                    case EDescriptorType.TextureBuffer:
                        break;
                    case EDescriptorType.UniformBuffer:
                        //return new UniformBufferBinding(binding.BindingIndex);
                    case EDescriptorType.StorageBuffer:
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
                        break;
                    case EDescriptorType.StorageBuffer:
                        break;
                }
            });
        }

        public void Bind()
        {
            _bindings.ForEach(binding => binding.Bind());
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
