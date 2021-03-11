using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface IGraphicsPipeline : IDisposable
    {
        public class ShaderStageCreateInfo
        {
            public sealed class Builder : ShaderStageCreateInfo
            {
                public new Builder Module(IShaderModule module)
                {
                    base.Module = module;
                    return this;
                }

                public new Builder EntryPoint(string entryPoint)
                {
                    base.EntryPoint = entryPoint;
                    return this;
                }

                public ShaderStageCreateInfo Build()
                {
                    return new ShaderStageCreateInfo()
                    {
                        Module = base.Module,
                        EntryPoint = base.EntryPoint
                    };
                }
            }

            public IShaderModule Module { get; private set; }
            public string EntryPoint { get; private set; }
        }

        public class VertexAttributeCreateInfo
        {
            public sealed class Builder : VertexAttributeCreateInfo
            {
                public Builder AddAttribute(VertexAttribute attribute)
                {
                    base.Attributes.Add(attribute);
                    return this;
                }

                public new Builder Attributes(List<VertexAttribute> attributes)
                {
                    base.Attributes.Clear();
                    base.Attributes.AddRange(attributes);
                    return this;
                }

                public VertexAttributeCreateInfo Build()
                {
                    return new VertexAttributeCreateInfo()
                    {
                        Attributes = new List<VertexAttribute>(base.Attributes)
                    };
                }
            }

            public List<VertexAttribute> Attributes { get; private set; } = new List<VertexAttribute>();
        }

        public class InputAssemblyCreateInfo
        {
            public sealed class Builder : InputAssemblyCreateInfo
            {
                public new Builder Topology(ETopology topology)
                {
                    base.Topology = topology;
                    return this;
                }

                public new Builder RestartEnabled(bool restart)
                {
                    base.RestartEnabled = restart;
                    return this;
                }

                public InputAssemblyCreateInfo Create()
                {
                    return new InputAssemblyCreateInfo()
                    {
                        Topology = base.Topology,
                        RestartEnabled = base.RestartEnabled
                    };
                }
            }

            public ETopology Topology { get; private set; }
            public bool RestartEnabled { get; private set; }
        }

        public class TessellationCreateInfo
        {
            public sealed class Builder : TessellationCreateInfo
            {
                public new Builder ControlPointCount(int controlPoints)
                {
                    base.ControlPointCount = controlPoints;
                    return this;
                }

                public TessellationCreateInfo Build()
                {
                    return new TessellationCreateInfo()
                    {
                        ControlPointCount = base.ControlPointCount
                    };
                }
            }

            public int ControlPointCount { get; private set; }
        }

        public class ViewportCreateInfo
        {
            public sealed class Builder : ViewportCreateInfo
            {
                public new Builder Viewport(Viewport viewport)
                {
                    base.Viewport = viewport;
                    return this;
                }

                public new Builder ScissorRegion(Scissor scissor)
                {
                    base.ScissorRegion = scissor;
                    return this;
                }

                public ViewportCreateInfo Build()
                {
                    return new ViewportCreateInfo()
                    {
                        Viewport = base.Viewport,
                        ScissorRegion = base.ScissorRegion
                    };
                }
            }

            public Viewport Viewport { get; private set; }
            public Scissor ScissorRegion { get; private set; }
        }

        public class DepthBiasCreateInfo
        {
            public sealed class Builder : DepthBiasCreateInfo
            {
                public new Builder ConstantFactor(float constantFactor)
                {
                    base.ConstantFactor = constantFactor;
                    return this;
                }

                public new Builder BiasClamp(float biasClamp)
                {
                    base.BiasClamp = biasClamp;
                    return this;
                }

                public new Builder SlopeFactor(float slopeFactor)
                {
                    base.SlopeFactor = slopeFactor;
                    return this;
                }

                public DepthBiasCreateInfo Build()
                {
                    return new DepthBiasCreateInfo()
                    {
                        ConstantFactor = base.ConstantFactor,
                        BiasClamp = base.BiasClamp,
                        SlopeFactor = base.SlopeFactor
                    };
                }
            }

            public float ConstantFactor { get; private set; } = 0.0f;
            public float BiasClamp { get; private set; } = 0.0f;
            public float SlopeFactor { get; private set; } = 1.0f;
        }

        public class RasterizerCreateInfo
        {
            public sealed class Builder : RasterizerCreateInfo
            {
                public new Builder DepthClampEnabled(bool enabled)
                {
                    base.DepthClampEnabled = enabled;
                    return this;
                }

                public new Builder RasterizerDiscardEnabled(bool enabled)
                {
                    base.RasterizerDiscardEnabled = enabled;
                    return this;
                }

                public new Builder PolygonMode(EPolygonMode mode)
                {
                    base.PolygonMode = mode;
                    return this;
                }

                public new Builder CullFaces(EPolgyonFace faces)
                {
                    base.CullFaces = faces;
                    return this;
                }

                public new Builder WindingOrder(EVertexWindingOrder order)
                {
                    base.WindingOrder = order;
                    return this;
                }

                public new Builder DepthBias(DepthBiasCreateInfo info)
                {
                    base.DepthBias = info;
                    return this;
                }

                public new Builder DepthBias(DepthBiasCreateInfo.Builder bldr)
                {
                    return DepthBias(bldr.Build());
                }

                public RasterizerCreateInfo Build()
                {
                    return new RasterizerCreateInfo
                    {
                        DepthClampEnabled = base.DepthClampEnabled,
                        RasterizerDiscardEnabled = base.RasterizerDiscardEnabled,
                        PolygonMode = base.PolygonMode,
                        CullFaces = base.CullFaces,
                        WindingOrder = base.WindingOrder,
                        DepthBias = base.DepthBias
                    };
                }
            }

            public bool DepthClampEnabled { get; private set; } = false;
            public bool RasterizerDiscardEnabled { get; private set; } = true;
            public EPolygonMode PolygonMode { get; private set; } = EPolygonMode.Fill;
            public EPolgyonFace CullFaces { get; private set; } = EPolgyonFace.None;
            public EVertexWindingOrder WindingOrder { get; private set; } = EVertexWindingOrder.Clockwise;
            public DepthBiasCreateInfo DepthBias { get; private set; }
        }

        public class MultisampleCreateInfo
        {
            public sealed class Builder : MultisampleCreateInfo
            {
                public new Builder Samples(ESampleCount count)
                {
                    base.Samples = count;
                    return this;
                }

                public new Builder MinimumSampleShading(float min)
                {
                    base.MinimumSampleShading = min;
                    return this;
                }

                public new Builder SampleMask(int mask)
                {
                    base.SampleMask = mask;
                    return this;
                }

                public new Builder AlphaToCoverageEnabled(bool enabled)
                {
                    base.AlphaToCoverageEnabled = enabled;
                    return this;
                }

                public new Builder AlphaToOneEnabled(bool enabled)
                {
                    base.AlphaToOneEnabled = enabled;
                    return this;
                }

                public MultisampleCreateInfo Build()
                {
                    return new MultisampleCreateInfo()
                    {
                        Samples = base.Samples,
                        MinimumSampleShading = base.MinimumSampleShading,
                        SampleMask = base.SampleMask,
                        AlphaToCoverageEnabled = base.AlphaToCoverageEnabled,
                        AlphaToOneEnabled = base.AlphaToOneEnabled
                    };
                }
            }

            public ESampleCount Samples { get; private set; } = ESampleCount.Samples1;
            public float? MinimumSampleShading { get; private set; } = null;
            public int? SampleMask { get; private set; } = null;
            public bool AlphaToCoverageEnabled { get; private set; } = false;
            public bool AlphaToOneEnabled { get; private set; } = false;
        }

        public class DepthBoundsCreateInfo
        {
            public sealed class Builder : DepthBoundsCreateInfo
            {
                public new Builder MinDepthBounds(float min)
                {
                    base.MinDepthBounds = min;
                    return this;
                }

                public new Builder MaxDepthBounds(float max)
                {
                    base.MaxDepthBounds = max;
                    return this;
                }

                public DepthBoundsCreateInfo Build()
                {
                    return new DepthBoundsCreateInfo()
                    {
                        MinDepthBounds = base.MinDepthBounds,
                        MaxDepthBounds = base.MaxDepthBounds
                    };
                }
            }

            public float MinDepthBounds { get; private set; }
            public float MaxDepthBounds { get; private set; }
        }

        public class StencilOpCreateInfo
        {
            public sealed class Builder : StencilOpCreateInfo
            {
                public new Builder Fail(EStencilOp op)
                {
                    base.Fail = op;
                    return this;
                }

                public new Builder Pass(EStencilOp op)
                {
                    base.Pass = op;
                    return this;
                }

                public new Builder DepthFail(EStencilOp op)
                {
                    base.DepthFail = op;
                    return this;
                }

                public new Builder CompareOp(ECompareOp op)
                {
                    base.CompareOp = op;
                    return this;
                }

                public new Builder CompareMask(uint mask)
                {
                    base.CompareMask = mask;
                    return this;
                }

                public new Builder WriteMask(uint mask)
                {
                    base.WriteMask = mask;
                    return this;
                }

                public new Builder Reference(uint reference)
                {
                    base.Reference = reference;
                    return this;
                }

                public StencilOpCreateInfo Build()
                {
                    return new StencilOpCreateInfo()
                    {
                        Fail = base.Fail,
                        Pass = base.Pass,
                        DepthFail = base.DepthFail,
                        CompareOp = base.CompareOp,
                        CompareMask = base.CompareMask,
                        WriteMask = base.WriteMask,
                        Reference = base.Reference
                    };
                }
            }

            public EStencilOp Fail { get; private set; }
            public EStencilOp Pass { get; private set; }
            public EStencilOp DepthFail { get; private set; }
            public ECompareOp CompareOp { get; private set; }
            public uint CompareMask { get; private set; } = 0;
            public uint WriteMask { get; private set; } = 0;
            public uint Reference { get; private set; } = 0;
        }

        public class StencilTestCreateInfo
        {
            public sealed class Builder : StencilTestCreateInfo
            {
                public new Builder Back(StencilOpCreateInfo back)
                {
                    base.Back = back;
                    return this;
                }

                public new Builder Back(StencilOpCreateInfo.Builder back)
                {
                    base.Back = back.Build();
                    return this;
                }

                public new Builder Front(StencilOpCreateInfo front)
                {
                    base.Front = front;
                    return this;
                }

                public new Builder Front(StencilOpCreateInfo.Builder front)
                {
                    base.Front = front.Build();
                    return this;
                }

                public StencilTestCreateInfo Build()
                {
                    return new StencilTestCreateInfo()
                    {
                        Back = base.Back,
                        Front = base.Front
                    };
                }
            }

            public StencilOpCreateInfo Back { get; private set; }
            public StencilOpCreateInfo Front { get; private set; }
        }

        public class DepthStencilCreateInfo
        {
            public sealed class Builder : DepthStencilCreateInfo
            {
                public new Builder DepthTestEnabled(bool enabled)
                {
                    base.DepthTestEnabled = enabled;
                    return this;
                }

                public new Builder DepthWriteEnabled(bool enabled)
                {
                    base.DepthWriteEnabled = enabled;
                    return this;
                }

                public new Builder DepthCompareOp(ECompareOp compareOp)
                {
                    base.DepthCompareOp = compareOp;
                    return this;
                }

                public new Builder DepthBoundsTest(DepthBoundsCreateInfo depthBoundsTest)
                {
                    base.DepthBoundsTest = depthBoundsTest;
                    return this;
                }

                public new Builder DepthBoundsTest(DepthBoundsCreateInfo.Builder depthBoundsTest)
                {
                    base.DepthBoundsTest = depthBoundsTest.Build();
                    return this;
                }

                public new Builder StencilTest(StencilTestCreateInfo stencilTest)
                {
                    base.StencilTest = stencilTest;
                    return this;
                }

                public new Builder StencilTest(StencilTestCreateInfo.Builder stencilTest)
                {
                    base.StencilTest = stencilTest.Build();
                    return this;
                }

                public DepthStencilCreateInfo Build()
                {
                    return new DepthStencilCreateInfo()
                    {
                        DepthTestEnabled = base.DepthTestEnabled,
                        DepthWriteEnabled = base.DepthWriteEnabled,
                        DepthCompareOp = base.DepthCompareOp,
                        DepthBoundsTest = base.DepthBoundsTest,
                        StencilTest = base.StencilTest
                    };
                }
            }

            public bool DepthTestEnabled { get; private set; }
            public bool DepthWriteEnabled { get; private set; }
            public ECompareOp DepthCompareOp { get; private set; }
            public DepthBoundsCreateInfo DepthBoundsTest { get; private set; }
            public StencilTestCreateInfo StencilTest { get; private set; }
        }

        public class ColorAttachmentBlendCreateInfo
        {
            public sealed class Builder : ColorAttachmentBlendCreateInfo
            {
                public new Builder ColorBlendOp(EBlendOp colorBlendOp)
                {
                    base.ColorBlendOp = colorBlendOp;
                    return this;
                }

                public new Builder SourceColorFactor(EBlendFactor sourceColorFactor)
                {
                    base.SourceColorFactor = sourceColorFactor;
                    return this;
                }

                public new Builder DestinationColorFactor(EBlendFactor destinationColorFactor)
                {
                    base.DestinationColorFactor = destinationColorFactor;
                    return this;
                }

                public new Builder AlphaBlendOp(EBlendOp alphaBlendOp)
                {
                    base.AlphaBlendOp = alphaBlendOp;
                    return this;
                }

                public new Builder SourceAlphaFactor(EBlendFactor sourceAlphaFactor)
                {
                    base.SourceAlphaFactor = sourceAlphaFactor;
                    return this;
                }

                public new Builder DestinationAlphaFactor(EBlendFactor destinationAlphaFactor)
                {
                    base.DestinationAlphaFactor = destinationAlphaFactor;
                    return this;
                }

                public new Builder RedWritable(bool writeRed)
                {
                    base.RedWritable = writeRed;
                    return this;
                }

                public new Builder GreenWritable(bool writeGreen)
                {
                    base.GreenWritable = writeGreen;
                    return this;
                }

                public new Builder BlueWritable(bool writeBlue)
                {
                    base.BlueWritable = writeBlue;
                    return this;
                }

                public new Builder AlphaWritable(bool writeAlpha)
                {
                    base.AlphaWritable = writeAlpha;
                    return this;
                }

                public ColorAttachmentBlendCreateInfo Build()
                {
                    return new ColorAttachmentBlendCreateInfo()
                    {
                        ColorBlendOp = base.ColorBlendOp,
                        SourceColorFactor = base.SourceColorFactor,
                        DestinationColorFactor = base.DestinationColorFactor,
                        AlphaBlendOp = base.AlphaBlendOp,
                        SourceAlphaFactor = base.SourceAlphaFactor,
                        DestinationAlphaFactor = base.DestinationAlphaFactor,
                        RedWritable = base.RedWritable,
                        GreenWritable = base.GreenWritable,
                        BlueWritable = base.BlueWritable,
                        AlphaWritable = base.AlphaWritable
                    };
                }
            }

            public EBlendOp ColorBlendOp { get; private set; }
            public EBlendFactor SourceColorFactor { get; private set; }
            public EBlendFactor DestinationColorFactor { get; private set; }
            public EBlendOp AlphaBlendOp { get; private set; }
            public EBlendFactor SourceAlphaFactor { get; private set; }
            public EBlendFactor DestinationAlphaFactor { get; private set; }
            public bool RedWritable { get; private set; }
            public bool GreenWritable { get; private set; }
            public bool BlueWritable { get; private set; }
            public bool AlphaWritable { get; private set; }
        }

        public class ColorBlendCreateInfo
        {
            public sealed class Builder : ColorBlendCreateInfo
            {
                public new Builder LogicOp(ELogicOp logicOp)
                {
                    base.LogicOp = logicOp;
                    return this;
                }

                public Builder AddBlend(ColorAttachmentBlendCreateInfo blend)
                {
                    Blend.Add(blend);
                    return this;
                }

                public Builder AddBlend(ColorAttachmentBlendCreateInfo.Builder blend)
                {
                    Blend.Add(blend.Build());
                    return this;
                }

                public ColorBlendCreateInfo Build()
                {
                    return new ColorBlendCreateInfo()
                    {
                        LogicOp = base.LogicOp,
                        Blend = base.Blend
                    };
                }
            }

            public ELogicOp LogicOp { get; private set; }
            public List<ColorAttachmentBlendCreateInfo> Blend { get; private set; } = new List<ColorAttachmentBlendCreateInfo>();
        }

        public class DynamicStateCreateInfo
        {
            public sealed class Builder : DynamicStateCreateInfo
            {
                public Builder AddDynamicState(EDynamicState state)
                {
                    DynamicState.Add(state);
                    return this;
                }

                public DynamicStateCreateInfo Build()
                {
                    return new DynamicStateCreateInfo()
                    {
                        DynamicState = base.DynamicState
                    };
                }
            }

            public List<EDynamicState> DynamicState { get; private set; } = new List<EDynamicState>();
        }

        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public Builder AddStageCreationInformation(ShaderStageCreateInfo stage)
                {
                    StageCreateInformation.Add(stage);
                    return this;
                }

                public new Builder VertexAttributeCreationInformation(VertexAttributeCreateInfo info)
                {
                    base.VertexAttributeCreationInformation = info;
                    return this;
                }

                public new Builder InputAssemblyCreationInformation(InputAssemblyCreateInfo info)
                {
                    base.InputAssemblyCreationInformation = info;
                    return this;
                }

                public new Builder TessellationCreationInformtion(TessellationCreateInfo info)
                {
                    base.TessellationCreationInformtion = info;
                    return this;
                }

                public new Builder ViewportCreationInformation(ViewportCreateInfo info)
                {
                    base.ViewportCreationInformation = info;
                    return this;
                }

                public new Builder RasterizerCreationInformation(RasterizerCreateInfo info)
                {
                    base.RasterizerCreationInformation = info;
                    return this;
                }

                public new Builder MultisamplingCreationInformation(MultisampleCreateInfo info)
                {
                    base.MultisamplingCreationInformation = info;
                    return this;
                }

                public new Builder DynamicStateCreationInformation(DynamicStateCreateInfo info)
                {
                    base.DynamicStateCreationInformation = info;
                    return this;
                }

                public new Builder RenderPass(IRenderPass pass)
                {
                    base.RenderPass = pass;
                    return this;
                }

                public new Builder PipelineLayout(IPipelineLayout layout)
                {
                    base.PipelineLayout = layout;
                    return this;
                }

                public CreateInfo Build()
                {
                    return new CreateInfo()
                    {
                        StageCreateInformation = base.StageCreateInformation,
                        VertexAttributeCreationInformation = base.VertexAttributeCreationInformation,
                        InputAssemblyCreationInformation = base.InputAssemblyCreationInformation,
                        TessellationCreationInformtion = base.TessellationCreationInformtion,
                        ViewportCreationInformation = base.ViewportCreationInformation,
                        RasterizerCreationInformation = base.RasterizerCreationInformation,
                        MultisamplingCreationInformation = base.MultisamplingCreationInformation,
                        DynamicStateCreationInformation = base.DynamicStateCreationInformation,
                        RenderPass = base.RenderPass,
                        PipelineLayout = base.PipelineLayout
                    };
                }
            }

            public List<ShaderStageCreateInfo> StageCreateInformation { get; private set; } = new List<ShaderStageCreateInfo>();
            public VertexAttributeCreateInfo VertexAttributeCreationInformation { get; private set; }
            public InputAssemblyCreateInfo InputAssemblyCreationInformation { get; private set; }
            public TessellationCreateInfo TessellationCreationInformtion { get; private set; }
            public ViewportCreateInfo ViewportCreationInformation { get; private set; }
            public RasterizerCreateInfo RasterizerCreationInformation { get; private set; }
            public MultisampleCreateInfo MultisamplingCreationInformation { get; private set; }
            public DynamicStateCreateInfo DynamicStateCreationInformation { get; private set; }
            public IRenderPass RenderPass { get; private set; }
            public IPipelineLayout PipelineLayout { get; private set; }
        }

        IPipelineLayout GetLayout();
        IVertexAttributeArray CreateVertexAttributeArray(List<IBuffer> vertexBuffers);
        IVertexAttributeArray CreateVertexAttributeArray(List<IBuffer> vertexBuffers, IBuffer elementBuffer);
    }
}
