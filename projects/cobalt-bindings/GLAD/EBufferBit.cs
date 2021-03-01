using System;

namespace Cobalt.Bindings.GL
{
    [Flags]
    public enum EBufferBit : uint
    {
        DepthBuffer   = 0x0100,
        StencilBuffer = 0x0400,
        ColorBuffer   = 0x4000,
    }
}