namespace Cobalt.Graphics.GL
{
    internal class FrameBuffer : IFrameBuffer
    {
        public uint Handle { get; private set; }

        public FrameBuffer()
        {
            Handle = 0;
        }

        public FrameBuffer(IFrameBuffer.CreateInfo info)
        {

        }

        public void Dispose()
        {

        }
    }
}
