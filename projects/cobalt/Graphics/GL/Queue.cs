using Cobalt.Graphics.API;

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

        public void Execute(IQueue.SubmitInfo info)
        {
            CommandBuffer buffer = info.Buffer as CommandBuffer;
            buffer.Execute();

            Fence fence = info.Signal as Fence;
            fence?.PlaceSync();
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
