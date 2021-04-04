using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface IFrameBuffer : IDisposable
    {
        public class CreateInfo
        {
            public class Attachment
            {
                public sealed class Builder : Attachment
                {
                    public new Builder ImageView(IImageView imageView)
                    {
                        base.ImageView = imageView;
                        return this;
                    }

                    public new Builder Usage(EImageUsage usage)
                    {
                        base.Usage = usage;
                        return this;
                    }
                }

                public IImageView ImageView { get; private set; }
                public EImageUsage Usage { get; private set; }
            }

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

                public Builder AddAttachment(Attachment attachment)
                {
                    Attachments.Add(attachment);
                    return this;
                }
            }

            public IRenderPass RenderPass { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public int Layers { get; private set; }
            public List<Attachment> Attachments { get; private set; } = new List<Attachment>();
        }
    }
}
