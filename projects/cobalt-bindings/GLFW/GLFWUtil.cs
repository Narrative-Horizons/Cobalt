using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Cobalt.Bindings.GLFW
{
    public class GLFWUtil
    {
        public static string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                var length = 0;
                while (Marshal.ReadByte(ptr, length) != 0)
                    length++;
                var buffer = new byte[length];
                Marshal.Copy(ptr, buffer, 0, length);
                return Encoding.UTF8.GetString(buffer);
            }

            return "";
        }
    }
}
