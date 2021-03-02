using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics
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
                    Attributes.Add(attribute);
                    return this;
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
            }

            public float ConstantFactor { get; private set; }
            public float BiasClamp { get; private set; }
            public float SlopeFactor { get; private set; }
        }
    }
}
