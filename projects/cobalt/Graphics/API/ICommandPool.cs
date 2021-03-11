using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface ICommandPool : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder TransientAllocations(bool transientAllocations)
                {
                    base.TransientAllocations = transientAllocations;
                    return this;
                }

                public new Builder ResetAllocations(bool resetAllocations)
                {
                    base.ResetAllocations = resetAllocations;
                    return this;
                }

                public new Builder Queue(IQueue queue)
                {
                    base.Queue = queue;
                    return this;
                }
            }

            public bool TransientAllocations { get; private set; }
            public bool ResetAllocations { get; private set; }
            public IQueue Queue { get; private set; }
        }

        List<ICommandBuffer> Allocate(ICommandBuffer.AllocateInfo info);
    }
}
