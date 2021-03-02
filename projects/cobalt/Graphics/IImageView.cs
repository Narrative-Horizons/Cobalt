using System;

namespace Cobalt.Graphics
{
    public interface IImageView : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder ViewType(EImageViewType viewType)
                {
                    base.ViewType = viewType;
                    return this;
                }

                public new Builder Format(EDataFormat format)
                {
                    base.Format = format;
                    return this;
                }

                public new Builder BaseMipLevel(int baseLevel)
                {
                    base.BaseMipLevel = baseLevel;
                    return this;
                }

                public new Builder MipLevelCount(int count)
                {
                    base.MipLevelCount = count;
                    return this;
                }

                public new Builder BaseArrayLayer(int baseLayer)
                {
                    base.BaseArrayLayer = baseLayer;
                    return this;
                }

                public new Builder ArrayLayerCount(int count)
                {
                    base.ArrayLayerCount = count;
                    return this;
                }
            }

            public EImageViewType ViewType { get; private set; }
            public EDataFormat Format { get; private set; }
            public int BaseMipLevel { get; private set; }
            public int MipLevelCount { get; private set; }
            public int BaseArrayLayer { get; private set; }
            public int ArrayLayerCount { get; private set; }
        }
    }
}
