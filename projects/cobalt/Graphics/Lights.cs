using Cobalt.Math;
using System.Runtime.InteropServices;

namespace Cobalt.Graphics
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DirectionalLightComponent
    {
        [FieldOffset(0)]
        public Vector3 Direction;

        [FieldOffset(16)]
        public Vector3 Ambient;

        [FieldOffset(32)]
        public Vector3 Diffuse;

        [FieldOffset(48)]
        public Vector3 Specular;

        [FieldOffset(64)]
        public float Intensity;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct PointLightComponent
    {
        [FieldOffset(0)]
        public Vector3 Ambient;

        [FieldOffset(16)]
        public Vector3 Diffuse;

        [FieldOffset(32)]
        public Vector3 Specular;

        [FieldOffset(48)]
        public float ConstantAttenuationFactor;

        [FieldOffset(52)]
        public float LinearAttenuationFactor;

        [FieldOffset(56)]
        public float QuadraticAttenuationFactor;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct SpotLightComponent
    {
        [FieldOffset(0)]
        public Vector3 Ambient;

        [FieldOffset(16)]
        public Vector3 Diffuse;

        [FieldOffset(32)]
        public Vector3 Specular;

        [FieldOffset(48)]
        public float ConstantAttenuationFactor;

        [FieldOffset(52)]
        public float LinearAttenuationFactor;

        [FieldOffset(56)]
        public float QuadraticAttenuationFactor;

        [FieldOffset(60)]
        public float SpotlightAngle;
    }
}
