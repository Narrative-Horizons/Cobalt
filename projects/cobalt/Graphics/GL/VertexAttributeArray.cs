using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class VertexAttributeArray : IVertexAttributeArray
    {
        public uint Handle { get; private set; }

        public VertexAttributeArray(IGraphicsPipeline.VertexAttributeCreateInfo info, List<IBuffer> buffers)
        { 
            
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
