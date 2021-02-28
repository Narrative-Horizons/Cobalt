using System;
using System.Collections.Generic;
using System.Text;

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
                    var info = new CreateInfo();
                    info.Debug = base.Debug;
                    info.QueueInformation = base.QueueInformation;
                    return info;
                }
            }

            public List<IQueue.CreateInfo> QueueInformation { get; private set; }

            public bool Debug { get; private set; }
        }
    }
}
