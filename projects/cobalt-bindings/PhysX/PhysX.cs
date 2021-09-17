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

        [StructLayout(LayoutKind.Explicit)]
        public struct PhysicsTransform
        {
            [FieldOffset(0)]
            public float x;
            [FieldOffset(4)]
            public float y;
            [FieldOffset(8)]
            public float z;
            [FieldOffset(12)]
            public float rx;
            [FieldOffset(16)]
            public float ry;
            [FieldOffset(24)]
            public float rz;
            [FieldOffset(28)]
            public uint generation;
            [FieldOffset(32)]
            public uint identifier;
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct SimulationResult
        {
            [FieldOffset(0)]
            IntPtr transforms;

            [FieldOffset(8)]
            uint count;
        }

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

        [DllImport(LIBRARY, EntryPoint = "fetch_results", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern SimulationResult FetchResults();
    }
}