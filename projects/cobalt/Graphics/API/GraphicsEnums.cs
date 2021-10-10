using System;

namespace Cobalt.Graphics.API
{
    public enum EDataFormat : uint
    {
        Unknown,
        BGRA8_SRGB,
        R8G8B8A8_SRGB,
        R8G8B8A8,
        R32_UINT,
        R32G32_UINT,
        R32G32_SFLOAT,
        R32G32B32_SFLOAT,
        R32G32B32A32_SFLOAT,
        D24_SFLOAT_S8_UINT,
        D32_SFLOAT,
    }

    public static class EnumExtensions
    {
        public static uint GetBppFromFormat(this EDataFormat format)
        {
            switch (format)
            {
                case EDataFormat.BGRA8_SRGB:
                case EDataFormat.R8G8B8A8_SRGB:
                case EDataFormat.R8G8B8A8:
                case EDataFormat.R32_UINT:
                case EDataFormat.D24_SFLOAT_S8_UINT:
                case EDataFormat.D32_SFLOAT:
                    return 4;
                case EDataFormat.R32G32_UINT:
                case EDataFormat.R32G32_SFLOAT:
                    return 8;
                case EDataFormat.R32G32B32_SFLOAT:
                    return 12;
                case EDataFormat.R32G32B32A32_SFLOAT:
                    return 16;
                default:
                    return 0;
            }
        }
    }

    public enum EPipelineBindPoint : uint
    {
        Graphics = 0,
        Compute = 1,
        RayTracingKHR = 1000165000,
        SubpassShadingHuawei = 1000369003,
        RayTracingNV = RayTracingKHR,
        MaxEnum = 0x7FFFFFFF
    }

    [Flags]
    public enum EPipelineStageFlags : uint
    {
        TopOfPipeBit = 0x00000001,
        DrawIndirectBit = 0x00000002,
        VertexInputBit = 0x00000004,
        VertexShaderBit = 0x00000008,
        TessellationControlShaderBit = 0x00000010,
        TessellationEvaluationShaderBit = 0x00000020,
        GeometryShaderBit = 0x00000040,
        FragmentShaderBit = 0x00000080,
        EarlyFragmentTestsBit = 0x00000100,
        LateFragmentTestsBit = 0x00000200,
        ColorAttachmentOutputBit = 0x00000400,
        ComputeShaderBit = 0x00000800,
        TransferBit = 0x00001000,
        BottomOfPipeBit = 0x00002000,
        HostBit = 0x00004000,
        AllGraphicsBit = 0x00008000,
        AllCommandsBit = 0x00010000,
        TransformFeedbackBitExt = 0x01000000,
        ConditionalRenderingBitExt = 0x00040000,
        AccelerationStructureBuildBitKHR = 0x02000000,
        RayTracingShaderBitKHR = 0x00200000,
        TaskShaderBitNV = 0x00080000,
        MeshShaderBitNV = 0x00100000,
        FragmentDensityProcessBitExt = 0x00800000,
        FragmentShadingRateAttachmentBitKHR = 0x00400000,
        CommandPreprocessBitNV = 0x00020000,
        NoneKHR = 0,
        ShadingRateImageBitNV = FragmentShadingRateAttachmentBitKHR,
        RayTracingShaderBitNV = RayTracingShaderBitKHR,
        AccelerationStructureBuildBitNV = AccelerationStructureBuildBitKHR,
        FlagBitsMaxEnum = 0x7FFFFFFF
    }

    [Flags]
    public enum EDependencyFlags : uint
    {
        ByRegionBit = 0x00000001,
        DeviceGroupBit = 0x00000004,
        ViewLocalBit = 0x00000002,
        ViewLocalBitKHR = ViewLocalBit,
        DeviceGroupBitKHR = DeviceGroupBit,
        FlagBitsMaxEnum = 0x7FFFFFFF
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
        DepthAttachment,
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

    public enum ECommandBufferLevel : uint
    {
        Primary,
        Secondary
    }

    public enum EFilter : uint
    {
        Nearest,
        Linear
    }

    public enum EMipmapMode : uint
    {
        Nearest,
        Linear
    }

    public enum EAddressMode : uint
    {
        Repeat,
        MirroredRepeat,
        ClampToEdge,
        MirroredClampToEdge
    }

    [Flags]
    public enum EAccessFlag : uint
    {
        IndirectCommandReadBit = 0x00000001,
        IndexReadBit = 0x00000002,
        VertexAttributeReadBit = 0x00000004,
        UniformReadBit = 0x00000008,
        InputAttachmentReadBit = 0x00000010,
        ShaderReadBit = 0x00000020,
        ShaderWriteBit = 0x00000040,
        ColorAttachmentReadBit = 0x00000080,
        ColorAttachmentWriteBit = 0x00000100,
        DepthStencilAttachmentReadBit = 0x00000200,
        DepthStencilAttachmentWriteBit = 0x00000400,
        TransferReadBit = 0x00000800,
        TransferWriteBit = 0x00001000,
        HostReadBit = 0x00002000,
        HostWriteBit = 0x00004000,
        MemoryReadBit = 0x00008000,
        MemoryWriteBit = 0x00010000,
        TransformFeedbackWriteBitExt = 0x02000000,
        TransformFeedbackCounterReadBitExt = 0x04000000,
        TransformFeedbackCounterWriteBitExt = 0x08000000,
        ConditionalRenderingReadBitExt = 0x00100000,
        ColorAttachmentReadNoncoherentBitExt = 0x00080000,
        AccelerationStructureReadBitKHR = 0x00200000,
        AccelerationStructureWriteBitKHR = 0x00400000,
        FragmentDensityMapReadBitExt = 0x01000000,
        FragmentShadingRateAttachmentReadBitKHR = 0x00800000,
        CommandPreprocessReadBitNV = 0x00020000,
        CommandPreprocessWriteBitNV = 0x00040000,
        NoneKHR = 0,
        ShadingRateImageReadBitNV = FragmentShadingRateAttachmentReadBitKHR,
        AccelerationStructureReadBitNV = AccelerationStructureReadBitKHR,
        AccelerationStructureWriteBitNV = AccelerationStructureWriteBitKHR,
        FlagBitsMaxEnum = 0x7FFFFFFF
    }
}
