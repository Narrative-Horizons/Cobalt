using Cobalt.Entities;
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
            _renderGraph.AddPass(new ResolvePass(), "resolve");
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
