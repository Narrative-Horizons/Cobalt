using System;

namespace Cobalt.Bindings.GLFW
{
    public struct GLFWWindow : IEquatable<GLFWWindow>
    {
        public static readonly GLFWWindow None;

        private readonly IntPtr handle;

        public static implicit operator IntPtr(GLFWWindow window)
        {
            return window.handle;
        }

        public static explicit operator GLFWWindow(IntPtr handle) => new GLFWWindow(handle);

        public GLFWWindow(IntPtr handle)
        {
            this.handle = handle;
        }

        public override string ToString()
        {
            return handle.ToString();
        }

        public bool Equals(GLFWWindow other)
        {
            return handle.Equals(other.handle);
        }

        public override bool Equals(object obj)
        {
            if (obj is GLFWWindow window)
            {
                return Equals(window);
            }

            return false;
        }

        public float Opacity
        {
            get => GLFW.GetWindowOpacity(this);
            set => GLFW.SetWindowOpacity(this, Math.Min(1.0f, Math.Max(0.0f, value)));
        }

        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public static bool operator ==(GLFWWindow left, GLFWWindow right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GLFWWindow left, GLFWWindow right)
        {
            return !left.Equals(right);
        }
    }
}
