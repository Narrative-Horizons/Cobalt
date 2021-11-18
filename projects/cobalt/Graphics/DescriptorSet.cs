using Cobalt.Bindings.Vulkan;

namespace Cobalt.Graphics
{
    public class Descriptor
    {
        internal readonly VK.DescriptorSet handle;
        internal readonly uint setIndex;
        internal readonly uint bindingIndex;

        internal Descriptor(VK.DescriptorSet set, uint setIndex, uint bindingIndex)
        {
            handle = set;
            this.setIndex = setIndex;
            this.bindingIndex = bindingIndex;
        }
    }
}