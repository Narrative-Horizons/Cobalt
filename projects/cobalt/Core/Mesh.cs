using Cobalt.Math;
using System;
using System.Collections.Generic;

namespace Cobalt.Core
{
    public class MeshNode
    {
        public List<Mesh> meshes = new List<Mesh>();
        public MeshNode parent;
        public List<MeshNode> children = new List<MeshNode>();
        //public Material material;
        public Matrix4 transform;
    }

    public struct Mesh
    {
        public Vector3[] positions;
        public Vector2[] texcoords;
        public Vector3[] normals;
        public Vector3[] tangents;
        public Vector3[] binormals;

        public uint[] triangles;

        public Guid GUID { get; internal set; }
    }
}
