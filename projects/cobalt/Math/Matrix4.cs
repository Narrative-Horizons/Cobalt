using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cobalt.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class Matrix4 : IEquatable<Matrix4>
    {
        public float[] buffer = new float[16];

        public Matrix4() : this(0.0f)
        {

        }

        public Matrix4(float scalar)
        {
            buffer = Enumerable.Repeat(scalar, 16).ToArray();
        }

        public Matrix4(Vector4 diagonal) : this(0.0f)
        {
            buffer[0] = diagonal.x;
            buffer[5] = diagonal.y;
            buffer[10] = diagonal.z;
            buffer[15] = diagonal.w;
        }

        public Matrix4(Vector4 col0, Vector4 col1, Vector4 col2, Vector4 col3)
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

        public float this[int row, int col]
        {
            get
            {
                return buffer[row * 4 + col];
            }

            set
            {
                buffer[row * 4 + col] = value;
            }
        }

        public Matrix4(Matrix4 matrix)
        {
            Array.Copy(matrix.buffer, buffer, 16);
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
            return Enumerable.SequenceEqual(buffer, other.buffer);
        }

        public override int GetHashCode()
        {
            return buffer.GetHashCode();
        }

        public static Matrix4 operator +(Matrix4 left, Matrix4 right)
        {
            for (int i = 0; i < 16; i++)
            {
                left.buffer[i] += right.buffer[i];
            }

            return left;
        }

        public static Matrix4 operator -(Matrix4 left, Matrix4 right)
        {
            for (int i = 0; i < 16; i++)
            {
                left.buffer[i] -= right.buffer[i];
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


            left[0, 0] = e00;
            left[0, 1] = e01;
            left[0, 2] = e02;
            left[0, 3] = e03;

            left[1, 0] = e10;
            left[1, 1] = e11;
            left[1, 2] = e12;
            left[1, 3] = e13;

            left[2, 0] = e20;
            left[2, 1] = e21;
            left[2, 2] = e22;
            left[2, 3] = e23;

            left[3, 0] = e30;
            left[3, 1] = e31;
            left[3, 2] = e32;
            left[3, 3] = e33;

            return left;
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

        public Matrix4 Transpose()
        {
            Swap(ref buffer[1], ref buffer[4]);
            Swap(ref buffer[2], ref buffer[8]);
            Swap(ref buffer[6], ref buffer[9]);
            Swap(ref buffer[3], ref buffer[12]);
            Swap(ref buffer[7], ref buffer[13]);
            Swap(ref buffer[11], ref buffer[14]);

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
