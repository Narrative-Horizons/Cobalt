using System;
using System.Collections.Generic;
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

            public List<string> inputAttachments;
            public List<string> colorAttachments;
            public string depthAttachment;
            public List<string> uniformBuffers;
            public List<string> storageBuffers;
        }

        private DirectedAcyclicGraph<PassInfo, PassDependencyInfo> _graph = new DirectedAcyclicGraph<PassInfo, PassDependencyInfo>();

        private readonly List<PassInfo> _graphicsPasses = new List<PassInfo>();
        private readonly List<PassInfo> _computePasses = new List<PassInfo>();

        private readonly List<Tuple<PassDependencyInfo, IPass, IPass>> _dependencies =
            new List<Tuple<PassDependencyInfo, IPass, IPass>>();

        private readonly Dictionary<IPass, PassInfo> _passInfos = new Dictionary<IPass, PassInfo>();

        public void AddPass(IPass pass, string name)
        {
            PassInfo info = new PassInfo
            {
                pass = pass, 
                name = name
            };

            switch (pass.GetType())
            {
                case IPass.PassType.Compute:
                    _computePasses.Add(info);
                    break;
                case IPass.PassType.Graphics:
                    _graphicsPasses.Add(info);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _passInfos[pass] = info;
        }

        public void AddDependency(IPass source, IPass dest, PassDependencyInfo info)
        {
            _dependencies.Add(new Tuple<PassDependencyInfo, IPass, IPass>(info, source, dest));
        }

        public void AddInputAttachment(IPass pass, string name)
        {
            _passInfos[pass].inputAttachments.Add(name);
        }

        public void AddColorAttachment(IPass pass, string name)
        {
            _passInfos[pass].colorAttachments.Add(name);
        }

        public void SetDepthAttachment(IPass pass, string name)
        {
            var info = _passInfos[pass];
            info.depthAttachment = name;
            _passInfos[pass] = info;
        }

        public void AddUniformBuffer(IPass pass, string name)
        {
            _passInfos[pass].uniformBuffers.Add(name);

        }

        public void AddStorageBuffer(IPass pass, string name)
        {
            _passInfos[pass].storageBuffers.Add(name);
        }

        public void Build()
        {
            foreach(var (pass, info) in _passInfos)
            {
                foreach (string inputAtt in info.inputAttachments)
                {
                    foreach (var (otherPass, otherInfo) in _passInfos)
                    {
                        if (otherPass == pass)
                            continue;

                        if (!otherInfo.colorAttachments.Contains(inputAtt) && otherInfo.depthAttachment != inputAtt)
                            continue;

                        // Match found, check dependencies
                        foreach (var (depInfo, source, dest) in _dependencies)
                        {
                            if ((source == pass && dest == otherPass) || (dest == pass && source == otherPass))
                            {
                                // Found the corresponsing dependencyinfo
                                // Do shit with this
                            }
                        }
                    }
                }
            }

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
