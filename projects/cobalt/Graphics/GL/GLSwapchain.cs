using Cobalt.Graphics.API;

namespace Cobalt.Graphics.GL
{
    internal class GLSwapchain : ISwapchain
    {
        public uint ImageCount { get; private set; }
        public IFrameBuffer FrameBuffer { get; private set; } = new FrameBuffer();
        public Window Window { get; private set; }

        public GLSwapchain(Window window, ISwapchain.CreateInfo info)
        {
            ImageCount = info.ImageCount;
            Window = window;
        }

        public void Dispose()
        {

        }

        public void Present(ISwapchain.PresentInfo info)
        {
            Window.Refresh();
        }

        public IFrameBuffer GetFrameBuffer(int frame)
        {
            return FrameBuffer;
        }

        public uint GetImageCount()
        {
            return ImageCount;
        }
    }
}
