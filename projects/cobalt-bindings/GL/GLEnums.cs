using System;

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

    [Flags]
    public enum EMapBit : uint
    {
        MapReadBit = 0x0001,
        MapWriteBit = 0x0002,
        MapPersistentBit = 0x0040,
        MapCoherentBit = 0x0080,
        DynamicStorageBit = 0x0100,
        ClientStorageBit = 0x0200
    }

    public enum EAccessType : uint
    {
        ReadOnly = 0x88B8,
        WriteOnly = 0x88B9,
        ReadWrite = 0x88BA
    }
}
