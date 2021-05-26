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
        private static KeyState[] KeyStates;
        private static KeyState[] LastKeyStates;

        private static KeyState[] MouseStates;
        private static KeyState[] LastMouseStates;

        internal static Vector2 LastMousePosition { get; set; } = new Vector2();
        internal static GLFWWindow window;

        public static Vector2 MousePosition { get; internal set; } = new Vector2();
        public static Vector2 MouseDelta
        {
            get
            {
                return LastMousePosition - MousePosition;
            }
        }

        public static Vector2 ScrollDelta { get; internal set; } = new Vector2();

        static Input()
        {
            KeyStates = new KeyState[(int)Keys.END_ENUM];
            MouseStates = new KeyState[(int)MouseButton.END_ENUM];
            LastKeyStates = new KeyState[(int)Keys.END_ENUM];
            LastMouseStates = new KeyState[(int)MouseButton.END_ENUM];
        }

        internal static void Init(GLFWWindow win)
        {
            window = win;
        }

        internal static void Update()
        {
            for(int i = 0; i < (int)Keys.END_ENUM; i++)
            {
                if (KeyStates[i] == KeyState.Pressed && LastKeyStates[i] == KeyState.Pressed)
                {
                    KeyStates[i] = KeyState.Down;
                }

                if(KeyStates[i] == KeyState.Up && LastKeyStates[i] == KeyState.Up)
                {
                    KeyStates[i] = KeyState.None;
                }

                LastKeyStates[i] = KeyStates[i];
            }

            for (int i = 0; i < (int)MouseButton.END_ENUM; i++)
            {
                if (MouseStates[i] == KeyState.Pressed && LastMouseStates[i] == KeyState.Pressed)
                {
                    MouseStates[i] = KeyState.Down;
                }

                if (MouseStates[i] == KeyState.Up && LastMouseStates[i] == KeyState.Up)
                {
                    MouseStates[i] = KeyState.None;
                }

                LastMouseStates[i] = MouseStates[i];
            }
        }

        public static bool IsKeyDown(Keys key)
        {
            return KeyStates[(int)key] == KeyState.Down || KeyStates[(int)key] == KeyState.Pressed;
        }

        public static bool IsKeyPressed(Keys key)
        {
            return KeyStates[(int)key] == KeyState.Pressed;
        }

        public static bool IsKeyUp(Keys key)
        {
            return KeyStates[(int)key] == KeyState.Up;
        }

        public static bool IsMouseButtonDown(MouseButton button)
        {
            return MouseStates[(int)button] == KeyState.Down || MouseStates[(int)button] == KeyState.Pressed;
        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {
            return MouseStates[(int)button] == KeyState.Pressed;
        }

        public static bool IsMouseButtonUp(MouseButton button)
        {
            return MouseStates[(int)button] == KeyState.Up;
        }

        public static void SetMousePosition(Vector2 position)
        {
            GLFW.SetCursorPosition(window, position.x, position.y);
        }

        internal static void SetKeyPressed(Keys key)
        {
            if (KeyStates[(int)key] == KeyState.Pressed || KeyStates[(int)key] == KeyState.Down)
                return;

            KeyStates[(int)key] = KeyState.Pressed;
        }

        internal static void SetKeyReleased(Keys key)
        {
            KeyStates[(int)key] = KeyState.Up;
        }

        internal static void SetMouseButtonPressed(MouseButton button)
        {
            if (MouseStates[(int)button] == KeyState.Pressed || MouseStates[(int)button] == KeyState.Down)
                return;

            MouseStates[(int)button] = KeyState.Pressed;
        }

        internal static void SetMouseButtonReleased(MouseButton button)
        {
            MouseStates[(int)button] = KeyState.Up;
        }
    }
}
