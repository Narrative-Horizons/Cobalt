using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics.GL
{
    internal class Queue : IQueue
    {
        private readonly uint _family;
        private readonly uint _queue;
        private readonly QueueProperties _props;

        public Queue(IQueue.CreateInfo info)
        {
            _family = info.FamilyIndex;
            _queue = info.QueueIndex;
            _props = info.Properties;
        }

        public void Dispose()
        {
            // Do Nothing
        }

        public uint GetFamilyIndex()
        {
            return _family;
        }

        public QueueProperties GetProperties()
        {
            return _props;
        }

        public uint GetQueueIndex()
        {
            return _queue;
        }
    }
}
