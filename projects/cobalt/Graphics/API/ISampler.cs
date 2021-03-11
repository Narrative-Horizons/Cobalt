using System;

namespace Cobalt.Graphics.API
{
    public interface ISampler : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder MagFilter(EFilter mag)
                {
                    base.MagFilter = mag;
                    return this;
                }

                public new Builder MinFilter(EFilter min)
                {
                    base.MinFilter = min;
                    return this;
                }

                public new Builder MipmapMode(EMipmapMode mode)
                {
                    base.MipmapMode = mode;
                    return this;
                }

                public new Builder AddressModeU(EAddressMode addrMode)
                {
                    base.AddressModeU = addrMode;
                    return this;
                }

                public new Builder AddressModeV(EAddressMode addrMode)
                {
                    base.AddressModeV = addrMode;
                    return this;
                }

                public new Builder AddressModeW(EAddressMode addrMode)
                {
                    base.AddressModeW = addrMode;
                    return this;
                }

                public new Builder MipLodBias(float bias)
                {
                    base.MipLodBias = bias;
                    return this; ;
                }

                public new Builder MaxAnisotropy(float maxAniso)
                {
                    base.MaxAnisotropy = maxAniso;
                    return this;
                }

                public new Builder CompareOp(ECompareOp op)
                {
                    base.CompareOp = op;
                    return this;
                }

                public new Builder MinimumLod(float lod)
                {
                    base.MinimumLod = lod;
                    return this;
                }

                public new Builder MaximumLod(float lod)
                {
                    base.MaximumLod = lod;
                    return this;
                }

                public new Builder UnnormalizedCoordinates(bool unnormalized)
                {
                    base.UnnormalizedCoordinates = unnormalized;
                    return this;
                }

                public CreateInfo Build()
                {
                    return new CreateInfo()
                    {
                        MagFilter = base.MagFilter,
                        MinFilter = base.MinFilter,
                        MipmapMode = base.MipmapMode,
                        AddressModeU = base.AddressModeU,
                        AddressModeV = base.AddressModeV,
                        AddressModeW = base.AddressModeW,
                        MipLodBias = base.MipLodBias,
                        MaxAnisotropy = base.MaxAnisotropy,
                        CompareOp = base.CompareOp,
                        MinimumLod = base.MinimumLod,
                        MaximumLod = base.MaximumLod,
                        UnnormalizedCoordinates = base.UnnormalizedCoordinates
                    };
                }
            }

            public EFilter MagFilter { get; private set; }
            public EFilter MinFilter { get; private set; }
            public EMipmapMode MipmapMode { get; private set; }
            public EAddressMode AddressModeU { get; private set; }
            public EAddressMode AddressModeV { get; private set; }
            public EAddressMode AddressModeW { get; private set; }
            public float MipLodBias { get; private set; }
            public float? MaxAnisotropy { get; private set; }
            public ECompareOp? CompareOp { get; private set; }
            public float MinimumLod { get; private set; }
            public float MaximumLod { get; private set; }
            public bool UnnormalizedCoordinates { get; private set; }
        }
    }
}
