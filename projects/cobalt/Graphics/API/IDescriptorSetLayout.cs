using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface IDescriptorSetLayout : IDisposable
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

        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public Builder AddBinding(DescriptorSetLayoutBinding binding)
                {
                    Binding.Add(binding);
                    return this;
                }

                public CreateInfo Build()
                {
                    // TODO Check for duplicate bindings
                    return new CreateInfo()
                    {
                        Binding = base.Binding
                    };
                }
            }

            public List<DescriptorSetLayoutBinding> Binding { get; private set; } = new List<DescriptorSetLayoutBinding>();
        }
    }
}
