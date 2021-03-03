using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics
{
    public interface IDescriptorSetLayout
    {
        public class DescriptorSetLayoutBinding
        {
            public sealed class Builder : DescriptorSetLayoutBinding
            {
                public new Builder BindingIndex(int bindingIndex)
                {
                    base.BindingIndex = bindingIndex;
                    return this;
                }

                public new Builder DescriptorType(EDescriptorType descriptorType)
                {
                    base.DescriptorType = descriptorType;
                    return this;
                }

                public new Builder Count(int count)
                {
                    base.Count = count;
                    return this;
                }

                public Builder AddAccessibleStage(EShaderType stage)
                {
                    AccessibleStage.Add(stage);
                    return this;
                }

                public new Builder Name(string name)
                {
                    base.Name = name;
                    return this;
                }

                public DescriptorSetLayoutBinding Build()
                {
                    return new DescriptorSetLayoutBinding()
                    {
                        BindingIndex = base.BindingIndex,
                        DescriptorType = base.DescriptorType,
                        Count = base.Count,
                        AccessibleStage = base.AccessibleStage,
                        Name = base.Name
                    };
                }
            }

            public int BindingIndex { get; private set; }
            public EDescriptorType DescriptorType { get; private set; }
            public int Count { get; private set; }
            public List<EShaderType> AccessibleStage { get; private set; } = new List<EShaderType>();
            public string Name { get; private set; }
        }
    }
}
