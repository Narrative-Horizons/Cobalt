using Cobalt.Graphics.API;
using System.Collections.Generic;

namespace Cobalt.Graphics.GL
{
    internal class CommandPool : ICommandPool
    {
        public List<ICommandBuffer> Buffers { get; private set; } = new List<ICommandBuffer>();
        private bool _resetFlag = false;

        public CommandPool(ICommandPool.CreateInfo info)
        {
            _resetFlag = info.ResetAllocations;
        }

        public List<ICommandBuffer> Allocate(ICommandBuffer.AllocateInfo info)
        {
            List<ICommandBuffer> nBufs = new List<ICommandBuffer>();
            for(int i = 0; i < info.Count; i++)
            {
                nBufs.Add(new CommandBuffer(info, _resetFlag));
            }
            Buffers.AddRange(nBufs);

            return nBufs;
        }

        public void Dispose()
        {
            Buffers.ForEach(buf => buf.Dispose());
            Buffers.Clear();
        }
    }
}
