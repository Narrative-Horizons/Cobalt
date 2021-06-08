using Cobalt.Core;
using Cobalt.Math;
using System;

namespace Cobalt.Entities.Components
{
    public class FreeLookCamera : CameraComponent
    {
        #region Constant Init Data
        private const float YAW = -90.0f;
        private const float PITCH = 0.0f;
        private const float SPEED = 0.005f;
        private const float SENSITIVITY = 0.25f;
        #endregion

        #region Data
        private float _yaw;
        private float _pitch;
        private float _movementSpeed;
        private float _mouseSensitivity;
        private bool _locked = true;
        #endregion

        public FreeLookCamera(float FoV, float near, float far, float aspect)
        {
            _yaw = YAW;
            _pitch = PITCH;
            Orthographic = false;
            FieldOfView = Math.Scalar.ToRadians(FoV);

            NearClipPlane = near;
            FarClipPlane = far;
            Aspect = aspect;

            _movementSpeed = SPEED;
            _mouseSensitivity = SENSITIVITY;
        }

        public override void OnInit()
        {
            base.OnInit();

            Transform.Forward = new Vector3(0, 0, -1);
            UpdateCameraVectors();
        }

        public override void OnUpdate()
        {
            ProcessKeyboard();
            ProcessMouseMovement();
        }

        private Vector2 oldMouse = new Vector2();

        private void ProcessMouseMovement()
        {
            if (!_locked)
            {
                return;
            }

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
                Transform.Position += Transform.Forward * _movementSpeed;
            }

            if (Input.IsKeyDown(Bindings.GLFW.Keys.S))
            {
                Transform.Position -= Transform.Forward * _movementSpeed;
            }

            if (Input.IsKeyDown(Bindings.GLFW.Keys.A))
            {
                Transform.Position += Transform.Right * _movementSpeed;
            }

            if (Input.IsKeyDown(Bindings.GLFW.Keys.D))
            {
                Transform.Position -= Transform.Right * _movementSpeed;
            }

            if (Input.IsKeyUp(Bindings.GLFW.Keys.Tab))
            {
                _locked = !_locked;
            }

            if(Input.IsKeyDown(Bindings.GLFW.Keys.LeftShift))
            {
                _movementSpeed = SPEED * 10;
            }
            else
            {
                _movementSpeed = SPEED;
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

            Transform.Forward = Vector3.Normalize(f);

            Transform.Right = Vector3.Cross(Transform.Forward, Vector3.UnitY).Normalized();
            Transform.Up = Vector3.Cross(Transform.Right, Transform.Forward).Normalized();
        }
    }
}
