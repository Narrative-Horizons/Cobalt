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

            public uint UUID;
        };

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe struct MeshDataImpl
        {
            [FieldOffset(0)]
            internal VertexData* vertices;
            [FieldOffset(8)]
            internal uint vertexCount;
            
            [FieldOffset(12)]
            internal uint* indices;

            [FieldOffset(20)]
            internal uint indexCount;

            [FieldOffset(24)]
            internal uint uuid;
        }

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
            [FieldOffset(20)]
            public float rz;
            [FieldOffset(24)]
            public float rw;
            [FieldOffset(28)]
            public uint generation;
            [FieldOffset(32)]
            public uint identifier;
        };

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct SimulationResult
        {
            [FieldOffset(0)]
            public PhysicsTransform* transforms;

            [FieldOffset(8)]
            public uint count;
        }

        [DllImport(LIBRARY, EntryPoint = "init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Init();

        [DllImport(LIBRARY, EntryPoint = "destroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Destroy();

        [DllImport(LIBRARY, EntryPoint = "create_mesh_shape", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CreateMeshShapeImpl(MeshDataImpl meshData);

        [DllImport(LIBRARY, EntryPoint = "create_mesh_collider", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateMeshCollider(ulong id, uint shapeId, float x, float y, float z);

        [DllImport(LIBRARY, EntryPoint = "simulate", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Simulate();

        [DllImport(LIBRARY, EntryPoint = "fetch_results", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern SimulationResult FetchResults();

        [DllImport(LIBRARY, EntryPoint = "get_results", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern SimulationResult GetResults();

        [DllImport(LIBRARY, EntryPoint = "sync", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Sync();

        public static void CreateMeshShape(MeshData data)
        {
            MeshDataImpl impl = new MeshDataImpl
            {
                uuid = data.UUID,
                vertexCount = data.vertexCount,
                indexCount = data.indexCount
            };

            fixed (VertexData* vertexPtr = &data.vertices[0])
            {
                impl.vertices = vertexPtr;
            }

            fixed (uint* indexPtr = &data.indices[0])
            {
                impl.indices = indexPtr;
            }

            CreateMeshShapeImpl(impl);
        }

    }
}