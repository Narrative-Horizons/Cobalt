using Cobalt.Core;

namespace Cobalt.Events
{
    public class WindowResizeEvent : EventData
    {
        public uint Width { get; internal set; }
        public uint Height { get; internal set; }

        public uint OldWidth { get; internal set; }
        public uint OldHeight { get; internal set; }

        internal WindowResizeEvent(uint oldWidth, uint newWidth, uint oldHeight, uint newHeight)
        {
            OldWidth = oldWidth;
            OldHeight = oldHeight;

            Width = newWidth;
            Height = newHeight;
        }
    }

    public class WindowCloseEvent : EventData
    {
        internal WindowCloseEvent()
        {
        }
    }
}
