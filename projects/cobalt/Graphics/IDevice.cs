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
                // TODO: Implement rest of builder
                
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
