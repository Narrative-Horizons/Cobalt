using System;
using System.Collections.Generic;
using Cobalt.Bindings.Vulkan;
using Cobalt.Core;
using Cobalt.Graphics.Enums;
using Cobalt.Graphics.Passes;
using log4net.Filter;
using static Cobalt.Bindings.Vulkan.VK;

namespace Cobalt.Graphics
{
    public class RenderGraph
    {
        public struct ImageDependencyInfo
        {
            public string name;

            public PipelineStageFlagBits lastWriteStage;
            public PipelineStageFlagBits firstReadStage;

            public AccessFlagBits lastWriteType;
            public AccessFlagBits firstReadType;

            public ImageLayout srcLayout;
            public ImageLayout dstLayout;
        }

        public struct BufferDependencyInfo
        {
            public string name;

            public AccessFlagBits lastWriteType;
            public AccessFlagBits firstReadType;

            public ulong offset;
            public ulong size;
        }

        public class PassDependencyInfo
        {
            public List<ImageDependencyInfo> imageDependencyInfos = new List<ImageDependencyInfo>();
            public List<BufferDependencyInfo> bufferDependencyInfos = new List<BufferDependencyInfo>();
        }

        private struct ComputePassInfo
        {
            public Shader compute;
        }

        private struct RenderPassInfo
        {
            public RenderPass renderPass;
        }

        private struct ImageInfo
        {
            public string name;
            public AttachmentLoadOp loadOp;
            public AttachmentStoreOp storeOp;

            public override bool Equals(object? obj)
            {
                return obj switch
                {
                    null => false,
                    ImageInfo image => image.name == name && image.storeOp == storeOp && image.loadOp == loadOp,
                    _ => false
                };
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(name, loadOp, storeOp);
            }

            public static bool operator ==(ImageInfo left, ImageInfo right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(ImageInfo left, ImageInfo right)
            {
                return !left.Equals(right);
            }
        }

        private class PassInfo
        {
            public string name;
            public IPass pass;

            public readonly List<ImageInfo> inputAttachments = new List<ImageInfo>();
            public readonly List<ImageInfo> colorAttachments = new List<ImageInfo>();
            public ImageInfo? depthAttachment;
            public readonly List<string> uniformBuffers = new List<string>();
            public readonly List<string> storageBuffers = new List<string>();

            public int renderPassID = -1;
        }

        private DirectedAcyclicGraph<PassInfo, PassDependencyInfo> _graph = new DirectedAcyclicGraph<PassInfo, PassDependencyInfo>();

        private readonly List<PassInfo> _graphicsPasses = new List<PassInfo>();
        private readonly List<PassInfo> _computePasses = new List<PassInfo>();

        private readonly List<Tuple<PassDependencyInfo, IPass, IPass>> _dependencies =
            new List<Tuple<PassDependencyInfo, IPass, IPass>>();

        private readonly Dictionary<IPass, PassInfo> _passInfos = new Dictionary<IPass, PassInfo>();

        private uint _attachmentIndex = 0;
        private readonly Dictionary<string, uint> _attachmentIndices = new Dictionary<string, uint>();

        private readonly Dictionary<string, AttachmentDescription> _attachmentDescMap =
            new Dictionary<string, AttachmentDescription>();

        public static string RenderGraphColorOutputTarget = "diffuse";
        public SwapchainResolvePass resolvePass;

        private readonly Device _device;

        public RenderGraph(Device device)
        {
            _device = device;

            resolvePass = AddPass(new SwapchainResolvePass(), "resolve") as SwapchainResolvePass;
            AddInputAttachment(resolvePass, RenderGraphColorOutputTarget, AttachmentLoadOp.Load, AttachmentStoreOp.DontCare);

            GetInfoFromPass(resolvePass).renderPassID = 0;
        }

