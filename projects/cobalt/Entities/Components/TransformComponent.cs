using Cobalt.Math;
using System.Collections.Generic;

namespace Cobalt.Entities.Components
{
    public class TransformComponent : BaseComponent
    {
        #region Private Data
        private Matrix4 _internalTransform = Matrix4.Identity;

        private Vector3 _position = Vector3.Zero;
        private Vector3 _scale = Vector3.One;
        private Quaternion _rotation = new Quaternion();
        #endregion

        #region Internal Data
        internal bool isDirty;
        #endregion

        #region Public Data
        public Matrix4 LocalTransform { get; set; }
        public TransformComponent Parent { get; set; }
        public List<TransformComponent> Children { get; set; } = new List<TransformComponent>();
        #endregion

        #region Properties
        public Vector3 Position {
            get => _position;
            set
            {
                _position = value;
                UpdateTransform();
            }
        }
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                UpdateTransform();
            }
        }
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateTransform();
            }
        }

        public Vector3 EulerAngles
        {
            get => _rotation.EulerAngles;
            set
            {
                _rotation = Quaternion.Euler(value);
                UpdateTransform();
            }
        }

        public Vector3 Forward
        {
            get => _rotation * Vector3.UnitZ;
            set
            {
                _rotation = Quaternion.LookRotation(value);
                UpdateTransform();
            }
        }
        public Vector3 Right
        {
            get => _rotation * Vector3.UnitX;
            set
            {
                _rotation = Quaternion.FromToRotation(Vector3.UnitX, value);
                UpdateTransform();
            }
        }
        public Vector3 Up
        {
            get => _rotation * Vector3.UnitY;
            set
            {
                _rotation = Quaternion.FromToRotation(Vector3.UnitY, value);
                UpdateTransform();
            }
        }
        
        public Matrix4 TransformMatrix 
        { 
            get => Parent != null ? _internalTransform * Parent.TransformMatrix : _internalTransform;
            set => _internalTransform = value;
        }
        #endregion

        #region Private API
        private void UpdateTransform()
        {
            _internalTransform = Matrix4.Scale(_scale) * Matrix4.Rotate(_rotation.EulerAngles) * Matrix4.Translate(_position);
            isDirty = true;
        }
        #endregion
    }
}
