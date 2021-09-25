using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface IVertexAttributeArray : IDisposable
    {
        public abstract List<IBuffer> GetVertexBuffers();
        public abstract IBuffer GetElementBuffer();
    }
}
