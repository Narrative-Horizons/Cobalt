using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class DescriptorSetLayout : IDescriptorSetLayout
    {
        public List<IDescriptorSetLayout.DescriptorSetLayoutBinding> Bindings { get; private set; } = new List<IDescriptorSetLayout.DescriptorSetLayoutBinding>();

        public DescriptorSetLayout(IDescriptorSetLayout.CreateInfo info)
        {
            Bindings = info.Binding;
        }

        public void Dispose()
        {
            Bindings.Clear();
        }
    }
}
