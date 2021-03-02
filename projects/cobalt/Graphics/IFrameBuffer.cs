using System;

namespace Cobalt.Graphics
{
    public interface IFrameBuffer : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder RenderPass(IRenderPass renderPass)
                {
                    base.RenderPass = renderPass;
                    return this;
                }

                public new Builder Width(int width)
                {
                    base.Width = width;
                    return this;
                }

                public new Builder Height(int height)
                {
                    base.Height = height;
                    return this;
                }

                public new Builder Layers(int layers)
                {
                    base.Layers = layers;
                    return this;
                }
            }

            public IRenderPass RenderPass { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public int Layers { get; private set; }
        }
    }
}
