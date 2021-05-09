using Cobalt.Math;
using System;

namespace Cobalt.Core
{
    public class MeshNode
    {
        public Mesh mesh;
        //public Material material;
        public Matrix4 transform;
    }

    public struct Mesh
    {
        public Vector3[] positions;
        public Vector2[] texcoords;
        public Vector3[] normals;
        public Vector3[] tangents;
        public Vector3[] bitangents;

        public uint[] triangles;

        public UInt64 UUID; 
    }
}
