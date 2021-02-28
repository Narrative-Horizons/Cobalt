using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class GraphicsApplication : IGraphicsApplication
    {
        public GraphicsApplication(IGraphicsApplication.CreateInfo info)
        {
            Info = info;
        }

        public IGraphicsApplication.CreateInfo Info { get; }

        public void Dispose()
        {
            // TODO: write me
        }
    }
}
