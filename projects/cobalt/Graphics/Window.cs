using Cobalt.Bindings.GLFW;
using Cobalt.Bindings.Utils;
using System;

namespace Cobalt.Graphics
{
    public class Window
    {
        public class CreateInfo
        {
            public sealed class Builder : CreateInfo
            {
                public new Builder Name(string name)
                {
                    base.Name = name;
                    return this;
                }

                public new Builder Width(int width)
                {
                    base.Width = width;
                    return this;
                }

                public new Builder Height(int height)
                {
                    base.Height = height;
                    return this;
                }

                public CreateInfo Build()
                {
                    CreateInfo info = new CreateInfo()
                    {
                        Name = base.Name,
                        Width = base.Width,
                        Height = base.Height
                    };

                    return info;
                }
            }

            public string Name { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
        }

        private readonly GLFWWindow _window;

        internal Window(CreateInfo info)
        {
            GLFW.SetErrorCallback(GlfwError);

            GLFW.WindowHint(Hint.ContextVersionMajor, 4);
            GLFW.WindowHint(Hint.ContextVersionMinor, 6);
            GLFW.WindowHint(Hint.OpenglProfile, Profile.Core);
            _window = GLFW.CreateWindow(info.Width, info.Height, info.Name, GLFWMonitor.None, GLFWWindow.None);

            GLFW.MakeContextCurrent(_window);
            GLFW.ShowWindow(_window);

            GLFW.SetWindowSizeCallback(_window, Resize);
            GLFW.SwapInterval(1);
        }

        public bool IsOpen()
        {
            return !GLFW.WindowShouldClose(_window);
        }

        public void Close()
        {
            GLFW.SetWindowShouldClose(_window, true);
        }

        private static void GlfwError(ErrorCode code, IntPtr message)
        {
            throw new Exception(Util.PtrToStringUTF8(message));
        }

        public void Poll()
        {
            GLFW.PollEvents();
        }

        private static void Resize(GLFWWindow window, int width, int height)
        {

        }

        public void Refresh()
        {
            GLFW.SwapBuffers(_window);
        }
    }
}