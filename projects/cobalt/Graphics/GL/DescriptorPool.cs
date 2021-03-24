using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class DescriptorPool : IDescriptorPool
    {
        public int MaxSets { get; private set; }
        public int AllocatedSets { get; private set; }
        public Dictionary<EDescriptorType, int> DescriptorsRemaining { get; private set; } = new Dictionary<EDescriptorType, int>();
        public Dictionary<EDescriptorType, int> MaxDescriptorCount { get; private set; } = new Dictionary<EDescriptorType, int>();
        public List<IDescriptorSet> Sets { get; private set; } = new List<IDescriptorSet>();

        public DescriptorPool(IDescriptorPool.CreateInfo info)
        {
            MaxSets = info.MaxSetCount;
            AllocatedSets = 0;
        }

        public List<IDescriptorSet> Allocate(IDescriptorSet.CreateInfo info)
        {
            if(info.Layouts.Count > (MaxSets - AllocatedSets))
            {
                throw new InvalidOperationException("Attempted allocation of more sets than the pool has memory for");
            }

            List<IDescriptorSet> result = new List<IDescriptorSet>();
            info.Layouts.ForEach(layout => result.Add(new DescriptorSet(layout)));

            return result;
        }

        public void Dispose()
        {
            Reset();
        }

        public void Free(List<IDescriptorSet> sets)
        {
            Dictionary<EDescriptorType, int> freedDescriptorBindings = new Dictionary<EDescriptorType, int>();
            sets.ForEach(set => ((DescriptorSetLayout)set.GetLayout()).Bindings.ForEach(binding =>
            {
                if(freedDescriptorBindings.ContainsKey(binding.DescriptorType))
                {
                    freedDescriptorBindings[binding.DescriptorType] += binding.Count;
                }
                else
                {
                    freedDescriptorBindings.Add(binding.DescriptorType, binding.Count);
                }
            }));

            // TODO Implement rest
        }

        public void Reset()
        {
            Sets.ForEach(set => set.Dispose());
            Sets.Clear();

            DescriptorsRemaining.Clear();
            AllocatedSets = 0;
        }
    }
}
