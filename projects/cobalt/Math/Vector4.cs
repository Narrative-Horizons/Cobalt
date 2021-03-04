using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Cobalt.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class Vector4 : IEquatable<Vector4>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4()
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
            w = 0.0f;
        }

        public Vector4(float value)
        {
            x = value;
            y = value;
            z = value;
            w = value;
        }

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4(Vector4 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
            w = vec.w;
        }

        public float this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return x;
                }

                if (index == 1)
                {
                    return y;
                }

                if (index == 2)
                {
                    return z;
                }

                if (index == 3)
                {
                    return w;
                }

                throw new IndexOutOfRangeException("You tried to access this vector at index: " + index);
            }

            set
            {
                if (index == 0)
                {
                    x = value;
                }
                else if (index == 1)
                {
                    y = value;
                }
                else if (index == 2)
                {
                    z = value;
                }
                else if (index == 3)
                {
                    w = value;
                }
                else
                {
                    throw new IndexOutOfRangeException("You tried to set this vector at index: " + index);
                }
            }
        }

        public float Length => MathF.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
        public float LengthSquared => (x * x) + (y * y) + (z * z) + (w * w);

        public Vector4 Normalized()
        {
            Vector4 v = this;
            v.Normalize();

            return v;
        }

        public void Normalize()
        {
            float scale = 1.0f / Length;

            x *= scale;
            y *= scale;
            z *= scale;
            w *= scale;
        }

        public float Dot(Vector4 right) => (x * right.x) + (y * right.y) + (z * right.z) + (w * right.w);

        public static float Dot(Vector4 left, Vector4 right) => left.Dot(right);

        public static Vector4 Reflect(Vector4 inbound, Vector4 normal)
        {
            float dot = Dot(inbound, normal);
            float dot2 = 2 * dot;

            Vector4 scaledNormal = normal * dot2;

            return inbound - scaledNormal;
        }

        public static Vector4 Refract(Vector4 inbound, Vector4 normal, float eta)
        {
            float dot = Dot(inbound, normal);
            float dot2 = dot * dot;
            float eta2 = eta * eta;

            float k = 1.0f - eta2 * (1 - dot2);
            if(k < 0)
            {
                return Zero;
            }

            Vector4 scaledInbound = inbound * eta;
            Vector4 scaledNormal = normal * (eta * dot + MathF.Sqrt(k));

            return scaledInbound - scaledNormal;
        }

        public static readonly Vector4 UnitX = new Vector4(1, 0, 0, 0);
        public static readonly Vector4 UnitY = new Vector4(0, 1, 0, 0);
        public static readonly Vector4 UnitZ = new Vector4(0, 0, 1, 0);
        public static readonly Vector4 UnitW = new Vector4(0, 0, 0, 1);

        public static readonly Vector4 Zero = new Vector4(0, 0, 0, 0);
        public static readonly Vector4 One = new Vector4(1, 1, 1, 1);

        public static readonly Vector4 PositiveInfinity = new Vector4(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public static readonly Vector4 NegativeInfinity = new Vector4(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        public static readonly int SizeInBytes = Unsafe.SizeOf<Vector4>();

        [XmlIgnore]
        public Vector4 Yxzw
        {
            get => new Vector4(y, x, z, w);
            set
            {
                y = value.x;
                x = value.y;
                z = value.z;
                w = value.w;
            }
        }

        [XmlIgnore]
        public Vector4 Yzxw
        {
            get => new Vector4(y, z, x, w);
            set
            {
                y = value.x;
                z = value.z;
                x = value.y;
                w = value.w;
            }
        }

        [XmlIgnore]
        public Vector4 Yzwx
        {
            get => new Vector4(y, z, w, x);
            set
            {
                y = value.x;
                z = value.z;
                w = value.w;
                x = value.y;
            }
        }

        [XmlIgnore]
        public Vector4 Zyxw
        {
            get => new Vector4(z, y, x, w);
            set
            {
                z = value.z;
                y = value.x;
                x = value.y;
                w = value.w;
            }
        }

        [XmlIgnore]
        public Vector4 Zxwy
        {
            get => new Vector4(z, x, w, y);
            set
            {
                z = value.z;
                x = value.y;
                w = value.w;
                y = value.x;
            }
        }

        [XmlIgnore]
        public Vector4 Zxyw
        {
            get => new Vector4(z, x, y, w);
            set
            {
                z = value.z;
                x = value.y;
                y = value.x;
                w = value.w;
            }
        }

        [Pure]
        public static Vector4 operator +(Vector4 left, Vector4 right)
        {
            left.x += right.x;
            left.y += right.y;
            left.z += right.z;
            left.w += right.w;

            return left;
        }

        [Pure]
        public static Vector4 operator -(Vector4 left, Vector4 right)
        {
            left.x -= right.x;
            left.y -= right.y;
            left.z -= right.z;
            left.w -= right.w;

            return left;
        }

        [Pure]
        public static Vector4 operator -(Vector4 vec)
        {
            vec.x = -vec.x;
            vec.y = -vec.y;
            vec.z = -vec.z;
            vec.w = -vec.w;

            return vec;
        }

        [Pure]
        public static Vector4 operator *(Vector4 vec, float scale)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            vec.w *= scale;

            return vec;
        }

        [Pure]
        public static Vector4 operator *(float scale, Vector4 vec)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            vec.w *= scale;

            return vec;
        }

        [Pure]
        public static Vector4 operator *(Vector4 left, Vector4 right)
        {
            left.x *= right.x;
            left.y *= right.y;
            left.z *= right.z;
            left.w *= right.w;

            return left;
        }

        [Pure]
        public static Vector4 operator /(Vector4 vec, float scale)
        {
            vec.x /= scale;
            vec.y /= scale;
            vec.z /= scale;
            vec.w /= scale;

            return vec;
        }

        [Pure]
        public static Vector4 operator /(float scale, Vector4 vec)
        {
            vec.x /= scale;
            vec.y /= scale;
            vec.z /= scale;
            vec.w /= scale;

            return vec;
        }

        [Pure]
        public static Vector4 operator /(Vector4 left, Vector4 right)
        {
            left.x /= right.x;
            left.y /= right.y;
            left.z /= right.z;
            left.w /= right.w;

            return left;
        }

        public static bool operator ==(Vector4 left, Vector4 right)
        {
            return left?.Equals(right) ?? false;
        }

        public static bool operator !=(Vector4 left, Vector4 right)
        {
            return !left.Equals(right);
        }

        private static readonly string ListSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        public override string ToString()
        {
            return string.Format("({0}{4} {1}{4} {2}{4} {3})", x, y, z, w, ListSeparator);
        }

        public override bool Equals(object obj)
        {
            if(obj is Vector4 vector)
            {
                return Equals(vector);
            }
            return false;
        }

        public bool Equals(Vector4 other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z, w);
        }

        [Pure]
        public void Deconstruct(out float x, out float y, out float z, out float w)
        {
            x = this.x;
            y = this.y;
            z = this.z;
            w = this.w;
        }
    }
}
