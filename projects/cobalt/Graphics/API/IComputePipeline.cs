using System;

namespace Cobalt.Graphics.API
{
    public interface IComputePipeline : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder Stage(ShaderStageCreateInfo stage)
                {
                    base.Stage = stage;
                    return this;
                }

                public new Builder Layout(IPipelineLayout layout)
                {
                    base.Layout = layout;
                    return this;
                }

                public CreateInfo Build()
                {
                    return new CreateInfo()
                    {
                        Stage = base.Stage,
                        Layout = base.Layout
                    };
                }
            }

            public ShaderStageCreateInfo Stage { get; private set; }
            public IPipelineLayout Layout { get; private set; }
        }

        IPipelineLayout GetLayout();
    }
}
