using System;

namespace Cobalt.Graphics
{
    public interface IFrameBuffer : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {

            }
        }
    }
}
