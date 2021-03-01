using Cobalt.Bindings.Utils;
using Cobalt.Bindings.GL;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Cobalt.Bindings.GLAD
{
    public class GLAD
    {
        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "../x86_64/GLAD@GL4.6-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "../x86_64/GLAD@GL4.6-native-bindings"; // mac
#else
        public const string LIBRARY = "../x86_64/GLAD@GL4.6-native-bindings";
#endif
        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GLProcAddressLoader(IntPtr procname);

        #endregion

        #region Private Functions

        [DllImport(LIBRARY, EntryPoint = "cobalt_glad_gl_get_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetStringImpl(EPropertyName name);

        #endregion

        [DllImport(LIBRARY, EntryPoint = "cobalt_glad_load_gl_proc_address", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LoadGLProcAddress(GLProcAddressLoader func);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glad_gl_clear_color", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ClearColor(float r, float g, float b, float a);

        public static string GetString(EPropertyName name)
        {
            return Util.PtrToStringUTF8(GetStringImpl(name));
        }
    }
}
