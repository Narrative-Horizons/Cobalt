using Cobalt.Core;

namespace Cobalt.Entities.Components
{
    public class MeshComponent : BaseComponent
    {
        public RenderableMesh Mesh { get; private set; }
        public MeshComponent() { }
        public MeshComponent(RenderableMesh mesh)
        {
            Mesh = mesh;
        }
    }
}
