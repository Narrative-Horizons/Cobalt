namespace Cobalt.Graphics.VK
{
    public class RenderPass
    {
        internal readonly Bindings.Vulkan.VK.RenderPass handle;
        internal readonly uint subpassIndex;

        public RenderPass(Bindings.Vulkan.VK.RenderPass handle, uint index)
        {
            this.handle = handle;
            subpassIndex = index;
        }
    }
}
