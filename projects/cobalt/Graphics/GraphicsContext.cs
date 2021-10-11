using Cobalt.Graphics.VK;
using System;

namespace Cobalt.Graphics
{
    public class GraphicsContext : IDisposable
    {
        #region Properties
        public Device ContextDevice { get; private set; }
        #endregion

        public GraphicsContext(Window window)
        {
            ContextDevice = Device.Create(window);
        }

        public void Dispose()
        {
            ContextDevice.Dispose();
        }
    }
}
