﻿using Cobalt.Entities;
using Cobalt.Graphics.Enums;
using Cobalt.Graphics.Passes;

namespace Cobalt.Graphics
{
    public class RenderSystem
    {
        private readonly GraphicsContext _context;
        private Registry _registry;

        private readonly RenderGraph _renderGraph;

        public RenderSystem(Registry registry, GraphicsContext context)
        {
            _context = context;
            _registry = registry;

            _renderGraph = new RenderGraph(_context.ContextDevice, _context.Swapchain, 3);

            GBufferPass gbufferPass = _renderGraph.AddPass(new GBufferPass(), "gbuffer") as GBufferPass;
            GBufferResolvePass gbufferResolvePass = _renderGraph.AddPass(new GBufferResolvePass(), "gbufferresolve") as GBufferResolvePass;
            TransparancyPass transparancyPass = _renderGraph.AddPass(new TransparancyPass(), "transparancyPass") as TransparancyPass;
            
            _renderGraph.AddColorAttachment(gbufferPass, "albedo", AttachmentLoadOp.Clear, AttachmentStoreOp.Store, 0, 0);
            _renderGraph.AddColorAttachment(gbufferPass, "normal", AttachmentLoadOp.Clear, AttachmentStoreOp.Store, 0, 1);
            _renderGraph.AddColorAttachment(gbufferPass, "metallic", AttachmentLoadOp.Clear, AttachmentStoreOp.Store, 0, 2);
            _renderGraph.SetDepthAttachment(gbufferPass, "depth", AttachmentLoadOp.Clear, AttachmentStoreOp.Store, 0, 3);

            _renderGraph.AddInputAttachment(gbufferResolvePass, "albedo", AttachmentLoadOp.Load, AttachmentStoreOp.DontCare, 0, 0);
            _renderGraph.AddInputAttachment(gbufferResolvePass, "normal", AttachmentLoadOp.Load, AttachmentStoreOp.DontCare, 0, 1);
            _renderGraph.AddInputAttachment(gbufferResolvePass, "metallic", AttachmentLoadOp.Load, AttachmentStoreOp.DontCare, 0, 2);
            _renderGraph.AddColorAttachment(gbufferResolvePass, RenderGraph.RenderGraphColorOutputTarget, AttachmentLoadOp.Clear, AttachmentStoreOp.Store, 0, 3);

            _renderGraph.AddDependency(gbufferPass, gbufferResolvePass, new RenderGraph.PassDependencyInfo()
            {
                imageDependencyInfos = { 
                    new RenderGraph.ImageDependencyInfo()
                    { 
                        name = "albedo",
                        dstLayout = ImageLayout.ShaderReadOnlyOptimal,
                        firstReadStage = PipelineStageFlagBits.ColorAttachmentOutputBit,
                        firstReadType = AccessFlagBits.ColorAttachmentWriteBit,
                        lastWriteStage = PipelineStageFlagBits.FragmentShaderBit,
                        lastWriteType = AccessFlagBits.ShaderReadBit,
                        srcLayout = ImageLayout.ColorAttachmentOptimal
                    },
                    new RenderGraph.ImageDependencyInfo()
                    {
                        name = "normal",
                        dstLayout = ImageLayout.ShaderReadOnlyOptimal,
                        firstReadStage = PipelineStageFlagBits.ColorAttachmentOutputBit,
                        firstReadType = AccessFlagBits.ColorAttachmentWriteBit,
                        lastWriteStage = PipelineStageFlagBits.FragmentShaderBit,
                        lastWriteType = AccessFlagBits.ShaderReadBit,
                        srcLayout = ImageLayout.ColorAttachmentOptimal
                    },
                    new RenderGraph.ImageDependencyInfo()
                    {
                        name = "metallic",
                        dstLayout = ImageLayout.ShaderReadOnlyOptimal,
                        firstReadStage = PipelineStageFlagBits.ColorAttachmentOutputBit,
                        firstReadType = AccessFlagBits.ColorAttachmentWriteBit,
                        lastWriteStage = PipelineStageFlagBits.FragmentShaderBit,
                        lastWriteType = AccessFlagBits.ShaderReadBit,
                        srcLayout = ImageLayout.ColorAttachmentOptimal
                    }
                }
            });

            _renderGraph.AddColorAttachment(transparancyPass, RenderGraph.RenderGraphColorOutputTarget, AttachmentLoadOp.Load, AttachmentStoreOp.Store, 0, 0);
            _renderGraph.SetDepthAttachment(transparancyPass, "depth", AttachmentLoadOp.Load, AttachmentStoreOp.DontCare, 0, 1);

            _renderGraph.AddDependency(gbufferResolvePass, transparancyPass, new RenderGraph.PassDependencyInfo());
            _renderGraph.AddDependency(transparancyPass, _renderGraph.resolvePass, new RenderGraph.PassDependencyInfo());

            _renderGraph.Build();
        }

        public void PreRender()
        {

        }

        public void Render()
        {
            _context.Render();
        }

        public void PostRender()
        {

        }
    }
}
