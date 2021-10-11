using Cobalt.Entities;

namespace Cobalt.Graphics
{
    public class RenderSystem
    {
        private GraphicsContext _context;
        private Registry _registry;

        public RenderSystem(Registry registry, GraphicsContext context)
        {
            _context = context;
            _registry = registry;
        }

        public void PreRender()
        {

        }

        public void Render()
        {

        }

        public void PostRender()
        {

        }
    }
}
