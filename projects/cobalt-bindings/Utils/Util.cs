using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Cobalt.Bindings.Utils
{
    public class Util
    {
        public static string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                int length = 0;
                while (Marshal.ReadByte(ptr, length) != 0)
                {
                    length++;
                }

                byte[] buffer = new byte[length];
                Marshal.Copy(ptr, buffer, 0, length);

                return Encoding.UTF8.GetString(buffer);
            }

            return "";
        }

        public static void Copy(IntPtr source, ushort[] destination, int startIndex, int length)
        {
            unsafe
            {
                var srcPtr = (ushort*)source;
                for (var i = startIndex; i < startIndex + length; ++i)
                {
                    destination[i] = *srcPtr;
                    ++srcPtr;
                }
            }
        }

        public delegate IntPtr FunctionLoader(byte[] name);
    }
}
