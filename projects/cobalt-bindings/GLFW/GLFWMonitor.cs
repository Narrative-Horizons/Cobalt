﻿using System;
using System.Drawing;

namespace Cobalt.Bindings.GLFW
{
    public struct GLFWMonitor : IEquatable<GLFWMonitor>
    {
        public static readonly GLFWMonitor None;

        private readonly IntPtr handle;

        public bool Equals(GLFWMonitor other)
        {
            return handle.Equals(other.handle);
        }

        public override bool Equals(object obj)
        {
            if (obj is GLFWMonitor monitor)
            {
                return Equals(monitor);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public static bool operator ==(GLFWMonitor left, GLFWMonitor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GLFWMonitor left, GLFWMonitor right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return handle.ToString();
        }

        public Rectangle WorkArea
        {
            get
            {
                GLFW.GetMonitorWorkArea(this, out int x, out int y, out int width, out int height);
                return new Rectangle(x, y, width, height);
            }
        }

        public PointF ContentScale
        {
            get
            {
                GLFW.GetMonitorContentScale(handle, out float x, out float y);
                return new PointF(x, y);
            }
        }

        public IntPtr UserPointer
        {
            get => GLFW.GetMonitorUserPointer(handle);
            set => GLFW.SetMonitorUserPointer(handle, value);
        }
    }
}
