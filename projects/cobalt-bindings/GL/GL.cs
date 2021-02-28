using Cobalt.Bindings.Utils;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Cobalt.Bindings.GL
{
    public static class GL
    {
        #region Delegates
        private delegate void PFNGLClearProc(EBufferBit mask);
        private delegate void PFNGLClearColorProc(float r, float g, float b, float a);
        private delegate IntPtr PFNGLGetString(EPropertyName name);
        #region GL46
        private delegate void PFNGLMultiDrawElementsIndirectCount(int mode, int type, IntPtr indirect, IntPtr drawCount, uint maxDrawCount, uint stride);
        #endregion
        #endregion

        #region NativeFunctions
        private static PFNGLClearProc glClear;
        private static PFNGLClearColorProc glClearColor;
        private static PFNGLGetString glGetString;
        private static PFNGLMultiDrawElementsIndirectCount glMultiDrawElementsIndirectCount;
        #endregion

        public static void Clear(EBufferBit mask)
        {
            glClear.Invoke(mask);
        }

        public static void ClearColor(float r, float g, float b, float a)
        {
            glClearColor.Invoke(r, g, b, a);
        }

        public static string GetString(EPropertyName name)
        {
            return Util.PtrToStringUTF8(glGetString.Invoke(name));
        }

        public static void MultiDrawElementsIndirectCount(int mode, int type, IntPtr indirect, IntPtr drawCount, uint maxDrawCount, uint stride)
        {
            glMultiDrawElementsIndirectCount.Invoke(mode, type, indirect, drawCount, maxDrawCount, stride);
        }

        public static void glInit(Func<byte[], IntPtr> getProcAddress)
        {
            T getProc<T>(byte[] name) => Marshal.GetDelegateForFunctionPointer<T>(getProcAddress(name));
            
            glClear = load(getProc<PFNGLClearProc>, "glClear");
            glClearColor = load(getProc<PFNGLClearColorProc>, "glClearColor");
            glGetString = load(getProc<PFNGLGetString>, "glGetString");
            glMultiDrawElementsIndirectCount = load(getProc<PFNGLMultiDrawElementsIndirectCount>, "glMultiDrawElementsIndirectCount");
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
