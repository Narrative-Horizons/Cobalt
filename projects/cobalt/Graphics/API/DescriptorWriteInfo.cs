using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public class DescriptorWriteInfo
    {
        public class DescriptorImageInfo
        {
            public sealed class Builder : DescriptorImageInfo
            {
                public new Builder Sampler(ISampler sampler)
                {
                    base.Sampler = sampler;
                    return this;
                }

                public new Builder View(IImageView view)
                {
                    base.View = view;
                    return this;
                }

                public new Builder Layout(EImageLayout layout)
                {
                    base.Layout = layout;
                    return this;
                }

                public DescriptorImageInfo Build()
                {
                    DescriptorImageInfo info = new DescriptorImageInfo
                    {
                        Layout = base.Layout,
                        Sampler = base.Sampler,
                        View = base.View
                    };

                    return info;
                }
            }

            public ISampler Sampler { get; private set; }
            public IImageView View { get; private set; }
            public EImageLayout Layout { get; private set; }
        }

        public class DescriptorBufferInfo
        {
            public sealed class Builder : DescriptorBufferInfo
            {
                public new Builder Buffer(IBuffer buffer)
                {
                    base.Buffer = buffer;
                    return this;
                }

                public new Builder Offset(int offset)
                {
                    base.Offset = offset;
                    return this;
                }

                public new Builder Range(int range)
                {
                    base.Range = range;
                    return this;
                }

                public DescriptorBufferInfo Build()
                {
                    DescriptorBufferInfo info = new DescriptorBufferInfo
                    {
                        Buffer = base.Buffer,
                        Offset = base.Offset,
                        Range = base.Range
                    };

                    return this;
                }
            }

            public IBuffer Buffer { get; private set; }
            public int Offset { get; private set; }
            public int Range { get; private set; }
        }

        public sealed class Builder : DescriptorWriteInfo
        {
            public new Builder DescriptorSet(IDescriptorSet set)
            {
                base.DescriptorSet = set;
                return this;
            }

            public new Builder BindingIndex(int binding)
            {
                base.BindingIndex = binding;
                return this;
            }

            public new Builder ArrayElement(int arrayElement)
            {
                base.ArrayElement = arrayElement;
                return this;
            }

            public Builder AddImageInfo(DescriptorImageInfo info)
            {
                base.ImageInfo.Add(info);
                return this;
            }

            public Builder AddImageInfo(DescriptorImageInfo.Builder info)
            {
                base.ImageInfo.Add(info.Build());
                return this;
            }

            public Builder AddBufferInfo(DescriptorBufferInfo info)
            {
                base.BufferInfo.Add(info);
                return this;
            }

            public Builder AddBufferInfo(DescriptorBufferInfo.Builder info)
            {
                base.BufferInfo.Add(info.Build());
                return this;
            }

            public DescriptorWriteInfo Build()
            {
                DescriptorWriteInfo info = new DescriptorWriteInfo
                {
                    ArrayElement = base.ArrayElement,
                    BindingIndex = base.BindingIndex,
                    DescriptorSet = base.DescriptorSet,
                    ImageInfo = base.ImageInfo,
                    BufferInfo = base.BufferInfo
                };

                return info;
            }
        }

        public IDescriptorSet DescriptorSet { get; private set; }
        public int BindingIndex { get; private set; }
        public int ArrayElement { get; private set; }
        public List<DescriptorImageInfo> ImageInfo { get; private set; } = new List<DescriptorImageInfo>();
        public List<DescriptorBufferInfo> BufferInfo { get; private set; } = new List<DescriptorBufferInfo>();
    }
}
