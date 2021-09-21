using System;
using System.Collections.Generic;

namespace Cobalt.Graphics.API
{
    public interface IPhysicalDevice : IDisposable
    {
        public class CreateInfo
        {
            internal sealed class Builder : CreateInfo
            {
                public new Builder Name(string name)
                {
                    base.Name = name;
                    return this;
                }

                public new Builder Debug(bool debug)
                {
                    base.Debug = debug;
                    return this;
                }

                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo
                    {
                        Debug = base.Debug,
                        Name = base.Name
                    };
                    return info;
                }
            }

            public string Name { get; private set; }
            public bool Debug { get; private set; }
        }

        public string Name();

        public bool Debug();

        public IDevice Create(IDevice.CreateInfo info);

        public List<IQueue.CreateInfo> QueueInfos();

        public bool SupportsCompute()
        {
            return QueueInfos().Find(info => info.Properties.Compute) != null;
        }

        public bool SupportsGraphics()
        {
            return QueueInfos().Find(info => info.Properties.Graphics) != null;
        }

        public bool SupportsPresent()
        {
            return QueueInfos().Find(info => info.Properties.Present) != null;
        }

        public bool SupportsTransfer()
        {
            return QueueInfos().Find(info => info.Properties.Transfer) != null;
        }
    }
}
