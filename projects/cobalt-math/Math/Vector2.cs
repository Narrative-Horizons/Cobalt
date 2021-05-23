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
    public struct Vector2 : IEquatable<Vector2>
    {
        public float x;
        public float y;

        public Vector2(float value)
        {
            x = value;
            y = value;
        }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2(Vector2 vec)
        {
            x = vec.x;
            y = vec.y;
        }

        public float this[int index]
        {
            get
            {
                if(index == 0)
                {
                    return x;
                }

                if(index == 1)
                {
                    return y;
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
                else
                {
                    throw new IndexOutOfRangeException("You tried to set this vector at index: " + index);
                }
            }
        }

        public float Length => MathF.Sqrt((x * x) + (y * y));
        public float LengthSquared => (x * x) + (y * y);

        public Vector2 Normalized()
        {
            Vector2 v = this;
            v.Normalize();

            return v;
        }

        public void Normalize()
        {
            float scale = 1.0f / Length;

            x *= scale;
            y *= scale;
        }

        public float Dot(Vector2 right) => (x * right.x) + (y * right.y);

        public static float Dot(Vector2 left, Vector2 right) => left.Dot(right);

        public static Vector2 Reflect(Vector2 inbound, Vector2 normal)
        {
            float dot = Dot(inbound, normal);
            float dot2 = 2 * dot;

            Vector2 scaledNormal = normal * dot2;

            return inbound - scaledNormal;
        }

        public static Vector2 Refract(Vector2 inbound, Vector2 normal, float eta)
        {
            float dot = Dot(inbound, normal);
            float dot2 = dot * dot;
            float eta2 = eta * eta;

            float k = 1.0f - eta2 * (1 - dot2);
            if(k < 0)
            {
                return Zero;
            }

            Vector2 scaledInbound = inbound * eta;
            Vector2 scaledNormal = normal * (eta * dot + MathF.Sqrt(k));

            return scaledInbound - scaledNormal;
        }

        public static readonly Vector2 UnitX = new Vector2(1, 0);
        public static readonly Vector2 UnitY = new Vector2(0, 1);

        public static readonly Vector2 Zero = new Vector2(0, 0);
        public static readonly Vector2 One = new Vector2(1, 1);

        public static readonly Vector2 PositiveInfinity = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        public static readonly Vector2 NegativeInfinity = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        public static readonly int SizeInBytes = Unsafe.SizeOf<Vector2>();

        [XmlIgnore]
        public Vector2 Yx
        {
            get => new Vector2(y, x);
            set
            {
                y = value.x;
                x = value.y;
            }
        }

        [Pure]
        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            left.x += right.x;
            left.y += right.y;

            return left;
        }

        [Pure]
        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            left.x -= right.x;
            left.y -= right.y;

            return left;
        }

        [Pure]
        public static Vector2 operator -(Vector2 vec)
        {
            vec.x = -vec.x;
            vec.y = -vec.y;

            return vec;
        }

        [Pure]
        public static Vector2 operator *(Vector2 vec, float scale)
        {
            vec.x *= scale;
            vec.y *= scale;

            return vec;
        }

        [Pure]
        public static Vector2 operator *(float scale, Vector2 vec)
        {
            vec.x *= scale;
            vec.y *= scale;

            return vec;
        }

        [Pure]
        public static Vector2 operator *(Vector2 left, Vector2 right)
        {
            left.x *= right.x;
            left.y *= right.y;

            return left;
        }

        [Pure]
        public static Vector2 operator /(Vector2 vec, float scale)
        {
            vec.x /= scale;
            vec.y /= scale;

            return vec;
        }

        [Pure]
        public static Vector2 operator /(float scale, Vector2 vec)
        {
            vec.x /= scale;
            vec.y /= scale;

            return vec;
        }

        [Pure]
        public static Vector2 operator /(Vector2 left, Vector2 right)
        {
            left.x /= right.x;
            left.y /= right.y;

            return left;
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !left.Equals(right);
        }

        private static readonly string ListSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        public override string ToString()
        {
            return string.Format("({0}{2} {1})", x, y, ListSeparator);
        }

        public override bool Equals(object obj)
        {
            if(obj is Vector2 vector)
            {
                return Equals(vector);
            }
            return false;
        }

        public bool Equals(Vector2 other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        [Pure]
        public void Deconstruct(out float x, out float y)
        {
            x = this.x;
            y = this.y;
        }
    }
}
