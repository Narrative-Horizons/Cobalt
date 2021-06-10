using System;
using System.Runtime.InteropServices;

namespace Cobalt.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4 : IEquatable<Matrix4>
    {
        public float M11;
        public float M12;
        public float M13;
        public float M14;

        public float M21;
        public float M22;
        public float M23;
        public float M24;

        public float M31;
        public float M32;
        public float M33;
        public float M34;

        public float M41;
        public float M42;
        public float M43;
        public float M44;

        public Matrix4(float scalar)
        {
            M11 = M12 = M13 = M14 = M21 = M22 = M23 = M24 = M31 = M32 = M33 = M34 = M41 = M42 = M43 = M44 = 0.0f;
        }

        public Matrix4(Vector4 diagonal) : this(0.0f)
        {
            M11 = diagonal.x;
            M22 = diagonal.y;
            M33 = diagonal.z;
            M44 = diagonal.w;
        }

        public Matrix4(Vector4 col0, Vector4 col1, Vector4 col2, Vector4 col3) : this(0.0f)
        {
            M11 = col0.x;
            M12 = col0.y;
            M13 = col0.z;
            M14 = col0.w;

            M21 = col1.x;
            M22 = col1.y;
            M23 = col1.z;
            M24 = col1.w;

            M31 = col2.x;
            M32 = col2.y;
            M33 = col2.z;
            M34 = col2.w;

            M41 = col3.x;
            M42 = col3.y;
            M43 = col3.z;
            M44 = col3.w;
        }

        public Matrix4(float[] data) : this(0.0f)
        {
            M11 = data[0];
            M12 = data[1];
            M13 = data[2];
            M14 = data[3];

            M21 = data[4];
            M22 = data[5];
            M23 = data[6];
            M24 = data[7];

            M31 = data[8];
            M32 = data[9];
            M33 = data[10];
            M34 = data[11];

            M41 = data[12];
            M42 = data[13];
            M43 = data[14];
            M44 = data[15];
        }

        public float this[int row, int col]
        {
            get
            {
                switch(row)
                {
                    case 0:
                        {
                            switch (col)
                            {
                                case 0:
                                    return M11;
                                case 1:
                                    return M12;
                                case 2:
                                    return M13;
                                case 3:
                                    return M14;
                            }
                            break;
                        }

                    case 1:
                        {
                            switch (col)
                            {
                                case 0:
                                    return M21;
                                case 1:
                                    return M22;
                                case 2:
                                    return M23;
                                case 3:
                                    return M24;
                            }
                            break;
                        }

                    case 2:
                        {
                            switch (col)
                            {
                                case 0:
                                    return M31;
                                case 1:
                                    return M32;
                                case 2:
                                    return M33;
                                case 3:
                                    return M34;
                            }
                            break;
                        }

                    case 3:
                        {
                            switch (col)
                            {
                                case 0:
                                    return M41;
                                case 1:
                                    return M42;
                                case 2:
                                    return M43;
                                case 3:
                                    return M44;
                            }
                            break;
                        }
                }

                return 0.0f;
            }

            set
            {
                switch (row)
                {
                    case 0:
                        {
                            switch (col)
                            {
                                case 0:
                                    M11 = value;
                                    break;
                                case 1:
                                    M12 = value;
                                    break;
                                case 2:
                                    M13 = value;
                                    break;
                                case 3:
                                    M14 = value;
                                    break;
                            }
                            break;
                        }

                    case 1:
                        {
                            switch (col)
                            {
                                case 0:
                                    M21 = value;
                                    break;
                                case 1:
                                    M22 = value;
                                    break;
                                case 2:
                                    M23 = value;
                                    break;
                                case 3:
                                    M24 = value;
                                    break;
                            }
                            break;
                        }

                    case 2:
                        {
                            switch (col)
                            {
                                case 0:
                                    M31 = value;
                                    break;
                                case 1:
                                    M32 = value;
                                    break;
                                case 2:
                                    M33 = value;
                                    break;
                                case 3:
                                    M34 = value;
                                    break;
                            }
                            break;
                        }

                    case 3:
                        {
                            switch (col)
                            {
                                case 0:
                                    M41 = value;
                                    break;
                                case 1:
                                    M42 = value;
                                    break;
                                case 2:
                                    M43 = value;
                                    break;
                                case 3:
                                    M44 = value;
                                    break;
                            }
                            break;
                        }
                }
            }
        }

        public Matrix4(Matrix4 matrix) : this(0.0f)
        {
            M11 = matrix.M11;
            M12 = matrix.M12;
            M13 = matrix.M13;
            M14 = matrix.M14;
            M21 = matrix.M21;
            M22 = matrix.M22;
            M23 = matrix.M23;
            M24 = matrix.M24;
            M31 = matrix.M31;
            M32 = matrix.M32;
            M33 = matrix.M33;
            M34 = matrix.M34;
            M41 = matrix.M41;
            M42 = matrix.M42;
            M43 = matrix.M43;
            M44 = matrix.M44;
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
            return M11 == other.M11 && M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
                M21 == other.M21 && M22 == other.M22 && M23 == other.M23 && M24 == other.M24 &&
                M31 == other.M31 && M32 == other.M32 && M33 == other.M33 && M34 == other.M34 &&
                M41 == other.M41 && M42 == other.M42 && M43 == other.M43 && M44 == other.M44;
        }

        public override string ToString()
        {
            return M11 + ", " + M12 + ", " + M13 + ", " + M14 + ", " 
                + M21 + ", " + M22 + ", " + M23 + ", " + M24 + ", " 
                + M31 + ", " + M32 + ", " + M33 + ", " + M34 + ", " 
                + M41 + ", " + M42 + ", " + M43 + ", " + M44;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(M11, M12, M13, M14) + HashCode.Combine(M21, M22, M23, M24) + HashCode.Combine(M31, M32, M33, M34) + HashCode.Combine(M41, M42, M43, M44);
        }

        public static Matrix4 operator +(Matrix4 left, Matrix4 right)
        {
            left.M11 += right.M11;
            left.M12 += right.M12;
            left.M13 += right.M13;
            left.M14 += right.M14;
            left.M21 += right.M21;
            left.M22 += right.M22;
            left.M23 += right.M23;
            left.M24 += right.M24;
            left.M31 += right.M31;
            left.M32 += right.M32;
            left.M33 += right.M33;
            left.M34 += right.M34;
            left.M41 += right.M41;
            left.M42 += right.M42;
            left.M43 += right.M43;
            left.M44 += right.M44;

            return left;
        }

        public static Matrix4 operator -(Matrix4 left, Matrix4 right)
        {
            left.M11 -= right.M11;
            left.M12 -= right.M12;
            left.M13 -= right.M13;
            left.M14 -= right.M14;
            left.M21 -= right.M21;
            left.M22 -= right.M22;
            left.M23 -= right.M23;
            left.M24 -= right.M24;
            left.M31 -= right.M31;
            left.M32 -= right.M32;
            left.M33 -= right.M33;
            left.M34 -= right.M34;
            left.M41 -= right.M41;
            left.M42 -= right.M42;
            left.M43 -= right.M43;
            left.M44 -= right.M44;

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

        public static Matrix4 Translate(Vector3 position)
        {
            Matrix4 ret = Matrix4.Identity;

            ret[3, 0] = position.x;
            ret[3, 1] = position.y;
            ret[3, 2] = position.z;

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

        public static Matrix4 FromRowMajorElements(
            float r1c1, float r1c2, float r1c3, float r1c4,
            float r2c1, float r2c2, float r2c3, float r2c4,
            float r3c1, float r3c2, float r3c3, float r3c4,
            float r4c1, float r4c2, float r4c3, float r4c4)
        {
            float[] m = new float[16];
            m[0]=r1c1; m[4]=r1c2;  m[8]=r1c3; m[12]=r1c4;
            m[1]=r2c1; m[5]=r2c2;  m[9]=r2c3; m[13]=r2c4;
            m[2]=r3c1; m[6]=r3c2; m[10]=r3c3; m[14]=r3c4;
            m[3]=r4c1; m[7]=r4c2; m[11]=r4c3; m[15]=r4c4;

            return new Matrix4(m);
        }

        public static Matrix4 LookAt(Vector3 eye, Vector3 center, Vector3 up)
        {
            Vector3 f = (center - eye).Normalized();
            Vector3 s = Vector3.Cross(up, f).Normalized();
            Vector3 u = Vector3.Cross(f, s);

            Matrix4 ret = Matrix4.Identity;

            ret[0, 0] = s.x;
            ret[1, 0] = s.y;
            ret[2, 0] = s.z;
            ret[0, 1] = u.x;
            ret[1, 1] = u.y;
            ret[2, 1] = u.z;
            ret[0, 2] = f.x;
            ret[1, 2] = f.y;
            ret[2, 2] = f.z;
            ret[3, 0] = -Vector3.Dot(s, eye);
            ret[3, 1] = -Vector3.Dot(u, eye);
            ret[3, 2] = -Vector3.Dot(f, eye);

            return ret;
        }

        public static Matrix4 Frustum(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            return Matrix4.FromRowMajorElements(2.0f * zNear / (right - left), 0, (right + left) / (right - left), 0,
                                                0, 2.0f * zNear / (top - bottom), (top + bottom) / (top - bottom), 0,
                                                0, 0, -(zFar + zNear) / (zNear - zFar), -2.0f * zFar * zNear / (zFar - zNear),
                                                0, 0, -1, 0);
        }

        public static Matrix4 Perspective(float fov, float aspect, float zNear, float zFar)
        {
            /*float ymax, xmax;

            ymax = zNear * MathF.Tan(fov);
            xmax = ymax * aspect;

            return Matrix4.Frustum(-xmax, xmax, -ymax, ymax, zNear, zFar);*/

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
            Swap(ref M12, ref M21);
            Swap(ref M31, ref M13);
            Swap(ref M32, ref M23);
            Swap(ref M41, ref M14);
            Swap(ref M42, ref M24);
            Swap(ref M43, ref M34);

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
