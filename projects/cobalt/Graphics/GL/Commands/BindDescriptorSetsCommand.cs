using Cobalt.Graphics.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL.Commands
{
    internal class BindDescriptorSetsCommand : ICommand
    {
        public List<IDescriptorSet> Sets { get; private set; } = new List<IDescriptorSet>();

        public BindDescriptorSetsCommand(List<IDescriptorSet> sets)
        {
            Sets = sets;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            Sets.ForEach(iset =>
            {
                DescriptorSet set = (DescriptorSet)iset;
                set.Bind();
            });
        }
    }

    internal class BindDynamicDescriptorSetsCommand : ICommand
    {
        public List<IDescriptorSet> Sets { get; private set; } = new List<IDescriptorSet>();
        public List<uint> Offsets { get; private set; } = new List<uint>();


        public BindDynamicDescriptorSetsCommand(List<IDescriptorSet> sets, List<uint> offsets)
        {
            Sets = sets;
            Offsets = offsets;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            Sets.ForEach(iset =>
            {
                DescriptorSet set = (DescriptorSet)iset;
                set.Bind(Offsets);
            });
        }
    }
}
