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
    public class Vector3 : IEquatable<Vector3>
    {
        public float x;
        public float y;
        public float z;

        public Vector3()
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
        }

        public Vector3(float value)
        {
            x = value;
            y = value;
            z = value;
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
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
                else
                {
                    throw new IndexOutOfRangeException("You tried to set this vector at index: " + index);
                }
            }
        }

        public float Length => MathF.Sqrt((x * x) + (y * y) + (z * z));
        public float LengthSquared => (x * x) + (y * y) + (z * z);

        public Vector3 Normalized()
        {
            Vector3 v = this;
            v.Normalize();

            return v;
        }

        public void Normalize()
        {
            float scale = 1.0f / Length;

            x *= scale;
            y *= scale;
            z *= scale;
        }

        public float Dot(Vector3 right) => (x * right.x) + (y * right.y) + (z * right.z);

        public static float Dot(Vector3 left, Vector3 right) => left.Dot(right);

        public static Vector3 Reflect(Vector3 inbound, Vector3 normal)
        {
            float dot = Dot(inbound, normal);
            float dot2 = 2 * dot;

            Vector3 scaledNormal = normal * dot2;

            return inbound - scaledNormal;
        }

        public static Vector3 Refract(Vector3 inbound, Vector3 normal, float eta)
        {
            float dot = Dot(inbound, normal);
            float dot2 = dot * dot;
            float eta2 = eta * eta;

            float k = 1.0f - eta2 * (1 - dot2);
            if(k < 0)
            {
                return Zero;
            }

            Vector3 scaledInbound = inbound * eta;
            Vector3 scaledNormal = normal * (eta * dot + MathF.Sqrt(k));

            return scaledInbound - scaledNormal;
        }

        public static Vector3 Cross(Vector3 left, Vector3 right)
        {
            float x = left.x * right.z - left.z * right.y;
            float y = left.z * right.x - left.x * right.z;
            float z = left.x * right.y - left.y * right.x;

            return new Vector3(x, y, z);
        }

        public static readonly Vector3 UnitX = new Vector3(1, 0, 0);
        public static readonly Vector3 UnitY = new Vector3(0, 1, 0);
        public static readonly Vector3 UnitZ = new Vector3(0, 0, 1);

        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        public static readonly Vector3 One = new Vector3(1, 1, 1);

        public static readonly Vector3 PositiveInfinity = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public static readonly Vector3 NegativeInfinity = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        public static readonly int SizeInBytes = Unsafe.SizeOf<Vector3>();

        [XmlIgnore]
        public Vector3 Yxz
        {
            get => new Vector3(y, x, z);
            set
            {
                y = value.x;
                x = value.y;
                z = value.z;
            }
        }

        [XmlIgnore]
        public Vector3 Zyx
        {
            get => new Vector3(z, y, x);
            set
            {
                z = value.z;
                y = value.x;
                x = value.y;
            }
        }

        [XmlIgnore]
        public Vector3 Zxy
        {
            get => new Vector3(z, x, y);
            set
            {
                z = value.z;
                x = value.y;
                y = value.x;
            }
        }

        [Pure]
        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            left.x += right.x;
            left.y += right.y;
            left.z += right.z;

            return left;
        }

        [Pure]
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            left.x -= right.x;
            left.y -= right.y;
            left.z -= right.z;

            return left;
        }

        [Pure]
        public static Vector3 operator -(Vector3 vec)
        {
            vec.x = -vec.x;
            vec.y = -vec.y;
            vec.z = -vec.z;

            return vec;
        }

        [Pure]
        public static Vector3 operator *(Vector3 vec, float scale)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;

            return vec;
        }

        [Pure]
        public static Vector3 operator *(float scale, Vector3 vec)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;

            return vec;
        }

        [Pure]
        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            left.x *= right.x;
            left.y *= right.y;
            left.z *= right.z;

            return left;
        }

        [Pure]
        public static Vector3 operator /(Vector3 vec, float scale)
        {
            vec.x /= scale;
            vec.y /= scale;
            vec.z /= scale;

            return vec;
        }

        [Pure]
        public static Vector3 operator /(float scale, Vector3 vec)
        {
            vec.x /= scale;
            vec.y /= scale;
            vec.z /= scale;

            return vec;
        }

        [Pure]
        public static Vector3 operator /(Vector3 left, Vector3 right)
        {
            left.x /= right.x;
            left.y /= right.y;
            left.z /= right.z;

            return left;
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !left.Equals(right);
        }

        private static readonly string ListSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        public override string ToString()
        {
            return string.Format("({0}{3} {1}{3} {2})", x, y, z, ListSeparator);
        }

        public override bool Equals(object obj)
        {
            if(obj is Vector3 vector)
            {
                return Equals(vector);
            }
            return false;
        }

        public bool Equals(Vector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }

        [Pure]
        public void Deconstruct(out float x, out float y, out float z)
        {
            x = this.x;
            y = this.y;
            z = this.z;
        }
    }
}
