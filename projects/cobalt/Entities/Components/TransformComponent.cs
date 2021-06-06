using Cobalt.Math;
using System.Collections.Generic;

namespace Cobalt.Entities.Components
{
    public class TransformComponent : BaseComponent
    {
        public Matrix4 TransformMatrix { get; set; } = Matrix4.Identity;
        public Entity Parent { get; set; } = Entity.Invalid;
        public List<Entity> Children { get; set; } = new List<Entity>();
        public TransformComponent()
        {

        }
    }
}
