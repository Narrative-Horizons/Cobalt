using Cobalt.Core;
using Cobalt.Math;
using System;

namespace Cobalt.Entities.Components
{
    public class DebugCameraComponent : BaseComponent
    {
        private const float YAW = -90.0f;
        private const float PITCH = 0.0f;
        private const float SPEED = 0.2f;
        private const float SENSITIVITY = 0.1f;
        private const float ZOOM = 45.0f;

        public Matrix4 View
        {
            get
            {
                return Matrix4.LookAt(position, position + front, up);
            }
        }
        public Matrix4 Projection
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

        private float _yaw;
        private float _pitch;
        private float _movementSpeed;
        private float _mouseSensitivity;
        private float _zoom;

        public DebugCameraComponent(Vector3 position, Vector3 up, float yaw = YAW, float pitch = PITCH)
        {
            front = new Vector3(0, 0, -1);
            _movementSpeed = SPEED;
            _mouseSensitivity = SENSITIVITY;
            _zoom = ZOOM;

            this.position = position;
            this.worldUp = up;

            UpdateCameraVectors();
        }

        public void Update()
        {
            ProcessKeyboard();
            ProcessMouseMovement();
        }

        private Vector2 oldMouse = new Vector2();

        private void ProcessMouseMovement()
        {
            Vector2 mouse = Input.MousePosition;
            Vector2 delta = mouse - oldMouse;

            delta *= _mouseSensitivity;

            _yaw -= delta.x;
            _pitch -= delta.y;

            if (_pitch > 89.0f)
                _pitch = 89.0f;
            if (_pitch < -89.0f)
                _pitch = -89.0f;

            UpdateCameraVectors();

            Input.SetMousePosition(new Vector2(1280.0f / 2.0f, 720.0f / 2.0f));
            oldMouse = new Vector2(1280.0f / 2.0f, 720.0f / 2.0f);
        }

        private void ProcessKeyboard()
        {
            if (Input.IsKeyDown(Bindings.GLFW.Keys.W))
            {
                position += front * _movementSpeed;
            }

            if (Input.IsKeyDown(Bindings.GLFW.Keys.S))
            {
                position -= front * _movementSpeed;
            }

            if (Input.IsKeyDown(Bindings.GLFW.Keys.A))
            {
                position += right * _movementSpeed;
            }

            if (Input.IsKeyDown(Bindings.GLFW.Keys.D))
            {
                position -= right * _movementSpeed;
            }
        }

        private void UpdateCameraVectors()
        {
            Vector3 f = new Vector3
            {
                x = MathF.Cos(Math.Scalar.ToRadians(_yaw)) * MathF.Cos(Math.Scalar.ToRadians(_pitch)),
                y = MathF.Sin(Math.Scalar.ToRadians(_pitch)),
                z = MathF.Sin(Math.Scalar.ToRadians(_yaw)) * MathF.Cos(Math.Scalar.ToRadians(_pitch))
            };

            front = Vector3.Normalize(f);

            right = Vector3.Cross(front, worldUp).Normalized();
            up = Vector3.Cross(right, front).Normalized();
        }
    }
}
