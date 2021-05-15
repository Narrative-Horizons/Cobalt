using Cobalt.Math;

namespace Cobalt.Entities.Components
{
    public class TransformComponent : BaseComponent
    {
        public Matrix4 transformMatrix { get; set; } = Matrix4.Identity;

        public TransformComponent()
        {

        }
    }
}
