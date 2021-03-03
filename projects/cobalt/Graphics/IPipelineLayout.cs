using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics
{
    public interface IPipelineLayout : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public Builder AddDescriptorSetLayout(IDescriptorSetLayout layout)
                {
                    DescriptorSetLayout.Add(layout);
                    return this;
                }

                public CreateInfo Build()
                {
                    return new CreateInfo()
                    {
                        DescriptorSetLayout = base.DescriptorSetLayout
                    };
                }
            }

            public List<IDescriptorSetLayout> DescriptorSetLayout { get; private set; } = new List<IDescriptorSetLayout>();
        }

        public List<IDescriptorSetLayout> GetDescriptorSetLayouts();
    }
}
