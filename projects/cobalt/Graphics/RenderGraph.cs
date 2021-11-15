using System;
using System.Linq;
using System.Collections.Generic;
using Cobalt.Bindings.Vulkan;
using Cobalt.Core;
using Cobalt.Graphics.Enums;
using Cobalt.Graphics.Passes;

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

        public struct AbsoluteSize
        {
            public uint width;
            public uint height;
        }

        public struct RelativeSize
        {
            public float width;
            public float height;
        }

        public struct RenderTargetCreateInfo
        {
            public AbsoluteSize? absoluteSize;
            public RelativeSize? relativeSize;

            public Format format;
        }

        public struct DepthTargetCreateInfo
        {
            public AbsoluteSize? absoluteSize;
            public RelativeSize? relativeSize;

            public Format format;
        }

        public struct BufferCreateInfo
        {
            public ulong size;
        }

        public class PassDependencyInfo
        {
            public List<ImageDependencyInfo> imageDependencyInfos = new List<ImageDependencyInfo>();
            public List<BufferDependencyInfo> bufferDependencyInfos = new List<BufferDependencyInfo>();
        }

        private struct ImageInfo
        {
            public string name;
            public AttachmentLoadOp loadOp;
            public AttachmentStoreOp storeOp;

            public override bool Equals(object obj)
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
            public Pass pass;

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

        private readonly List<Tuple<PassDependencyInfo, Pass, Pass>> _dependencies =
            new List<Tuple<PassDependencyInfo, Pass, Pass>>();

        private readonly Dictionary<Pass, PassInfo> _passInfos = new Dictionary<Pass, PassInfo>();

        public static string RenderGraphColorOutputTarget = "renderGraphColorOutput";
        public SwapchainResolvePass resolvePass;

        private readonly Dictionary<string, RenderTargetCreateInfo> _renderTargets = new Dictionary<string, RenderTargetCreateInfo>();
        private readonly Dictionary<string, DepthTargetCreateInfo> _depthTargets = new Dictionary<string, DepthTargetCreateInfo>();
        private readonly Dictionary<string, BufferCreateInfo> _readOnlyBufferTargets = new Dictionary<string, BufferCreateInfo>();
        private readonly Dictionary<string, BufferCreateInfo> _readWriteBufferTargets = new Dictionary<string, BufferCreateInfo>();

        private readonly Dictionary<string, Image> _images = new Dictionary<string, Image>();
        private readonly Dictionary<string, VK.Buffer> _buffers = new Dictionary<string, VK.Buffer>();

        private readonly Dictionary<PassInfo, Tuple<RenderPass, uint>> _subpassIndices =
            new Dictionary<PassInfo, Tuple<RenderPass, uint>>();

        private readonly Dictionary<string, Tuple<uint, uint>> _bindingIndices =
            new Dictionary<string, Tuple<uint, uint>>();

        private readonly Device _device;
        private readonly Swapchain _swapchain;

        private readonly uint _framesInFlight;

        public RenderGraph(Device device, Swapchain swapchain, uint framesInFlight)
        {
            _device = device;
            _swapchain = swapchain;
            _framesInFlight = framesInFlight;

            resolvePass = AddPass(new SwapchainResolvePass(), "resolve") as SwapchainResolvePass;
            AddInputAttachment(resolvePass, RenderGraphColorOutputTarget, AttachmentLoadOp.Load, AttachmentStoreOp.DontCare, 0, 0);
            AddColorAttachment(resolvePass, "swapchain", AttachmentLoadOp.Clear, AttachmentStoreOp.Store, 0, 1);

            GetInfoFromPass(resolvePass).renderPassID = 0;
        }

        public Pass AddPass(Pass pass, string name)
        {
            PassInfo info = new PassInfo
            {
                name = name,
                pass = pass
            };

            switch (pass.GetPassType())
            {
                case Pass.PassType.Compute:
                    _computePasses.Add(info);
                    break;
                case Pass.PassType.Graphics:
                    _graphicsPasses.Add(info);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _passInfos[pass] = info;

            return pass;
        }

        public void AddDependency(Pass source, Pass dest, PassDependencyInfo info)
        {
            _dependencies.Add(new Tuple<PassDependencyInfo, Pass, Pass>(info, source, dest));
        }

        public void AddInputAttachment(Pass pass, string name, AttachmentLoadOp loadOp, AttachmentStoreOp storeOp, uint setIndex, uint bindingIndex)
        {
            ImageInfo info = new ImageInfo
            {
                loadOp = loadOp,
                storeOp = storeOp,
                name = name
            };

            _passInfos[pass].inputAttachments.Add(info);
            _bindingIndices[name] = new Tuple<uint, uint>(setIndex, bindingIndex);
        }

        public void AddColorAttachment(Pass pass, string name, AttachmentLoadOp loadOp, AttachmentStoreOp storeOp, uint setIndex, uint bindingIndex)
        {
            ImageInfo info = new ImageInfo
            {
                loadOp = loadOp,
                storeOp = storeOp,
                name = name
            };

            _passInfos[pass].colorAttachments.Add(info);
            _bindingIndices[name] = new Tuple<uint, uint>(setIndex, bindingIndex);
        }

        public void SetDepthAttachment(Pass pass, string name, AttachmentLoadOp loadOp, AttachmentStoreOp storeOp, uint setIndex, uint bindingIndex)
        {
            var info = _passInfos[pass];

            ImageInfo imageInfo = new ImageInfo
            {
                loadOp = loadOp,
                storeOp = storeOp,
                name = name
            };
            info.depthAttachment = imageInfo;
            _passInfos[pass] = info;
            _bindingIndices[name] = new Tuple<uint, uint>(setIndex, bindingIndex);
        }

        public void AddUniformBuffer(Pass pass, string name, uint setIndex, uint bindingIndex)
        {
            _passInfos[pass].uniformBuffers.Add(name);
            _bindingIndices[name] = new Tuple<uint, uint>(setIndex, bindingIndex);
        }

        public void AddStorageBuffer(Pass pass, string name, uint setIndex, uint bindingIndex)
        {
            _passInfos[pass].storageBuffers.Add(name);
            _bindingIndices[name] = new Tuple<uint, uint>(setIndex, bindingIndex);
        }

        public void AddRenderTarget(string name, RenderTargetCreateInfo info)
        {
            if (_renderTargets.ContainsKey(name))
                return;

            _renderTargets.Add(name, info);
        }

        public void AddDepthTarget(string name, DepthTargetCreateInfo info)
        {
            if (_depthTargets.ContainsKey(name))
                return;

            _depthTargets.Add(name, info);
        }

        public void AddReadOnlyBuffer(string name, BufferCreateInfo info)
        {
            if (_readOnlyBufferTargets.ContainsKey(name))
                return;

            _readOnlyBufferTargets.Add(name, info);
        }

        public void AddReadWriteBuffer(string name, BufferCreateInfo info)
        {
            if (_readWriteBufferTargets.ContainsKey(name))
                return;

            _readWriteBufferTargets.Add(name, info);
        }

        private PassInfo GetInfoFromPass(Pass pass)
        {
            return _passInfos[pass];
        }

        private void GraphBuildHelper(PassInfo node,
            HashSet<Tuple<PassDependencyInfo, Pass, Pass>> visited, 
            Dictionary<PassInfo, List<Tuple<PassDependencyInfo, Pass, Pass>>> depthSearchMap,
            int newPassID)
        {
            foreach (var edge in depthSearchMap[node])
            {
                if (visited.Contains(edge))
                {
                    continue;
                }

                Pass source = edge.Item2;
                PassInfo sourceInfo = GetInfoFromPass(source);

                foreach (ImageInfo inputAtt in node.inputAttachments)
                {
                    if (sourceInfo.colorAttachments.Count(attachment => attachment.name == inputAtt.name) == 0 &&
                        (sourceInfo.depthAttachment?.name ?? null) != inputAtt.name) 
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

        private void BuildGraph()
        {
            // Depth search graph
            Dictionary<PassInfo, List<Tuple<PassDependencyInfo, Pass, Pass>>> depthSearchMap =
                new Dictionary<PassInfo, List<Tuple<PassDependencyInfo, Pass, Pass>>>();

            // Edge visited list
            HashSet<Tuple<PassDependencyInfo, Pass, Pass>> visited = new HashSet<Tuple<PassDependencyInfo, Pass, Pass>>();

            // Add all passes to the graph for sorting
            foreach (var (_, info) in _passInfos)
            {
                depthSearchMap.Add(info, new List<Tuple<PassDependencyInfo, Pass, Pass>>());
            }

            // Add all the dependencies for the edges on the graph
            foreach (var depInfo in _dependencies)
            {
                depthSearchMap[GetInfoFromPass(depInfo.Item3)].Add(depInfo);
            }

            // Get the final resolve node
            PassInfo resolveNodeInfo = null;
            foreach (var (info, _) in depthSearchMap)
            {
                if (info.renderPassID != 0)
                    continue;

                // Resolve node
                resolveNodeInfo = info;
                break;
            }

            // Did we find the final resolve node?
            if (resolveNodeInfo == null)
            {
                throw new InvalidOperationException("No resolve node found");
            }

            // Recursively go to the graph and group each subpass to a renderpass id
            GraphBuildHelper(resolveNodeInfo, visited, depthSearchMap, 1);

            // Compile a sorted list based on the render pass ID
            Dictionary<int, List<PassInfo>> sortedRenderList = new Dictionary<int, List<PassInfo>>();
            foreach (var (passInfo, _) in depthSearchMap)
            {
                if (!sortedRenderList.ContainsKey(passInfo.renderPassID))
                {
                    sortedRenderList[passInfo.renderPassID] = new List<PassInfo>();
                }

                sortedRenderList[passInfo.renderPassID].Add(passInfo);
            }

            // Start building Vulkan Subpasses and RenderPass from the sorted renderpass list
            foreach (var (_, passes) in sortedRenderList)
            {
                DirectedAcyclicGraph<PassInfo, PassDependencyInfo> subpassGraph =
                    new DirectedAcyclicGraph<PassInfo, PassDependencyInfo>();

                Dictionary<PassInfo, DirectedAcyclicGraph<PassInfo, PassDependencyInfo>.GraphVertex> subpassVertices =
                    new Dictionary<PassInfo, DirectedAcyclicGraph<PassInfo, PassDependencyInfo>.GraphVertex>();

                foreach (PassInfo passInfo in passes)
                {
                    subpassVertices[passInfo] = subpassGraph.AddVertex(passInfo);
                }

                foreach (PassInfo passInfo in passes)
                {
                    List<Tuple<PassDependencyInfo, Pass, Pass>> outboundEdges = depthSearchMap[passInfo];
                    foreach (var (dep, dest, source) in outboundEdges)
                    {
                        if (!passes.Contains(GetInfoFromPass(dest)))
                            continue;

                        subpassGraph.AddEdge(dep, subpassVertices[GetInfoFromPass(dest)],
                            subpassVertices[GetInfoFromPass(source)]);
                    }
                }

                List<DirectedAcyclicGraph<PassInfo, PassDependencyInfo>.GraphVertex> sortedSubPasses = subpassGraph.Sort();
                Dictionary<PassInfo, uint> indexedPassInfos = new Dictionary<PassInfo, uint>();

                for (uint i = 0; i < sortedSubPasses.Count; i++)
                {
                    indexedPassInfos.Add(sortedSubPasses[(int)i].UserData, i);
                }

                List<SubpassDependency> subpasses = new List<SubpassDependency>();
                SubpassDependency firstPassDep = new SubpassDependency
                {
                    srcSubpass = uint.MaxValue,
                    dstSubpass = 0,
                    srcStageMask = (uint)PipelineStageFlagBits.BottomOfPipeBit,
                    dstStageMask = (uint)PipelineStageFlagBits.ColorAttachmentOutputBit,
                    srcAccessMask = (uint)AccessFlagBits.MemoryReadBit,
                    dstAccessMask = (uint)AccessFlagBits.ColorAttachmentReadBit | (uint)AccessFlagBits.ColorAttachmentWriteBit,
                    dependencyFlags = (uint)DependencyFlagBits.ByRegionBit
                };
                subpasses.Add(firstPassDep);

                foreach (var edge in subpassGraph.GetEdges())
                {
                    SubpassDependency subDep = new SubpassDependency
                    {
                        srcSubpass = indexedPassInfos[edge.Source.UserData],
                        dstSubpass = indexedPassInfos[edge.Destination.UserData],
                        dependencyFlags = (uint)DependencyFlagBits.ByRegionBit
                    };

                    foreach (ImageDependencyInfo imageInfo in edge.UserData.imageDependencyInfos)
                    {
                        subDep.srcStageMask |= (uint)imageInfo.lastWriteStage;
                        subDep.dstStageMask |= (uint)imageInfo.firstReadStage;

                        subDep.srcAccessMask |= (uint)imageInfo.lastWriteType;
                        subDep.dstAccessMask |= (uint)imageInfo.firstReadType;
                    }

                    subpasses.Add(subDep);
                }

                SubpassDependency lastPassDep = new SubpassDependency
                {
                    srcSubpass = (uint)subpasses.Count - 1,
                    dstSubpass = uint.MaxValue,
                    srcStageMask = (uint)PipelineStageFlagBits.ColorAttachmentOutputBit,
                    dstStageMask = (uint)PipelineStageFlagBits.BottomOfPipeBit,
                    srcAccessMask = (uint)AccessFlagBits.ColorAttachmentReadBit |
                                    (uint)AccessFlagBits.ColorAttachmentWriteBit,
                    dependencyFlags = (uint)DependencyFlagBits.ByRegionBit
                };

                subpasses.Add(lastPassDep);

                RenderPassCreateInfo renderPassInfo = new RenderPassCreateInfo
                {
                    subpassCount = (uint)passes.Count
                };

                List<SubpassDescription> subDescs = new List<SubpassDescription>();
                List<AttachmentDescription> attachmentDescs = new List<AttachmentDescription>();

                Dictionary<string, int> attachmentDescIndices = new Dictionary<string, int>();

                foreach (var pass in passes)
                {
                    foreach (ImageInfo image in pass.colorAttachments)
                    {
                        if (attachmentDescIndices.ContainsKey(image.name))
                            continue;

                        AttachmentDescription colorAtt = new AttachmentDescription
                        {
                            format = (uint)_renderTargets[image.name].format,
                            samples = (uint)SampleCountFlagBits.Count1Bit, // Infer from somewhere else
                            loadOp = (uint)image.loadOp,
                            storeOp = (uint)image.storeOp,
                            initialLayout = (uint)ImageLayout.Undefined,
                            finalLayout = (uint)ImageLayout.ColorAttachmentOptimal
                        };

                        attachmentDescIndices.Add(image.name, attachmentDescs.Count);
                        attachmentDescs.Add(colorAtt);
                    }

                    foreach (ImageInfo image in pass.inputAttachments)
                    {
                        if (attachmentDescIndices.ContainsKey(image.name))
                            continue;

                        AttachmentDescription inputAtt = new AttachmentDescription
                        {
                            format = (uint)_renderTargets[image.name].format,
                            samples = (uint)SampleCountFlagBits.Count1Bit, // Infer from somewhere else
                            loadOp = (uint)image.loadOp,
                            storeOp = (uint)image.storeOp,
                            initialLayout = (uint)ImageLayout.Undefined,
                            finalLayout = (uint)ImageLayout.ShaderReadOnlyOptimal
                        };

                        attachmentDescIndices.Add(image.name, attachmentDescs.Count);
                        attachmentDescs.Add(inputAtt);
                    }

                    if (pass.depthAttachment != null && !attachmentDescIndices.ContainsKey(pass.depthAttachment.Value.name))
                    {
                        AttachmentDescription depthAtt = new AttachmentDescription
                        {
                            format = (uint)_depthTargets[pass.depthAttachment.Value.name].format,
                            samples = (uint)SampleCountFlagBits.Count1Bit, // Infer from somewhere else
                            loadOp = (uint)pass.depthAttachment.Value.loadOp,
                            storeOp = (uint)pass.depthAttachment.Value.storeOp,
                            stencilLoadOp = (uint)pass.depthAttachment.Value.loadOp, // ?
                            stencilStoreOp = (uint)pass.depthAttachment.Value.storeOp, // ?
                            initialLayout = (uint)ImageLayout.Undefined,
                            finalLayout = (uint)ImageLayout.DepthStencilAttachmentOptimal, // Could be Just depth too
                        };

                        attachmentDescIndices.Add(pass.depthAttachment.Value.name, attachmentDescs.Count);
                        attachmentDescs.Add(depthAtt);
                    }
                }

                foreach (var pass in passes)
                {
                    SubpassDescription subpassInfo = new SubpassDescription
                    {
                        colorAttachmentCount = (uint)pass.colorAttachments.Count,
                        inputAttachmentCount = (uint)pass.inputAttachments.Count
                    };

                    List<AttachmentReference> colorAttachmentRefs = new List<AttachmentReference>();
                    foreach (ImageInfo image in pass.colorAttachments)
                    {
                        AttachmentReference colorRef = new AttachmentReference
                        {
                            layout = (uint)ImageLayout.ColorAttachmentOptimal,
                            attachment = (uint)attachmentDescIndices[image.name]
                        };

                        colorAttachmentRefs.Add(colorRef);
                    }

                    subpassInfo.colorAttachments = colorAttachmentRefs.ToArray();


                    List<AttachmentReference> inputAttachmentRefs = new List<AttachmentReference>();
                    foreach (ImageInfo image in pass.inputAttachments)
                    {
                        AttachmentReference colorRef = new AttachmentReference
                        {
                            layout = (uint)ImageLayout.ShaderReadOnlyOptimal,
                            attachment = (uint)attachmentDescIndices[image.name]
                        };

                        inputAttachmentRefs.Add(colorRef);
                    }

                    subpassInfo.attachments = inputAttachmentRefs.ToArray();

                    if (pass.depthAttachment != null)
                    {
                        AttachmentReference depthRef = new AttachmentReference
                        {
                            layout = (uint)ImageLayout.DepthStencilAttachmentOptimal,
                            attachment = (uint)attachmentDescIndices[pass.depthAttachment.Value.name]
                        };

                        subpassInfo.depthStencilAttachments = new[] { depthRef };
                    }

                    subpassInfo.pipelineBindPoint = (uint)PipelineBindPoint.Graphics;
                    subDescs.Add(subpassInfo);
                }

                renderPassInfo.subpassCount = (uint)subDescs.Count;
                renderPassInfo.subpasses = subDescs.ToArray();
                renderPassInfo.attachmentCount = (uint)attachmentDescs.Count;
                renderPassInfo.attachments = attachmentDescs.ToArray();

                // Setup dependencies
                renderPassInfo.dependencyCount = (uint)subpasses.Count;
                renderPassInfo.dependencies = subpasses.ToArray();

                // get set of passes in vk renderpass
                RenderPass renderPass = VK.CreateRenderPass(_device.handle, renderPassInfo);

                foreach (PassInfo pass in passes)
                {
                    _subpassIndices.Add(pass, new Tuple<RenderPass, uint>(renderPass, indexedPassInfos[pass]));
                }

                // add to graph
                // add edges (dependency information)
                // do topo sort on graph
            }
        }

        private void BuildResources()
        {
            Dictionary<string, uint> imageUsageMap = new Dictionary<string, uint>();

            // Go over all passes and check the attachments
            foreach (var (_, info) in _passInfos)
            {
                foreach (ImageInfo image in info.colorAttachments)
                {
                    imageUsageMap[image.name] |= (uint) ImageUsageFlagBits.ColorAttachmentBit;
                }

                foreach (ImageInfo image in info.inputAttachments)
                {
                    imageUsageMap[image.name] |= (uint)ImageUsageFlagBits.InputAttachmentBit;
                }

                if (info.depthAttachment != null)
                {
                    imageUsageMap[info.depthAttachment.Value.name] |=
                        (uint) ImageUsageFlagBits.DepthStencilAttachmentBit;
                }
            }

            foreach (var (name, info) in _renderTargets)
            {
                ImageCreateInfo createInfo = new ImageCreateInfo();
                if (info.absoluteSize != null)
                {
                    createInfo.width = info.absoluteSize.Value.width;
                    createInfo.height = info.absoluteSize.Value.height;
                }
                else if (info.relativeSize != null)
                {
                    createInfo.width = (uint)(_swapchain.Width * info.relativeSize.Value.width);
                    createInfo.height = (uint)(_swapchain.Height * info.relativeSize.Value.height);
                }

                createInfo.usage = imageUsageMap[name];
                createInfo.format = (uint)info.format;
                createInfo.samples = (uint)SampleCountFlagBits.Count1Bit;
                createInfo.sharingMode = (uint)SharingMode.Exclusive;
                createInfo.depth = 1;
                createInfo.initialLayout = (uint)ImageLayout.Undefined;
                createInfo.mipLevels = 1;
                createInfo.arrayLayers = 1;
                createInfo.tiling = (uint)ImageTiling.Optimal;
                createInfo.imageType = (uint)ImageType.Type2D;

                for (uint i = 0; i < _framesInFlight; i++)
                {
                    Image image = new Image(_device, createInfo, name, i);
                    _images.Add(name, image);
                }
            }

            foreach (var (name, info) in _depthTargets)
            {
                ImageCreateInfo createInfo = new ImageCreateInfo();
                if (info.absoluteSize != null)
                {
                    createInfo.width = info.absoluteSize.Value.width;
                    createInfo.height = info.absoluteSize.Value.height;
                }
                else if (info.relativeSize != null)
                {
                    createInfo.width = (uint)(_swapchain.Width * info.relativeSize.Value.width);
                    createInfo.height = (uint)(_swapchain.Height * info.relativeSize.Value.height);
                }

                createInfo.usage = imageUsageMap[name];
                createInfo.format = (uint)info.format;
                createInfo.samples = (uint)SampleCountFlagBits.Count1Bit;
                createInfo.sharingMode = (uint)SharingMode.Exclusive;
                createInfo.depth = 1;
                createInfo.initialLayout = (uint)ImageLayout.Undefined;
                createInfo.mipLevels = 1;
                createInfo.arrayLayers = 1;
                createInfo.tiling = (uint)ImageTiling.Optimal;
                createInfo.imageType = (uint)ImageType.Type2D;

                for (uint i = 0; i < _framesInFlight; i++)
                {
                    Image image = new Image(_device, createInfo, name, i);
                    _images.Add(name, image);
                }
            }

            foreach (var (name, info) in _readOnlyBufferTargets)
            {
                Bindings.Vulkan.BufferCreateInfo createInfo = new Bindings.Vulkan.BufferCreateInfo
                {
                    size = info.size,
                    usage = (uint) BufferUsageFlagBits.UniformBufferBit,
                    sharingMode = (uint) SharingMode.Exclusive
                };

                BufferMemoryCreateInfo memoryInfo = new BufferMemoryCreateInfo
                {
                    usage = (uint) MemoryUsage.CpuToGpu,
                    requiredFlags =
                        (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit),
                    preferredFlags = 0
                };

                for (uint i = 0; i < _framesInFlight; i++)
                {
                    VK.Buffer uniformBuffer = CreateBuffer(_device.handle, createInfo, memoryInfo);
                    _buffers.Add(name + i, uniformBuffer);
                }
            }

            foreach (var (name, info) in _readWriteBufferTargets)
            {
                Bindings.Vulkan.BufferCreateInfo createInfo = new Bindings.Vulkan.BufferCreateInfo
                {
                    size = info.size,
                    usage = (uint) BufferUsageFlagBits.StorageBufferBit,
                    sharingMode = (uint) SharingMode.Exclusive
                };

                BufferMemoryCreateInfo memoryInfo = new BufferMemoryCreateInfo
                {
                    usage = (uint) MemoryUsage.CpuToGpu,
                    requiredFlags =
                        (uint) (MemoryPropertyFlagBits.HostVisibleBit | MemoryPropertyFlagBits.HostCoherentBit),
                    preferredFlags = 0
                };

                for (uint i = 0; i < _framesInFlight; i++)
                {
                    VK.Buffer storageBuffer = CreateBuffer(_device.handle, createInfo, memoryInfo);
                    _buffers.Add(name + i, storageBuffer);
                }
            }

            // Build shaders
            foreach (var (_, info) in _passInfos)
            {
                for (uint i = 0; i < info.pass.Shaders.Count; i++)
                {
                    var storedShader = info.pass.Shaders.ElementAt((int)i);
                    PassShaderInfo passShaderInfo = storedShader.Value.Item2;

                    ShaderCreateInfo shaderInfo = new ShaderCreateInfo
                    {
                        subPassIndex = _subpassIndices[info].Item2,
                        pass = _subpassIndices[info].Item1,
                        vertexModulePath = passShaderInfo.vertexModulePath,
                        fragmentModulePath = passShaderInfo.fragmentModulePath,
                        geometryModulePath = passShaderInfo.geometryModulePath,
                        tesselationEvalModulePath = passShaderInfo.tesselationEvalModulePath,
                        tesselationControlModulePath = passShaderInfo.tesselationControlModulePath,
                        computeModulePath = passShaderInfo.computeModulePath
                    };

                    Dictionary<uint, List<ShaderLayoutBindingCreateInfo>> shaderBindings = new Dictionary<uint, List<ShaderLayoutBindingCreateInfo>>();
                    foreach(ImageInfo inputAtt in info.inputAttachments)
                    {
                        ShaderLayoutBindingCreateInfo bindingInfo = new ShaderLayoutBindingCreateInfo
                        {
                            bindingIndex = _bindingIndices[inputAtt.name].Item2,
                            descriptorCount = 1,
                            stageFlags = (uint) ShaderStageFlagBits.FragmentBit,
                            type = (uint) DescriptorType.InputAttachment
                        };

                        shaderBindings[_bindingIndices[inputAtt.name].Item1].Add(bindingInfo);
                    }

                    foreach (string buffer in info.uniformBuffers)
                    {
                        ShaderLayoutBindingCreateInfo bindingInfo = new ShaderLayoutBindingCreateInfo
                        {
                            bindingIndex = _bindingIndices[buffer].Item2,
                            descriptorCount = 1,
                            stageFlags = (uint)ShaderStageFlagBits.FragmentBit | (uint) ShaderStageFlagBits.VertexBit,
                            type = (uint)DescriptorType.UniformBuffer
                        };

                        shaderBindings[_bindingIndices[buffer].Item1].Add(bindingInfo);
                    }

                    foreach (string buffer in info.storageBuffers)
                    {
                        ShaderLayoutBindingCreateInfo bindingInfo = new ShaderLayoutBindingCreateInfo
                        {
                            bindingIndex = _bindingIndices[buffer].Item2,
                            descriptorCount = 1,
                            stageFlags = (uint)ShaderStageFlagBits.FragmentBit | (uint)ShaderStageFlagBits.VertexBit,
                            type = (uint)DescriptorType.StorageBuffer
                        };

                        shaderBindings[_bindingIndices[buffer].Item1].Add(bindingInfo);
                    }

                    SortedDictionary<uint, ShaderLayoutSetCreateInfo> sets = new SortedDictionary<uint, ShaderLayoutSetCreateInfo>();

                    foreach (var (setIndex, bindings) in shaderBindings)
                    {
                        ShaderLayoutSetCreateInfo setInfo = new ShaderLayoutSetCreateInfo
                        {
                            bindingCount = (uint)bindings.Count,
                            bindingInfos = bindings.ToArray()
                        };

                        sets.Add(setIndex, setInfo);
                    }

                    List<ShaderLayoutSetCreateInfo> sortedSets = new List<ShaderLayoutSetCreateInfo>();
                    foreach (var (index, setInfo) in sets)
                    {
                        sortedSets.Add(setInfo);
                    }

                    ShaderLayoutCreateInfo shaderLayout = new ShaderLayoutCreateInfo
                    {
                        setCount = (uint)sortedSets.Count, 
                        setInfos = sortedSets.ToArray()
                    };

                    shaderInfo.layoutInfo = shaderLayout;

                    VK.Shader shader = CreateShader(_device.handle, shaderInfo);
                    var toUpdate = info.pass.Shaders[storedShader.Key];
                    toUpdate.Item1.handle = shader;
                    info.pass.Shaders[storedShader.Key] = toUpdate;
                }
            }
        }
        
        public void Build()
        {
            // build graph
            BuildGraph();
            
            // build resources
            BuildResources();
            
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