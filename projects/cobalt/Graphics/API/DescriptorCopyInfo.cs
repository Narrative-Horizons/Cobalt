namespace Cobalt.Graphics.API
{
    public struct DescriptorCopyInfo
    {
        public int count;
        public IDescriptorSet src;
        public int srcBinding;
        public int srcArrayElement;
        public IDescriptorSet dst;
        public int dstBinding;
        public int dstArrayElement;
    }
}
