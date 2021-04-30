using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Cobalt.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{DebugString}")]
    public struct Matrix4 : IEquatable<Matrix4>
    {
        private unsafe fixed float buffer[16];

        public unsafe string DebugString
        {
            get
            {
                unsafe
                {
                    return "Aw " + this.ToString() + " motherfucker! Aw " + this.ToString2() + " g'damn!";
                }
            }
        }

        public Matrix4(float scalar)
        {
            unsafe
            {
                for(int i = 0; i < 16; i++)
                {
                    buffer[i] = scalar;
                }
            }
        }

        public Matrix4(Vector4 diagonal) : this(0.0f)
        {
            unsafe
            {
                buffer[0] = diagonal.x;
                buffer[5] = diagonal.y;
                buffer[10] = diagonal.z;
                buffer[15] = diagonal.w;
            }
        }

        public Matrix4(Vector4 col0, Vector4 col1, Vector4 col2, Vector4 col3)
        {
            unsafe
            {
                buffer[0] = col0.x;
                buffer[1] = col0.y;
                buffer[2] = col0.z;
                buffer[3] = col0.w;

                buffer[4] = col1.x;
                buffer[5] = col1.y;
                buffer[6] = col1.z;
                buffer[7] = col1.w;

                buffer[8] = col2.x;
                buffer[9] = col2.y;
                buffer[10] = col2.z;
                buffer[11] = col2.w;

                buffer[12] = col3.x;
                buffer[13] = col3.y;
                buffer[14] = col3.z;
                buffer[15] = col3.w;
            }
        }

        public float this[int row, int col]
        {
            get
            {
                unsafe
                {
                    return buffer[row * 4 + col];
                }
            }

            set
            {
                unsafe
                {
                    buffer[row * 4 + col] = value;
                }
            }
        }

        public Matrix4(Matrix4 matrix)
        {
            unsafe
            {
                for(int i = 0; i < 16; i++)
                {
                    buffer[i] = matrix.buffer[i];
                }
            }
        }

        public static readonly Matrix4 Identity = new Matrix4(Vector4.One);

        public override bool Equals(object obj)
        {
            if (obj is Matrix4 matrix)
            {
                return Equals(matrix);
            }
            return false;
        }

        public bool Equals(Matrix4 other)
        {
            unsafe
            {
                bool equal = true;
                for (int i = 0; i < 16; i++)
                {
                    if (buffer[i] != other.buffer[i])
                        equal = false;
                }

                return equal;
            }
        }

        public override int GetHashCode()
        {
            unsafe
            {
                float hc = 17;
                for (int i = 0; i < 16; i++)
                {
                   hc = unchecked(31 * hc + buffer[i]);
                }
                return (int)hc;
            }
        }

        public override string ToString()
        {
            unsafe
            {
                string ret = "";

                fixed (float* b = buffer)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        ret += b[i] + ", ";
                    }
                }
                
                return ret;
            }
        }

        public string ToString2()
        {
            unsafe
            {
                string ret = "";

                for (int i = 0; i < 16; i++)
                {
                    ret += buffer[i] + ", ";
                }
                return ret;
            }
        }

        public static Matrix4 operator +(Matrix4 left, Matrix4 right)
        {
            unsafe
            {
                for (int i = 0; i < 16; i++)
                {
                    left.buffer[i] += right.buffer[i];
                }
            }

            return left;
        }

        public static Matrix4 operator -(Matrix4 left, Matrix4 right)
        {
            unsafe
            {
                for (int i = 0; i < 16; i++)
                {
                    left.buffer[i] -= right.buffer[i];
                }
            }

            return left;
        }

        public static Matrix4 operator *(Matrix4 left, Matrix4 right)
        {
            float e00 = left[0, 0] * right[0, 0] + left[0, 1] * right[1, 0]
                + left[0, 2] * right[2, 0] + left[0, 3] * right[3, 0];
            float e01 = left[0, 0] * right[0, 1] + left[0, 1] * right[1, 1]
                + left[0, 2] * right[2, 1] + left[0, 3] * right[3, 1];
            float e02 = left[0, 0] * right[0, 2] + left[0, 1] * right[1, 2]
                + left[0, 2] * right[2, 2] + left[0, 3] * right[3, 2];
            float e03 = left[0, 0] * right[0, 3] + left[0, 1] * right[1, 3]
                + left[0, 2] * right[2, 3] + left[0, 3] * right[3, 3];

            float e10 = left[1, 0] * right[0, 0] + left[1, 1] * right[1, 0]
                + left[1, 2] * right[2, 0] + left[1, 3] * right[3, 0];
            float e11 = left[1, 0] * right[0, 1] + left[1, 1] * right[1, 1]
                + left[1, 2] * right[2, 1] + left[1, 3] * right[3, 1];
            float e12 = left[1, 0] * right[0, 2] + left[1, 1] * right[1, 2]
                + left[1, 2] * right[2, 2] + left[1, 3] * right[3, 2];
            float e13 = left[1, 0] * right[0, 3] + left[1, 1] * right[1, 3]
                + left[1, 2] * right[2, 3] + left[1, 3] * right[3, 3];

            float e20 = left[2, 0] * right[0, 0] + left[2, 1] * right[1, 0]
                + left[2, 2] * right[2, 0] + left[2, 3] * right[3, 0];
            float e21 = left[2, 0] * right[0, 1] + left[2, 1] * right[1, 1]
                + left[2, 2] * right[2, 1] + left[2, 3] * right[3, 1];
            float e22 = left[2, 0] * right[0, 2] + left[2, 1] * right[1, 2]
                + left[2, 2] * right[2, 2] + left[2, 3] * right[3, 2];
            float e23 = left[2, 0] * right[0, 3] + left[2, 1] * right[1, 3]
                + left[2, 2] * right[2, 3] + left[2, 3] * right[3, 3];

            float e30 = left[3, 0] * right[0, 0] + left[3, 1] * right[1, 0]
                + left[3, 2] * right[2, 0] + left[3, 3] * right[3, 0];
            float e31 = left[3, 0] * right[0, 1] + left[3, 1] * right[1, 1]
                + left[3, 2] * right[2, 1] + left[3, 3] * right[3, 1];
            float e32 = left[3, 0] * right[0, 2] + left[3, 1] * right[1, 2]
                + left[3, 2] * right[2, 2] + left[3, 3] * right[3, 2];
            float e33 = left[3, 0] * right[0, 3] + left[3, 1] * right[1, 3]
                + left[3, 2] * right[2, 3] + left[3, 3] * right[3, 3];

            Matrix4 ret = Matrix4.Identity;

            ret[0, 0] = e00;
            ret[0, 1] = e01;
            ret[0, 2] = e02;
            ret[0, 3] = e03;
            
            ret[1, 0] = e10;
            ret[1, 1] = e11;
            ret[1, 2] = e12;
            ret[1, 3] = e13;
            
            ret[2, 0] = e20;
            ret[2, 1] = e21;
            ret[2, 2] = e22;
            ret[2, 3] = e23;
            
            ret[3, 0] = e30;
            ret[3, 1] = e31;
            ret[3, 2] = e32;
            ret[3, 3] = e33;

            return ret;
        }

        public static Matrix4 Scale(Vector3 scale)
        {
            Matrix4 ret = Matrix4.Identity;

            ret[0, 0] = scale.x;
            ret[1, 1] = scale.y;
            ret[2, 2] = scale.z;

            return ret;
        }

        public static Matrix4 Rotate(Vector3 eulerAngles)
        {
            Matrix4 ret = Matrix4.Identity;
            ret *= Rotate(new Vector3(1, 0, 0), eulerAngles.x);
            ret *= Rotate(new Vector3(0, 1, 0), eulerAngles.y);
            ret *= Rotate(new Vector3(0, 0, 1), eulerAngles.z);

            return ret;
        }

        public static Matrix4 Rotate(Vector3 axis, float degrees)
        {
            float radians = Scalar.ToRadians(degrees);
            float cosine = MathF.Cos(radians);
            float sine = MathF.Sin(radians);
            float oneMinusCosine = 1.0f - cosine;

            float xy = axis.x * axis.y;
            float yz = axis.y * axis.z;
            float xz = axis.x * axis.z;

            float xSine = axis.x * sine;
            float ySine = axis.y * sine;
            float zSine = axis.z * sine;

            float f00 = axis.x * axis.x * oneMinusCosine + cosine;
            float f01 = xy * oneMinusCosine + zSine;
            float f02 = xz * oneMinusCosine - ySine;

            float f10 = xy * oneMinusCosine - zSine;
            float f11 = axis.y * axis.y * oneMinusCosine + cosine;
            float f12 = yz * oneMinusCosine + xSine;

            float f20 = xz * oneMinusCosine + ySine;
            float f21 = yz * oneMinusCosine - xSine;
            float f22 = axis.z * axis.z * oneMinusCosine + cosine;

            Matrix4 ret = Matrix4.Identity;

            float t00 = ret[0, 0] * f00 + ret[0, 1] * f01 + ret[0, 2] * f02;
            float t01 = ret[1, 0] * f00 + ret[1, 1] * f01 + ret[1, 2] * f02;
            float t02 = ret[2, 0] * f00 + ret[2, 1] * f01 + ret[2, 2] * f02;
            float t03 = ret[3, 0] * f00 + ret[3, 1] * f01 + ret[3, 2] * f02;
            float t10 = ret[0, 0] * f10 + ret[0, 1] * f11 + ret[0, 2] * f12;
            float t11 = ret[1, 0] * f10 + ret[1, 1] * f11 + ret[1, 2] * f12;
            float t12 = ret[2, 0] * f10 + ret[2, 1] * f11 + ret[2, 2] * f12;
            float t13 = ret[3, 0] * f10 + ret[3, 1] * f11 + ret[3, 2] * f12;
            float t20 = ret[0, 0] * f20 + ret[0, 1] * f21 + ret[0, 2] * f22;
            float t21 = ret[1, 0] * f20 + ret[1, 1] * f21 + ret[1, 2] * f22;
            float t22 = ret[2, 0] * f20 + ret[2, 1] * f21 + ret[2, 2] * f22;
            float t23 = ret[3, 0] * f20 + ret[3, 1] * f21 + ret[3, 2] * f22;

            ret[0, 0] = t00;
            ret[1, 0] = t01;
            ret[2, 0] = t02;
            ret[3, 0] = t03;
            ret[0, 1] = t10;
            ret[1, 1] = t11;
            ret[2, 1] = t12;
            ret[3, 1] = t13;
            ret[0, 2] = t20;
            ret[1, 2] = t21;
            ret[2, 2] = t22;
            ret[3, 2] = t23;

            return ret;
        }

        public static Matrix4 LookAt(Vector3 eye, Vector3 center, Vector3 up)
        {
            Vector3 f = (center - eye).Normalized();
            Vector3 s = Vector3.Cross(up, f).Normalized();
            Vector3 u = Vector3.Cross(f, s);

            Matrix4 ret = Matrix4.Identity;

            ret[0,0] = s.x;
            ret[1,0] = s.y;
            ret[2,0] = s.z;
            ret[0,1] = u.x;
            ret[1,1] = u.y;
            ret[2,1] = u.z;
            ret[0,2] = f.x;
            ret[1,2] = f.y;
            ret[2,2] = f.z;
            ret[3,0] = -Vector3.Dot(s, eye);
            ret[3,1] = -Vector3.Dot(u, eye);
            ret[3,2] = -Vector3.Dot(f, eye);

            return ret;
        }

        public static Matrix4 Perspective(float fov, float aspect, float zNear, float zFar)
        {
            float tanHalfFov = MathF.Tan(fov / 2.0f);

            Matrix4 ret = Matrix4.Identity;

            float m00 = 1.0f / (aspect * tanHalfFov);
            float m11 = 1.0f / tanHalfFov;
            float m22 = zFar / (zFar - zNear);
            float m32 = -(zFar * zNear) / (zFar - zNear);

            ret[0, 0] = m00;
            ret[1, 1] = m11;
            ret[2, 2] = m22;
            ret[2, 3] = 1.0f;
            ret[3, 2] = m32;

            return ret;
        }

        public static Vector4 operator *(Matrix4 left, Vector4 right)
        {
            float x = left[0, 0] * right.x + left[0, 1] * right.y + left[0, 2] * right.z + left[0, 3] * right.w;
            float y = left[1, 0] * right.x + left[1, 1] * right.y + left[1, 2] * right.z + left[1, 3] * right.w;
            float z = left[2, 0] * right.x + left[2, 1] * right.y + left[2, 2] * right.z + left[2, 3] * right.w;
            float w = left[3, 0] * right.x + left[3, 1] * right.y + left[3, 2] * right.z + left[3, 3] * right.w;

            right.x = x;
            right.y = y;
            right.z = z;
            right.w = w;

            return right;
        }

        public static implicit operator Matrix4(System.Numerics.Matrix4x4 v)
        {
            Matrix4 res = new Matrix4();

            res[0, 0] = v.M11;
            res[0, 1] = v.M12;
            res[0, 2] = v.M13;
            res[0, 3] = v.M14;
            
            res[1, 0] = v.M21;
            res[1, 1] = v.M22;
            res[1, 2] = v.M23;
            res[1, 3] = v.M24;
            
            res[2, 0] = v.M31;
            res[2, 1] = v.M32;
            res[2, 2] = v.M33;
            res[2, 3] = v.M34;
            
            res[3, 0] = v.M41;
            res[3, 1] = v.M42;
            res[3, 2] = v.M43;
            res[3, 3] = v.M44;

            return res;
        }

        public Matrix4 Transpose()
        {
            unsafe
            {
                Swap(ref buffer[1], ref buffer[4]);
                Swap(ref buffer[2], ref buffer[8]);
                Swap(ref buffer[6], ref buffer[9]);
                Swap(ref buffer[3], ref buffer[12]);
                Swap(ref buffer[7], ref buffer[13]);
                Swap(ref buffer[11], ref buffer[14]);
            }

            return this;
        }

        public float Determinant()
        {
            float factor0 = this[2, 2] * this[3, 3] - this[3, 2] * this[2, 3];
            float factor1 = this[2, 1] * this[3, 3] - this[3, 1] * this[2, 3];
            float factor2 = this[2, 1] * this[3, 2] - this[3, 1] * this[2, 3];
            float factor3 = this[2, 0] * this[3, 3] - this[3, 0] * this[2, 3];
            float factor4 = this[2, 0] * this[3, 2] - this[3, 0] * this[2, 3];
            float factor5 = this[2, 0] * this[3, 1] - this[3, 0] * this[2, 2];

            float detCoFactor0 = this[1, 1] * factor0 - this[1, 2] * factor1 + this[1, 3] * factor2;
            float detCoFactor1 = -this[1, 0] * factor0 - this[1, 2] * factor3 + this[1, 3] * factor4;
            float detCoFactor2 = this[1, 0] * factor1 - this[1, 1] * factor3 + this[1, 3] * factor5;
            float detCoFactor3 = -this[1, 0] * factor2 - this[1, 1] * factor4 + this[1, 2] * factor5;

            return this[0, 0] * detCoFactor0 + this[0, 1] * detCoFactor1
                + this[0, 2] * detCoFactor2 + this[0, 3] * detCoFactor3;
        }

        public Matrix4 Inverse()
        {
            float[,] scratchBuffer = new float[4, 4];

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    scratchBuffer[row, col] = this[row, col];
                }
                scratchBuffer[row, row + 4] = 1;
            }

            bool hasInverse = ToRref(4, 4, ref scratchBuffer);
            if (hasInverse == false)
            {
                return Identity;
            }

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    this[row, col] = scratchBuffer[row, col + 4];
                }
            }

            return this;
        }

        private bool ToRref(int rows, int cols, ref float[,] buffer)
        {
            for (int i = rows - 1; i > 0; --i)
            {
                if (buffer[i - 1, 0] < buffer[i, 0])
                {
                    Swap(ref buffer[i, 0], ref buffer[i - 1, 0]);
                    Swap(ref buffer[i, 1], ref buffer[i - 1, 1]);
                    Swap(ref buffer[i, 2], ref buffer[i - 1, 2]);
                    Swap(ref buffer[i, 3], ref buffer[i - 1, 3]);
                }
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (j != i)
                    {
                        float tmp = buffer[j, i] / buffer[i, i];
                        if (float.IsFinite(tmp) == false)
                        {
                            return false;
                        }
                        for (int k = 0; k < 2 * cols; k++)
                        {
                            buffer[j, k] -= buffer[i, k] * tmp;
                        }
                    }
                }
            }

            for (int i = 0; i < rows; i++)
            {
                float tmp = buffer[i, i];

                for (int j = 0; j < 2 * cols; j++)
                {
                    buffer[i, j] /= tmp;
                }
            }

            return true;
        }

        private void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }
    }
}
