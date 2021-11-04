using System;
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
            public string name;
            public RenderPassInfo? passInfo;
            public ComputePassInfo? computeInfo;
            public IPass pass;
        }

        private DirectedAcyclicGraph<PassInfo, PassDependencyInfo> _graph = new DirectedAcyclicGraph<PassInfo, PassDependencyInfo>();

        public void AddPass(IPass pass, string name)
        {
            PassInfo info = new PassInfo();
            info.name = name;
            info.pass = pass;
            switch (pass.GetType())
            {
                case IPass.PassType.Compute:
                    info.computeInfo = new ComputePassInfo();
                    break;
                case IPass.PassType.Graphics:
                    info.passInfo = new RenderPassInfo();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _graph.AddVertex(info);
        }

        public void AddDependency(IPass source, IPass dest, PassDependencyInfo info)
        {
            //_graph.AddEdge(info, source, dest);
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
