using Cobalt.Math;
using System.Collections.Generic;

namespace Cobalt.Entities.Components
{
    public class TransformComponent : BaseComponent
    {


        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Rotation { get; set; }


        public Vector3 Forward { get; set; }
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }
        
        public Matrix4 TransformMatrix { get; set; } = Matrix4.Identity;
        public Entity Parent { get; set; } = Entity.Invalid;
        public List<Entity> Children { get; set; } = new List<Entity>();
        public TransformComponent()
        {

        }
    }
}
