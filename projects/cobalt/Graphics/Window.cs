using Cobalt.Bindings.GLFW;
using Cobalt.Bindings.Utils;
using Cobalt.Core;
using Cobalt.Events;
using Cobalt.Math;
using System;
using OpenGL = Cobalt.Bindings.GL.GL;

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

        /// TODO: Make this not static
        internal static uint _oldWidth, _oldHeight;
        internal static double _oldMouseX, _oldMouseY;

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

            GLFW.SetKeyCallback(_window, (GLFWWindow window, Keys key, int scanCode, InputState state, ModifierKeys mods) =>
            {
                switch (state)
                {
                    case InputState.Release:
                        Input.SetKeyReleased(key);
                        EventManager.Default.Dispatch(new KeyReleasedEvent(key));
                        break;
                    case InputState.Press:
                        Input.SetKeyPressed(key);
                        EventManager.Default.Dispatch(new KeyPressEvent(key));
                        break;
                    case InputState.Repeat:
                        Input.SetKeyPressed(key);
                        EventManager.Default.Dispatch(new KeyRepeatEvent(key));
                        break;
                }
            });

            GLFW.SetMouseButtonCallback(_window, (GLFWWindow window, MouseButton button, InputState state, ModifierKeys modifiers) =>
            {
                switch (state)
                {
                    case InputState.Release:
                        Input.SetMouseButtonReleased(button);
                        EventManager.Default.Dispatch(new MouseReleasedEvent(button));
                        break;
                    case InputState.Press:
                        Input.SetMouseButtonPressed(button);
                        EventManager.Default.Dispatch(new MousePressEvent(button));
                        break;
                    case InputState.Repeat:
                        Input.SetMouseButtonPressed(button);
                        EventManager.Default.Dispatch(new MouseRepeatEvent(button));
                        break;
                }
            });

            GLFW.SetCursorPositionCallback(_window, (GLFWWindow window, double x, double y) =>
            {
                EventManager.Default.Dispatch(new CursorMovedEvent(x, y, _oldMouseX, _oldMouseY));

                Input.LastMousePosition = new Vector2((float)_oldMouseX, (float)_oldMouseY);
                Input.MousePosition = new Vector2((float)x, (float)y);

                _oldMouseX = x;
                _oldMouseY = y;
            });

            GLFW.SetMouseScrollCallback(_window, (GLFWWindow window, double x, double y) =>
            {
                EventManager.Default.Dispatch(new MouseScrollEvent(x, y));

                Input.ScrollDelta = new Vector2((float)x, (float)y);
            });

            Input.Init(_window);
        }

        public bool IsOpen()
        {
            return !GLFW.WindowShouldClose(_window);
        }

        public void Close()
        {
            EventManager.Default.Dispatch(new WindowCloseEvent());
            GLFW.SetWindowShouldClose(_window, true);
        }

        private static void GlfwError(ErrorCode code, IntPtr message)
        {
            throw new Exception(Util.PtrToStringUTF8(message));
        }

        public void Poll()
        {
            Input.Update();
            GLFW.PollEvents();
        }

        private void Resize(GLFWWindow window, int width, int height)
        {
            EventManager.Default.Dispatch(new WindowResizeEvent((uint)width, (uint)height, _oldWidth, _oldHeight));

            _oldWidth = (uint)width;
            _oldHeight = (uint)height;
        }

        public void Refresh()
        {
            OpenGL.Finish();
            GLFW.SwapBuffers(_window);
        }
    }
}