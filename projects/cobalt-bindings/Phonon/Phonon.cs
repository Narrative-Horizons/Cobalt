using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.Phonon
{
    public static class Phonon
    {
        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "bin/phonon.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/phonon";
#else
        public const string LIBRARY = "bin/phonon";
#endif
        #endregion

        #region Phonon Data
        public enum ConvolutionType : int
        {
            Phonon,
            TrueAudioNext,
        }

        public enum Error : int
        {
            Success,
            Failure,
            OutOfMemory,
            Initialization,
        }

        public enum ComputeDeviceType : int
        {
            Cpu,
            Gpu,
            Any,
        }

        public enum SceneType : int
        {
            Phonon,
            Embree,
            RadeonRays,
            Custom,
        }

        public enum SimulationType : int
        {
            Realtime,
            Baked,
        }

        public enum HrtfDatabaseType : int
        {
            Default,
            Sofa,
        }

        public enum HrtfInterpolation : int
        {
            Nearest,
            Bilinear,
        }

        public enum ChannelLayoutType : int
        {
            Speakers,
            Ambisonics,
        }
        public enum ChannelLayout : int
        {
            Mono,
            Stereo,
            Quadraphonic,
            FivePointOne,
            SevenPointOne,
            Custom,
        }

        public enum AmbisonicsOrdering : int
        {
            FurseMalham,
            Acn,
        }

        public enum AmbisonicsNormalization : int
        {
            FurseMalham,
            SN3D,
            N3D,
        }

        public enum ChannelOrder : int
        {
            Interleaved,
            Deinterleaved,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public partial struct RenderingSettings
        {
            public int samplingRate;
            public int frameSize;
            public ConvolutionType convolutionType;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public partial struct AudioFormat
        {
            public ChannelLayoutType channelLayoutType;
            public ChannelLayout channelLayout;
            public int numSpeakers;
            public IntPtr speakerDirections;
            public int ambisonicsOrder;
            public AmbisonicsOrdering ambisonicsOrdering;
            public AmbisonicsNormalization ambisonicsNormalization;
            public ChannelOrder channelOrder;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public partial struct AudioBuffer
        {
            public AudioFormat format;

            public int numSamples;

            public IntPtr interleavedBuffer;

            public IntPtr deinterleavedBuffer;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public partial struct HrtfParams
        {
            public HrtfDatabaseType type;
            public IntPtr hrtfData;
            public IntPtr sofaFileName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public partial struct Material
        {
            public float lowFreqAbsorption;
            public float midFreqAbsorption;
            public float highFreqAbsorption;
            public float scattering;
            public float lowFreqTransmission;

            public float midFreqTransmission;
            public float highFreqTransmission;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe partial struct Triangle
        {
            public fixed int indices[3];
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public partial struct SimulationSettings
        {
            public SceneType sceneType;
            public int maxNumOcclusionSamples;
            public int numRays;
            public int numDiffuseSamples;
            public int numBounces;
            public int numThreads;
            public float irDuration;
            public int ambisonicsOrder;
            public int maxConvolutionSources;
            public int bakingBatchSize;
            public float irradianceMinDistance;
        }
        #endregion

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LogFunction(IntPtr message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr AllocateFunction(ulong arg0, ulong arg1);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FreeFunction(IntPtr arg0);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LoadSceneProgressCallback(float progress);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FinalizeSceneProgressCallback(float progress);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ClosestHitCallback(ref float origin, ref float direction, float minDistance, float maxDistance, ref float hitDistance, ref float hitNormal, out IntPtr hitMaterial, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AnyHitCallback(ref float origin, ref float direction, float minDistance, float maxDistance, ref int hitExists, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BatchedClosestHitCallback(int numRays, ref Vector3 origins, ref Vector3 directions, int rayStride, ref float minDistances, ref float maxDistances, ref float hitDistances, ref Vector3 hitNormals, out IntPtr hitMaterials, int hitStride, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BatchedAnyHitCallback(int numRays, ref Vector3 origins, ref Vector3 directions, int rayStride, ref float minDistances, ref float maxDistances, ref byte hitExists, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SimulationThreadCreateCallback();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SimulationThreadDestroyCallback();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ProbePlacementProgressCallback(float progress);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BakeProgressCallback(float progress);

        [DllImport(LIBRARY, EntryPoint = "iplCreateContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern Error CreateContext(LogFunction logCallback, AllocateFunction allocateCallback, FreeFunction freeCallback, out IntPtr context);

        [DllImport(LIBRARY, EntryPoint = "iplDestroyContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyContext(ref IntPtr context);

        [DllImport(LIBRARY, EntryPoint = "iplCleanup", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Cleanup();

        [DllImport(LIBRARY, EntryPoint = "iplCreateBinauralRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Error CreateBinauralRenderer(IntPtr context, RenderingSettings renderingSettings, HrtfParams parameters, out IntPtr renderer);

        [DllImport(LIBRARY, EntryPoint = "iplDestroyBinauralRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyBinauralRenderer(ref IntPtr renderer);

        [DllImport(LIBRARY, EntryPoint = "iplCreateBinauralEffect", CallingConvention = CallingConvention.Cdecl)]
        public static extern Error CreateBinauralEffect(IntPtr renderer, AudioFormat inputFormat, AudioFormat outputFormat, out IntPtr effect);

        [DllImport(LIBRARY, EntryPoint = "iplDestroyBinauralEffect", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyBinauralEffect(ref IntPtr effect);

        [DllImport(LIBRARY, EntryPoint = "iplApplyBinauralEffect", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ApplyBinauralEffect(IntPtr effect, IntPtr binauralRenderer, AudioBuffer inputAudio, Vector3 direction, HrtfInterpolation interpolation, float spatialBlend, AudioBuffer outputAudio);

        [DllImport(LIBRARY, EntryPoint = "iplApplyBinauralEffectWithParameters", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ApplyBinauralEffectWithParameters(IntPtr effect, IntPtr binauralRenderer, AudioBuffer inputAudio, Vector3 direction, HrtfInterpolation interpolation, 
            [MarshalAs(UnmanagedType.U4)] bool enableSpatialBlend, float spatialBlend, AudioBuffer outputAudio, ref float leftDelay, ref float rightDelay);
        
        [DllImport(LIBRARY, EntryPoint = "iplFlushBinauralEffect", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FlushBinauralEffect(IntPtr effect);

        [DllImport(LIBRARY, EntryPoint = "iplCreateScene", CallingConvention = CallingConvention.Cdecl)]
        public static extern Error CreateScene(IntPtr context, IntPtr computeDevice, SceneType sceneType, int numMaterials, ref Material materials, ClosestHitCallback closestHitCallback, AnyHitCallback anyHitCallback, BatchedClosestHitCallback batchedClosestHitCallback, BatchedAnyHitCallback batchedAnyHitCallback, IntPtr userData, out IntPtr scene);

        [DllImport(LIBRARY, EntryPoint = "iplDestroyScene", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyScene(ref IntPtr scene);

        [DllImport(LIBRARY, EntryPoint = "iplSaveScene", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SaveScene(IntPtr scene, out byte data);

        [DllImport(LIBRARY, EntryPoint = "iplLoadScene", CallingConvention = CallingConvention.Cdecl)]
        public static extern Error LoadScene(IntPtr context, SceneType sceneType, ref byte data, int size, IntPtr computeDevice, LoadSceneProgressCallback progressCallback, out IntPtr scene);

        [DllImport(LIBRARY, EntryPoint = "iplSaveSceneAsObj", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SaveSceneAsObj(IntPtr scene, IntPtr fileBaseName);

        [DllImport(LIBRARY, EntryPoint = "iplCreateStaticMesh", CallingConvention = CallingConvention.Cdecl)]
        public static extern Error CreateStaticMesh(IntPtr scene, int numVertices, int numTriangles, ref Vector3[] vertices, ref Triangle[] triangles, ref int materialIndices, out IntPtr staticMesh);

        [DllImport(LIBRARY, EntryPoint = "iplDestroyStaticMesh", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyStaticMesh(ref IntPtr staticMesh);

        [DllImport(LIBRARY, EntryPoint = "iplCreateEnvironment", CallingConvention = CallingConvention.Cdecl)]
        public static extern Error CreateEnvironment(IntPtr context, IntPtr computeDevice, SimulationSettings simulationSettings, IntPtr scene, IntPtr probeManager, out IntPtr environment);

        [DllImport(LIBRARY, EntryPoint = "iplDestroyEnvironment", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyEnvironment(ref IntPtr environment);

        [DllImport(LIBRARY, EntryPoint = "iplSetNumBounces", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetNumBounces(IntPtr environment, int numBounces);
    }
}