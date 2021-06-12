using Cobalt.Math;
using System.Collections.Generic;

namespace Cobalt.Entities.Components
{
    public class TransformComponent : BaseComponent
    {
        private Matrix4 _internalTransform = Matrix4.Identity;

        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Rotation { get; set; }


        public Vector3 Forward { get; set; }
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }
        
        public Matrix4 TransformMatrix 
        { 
            get { return Parent != null ? _internalTransform * Parent.TransformMatrix : _internalTransform; }
            set
            {
                _internalTransform = value;
            }
        }

        public Matrix4 LocalTransform { get; set; }

        public TransformComponent Parent { get; set; }
        public List<TransformComponent> Children { get; set; } = new List<TransformComponent>();
        public TransformComponent()
        {

        }
    }
}