        public IPass AddPass(IPass pass, string name)
        {
            PassInfo info = new PassInfo
            {
                name = name,
                pass = pass
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

            return pass;
        }

        public void AddDependency(IPass source, IPass dest, PassDependencyInfo info)
        {
            _dependencies.Add(new Tuple<PassDependencyInfo, IPass, IPass>(info, source, dest));
        }

        public void AddInputAttachment(IPass pass, string name, AttachmentLoadOp loadOp, AttachmentStoreOp storeOp)
        {
            if (!_attachmentIndices.ContainsKey(name))
            {
                _attachmentIndices.Add(name, _attachmentIndex++);
            }

            ImageInfo info = new ImageInfo
            {
                loadOp = loadOp,
                storeOp = storeOp,
                name = name
            };

            _passInfos[pass].inputAttachments.Add(info);
        }

        public void AddColorAttachment(IPass pass, string name, AttachmentLoadOp loadOp, AttachmentStoreOp storeOp)
        {
            if (!_attachmentIndices.ContainsKey(name))
            {
                _attachmentIndices.Add(name, _attachmentIndex++);
            }

            ImageInfo info = new ImageInfo
            {
                loadOp = loadOp,
                storeOp = storeOp,
                name = name
            };

            _passInfos[pass].colorAttachments.Add(info);
        }

        public void SetDepthAttachment(IPass pass, string name, AttachmentLoadOp loadOp, AttachmentStoreOp storeOp)
        {
            if (!_attachmentIndices.ContainsKey(name))
            {
                _attachmentIndices.Add(name, _attachmentIndex++);
            }

            var info = _passInfos[pass];

            ImageInfo imageInfo = new ImageInfo
            {
                loadOp = loadOp,
                storeOp = storeOp,
                name = name
            };
            info.depthAttachment = imageInfo;
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

        private PassInfo GetInfoFromPass(IPass pass)
        {
            return _passInfos[pass];
        }

        private void GraphBuildHelper(PassInfo node,
            HashSet<Tuple<PassDependencyInfo, IPass, IPass>> visited, 
            Dictionary<PassInfo, List<Tuple<PassDependencyInfo, IPass, IPass>>> depthSearchMap,
            int newPassID)
        {
            foreach (var edge in depthSearchMap[node])
            {
                if (visited.Contains(edge))
                {
                    continue;
                }

                IPass source = edge.Item2;
                PassInfo sourceInfo = GetInfoFromPass(source);

                foreach (ImageInfo inputAtt in node.inputAttachments)
                {
                    if (!sourceInfo.colorAttachments.Contains(inputAtt) &&
                        sourceInfo.depthAttachment != inputAtt) 
                        continue;

                    // We are defining them as the VK RenderPass
                    sourceInfo.renderPassID = node.renderPassID;

                    break;
                }

                if (sourceInfo.renderPassID == -1 || sourceInfo.renderPassID == 0)
                {
                    sourceInfo.renderPassID = newPassID;
                }

                visited.Add(edge);
                GraphBuildHelper(sourceInfo, visited, depthSearchMap, ++newPassID);
            }
        }

        public void Build()
        {
            Dictionary<PassInfo, List<Tuple<PassDependencyInfo, IPass, IPass>>> depthSearchMap =
                new Dictionary<PassInfo, List<Tuple<PassDependencyInfo, IPass, IPass>>>();

            HashSet<Tuple<PassDependencyInfo, IPass, IPass>> visited = new HashSet<Tuple<PassDependencyInfo, IPass, IPass>>();

            foreach (var (_, info) in _passInfos)
            {
                depthSearchMap.Add(info, new List<Tuple<PassDependencyInfo, IPass, IPass>>());
            }

            foreach (var depInfo in _dependencies)
            {
                depthSearchMap[GetInfoFromPass(depInfo.Item3)].Add(depInfo);
            }

            PassInfo resolveNodeInfo = null;
            foreach (var (info, _) in depthSearchMap)
            {
                if (info.renderPassID != 0) 
                    continue;

                // Resolve node
                resolveNodeInfo = info;
                break;
            }

            if (resolveNodeInfo == null)
            {
                throw new InvalidOperationException("No resolve node found");
            }

            GraphBuildHelper(resolveNodeInfo, visited, depthSearchMap, 1);

            Dictionary<int, List<PassInfo>> sortedRenderList = new Dictionary<int, List<PassInfo>>();
            foreach (var pass in depthSearchMap)
            {
                if (!sortedRenderList.ContainsKey(pass.Key.renderPassID))
                {
                    sortedRenderList[pass.Key.renderPassID] = new List<PassInfo>();
                }

                sortedRenderList[pass.Key.renderPassID].Add(pass.Key);
            }

            foreach (var (index, passes) in sortedRenderList)
            {
                RenderPassCreateInfo renderPassInfo = new RenderPassCreateInfo();
                renderPassInfo.subpassCount = (uint) passes.Count;
                List<SubpassDescription> subDescs = new List<SubpassDescription>();
                List<AttachmentDescription> attachmentDescs = new List<AttachmentDescription>();
                foreach (var pass in passes)
                {
                    SubpassDescription subpassInfo = new SubpassDescription();

                    subpassInfo.colorAttachmentCount = (uint) pass.colorAttachments.Count;
                    List<AttachmentReference> colorAttachmentRefs = new List<AttachmentReference>();
                    foreach (ImageInfo image in pass.colorAttachments)
                    {
                        AttachmentReference colorRef = new AttachmentReference();
                        colorRef.layout = (uint)ImageLayout.ColorAttachmentOptimal;
                        colorRef.attachment = _attachmentIndices[image.name];

                        colorAttachmentRefs.Add(colorRef);

                        if (!_attachmentDescMap.ContainsKey(image.name))
                        {
                            AttachmentDescription colorAtt = new AttachmentDescription();
                            colorAtt.format = (uint) Format.B8G8R8A8Srgb; // Infer from loaded image
                            colorAtt.samples = (uint) SampleCountFlagBits.Count1Bit; // Infer from somewhere else
                            colorAtt.loadOp = (uint) image.loadOp;
                            colorAtt.storeOp = (uint) image.storeOp;
                            colorAtt.initialLayout = (uint) ImageLayout.Undefined;
                            colorAtt.finalLayout = (uint) ImageLayout.ColorAttachmentOptimal;

                            attachmentDescs.Add(colorAtt);

                            _attachmentDescMap.Add(image.name, colorAtt);
                        }
                    }

                    subpassInfo.colorAttachments = colorAttachmentRefs.ToArray();

                    subpassInfo.inputAttachmentCount = (uint) pass.inputAttachments.Count;
                    List<AttachmentReference> inputAttachmentRefs = new List<AttachmentReference>();
                    foreach (ImageInfo image in pass.inputAttachments)
                    {
                        AttachmentReference colorRef = new AttachmentReference();
                        colorRef.layout = (uint)ImageLayout.ShaderReadOnlyOptimal;
                        colorRef.attachment = _attachmentIndices[image.name];

                        inputAttachmentRefs.Add(colorRef);

                        if (!_attachmentDescMap.ContainsKey(image.name))
                        {
                            AttachmentDescription inputAtt = new AttachmentDescription();
                            inputAtt.format = (uint) Format.B8G8R8A8Srgb; // Infer from loaded image
                            inputAtt.samples = (uint) SampleCountFlagBits.Count1Bit; // Infer from somewhere else
                            inputAtt.loadOp = (uint) image.loadOp;
                            inputAtt.storeOp = (uint) image.storeOp;
                            inputAtt.initialLayout = (uint) ImageLayout.Undefined;
                            inputAtt.finalLayout = (uint) ImageLayout.ShaderReadOnlyOptimal;

                            attachmentDescs.Add(inputAtt);
                            _attachmentDescMap.Add(image.name, inputAtt);
                        }
                    }

                    subpassInfo.attachments = inputAttachmentRefs.ToArray();

                    if (pass.depthAttachment != null)
                    {
                        AttachmentReference depthRef = new AttachmentReference();
                        depthRef.layout = (uint) ImageLayout.DepthAttachmentOptimal;
                        depthRef.attachment = _attachmentIndices[pass.depthAttachment.Value.name];

                        subpassInfo.depthStencilAttachments = new[] {depthRef};

                        if (!_attachmentDescMap.ContainsKey(pass.depthAttachment.Value.name))
                        {
                            AttachmentDescription depthAtt = new AttachmentDescription();
                            depthAtt.format = (uint) Format.D24UnormS8Uint; // Infer from loaded image
                            depthAtt.samples = (uint) SampleCountFlagBits.Count1Bit; // Infer from somewhere else
                            depthAtt.loadOp = (uint) pass.depthAttachment.Value.loadOp;
                            depthAtt.storeOp = (uint) pass.depthAttachment.Value.storeOp;
                            depthAtt.stencilLoadOp = (uint) pass.depthAttachment.Value.loadOp; // ?
                            depthAtt.stencilLoadOp = (uint) pass.depthAttachment.Value.storeOp; // ?
                            depthAtt.initialLayout = (uint) ImageLayout.Undefined;
                            depthAtt.finalLayout =
                                (uint) ImageLayout.DepthStencilAttachmentOptimal; // Could be Just depth too

                            attachmentDescs.Add(depthAtt);
                            _attachmentDescMap.Add(pass.depthAttachment.Value.name, depthAtt);
                        }
                    }

                    subpassInfo.pipelineBindPoint = (uint) PipelineBindPoint.Graphics;
                    subDescs.Add(subpassInfo);
                }

                renderPassInfo.subpassCount = (uint) subDescs.Count;
                renderPassInfo.subpasses = subDescs.ToArray();
                renderPassInfo.attachmentCount = (uint) attachmentDescs.Count;
                renderPassInfo.attachments = attachmentDescs.ToArray();

                // Setup dependencies
                renderPassInfo.dependencyCount = 0;
                renderPassInfo.dependencies = null;

                RenderPass renderPass = VK.CreateRenderPass(_device.handle, renderPassInfo);

                // get set of passes in vk renderpass
                // add to graph
                // add edges (dependency information)
                // do topo sort on graph
            }

            // build resources
            // do topo sort on passinfos that are renderpassinfo graphs
            // use to build vkrenderpass
            // get shader information from each subpass
            // build shader object
            // build framebuffer
            // do topo sort on overall node
            // build sync structures (semaphores, barriers)
            // build execution list (iterates over all ipasses & sync structures) to commandbuffer
        }
    }
}
