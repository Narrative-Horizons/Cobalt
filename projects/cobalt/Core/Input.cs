using Cobalt.Bindings.GLFW;
using Cobalt.Math;
using System;
using System.Collections.Generic;
using System.Text;

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

        internal static Vector2 LastMousePosition = new Vector2();
        internal static GLFWWindow window;

        public static Vector2 MousePosition { get; internal set; } = new Vector2();
        public static Vector2 MouseDelta 
        { 
            get 
            {
                return MousePosition - LastMousePosition;    
            } 
        }

        static Input()
        {
            KeyStates = new KeyState[(int)Keys.END_ENUM];
            LastKeyStates = new KeyState[(int)Keys.END_ENUM];
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

            LastMousePosition = MousePosition;
        }

        public static bool IsKeyDown(Keys key)
        {
            return KeyStates[(int)key] == KeyState.Down || KeyStates[(int)key] == KeyState.Pressed;
        }

        public static bool IsKeyPressed(Keys key)
        {
            return KeyStates[(int)key] == KeyState.Pressed;
        }

        public static void SetMousePosition(Vector2 position)
        {
            GLFW.SetCursorPosition(window, position.x, position.y);
            MousePosition = position;
        }

        public static bool IsKeyUp(Keys key)
        {
            return KeyStates[(int)key] == KeyState.Up;
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
    }
}
