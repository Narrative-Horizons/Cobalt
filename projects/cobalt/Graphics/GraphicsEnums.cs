using System;

namespace Cobalt.Graphics
{
    public enum EDataFormat : uint
    {
        Unknown,
        BGRA8_SRGB,
        R8G8B8A8_SRGB,
        R8G8B8A8,
        R32G32_SFLOAT,
        R32G32B32_SFLOAT,
        R32G32B32A32_SFLOAT
    }

    public enum EImageType : uint
    {
        Image1D,
        Image2D,
        Image3D,
        ImageCube
    }

    [Flags]
    public enum ESampleCount : uint
    {
        Samples1 = 0x01,
        Samples2 = 0x02,
        Samples4 = 0x04,
        Samples8 = 0x08,
        Samples16 = 0x10
    }

    public enum EImageUsage : uint
    {
        TransferSource,
        TransferDestination,
        Sampled,
        Storage,
        ColorAttachment,
        DepthStencilAttachment,
        InputAttachment
    }

    public enum EImageLayout : uint
    {
        Undefined,
        ColorAttachment,
        DepthAttachment,
        DepthAttachmentStencilReadOnly,
        DepthReadOnly,
        DepthReadOnlyStencilAttachment,
        DepthStencilAttachment,
        DepthStencilReadOnly,
        General,
        PresentSource,
        ShaderReadOnly,
        StencilAttachment,
        StencilReadOnly,
        TransferSrc,
        TransferDst
    }

    public enum EMemoryUsage : uint
    {
        GPUOnly,
        CPUOnly,
        CPUToGPU,
        GPUToCPU
    }

    public enum EMemoryProperty : uint
    {
        DeviceLocal,
        HostVisible,
        HostCoherent,
        HostCached
    }

    public enum EBufferUsage : uint
    {
        TransferSource,
        TransferDestination,
        UniformBuffer,
        StorageBuffer,
        ArrayBuffer,
        IndexBuffer,
        TextureBuffer,
        IndirectBuffer
    }

    public enum EColorSpace : uint
    {
        SrgbNonLinear
    }

    public enum EPresentMode : uint
    {
        FirstInFirstOut,
        Mailbox
    }

    public enum EAttachmentLoad : uint
    {
        Load,
        Clear,
        DontCare
    }

    public enum EAttachmentStore : uint
    {
        Store,
        DontCare
    }
}
