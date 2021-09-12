using System;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.PhysX
{
    public static unsafe class PhysX
    {
        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "bin/PhysX@4.1.2-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/PhysX@4.1.2-native-bindings";
#else
        public const string LIBRARY = "bin/PhysX@4.1.2-native-bindings";
#endif
        #endregion

        public struct VertexData
        {
            public float x, y, z;
        };

        public struct MeshData
        {
            public VertexData[] vertices;
            public uint vertexCount;

            public uint[] indices;
            public uint indexCount;

            public uint uuid;
        };

        [DllImport(LIBRARY, EntryPoint = "init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Init();

        [DllImport(LIBRARY, EntryPoint = "destroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Destroy();

        [DllImport(LIBRARY, EntryPoint = "create_mesh_shape", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateMeshShape(MeshData meshData);

        [DllImport(LIBRARY, EntryPoint = "create_mesh_collider", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateMeshCollider(ulong id, uint shapeId, float x, float y, float z);

        [DllImport(LIBRARY, EntryPoint = "simulate", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Simulate();
    }
}