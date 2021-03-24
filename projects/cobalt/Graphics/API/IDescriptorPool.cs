using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.API
{
    public interface IDescriptorPool : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder MaxSetCount(int maxSetCount)
                {
                    base.MaxSetCount = maxSetCount;
                    return this;
                }

                public Builder AddPoolSize(EDescriptorType type, int count)
                {
                    if(count < 0)
                    {
                        throw new InvalidOperationException("Count must be a non-negative integer");
                    }

                    base.PoolSizes[type] = count;
                    return this;
                }

                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo
                    {
                        MaxSetCount = base.MaxSetCount,
                        PoolSizes = base.PoolSizes
                    };

                    return info;
                }
            }

            public int MaxSetCount { get; private set; } = 1;
            public Dictionary<EDescriptorType, int> PoolSizes { get; private set; } = new Dictionary<EDescriptorType, int>();
        }

        public List<IDescriptorSet> Allocate(IDescriptorSet.CreateInfo info);
        public void Free(List<IDescriptorSet> sets);

        public void Reset();
    }
}
