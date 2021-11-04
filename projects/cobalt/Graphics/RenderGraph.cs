using Cobalt.Core;
using static Cobalt.Bindings.Vulkan.VK;

namespace Cobalt.Graphics
{
    public class RenderGraph
    {
        public struct PassDependencyInfo
        {

        }

        private struct ComputePassInfo
        {
            public Shader compute;
        }

        private struct RenderPassInfo
        {
            public RenderPass renderPass;
            public DirectedAcyclicGraph<IPass, PassDependencyInfo> graph;
        }

        private struct PassInfo
        {
            public RenderPassInfo? passInfo;
            public ComputePassInfo? computeInfo;
        }

        private DirectedAcyclicGraph<PassInfo, PassDependencyInfo> _graph = new DirectedAcyclicGraph<PassInfo, PassDependencyInfo>();

        public void AddPass(IPass pass, string name)
        {

        }

        public void AddDependency(IPass source, IPass dest, PassDependencyInfo info)
        {

        }

        public void Build()
        {
            // build resources
            // do topo sort on passinfos that are renderpassinfo graphs
            // use to build vkrenderpass
            // get shader information from each subpass
            // build shader object
            // build framebuffer
            // do topo sort on overall graph
            // build sync structures (semaphores, barriers)
            // build execution list (iterates over all ipasses & sync structures) to commandbuffer
        }
    }
}
