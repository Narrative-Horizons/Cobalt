using Cobalt.Core;

namespace Cobalt.Entities.Components
{
    public struct MeshComponent
    {
        public RenderableMesh Mesh { get; private set; }
        public MeshComponent(RenderableMesh mesh)
        {
            Mesh = mesh;
        }
    }
}
