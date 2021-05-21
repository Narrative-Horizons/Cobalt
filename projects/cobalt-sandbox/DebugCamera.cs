using Cobalt.Core;
using Cobalt.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Sandbox
{
    public class DebugCamera
    {
        const float YAW = -90.0f;
        const float PITCH = 0.0f;
        const float SPEED = 0.2f;
        const float SENSITIVITY = 0.1f;
        const float ZOOM = 45.0f;

        public Matrix4 view
        {
            get
            {
                return Matrix4.LookAt(position, position + front, up);
            }
        }
        public Matrix4 projection
        {
            get
            {
                return Matrix4.Perspective(Math.Scalar.ToRadians(60.0f), 16.0f / 9.0f, 1f, 500.0f);
            }
        }

        public Vector3 position = new Vector3();
        public Vector3 front = new Vector3();
        public Vector3 up = new Vector3();
        public Vector3 right = new Vector3();
        public Vector3 worldUp = new Vector3();

        float Yaw;
        float Pitch;
        float MovementSpeed;
        float MouseSensitivity;
        float Zoom;

        public DebugCamera(Vector3 position, Vector3 up, float yaw = YAW, float pitch = PITCH)
        {
            front = new Vector3(0, 0, -1);
            MovementSpeed = SPEED;
            MouseSensitivity = SENSITIVITY;
            Zoom = ZOOM;

            this.position = position;
            this.worldUp = up;

            UpdateCameraVectors();
        }

        public void Update()
        {
            ProcessKeyboard();
            ProcessMouseMovement();
        }

        private void ProcessMouseMovement()
        {
            Vector2 mouse = Input.MouseDelta;
            Input.SetMousePosition(new Vector2(1280.0f / 2.0f, 720.0f / 2.0f));

            mouse *= MouseSensitivity;

            Yaw -= mouse.x;
            Pitch -= mouse.y;

            if (Pitch > 89.0f)
                Pitch = 89.0f;
            if (Pitch < -89.0f)
                Pitch = -89.0f;

            UpdateCameraVectors();
        }

        private void ProcessKeyboard()
        {
            if(Input.IsKeyDown(Bindings.GLFW.Keys.W))
            {
                position += front * MovementSpeed;
            }

            if (Input.IsKeyDown(Bindings.GLFW.Keys.S))
            {
                position -= front * MovementSpeed;
            }

            if (Input.IsKeyDown(Bindings.GLFW.Keys.A))
            {
                position += right * MovementSpeed;
            }

            if (Input.IsKeyDown(Bindings.GLFW.Keys.D))
            {
                position -= right * MovementSpeed;
            }
        }

        private void UpdateCameraVectors()
        {
            Vector3 f = new Vector3
            {
                x = MathF.Cos(Math.Scalar.ToRadians(Yaw)) * MathF.Cos(Math.Scalar.ToRadians(Pitch)),
                y = MathF.Sin(Math.Scalar.ToRadians(Pitch)),
                z = MathF.Sin(Math.Scalar.ToRadians(Yaw)) * MathF.Cos(Math.Scalar.ToRadians(Pitch))
            };

            front = Vector3.Normalize(f);

            right = Vector3.Cross(front, worldUp).Normalized();
            up = Vector3.Cross(right, front).Normalized();
        }

    }
}
