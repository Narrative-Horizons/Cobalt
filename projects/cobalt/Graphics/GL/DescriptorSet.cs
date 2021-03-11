using Cobalt.Graphics.API;
using System.Collections.Generic;

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
            private int _index;
            private Sampler[] _samplers;
            private ImageView[] _images;
            private long[] _handles;

            public CombinedImageSamplerBinding(int index, int count)
            {
                _index = index;
                _samplers = new Sampler[count];
                _images = new ImageView[count];
                _handles = new long[count];
            }

            public void Bind()
            {

            }
        }

        private IDescriptorSetLayout _layout;
        private List<IBinding> _bindings;

        public DescriptorSet(IDescriptorSetLayout layout)
        {
            this._layout = layout;
            // TODO Bindings
        }

        public void Write(List<DescriptorWriteInfo> writeInfo)
        {

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
