using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Cobalt.Math
{
    [Serializable]
    [DataContract]
    public class Quaternion : IEquatable<Quaternion>
    {
        [DataMember(Order = 1)]
        public float x;
        [DataMember(Order = 2)]
        public float y;
        [DataMember(Order = 3)]
        public float z;
        [DataMember(Order = 4)]
        public float w;

        public Vector3 Xyz
        {
            get => new Vector3(x, y, z);
            set
            {
                x = value.x;
                y = value.y;
                z = value.z;
            }
        }

        public float this[int index]
        {
            get
            {
                return index switch
                {
                    0 => this.x,
                    1 => this.y,
                    2 => this.z,
                    3 => this.w,
                    _ => throw new IndexOutOfRangeException("Invalid Quaternion index: " + index +
                                                            ", can use only 0,1,2,3")
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    case 3:
                        this.w = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaternion index: " + index +
                                                           ", can use only 0,1,2,3");
                }
            }
        }

        public static Quaternion Identity => new Quaternion(0, 0, 0, 1);

        public Vector3 EulerAngles
        {
            get => ToEulerRad(this) * Scalar.RadToDeg;
            set => Set(FromEulerRad(value * Scalar.DegToRad));
        }

        public float Length => MathF.Sqrt(x * x + y * y + z * z + w * w);
        public float LengthSquared => (x * x + y * y + z * z + w * w);

        public Quaternion()
        {
            x = y = z = w = 0;
        }

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion(Vector3 v, float w)
        {
            this.x = v.x;
            this.y = v.x;
            this.z = v.z;
            this.w = w;
        }

        public void Set(float nx, float ny, float nz, float nw)
        {
            this.x = nx;
            this.y = ny;
            this.z = nz;
            this.w = nw;
        }

        public void Set(Quaternion v)
        {
            Set(v.x, v.y, v.z, v.w);
        }

        public void Normalize()
        {
            float scale = 1.0f / Length;
            Xyz *= scale;
            w *= scale;
        }

        public static Quaternion Normalize(Quaternion v)
        {
            Normalize(ref v, out var result);

            return result;
        }

        public static void Normalize(ref Quaternion v, out Quaternion result)
        {
            float scale = 1.0f / v.Length;
            result = new Quaternion(v.Xyz * scale, v.w * scale);
        }

        public static float Dot(Quaternion a, Quaternion b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public static Quaternion AngleAxis(float angle, Vector3 axis)
        {
            return AngleAxis(angle, ref axis);
        }

        private static Quaternion AngleAxis(float angle, ref Vector3 axis)
        {
            if (axis.LengthSquared == 0.0f)
            {
                return Identity;
            }

            float radians = Scalar.ToRadians(angle);
            radians *= 0.5f;
            axis.Normalize();
            axis *= MathF.Sin(radians);

            Quaternion result = new Quaternion(axis, MathF.Cos(radians));

            return Normalize(result);
        }

        public void ToAngleAxis(out float angle, out Vector3 axis)
        {
            Quaternion.ToAxisAngleRad(this, out axis, out angle);
            angle = Scalar.ToDegrees(angle);
        }

        public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            return RotateTowards(LookRotation(fromDirection), LookRotation(toDirection), float.MaxValue);
        }

        public void SetFromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            Set(FromToRotation(fromDirection, toDirection));
        }

        public static Quaternion LookRotation(Vector3 forward, [DefaultValue("Vector3.UnitY")] Vector3 up)
        {
            return LookRotation(ref forward, ref up);
        }

        public static Quaternion LookRotation(Vector3 forward)
        {
            Vector3 up = Vector3.UnitY;
            return LookRotation(ref forward, ref up);
        }

        private static Quaternion LookRotation(ref Vector3 forward, ref Vector3 up)
        {
            forward.Normalize();
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
            up = Vector3.Cross(forward, right);

            float m00 = right.x;
            float m01 = right.y;
            float m02 = right.z;
            float m10 = up.x;
            float m11 = up.y;
            float m12 = up.z;
            float m20 = forward.x;
            float m21 = forward.y;
            float m22 = forward.z;

            float num8 = (m00 + m11) + m22;
            Quaternion quaternion = new Quaternion();
            if (num8 > 0f)
            {
                float num = MathF.Sqrt(num8 + 1f);
                quaternion.w = num * 0.5f;
                num = 0.5f / num;
                quaternion.x = (m12 - m21) * num;
                quaternion.y = (m20 - m02) * num;
                quaternion.z = (m01 - m10) * num;
                return quaternion;
            }
            if ((m00 >= m11) && (m00 >= m22))
            {
                float num7 = MathF.Sqrt(((1f + m00) - m11) - m22);
                float num4 = 0.5f / num7;
                quaternion.x = 0.5f * num7;
                quaternion.y = (m01 + m10) * num4;
                quaternion.z = (m02 + m20) * num4;
                quaternion.w = (m12 - m21) * num4;
                return quaternion;
            }
            if (m11 > m22)
            {
                float num6 = MathF.Sqrt(((1f + m11) - m00) - m22);
                float num3 = 0.5f / num6;
                quaternion.x = (m10 + m01) * num3;
                quaternion.y = 0.5f * num6;
                quaternion.z = (m21 + m12) * num3;
                quaternion.w = (m20 - m02) * num3;
                return quaternion;
            }
            float num5 =  MathF.Sqrt(((1f + m22) - m00) - m11);
            float num2 = 0.5f / num5;
            quaternion.x = (m20 + m02) * num2;
            quaternion.y = (m21 + m12) * num2;
            quaternion.z = 0.5f * num5;
            quaternion.w = (m01 - m10) * num2;
            return quaternion;
        }

        public void SetLookRotation(Vector3 view)
        {
            Vector3 up = Vector3.UnitY;
            this.SetLookRotation(view, up);
        }

        public void SetLookRotation(Vector3 view, [DefaultValue("Vector3.UnitY")] Vector3 up)
        {
            Set(LookRotation(view, up));
        }

        public static Quaternion Slerp(Quaternion a, Quaternion b, float t)
        {
            return Slerp(ref a, ref b, t);
        }

        private static Quaternion Slerp(ref Quaternion a, ref Quaternion b, float t)
        {
            if (t > 1) t = 1;
            if (t < 0) t = 0;

            return SlerpUnclamped(ref a, ref b, t);
        }

        public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, float t)
        {
            return SlerpUnclamped(ref a, ref b, t);
        }

        private static Quaternion SlerpUnclamped(ref Quaternion a, ref Quaternion b, float t)
        {
            // if either input is zero, return the other.
            if (a.LengthSquared == 0.0f)
            {
                return b.LengthSquared == 0.0f ? Identity : b;
            }

            if (b.LengthSquared == 0.0f)
            {
                return a;
            }

            float cosHalfAngle = a.w * b.w + Vector3.Dot(a.Xyz, b.Xyz);

            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
            {
                // angle = 0.0f, so just return one input.
                return a;
            }

            if (cosHalfAngle < 0.0f)
            {
                b.Xyz = -b.Xyz;
                b.w = -b.w;
                cosHalfAngle = -cosHalfAngle;
            }

            float blendA;
            float blendB;
            if (cosHalfAngle < 0.99f)
            {
                // do proper slerp for big angles
                float halfAngle = MathF.Acos(cosHalfAngle);
                float sinHalfAngle = MathF.Sin(halfAngle);
                float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = MathF.Sin(halfAngle * (1.0f - t)) * oneOverSinHalfAngle;
                blendB = MathF.Sin(halfAngle * t) * oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1.0f - t;
                blendB = t;
            }

            Quaternion result = new Quaternion(blendA * a.Xyz + blendB * b.Xyz, blendA * a.w + blendB * b.w);
            return result.LengthSquared > 0.0f ? Normalize(result) : Identity;
        }

        public static Quaternion Lerp(Quaternion a, Quaternion b, float t)
        {
            if (t > 1) t = 1;
            if (t < 0) t = 0;
            return Slerp(ref a, ref b, t); // TODO: use lerp not slerp, "Because quaternion works in 4D. Rotation in 4D are linear" ???
        }
        public static Quaternion LerpUnclamped(Quaternion a, Quaternion b, float t)
        {
            return Slerp(ref a, ref b, t);
        }

        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
        {
            float num = Angle(from, to);
            if (num == 0f)
            {
                return to;
            }
            float t = MathF.Min(1f, maxDegreesDelta / num);
            return SlerpUnclamped(from, to, t);
        }

        public static float Angle(Quaternion a, Quaternion b)
        {
            float f = Dot(a, b);
            return MathF.Acos(MathF.Min(MathF.Abs(f), 1f)) * 2f * Scalar.RadToDeg;
        }

        public static Quaternion Inverse(Quaternion rotation)
        {
            float lengthSq = rotation.LengthSquared;
            if (lengthSq == 0.0) return rotation;

            float i = 1.0f / lengthSq;
            return new Quaternion(rotation.Xyz * -i, rotation.w * i);
        }

        public static Quaternion Euler(float x, float y, float z)
        {
            return FromEulerRad(new Vector3(x, y, z) * Scalar.DegToRad);
        }

        public static Quaternion Euler(Vector3 euler)
        {
            return FromEulerRad(euler * Scalar.DegToRad);
        }

        private static Vector3 ToEulerRad(Quaternion rotation)
        {
            float sqw = rotation.w * rotation.w;
            float sqx = rotation.x * rotation.x;
            float sqy = rotation.y * rotation.y;
            float sqz = rotation.z * rotation.z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = rotation.x * rotation.w - rotation.y * rotation.z;
            Vector3 v;

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.y = 2f * MathF.Atan2(rotation.y, rotation.x);
                v.x = MathF.PI / 2;
                v.z = 0;
                return NormalizeAngles(v * Scalar.RadToDeg);
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.y = -2f * MathF.Atan2(rotation.y, rotation.x);
                v.x = -MathF.PI / 2;
                v.z = 0;
                return NormalizeAngles(v * Scalar.RadToDeg);
            }
            Quaternion q = new Quaternion(rotation.w, rotation.z, rotation.x, rotation.y);
            v.y = MathF.Atan2(2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w));     // Yaw
            v.x = MathF.Asin(2f * (q.x * q.z - q.w * q.y));                             // Pitch
            v.z = MathF.Atan2(2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z));      // Roll
            return NormalizeAngles(v * Scalar.RadToDeg);
        }

        private static Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.x = NormalizeAngle(angles.x);
            angles.y = NormalizeAngle(angles.y);
            angles.z = NormalizeAngle(angles.z);
            return angles;
        }
        private static float NormalizeAngle(float angle)
        {
            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;
            return angle;
        }

        private static Quaternion FromEulerRad(Vector3 euler)
        {
            float yaw = euler.x;
            float pitch = euler.y;
            float roll = euler.z;
            float rollOver2 = roll * 0.5f;
            float sinRollOver2 = MathF.Sin(rollOver2);
            float cosRollOver2 = MathF.Cos(rollOver2);
            float pitchOver2 = pitch * 0.5f;
            float sinPitchOver2 = MathF.Sin(pitchOver2);
            float cosPitchOver2 = MathF.Cos(pitchOver2);
            float yawOver2 = yaw * 0.5f;
            float sinYawOver2 = MathF.Sin(yawOver2);
            float cosYawOver2 = MathF.Cos(yawOver2);

            Quaternion result = new Quaternion
            {
                x = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2,
                y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2,
                z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2,
                w = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2
            };
            return result;

        }
        private static void ToAxisAngleRad(Quaternion q, out Vector3 axis, out float angle)
        {
            if (MathF.Abs(q.w) > 1.0f)
                q.Normalize();
            angle = 2.0f * MathF.Acos(q.w); // angle
            float den = MathF.Sqrt(1.0f - q.w * q.w);
            if (den > 0.0001f)
            {
                axis = q.Xyz / den;
            }
            else
            {
                // This occurs when the angle is zero. 
                // Not a problem: just set an arbitrary normalized axis.
                axis = new Vector3(1, 0, 0);
            }
        }

        public override string ToString()
        {
            return $"({this.x:F1}, {this.y:F1}, {this.z:F1}, {this.w:F1})";
        }

        public string ToString(string format)
        {
            return $"({this.x.ToString(format)}, {this.y.ToString(format)}, {this.z.ToString(format)}, {this.w.ToString(format)})";
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
        }
        public override bool Equals(object other)
        {
            if (!(other is Quaternion quaternion))
            {
                return false;
            }

            return this.x.Equals(quaternion.x) && this.y.Equals(quaternion.y) && this.z.Equals(quaternion.z) && this.w.Equals(quaternion.w);
        }
        public bool Equals(Quaternion other)
        {
            return other != null && this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z) && this.w.Equals(other.w);
        }
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }
        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float num = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num8 = rotation.x * num3;
            float num9 = rotation.y * num3;
            float num10 = rotation.w * num;
            float num11 = rotation.w * num2;
            float num12 = rotation.w * num3;
            Vector3 result;
            result.x = (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
            result.y = (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y + (num9 - num10) * point.z;
            result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (1f - (num4 + num5)) * point.z;
            return result;
        }
        public static bool operator ==(Quaternion lhs, Quaternion rhs)
        {
            return Dot(lhs, rhs) > 0.999999f;
        }
        public static bool operator !=(Quaternion lhs, Quaternion rhs)
        {
            return Dot(lhs, rhs) <= 0.999999f;
        }
    }
}
