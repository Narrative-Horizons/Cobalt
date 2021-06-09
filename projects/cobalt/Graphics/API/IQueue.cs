using System;

namespace Cobalt.Graphics.API
{
    public sealed class QueueProperties
    {
        public bool Graphics { get; internal set; }
        public bool Transfer { get; internal set; }
        public bool Compute { get; internal set; }
        public bool Present { get; internal set; }
    }

    public interface IQueue : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder QueueIndex(uint index)
                {
                    base.QueueIndex = index;
                    return this;
                }

                public new Builder FamilyIndex(uint index)
                {
                    base.FamilyIndex = index;
                    return this;
                }

                public new Builder Properties(QueueProperties properties)
                {
                    base.Properties = properties;
                    return this;
                }

                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo
                    {
                        FamilyIndex = base.FamilyIndex,
                        QueueIndex = base.QueueIndex,
                        Properties = base.Properties
                    };
                    return info;
                }
            }

            public uint QueueIndex { get; private set; }
            public uint FamilyIndex { get; private set; }
            public QueueProperties Properties { get; private set; }
        }

        public class SubmitInfo
        {
            public sealed class Builder : SubmitInfo
            {
                public new Builder Buffer(ICommandBuffer buffer)
                {
                    base.Buffer = buffer;
                    return this;
                }

                public new Builder Signal(IFence signal)
                {
                    base.Signal = signal;
                    return this;
                }

                public SubmitInfo Build()
                {
                    return new SubmitInfo()
                    {
                        Buffer = base.Buffer,
                        Signal = base.Signal
                    };
                }
            }

            public ICommandBuffer Buffer { get; private set; }
            public IFence Signal { get; private set; }
        }

        void Execute(SubmitInfo info);

        uint GetQueueIndex();

        uint GetFamilyIndex();

        QueueProperties GetProperties();

    }
}
