using Cobalt.Bindings.GLFW;
using Cobalt.Math;

namespace Cobalt.Core
{
    public enum KeyState
    {
        None,
        Pressed,
        Down,
        Up
    }

    public static class Input
    {
        private static readonly KeyState[] _keyStates;
        private static readonly KeyState[] _lastKeyStates;

        private static readonly KeyState[] _mouseStates;
        private static readonly KeyState[] _lastMouseStates;

        internal static Vector2 LastMousePosition { get; set; } = new Vector2();
        internal static GLFWWindow window;

        public static Vector2 MousePosition { get; internal set; } = new Vector2();
        public static Vector2 MouseDelta => LastMousePosition - MousePosition;

        public static Vector2 ScrollDelta { get; internal set; } = new Vector2();

        static Input()
        {
            _keyStates = new KeyState[(int)Keys.END_ENUM];
            _mouseStates = new KeyState[(int)MouseButton.END_ENUM];
            _lastKeyStates = new KeyState[(int)Keys.END_ENUM];
            _lastMouseStates = new KeyState[(int)MouseButton.END_ENUM];
        }

        internal static void Init(GLFWWindow win)
        {
            window = win;
        }

        internal static void Update()
        {
            for(int i = 0; i < (int)Keys.END_ENUM; i++)
            {
                if (_keyStates[i] == KeyState.Pressed && _lastKeyStates[i] == KeyState.Pressed)
                {
                    _keyStates[i] = KeyState.Down;
                }

                if(_keyStates[i] == KeyState.Up && _lastKeyStates[i] == KeyState.Up)
                {
                    _keyStates[i] = KeyState.None;
                }

                _lastKeyStates[i] = _keyStates[i];
            }

            for (int i = 0; i < (int)MouseButton.END_ENUM; i++)
            {
                if (_mouseStates[i] == KeyState.Pressed && _lastMouseStates[i] == KeyState.Pressed)
                {
                    _mouseStates[i] = KeyState.Down;
                }

                if (_mouseStates[i] == KeyState.Up && _lastMouseStates[i] == KeyState.Up)
                {
                    _mouseStates[i] = KeyState.None;
                }

                _lastMouseStates[i] = _mouseStates[i];
            }
        }

        public static bool IsKeyDown(Keys key)
        {
            return _keyStates[(int)key] == KeyState.Down || _keyStates[(int)key] == KeyState.Pressed;
        }

        public static bool IsKeyPressed(Keys key)
        {
            return _keyStates[(int)key] == KeyState.Pressed;
        }

        public static bool IsKeyUp(Keys key)
        {
            return _keyStates[(int)key] == KeyState.Up;
        }

        public static bool IsMouseButtonDown(MouseButton button)
        {
            return _mouseStates[(int)button] == KeyState.Down || _mouseStates[(int)button] == KeyState.Pressed;
        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {
            return _mouseStates[(int)button] == KeyState.Pressed;
        }

        public static bool IsMouseButtonUp(MouseButton button)
        {
            return _mouseStates[(int)button] == KeyState.Up;
        }

        public static void SetMousePosition(Vector2 position)
        {
            GLFW.SetCursorPosition(window, position.x, position.y);
        }

        internal static void SetKeyPressed(Keys key)
        {
            if (_keyStates[(int)key] == KeyState.Pressed || _keyStates[(int)key] == KeyState.Down)
                return;

            _keyStates[(int)key] = KeyState.Pressed;
        }

        internal static void SetKeyReleased(Keys key)
        {
            _keyStates[(int)key] = KeyState.Up;
        }

        internal static void SetMouseButtonPressed(MouseButton button)
        {
            if (_mouseStates[(int)button] == KeyState.Pressed || _mouseStates[(int)button] == KeyState.Down)
                return;

            _mouseStates[(int)button] = KeyState.Pressed;
        }

        internal static void SetMouseButtonReleased(MouseButton button)
        {
            _mouseStates[(int)button] = KeyState.Up;
        }
    }
}
