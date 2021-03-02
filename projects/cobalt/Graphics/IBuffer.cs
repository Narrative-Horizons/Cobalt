using System;
using System.Collections.Generic;

namespace Cobalt.Graphics
{
    public interface IBuffer : IDisposable
    {
        public class MemoryInfo
        {
            public sealed class Builder : MemoryInfo
            {
                public new Builder Usage(EMemoryUsage usage)
                {
                    base.Usage = usage;
                    return this;
                }

                public Builder AddRequiredProperty(EMemoryProperty required)
                {
                    Required.Add(required);
                    return this;
                }

                public Builder AddPreferredProperty(EMemoryProperty preferred)
                {
                    Preferred.Add(preferred);
                    return this;
                }
            }

            public EMemoryUsage Usage { get; private set; }
            public List<EMemoryProperty> Required { get; private set; }
            public List<EMemoryProperty> Preferred { get; private set; }
        }

        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder Size(int size)
                {
                    base.Size = size;
                    return this;
                }

                public Builder AddUsage(EBufferUsage usage)
                {
                    Usage.Add(usage);
                    return this;
                }

                public Builder AddQueue(IQueue queue)
                {
                    Queues.Add(queue);
                    return this;
                }

                public new Builder InitialPayload(object payload)
                {
                    base.InitialPayload = payload;
                    return this;
                }
            }
            public int Size { get; private set; }
            public List<EBufferUsage> Usage { get; private set; }
            public List<IQueue> Queues { get; private set; }
            public object InitialPayload { get; private set; }
        }

        object Map();
        object Map(int offset, int size);

        void Unmap();
    }
}
