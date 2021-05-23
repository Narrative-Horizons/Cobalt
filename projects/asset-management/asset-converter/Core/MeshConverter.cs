using Cobalt.Math;
using System.Runtime.InteropServices;

namespace CobaltConverter.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshVertex
    {
        public Vector3 position;
        public Vector2[] texcoords;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 binormal;
        public Vector4[] colors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshFace
    {
        public uint[] indices;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CobaltMesh
    {
        public MeshVertex[] Vertices { get; internal set; }
        public MeshFace[] Faces { get; internal set; }

        public uint MaterialIndex { get; internal set; }
    }

    public enum CobaltMeshTextureType
    {
        BaseColor = 0,
        Normal,
        Emission,
        RoughnessMetallic,
        OcclusionRoughnessMetallic,
        Occlusion,
        Roughness,
        Metallic
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CobaltMeshTexture
    {
        public CobaltMeshTextureType Type { get; internal set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CobaltMeshMaterial
    {
        public string Name { get; internal set; }
        public CobaltMeshTexture[] Textures { get; internal set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CobaltModelNode
    {
        public string Name { get; internal set; }
        public Matrix4 Transformation { get; internal set; }
        public CobaltModelNode Parent { get; internal set; }
        public CobaltModelNode[] Children { get; internal set; }
        public uint[] Meshes { get; internal set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CobaltModel
    {
        public uint Flags { get; internal set; }
        public CobaltModelNode RootNode { get; internal set; }
        public CobaltMesh[] Meshes { get; internal set; }
        public CobaltMeshMaterial[] Materials { get; internal set; }

        /// TODO: Animations
        /// TODO: Other scene objects (lights, camera, etc)
    }

    public class MeshConverter
    {
        public static CobaltModel ConvertModel(string path)
        {
            return null;
        }
    }
}
