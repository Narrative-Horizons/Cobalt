using Cobalt.Core;
using System;

namespace Cobalt.Entities.Components
{
    public struct MeshComponent
    {
        public UInt64 UUID { get; private set; }
        public MeshComponent(Mesh mesh)
        {
            UUID = mesh.UUID;
        }
    }
}
