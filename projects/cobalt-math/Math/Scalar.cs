using System;

namespace Cobalt.Math
{
    public static class Scalar
    {
        public static float RadToDeg = 180.0f / MathF.PI;
        public static float DegToRad = MathF.PI / 180.0f;

        public static float ToRadians(float degrees)
        {
            return (degrees / 180.0f) * MathF.PI;
        }

        public static float ToDegrees(float radians)
        {
            return radians * (180.0f / MathF.PI);
        }
    }
}
