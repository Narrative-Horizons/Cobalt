using System;

namespace Cobalt.Graphics
{
    public interface IImageView : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {

            }
        }
    }
}
