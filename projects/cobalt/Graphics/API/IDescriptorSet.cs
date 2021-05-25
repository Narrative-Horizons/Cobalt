using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface IDescriptorSet : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public Builder AddLayout(IDescriptorSetLayout layout)
                {
                    Layouts.Add(layout);
                    return this;
                }

                public Builder AddLayout(List<IDescriptorSetLayout> layouts)
                {
                    Layouts.AddRange(layouts);
                    return this;
                }

                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo
                    {
                        Layouts = base.Layouts
                    };

                    return info;
                }
            }
            public List<IDescriptorSetLayout> Layouts { get; private set; } = new List<IDescriptorSetLayout>();
        }

        public IDescriptorSetLayout GetLayout();
    }
}
