using Cobalt.Bindings.GLFW;
using Cobalt.Core;

namespace Cobalt.Events
{
    public class KeyPressEvent : EventData
    {
        public Keys Key { get; internal set; }
        public KeyPressEvent(Keys key)
        {
            Key = key;
        }
    }

    public class KeyRepeatEvent : EventData
    {
        public Keys Key { get; internal set; }
        public KeyRepeatEvent(Keys key)
        {
            Key = key;
        }
    }

    public class KeyReleasedEvent : EventData
    {
        public Keys Key { get; internal set; }
        public KeyReleasedEvent(Keys key)
        {
            Key = key;
        }
    }

    public class MousePressEvent : EventData
    {
        public MouseButton Button { get; internal set; }
        public MousePressEvent(MouseButton button)
        {
            Button = button;
        }
    }

    public class MouseRepeatEvent : EventData
    {
        public MouseButton Button { get; internal set; }
        public MouseRepeatEvent(MouseButton button)
        {
            Button = button;
        }
    }

    public class MouseReleasedEvent : EventData
    {
        public MouseButton Button { get; internal set; }
        public MouseReleasedEvent(MouseButton button)
        {
            Button = button;
        }
    }

    public class CursorMovedEvent : EventData
    {
        public double X { get; internal set; }
        public double Y { get; internal set; }

        public double OldX { get; internal set; }
        public double OldY { get; internal set; }

        public double DeltaX
        {
            get
            {
                return X - OldX;
            }
        }

        public double DeltaY
        {
            get
            {
                return Y - OldY;
            }
        }

        public CursorMovedEvent(double x, double y, double oldX, double oldY)
        {
            X = x;
            Y = y;
            OldX = oldX;
            OldY = oldY;
        }
    }

    public class MouseScrollEvent : EventData
    {
        public double X { get; internal set; }
        public double Y { get; internal set; }

        public MouseScrollEvent(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
