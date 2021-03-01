using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class Device : IDevice
    {
        public bool Debug { get; private set; }

        private readonly List<IQueue> _queues = new List<IQueue>();

        public Device(IDevice.CreateInfo info)
        {
            info.QueueInformation.ForEach(info =>
            {
                _queues.Add(new Queue(info));
            });

            Debug = info.Debug;
        }

        public void Dispose()
        {
            _queues.ForEach(queue => queue.Dispose());
        }
    }
}
