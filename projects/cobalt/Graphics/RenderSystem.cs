using Cobalt.Bindings.Vulkan;
using Cobalt.Entities;
using Cobalt.Graphics.Enums;
using Cobalt.Graphics.Passes;

namespace Cobalt.Graphics
{
    public class RenderSystem
    {
        private readonly GraphicsContext _context;
        private Registry _registry;

        private readonly VK.CommandBuffer _graphicsBuffer;
        private readonly VK.CommandBuffer _computeBuffer;
        private readonly CommandList[] _computeList;
        private readonly CommandList[] _graphicsList;

        private readonly uint _framesInFlight = 3;

        public RenderSystem(Registry registry, GraphicsContext context)
        {
            _context = context;
            _registry = registry;

            CommandBufferCreateInfo commandBufferInfo = new CommandBufferCreateInfo
            {
                amount = _framesInFlight, 
                pool = (uint) CommandBufferPoolType.Graphics, 
                primary = true
            };

            _graphicsBuffer = VK.CreateCommandBuffer(context.ContextDevice.handle, commandBufferInfo);

            commandBufferInfo.pool = (uint) CommandBufferPoolType.Compute;

            //_computeBuffer = VK.CreateCommandBuffer(context.ContextDevice.handle, commandBufferInfo);

            _computeList = new CommandList[_framesInFlight];
            _graphicsList = new CommandList[_framesInFlight];

            for (uint i = 0; i < _framesInFlight; i++)
            {
                _computeList[i] = new CommandList(_computeBuffer, i);
                _graphicsList[i] = new CommandList(_graphicsBuffer, i);
            }

            UberPBRPass uberPass = new UberPBRPass(_context.ContextDevice, _framesInFlight);
            SwapchainResolvePass resolvePass = new SwapchainResolvePass(_context.ContextDevice, _framesInFlight);

            for (uint i = 0; i < _framesInFlight; i++)
            {
                _graphicsList[i].Begin();
                uberPass.Execute(_graphicsList[i], i);
                resolvePass.Execute(_graphicsList[i], i);
                _graphicsList[i].End();
            }
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
