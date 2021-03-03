using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class PipelineLayout : IPipelineLayout
    {
        public List<IDescriptorSetLayout> layouts { get; private set; } = new List<IDescriptorSetLayout>();

        public PipelineLayout(IPipelineLayout.CreateInfo info)
        {
            layouts = info.DescriptorSetLayout;
        }

        public void Dispose()
        {
            layouts.Clear();
        }

        public List<IDescriptorSetLayout> GetDescriptorSetLayouts()
        {
            return layouts;
        }
    }
}
