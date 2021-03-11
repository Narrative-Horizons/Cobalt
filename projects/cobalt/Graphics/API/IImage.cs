using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface IImage : IDisposable
    {
        public class MemoryInfo
        {
            public sealed class Builder : MemoryInfo
            {
                public new Builder Usage(EMemoryUsage usage)
                {
                    base.Usage = usage;
                    return this;
                }

                public Builder AddRequiredProperty(EMemoryProperty required)
                {
                    Required.Add(required);
                    return this;
                }

                public Builder AddPreferredProperty(EMemoryProperty preferred)
                {
                    Preferred.Add(preferred);
                    return this;
                }
            }

            public EMemoryUsage Usage { get; private set; }
            public List<EMemoryProperty> Required { get; private set; }
            public List<EMemoryProperty> Preferred { get; private set; }

        }

        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder Format(EDataFormat format)
                {
                    base.Format = format;
                    return this;
                }

                public new Builder Type(EImageType type)
                {
                    base.Type = type;
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

                public new Builder Depth(int depth)
                {
                    base.Depth = depth;
                    return this;
                }

                public new Builder MipCount(int mipCount)
                {
                    base.MipCount = mipCount;
                    return this;
                }

                public new Builder LayerCount(int layerCount)
                {
                    base.LayerCount = layerCount;
                    return this;
                }

                public new Builder SampleCount(ESampleCount count)
                {
                    base.SampleCount = count;
                    return this;
                }

                public Builder AddUsage(EImageUsage usage)
                {
                    Usage.Add(usage);
                    return this;
                }

                public Builder AddQueue(IQueue queue)
                {
                    Queues.Add(queue);
                    return this;
                }

                public new Builder InitialLayout(EImageLayout layout)
                {
                    base.InitialLayout = layout;
                    return this;
                }
            }

            public EDataFormat Format { get; private set; }
            public EImageType Type { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public int Depth { get; private set; }
            public int MipCount { get; private set; }
            public int LayerCount { get; private set; }
            public ESampleCount SampleCount { get; private set; }
            public List<EImageUsage> Usage { get; private set; }
            public List<IQueue> Queues { get; private set; }
            public EImageLayout InitialLayout { get; private set; }
        }

        public IImageView CreateImageView(IImageView.CreateInfo info);
    }
}
