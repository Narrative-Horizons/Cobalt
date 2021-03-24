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
}
