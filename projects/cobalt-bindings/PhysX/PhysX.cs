using System;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.PhysX
{
    public class PhysX
    {
        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "bin/PhysX@4.1.2-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/PhysX@4.1.2-native-bindings";
#else
        public const string LIBRARY = "bin/PhysX@4.1.2-native-bindings";
#endif
        #endregion

        [DllImport(LIBRARY, EntryPoint = "test", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Test();
    }
}