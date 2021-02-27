using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Cobalt.Bindings.GL
{
    public static class GL
    {
        private delegate void ClearProc(EBufferBit mask);
        private static ClearProc glClear;

        private delegate void ClearColorProc(float r, float g, float b, float a);
        private static ClearColorProc glClearColor;

        public static void Clear(EBufferBit mask)
        {
            glClear.Invoke(mask);
        }

        public static void ClearColor(float r, float g, float b, float a)
        {
            glClearColor.Invoke(r, g, b, a);
        }

        public static void glInit(Func<byte[], IntPtr> getProcAddress)
        {
            T getProc<T>(byte[] name) => Marshal.GetDelegateForFunctionPointer<T>(getProcAddress(name));
            
            glClear = load(getProc<ClearProc>, "glClear");
            glClearColor = load(getProc<ClearColorProc>, "glClearColor");
        }

        private static T load<T>(Func<byte[], T> loader, string name)
        {
            T result = loader.Invoke(Encoding.UTF8.GetBytes(name));
            if (result == null)
            {
                throw new InvalidOperationException("Failed to load " + name);
            }
            return result;
        }
    }
}
