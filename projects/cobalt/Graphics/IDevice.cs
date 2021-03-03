using System;
using System.Collections.Generic;

namespace Cobalt.Graphics
{
    public interface IDevice : IDisposable
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder Debug(bool debug)
                {
                    base.Debug = debug;
                    return this;
                }

                public Builder AddQueueInformation(IQueue.CreateInfo info)
                {
                    base.QueueInformation.Add(info);
                    return this;
                }

                public new Builder QueueInformation(List<IQueue.CreateInfo> infos)
                {
                    base.QueueInformation.Clear();
                    base.QueueInformation.AddRange(infos);
                    return this;
                }
                
                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo
                    {
                        Debug = base.Debug,
                        QueueInformation = new List<IQueue.CreateInfo>(base.QueueInformation)
                    };
                    return info;
                }
            }

            public List<IQueue.CreateInfo> QueueInformation { get; private set; } = new List<IQueue.CreateInfo>();

            public bool Debug { get; private set; }
        }

        public List<IQueue> Queues();

        public IRenderSurface GetSurface(Window window);
    }
}
