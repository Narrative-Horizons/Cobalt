using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Bindings.GL
{
    public enum ETextureTarget : uint
    {
        Texture1D = 0x0DE0,
        Texture1DArray = 0x8C18,
        Texture2D = 0x0DE1,
        Texture2DArray = 0x8C1A,
        Texture3D = 0x806F,
        TextureCubeMap = 0x8513,
        TextureCubeMapArray = 0x9009
    }
}
