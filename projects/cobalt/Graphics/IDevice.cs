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
                    QueueInformation.Add(info);
                    return this;
                }
                
                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo
                    {
                        Debug = base.Debug,
                        QueueInformation = base.QueueInformation
                    };
                    return info;
                }
            }

            public List<IQueue.CreateInfo> QueueInformation { get; private set; }

            public bool Debug { get; private set; }
        }
    }
}
