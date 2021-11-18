using Cobalt.Entities;
using Cobalt.Graphics.Enums;
using Cobalt.Graphics.Passes;

namespace Cobalt.Graphics
{
    public class RenderSystem
    {
        private readonly GraphicsContext _context;
        private Registry _registry;

        public RenderSystem(Registry registry, GraphicsContext context)
        {
            _context = context;
            _registry = registry;

            UberPBRPass uberPass = new UberPBRPass(_context.ContextDevice, 3);
            int jonathan = 0;
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
