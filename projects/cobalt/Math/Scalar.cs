using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Math
{
    public static class Scalar
    {
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
