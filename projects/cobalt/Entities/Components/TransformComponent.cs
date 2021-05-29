using Cobalt.Math;

namespace Cobalt.Entities.Components
{
    public class TransformComponent : BaseComponent
    {
        public Matrix4 TransformMatrix { get; set; } = Matrix4.Identity;
        public Entity Parent { get; set; } = Entity.Invalid;
        public Entity FirstChild { get; set; } = Entity.Invalid;
        public Entity NextSibling { get; set; } = Entity.Invalid;

        public TransformComponent()
        {

        }
    }
}
