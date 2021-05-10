using Cobalt.Core;

namespace Cobalt.Entities.Components
{
    public struct MeshComponent
    {
        public Mesh Mesh { get; private set; }
        public MeshComponent(Mesh mesh)
        {
            Mesh = mesh;
        }
    }
}
