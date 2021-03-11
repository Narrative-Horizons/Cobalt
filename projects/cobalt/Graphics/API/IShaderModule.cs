using System;
using System.IO;

namespace Cobalt.Graphics.API
{
    public interface IShaderModule : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder ResourceStream(Stream resourceStream)
                {
                    base.ResourceStream = resourceStream;
                    return this;
                }

                public new Builder Type(EShaderType type)
                {
                    base.Type = type;
                    return this;
                }
            }

            public Stream ResourceStream { get; private set; }
            public EShaderType Type { get; private set; }
        }

        public EShaderType Type();
    }
}
