using Cobalt.Entities;
using Cobalt.Graphics.Enums;
using Cobalt.Graphics.Passes;

namespace Cobalt.Graphics
{
    public class RenderSystem
    {
        private GraphicsContext _context;
        private Registry _registry;

        private RenderGraph _renderGraph;

        public RenderSystem(Registry registry, GraphicsContext context)
        {
            _context = context;
            _registry = registry;

            _renderGraph = new RenderGraph();

            GBufferPass gbufferPass = _renderGraph.AddPass(new GBufferPass(), "gbuffer") as GBufferPass;
            GBufferResolvePass gbufferResolvePass = _renderGraph.AddPass(new GBufferResolvePass(), "gbufferresolve") as GBufferResolvePass;
            TransparancyPass transparancyPass = _renderGraph.AddPass(new TransparancyPass(), "transparancyPass") as TransparancyPass;
            
            _renderGraph.AddColorAttachment(gbufferPass, "albedo", AttachmentLoadOp.Clear, AttachmentStoreOp.Store);
            _renderGraph.AddColorAttachment(gbufferPass, "normal", AttachmentLoadOp.Clear, AttachmentStoreOp.Store);
            _renderGraph.AddColorAttachment(gbufferPass, "metallic", AttachmentLoadOp.Clear, AttachmentStoreOp.Store);
            _renderGraph.SetDepthAttachment(gbufferPass, "depth", AttachmentLoadOp.Clear, AttachmentStoreOp.Store);

            _renderGraph.AddInputAttachment(gbufferResolvePass, "albedo", AttachmentLoadOp.Load, AttachmentStoreOp.DontCare);
            _renderGraph.AddInputAttachment(gbufferResolvePass, "normal", AttachmentLoadOp.Load, AttachmentStoreOp.DontCare);
            _renderGraph.AddInputAttachment(gbufferResolvePass, "metallic", AttachmentLoadOp.Load, AttachmentStoreOp.DontCare);
            _renderGraph.AddColorAttachment(gbufferResolvePass, RenderGraph.RenderGraphColorOutputTarget, AttachmentLoadOp.Clear, AttachmentStoreOp.Store);

            _renderGraph.AddDependency(gbufferPass, gbufferResolvePass, new RenderGraph.PassDependencyInfo());

            _renderGraph.AddColorAttachment(transparancyPass, RenderGraph.RenderGraphColorOutputTarget, AttachmentLoadOp.Load, AttachmentStoreOp.Store);
            _renderGraph.SetDepthAttachment(transparancyPass, "depth", AttachmentLoadOp.Load, AttachmentStoreOp.DontCare);

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
