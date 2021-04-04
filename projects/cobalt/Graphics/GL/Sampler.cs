using Cobalt.Bindings.GL;
using Cobalt.Graphics.API;

using static Cobalt.Bindings.GL.GL;

namespace Cobalt.Graphics.GL
{
    internal class Sampler : ISampler
    {
        public readonly uint Handle;

        public Sampler(ISampler.CreateInfo info)
        {
            Handle = CreateSamplers();
            var wrapU = ToWrap(info.AddressModeU);
            var wrapV = ToWrap(info.AddressModeV);
            var wrapW = ToWrap(info.AddressModeW);
            var minFilter = ToFilter(info.MinFilter, info.MipmapMode);
            var magFilter = ToFilter(info.MagFilter);
            var minLod = info.MinimumLod;
            var maxLod = info.MaximumLod;
            var lodBias = info.MipLodBias;

            SamplerParameterI(Handle, (uint)ETextureParameterName.TextureWrapS, (int)wrapU);
            SamplerParameterI(Handle, (uint)ETextureParameterName.TextureWrapT, (int)wrapV);
            SamplerParameterI(Handle, (uint)ETextureParameterName.TextureWrapR, (int)wrapW);
            SamplerParameterI(Handle, (uint)ETextureParameterName.TextureMinFilter, (int)minFilter);
            SamplerParameterI(Handle, (uint)ETextureParameterName.TextureMagFilter, (int)magFilter);
            SamplerParameterF(Handle, (uint)ETextureParameterName.TextureMinLod, minLod);
            SamplerParameterF(Handle, (uint)ETextureParameterName.TextureMaxLod, maxLod);
            SamplerParameterF(Handle, (uint)ETextureParameterName.TextureLodBias, lodBias);

            if (info.CompareOp != null)
            {
                var func = ToFunc((ECompareOp) info.CompareOp);
                SamplerParameterI(Handle, (uint)ETextureParameterName.TextureCompareMode, (int)ETextureParameter.CompareRefToTexture);
                SamplerParameterI(Handle, (uint)ETextureParameterName.TextureCompareFunc, (int)func);
            }

            // TODO: Aniso
        }

        public void Dispose()
        {
            DeleteSamplers(Handle);
        }

        private static ETextureParameter ToWrap(EAddressMode addrMode)
        {
            switch (addrMode)
            {
                case EAddressMode.ClampToEdge:
                    return ETextureParameter.ClampToEdge;
                case EAddressMode.MirroredRepeat:
                    return ETextureParameter.MirroredRepeat;
                case EAddressMode.MirroredClampToEdge:
                    return ETextureParameter.MirrorClampToEdge;
                case EAddressMode.Repeat:
                    return ETextureParameter.ClampToEdge;
                default:
                    break;
            }
            throw new System.InvalidOperationException("Invalid address mode.");
        }

        private static ETextureParameter ToFilter(EFilter filter)
        {
            switch (filter)
            {
                case EFilter.Linear:
                    return ETextureParameter.Linear;
                case EFilter.Nearest:
                    return ETextureParameter.Nearest;
                default:
                    break;
            }
            throw new System.InvalidOperationException("Invalid texture filter.");
        }

        private static ETextureParameter ToFilter(EFilter filter, EMipmapMode mip)
        {
            switch (filter)
            {
                case EFilter.Linear:
                    switch (mip)
                    {
                        case EMipmapMode.Linear:
                            return ETextureParameter.LinearMipMapLinear;
                        case EMipmapMode.Nearest:
                            return ETextureParameter.LinearMipMapNearest;
                        default:
                            break;
                    }
                    break;
                case EFilter.Nearest:
                    switch (mip)
                    {
                        case EMipmapMode.Linear:
                            return ETextureParameter.NearestMipMapLinear;
                        case EMipmapMode.Nearest:
                            return ETextureParameter.NearestMipMapNearest;
                        default:
                            break;
                    }
                    break;
            }
            throw new System.InvalidOperationException("Invalid texture filter and mipmap mode combination.");
        }

        private static EStencilFunction ToFunc(ECompareOp op)
        {
            switch (op)
            {
                case ECompareOp.Always:
                    return EStencilFunction.Always;
                case ECompareOp.Equal:
                    return EStencilFunction.Equal;
                case ECompareOp.Greater:
                    return EStencilFunction.Greater;
                case ECompareOp.GreaterOrEqual:
                    return EStencilFunction.Gequal;
                case ECompareOp.Less:
                    return EStencilFunction.Less;
                case ECompareOp.LessOrEqual:
                    return EStencilFunction.Lequal;
                case ECompareOp.Never:
                    return EStencilFunction.Never;
                case ECompareOp.NotEqual:
                    return EStencilFunction.Notequal;
            }
            throw new System.InvalidOperationException("Invalid stencil function.");
        }
    }
}