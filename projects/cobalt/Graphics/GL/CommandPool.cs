using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class CommandPool : ICommandPool
    {
        public List<ICommandBuffer> Allocate(ICommandBuffer.AllocateInfo info)
        {
            return null;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
