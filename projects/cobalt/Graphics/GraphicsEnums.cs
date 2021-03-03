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

    public enum EImageViewType : uint
    {
        ViewType1D,
        ViewType2D,
        ViewType3D,
        ViewTypeCube,
        ViewType1DArray,
        ViewType2DArray,
        ViewTypeCubeArray
    }

    public enum EShaderType : uint
    {
        Vertex,
        TessellationControl,
        TessellationEvaluation,
        Geometry,
        Fragment,
        Compute
    }

    public enum EVertexInputRate : uint
    {
        PerVertex,
        PerInstance
    }
    
    public enum ETopology : uint
    {
        PointList,
        LineList,
        LineStrip,
        TriangleList,
        TriangleStrip,
        TriangleFan
    }

    public enum EPolygonMode : uint
    {
        Fill,
        Point,
        Wireframe
    }

    public enum EPolgyonFace : uint
    {
        None,
        Back,
        Front,
        FrontAndBack
    }

    public enum EVertexWindingOrder : uint
    {
        Clockwise,
        CounterClockwise
    }

    public enum EStencilOp : uint
    {
        Keep,
        Zero,
        Replace,
        IncrementAndClamp,
        DecrementAndClamp,
        Invert,
        IncrementAndWrap,
        DecrementAndWrap
    }

    public enum ECompareOp : uint
    {
        Never,
        Less,
        Equal,
        LessOrEqual,
        Greater,
        NotEqual,
        GreaterOrEqual,
        Always
    }

    public enum EBlendOp : uint
    {
        Add,
        Subtract,
        ReverseSubtract,
        Minimum,
        Maximum
    }

    public enum EBlendFactor : uint
    {
        Zero,
        One,
        SrcColor,
        OneMinusSrcColor,
        DstColor,
        OneMinusDstColor,
        SrcAlpha,
        OneMinusSrcAlpha,
        DstAlpha,
        OneMinusDstAlpha,
        ConstantColor,
        OneMinusConstantColor,
        ConstantAlpha,
        OneMinusConstantAlpha,
        AlphaSaturate
    }

    public enum ELogicOp : uint
    {
        Clear,
        Set,
        Copy,
        CopyInverted,
        Invert,
        And,
        Nand,
        Or,
        Nor,
        Xor,
        Equiv,
        AndReverse,
        AndInverted,
        OrReverse,
        OrInverted
    }

    public enum EDynamicState : uint
    {
        Viewport,
        Scissor,
        LineWidth,
        DepthBias,
        BlendConstants,
        DepthBounds,
        StencilCompareMask,
        WriteMask,
        Reference
    }

    public enum EDescriptorType : uint
    {
        Sampler,
        SampledImage,
        CombinedImageSampler,
        TextureBuffer,
        UniformBuffer,
        StorageBuffer
    }
}
