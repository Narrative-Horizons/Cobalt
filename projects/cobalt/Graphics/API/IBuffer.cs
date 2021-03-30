using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
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
            public List<EMemoryProperty> Required { get; private set; } = new List<EMemoryProperty>();
            public List<EMemoryProperty> Preferred { get; private set; } = new List<EMemoryProperty>();
        }

        public class CreateInfo<T> where T : unmanaged
        {
            public sealed class Builder : CreateInfo<T> {

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

                public new Builder InitialPayload(T payload)
                {
                    return InitialPayload(new T[] { payload });
                }

                public new Builder InitialPayload(T[] payload)
                {
                    base.InitialPayload = payload;
                    unsafe
                    {
                        base.Size = sizeof(T) * payload.Length;
                    }
                    return this;
                }
            }
            public int Size { get; private set; }
            public List<EBufferUsage> Usage { get; private set; } = new List<EBufferUsage>();
            public List<IQueue> Queues { get; private set; } = new List<IQueue>();
            public T[] InitialPayload { get; private set; }
        }

        public static CreateInfo<T>.Builder FromPayload<T>(T payload) where T : unmanaged
        {
            return new CreateInfo<T>.Builder().InitialPayload(payload);
        }

        public static CreateInfo<T>.Builder FromPayload<T>(T[] payload) where T : unmanaged
        {
            return new CreateInfo<T>.Builder().InitialPayload(payload);
        }

        IntPtr Map();
        IntPtr Map(int offset, int size);

        void Unmap();
    }
}
