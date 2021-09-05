using Cobalt.Graphics.API;
using Cobalt.Math;
using System;
using System.Collections.Generic;

namespace Cobalt.Core
{
    public class TextureData
    {
        public AssimpTextureData data;
        public ImageAsset asset;

        public IImage image;
        public IImageView view;
        public ISampler sampler;
    }

    public class MaterialData
    {
        public TextureData Albedo { get; set; }
        public TextureData Normal { get; set; }
        public TextureData Emissive { get; set; }
        public TextureData ORM { get; set; }
    }

    public class MeshNode
    {
        public List<Mesh> meshes = new List<Mesh>();
        public MeshNode parent;
        public List<MeshNode> children = new List<MeshNode>();
        public Matrix4 transform;
    }

    public struct Mesh
    {
        public Vector3[] positions;
        public Vector2[] texcoords;
        public Vector3[] normals;
        public Vector3[] tangents;
        public Vector3[] binormals;

        public MaterialData material;

        public uint[] triangles;

        public Guid GUID { get; internal set; }
    }
}
