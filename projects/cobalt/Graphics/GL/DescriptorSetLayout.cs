using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class DescriptorSetLayout : IDescriptorSetLayout
    {
        public List<IDescriptorSetLayout.DescriptorSetLayoutBinding> bindings { get; private set; } = new List<IDescriptorSetLayout.DescriptorSetLayoutBinding>();

        public DescriptorSetLayout(IDescriptorSetLayout.CreateInfo info)
        {
            bindings = info.Binding;
        }

        public void Dispose()
        {
            bindings.Clear();
        }
    }
}
