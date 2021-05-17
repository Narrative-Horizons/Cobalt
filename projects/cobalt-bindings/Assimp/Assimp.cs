using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Cobalt.Bindings.Assimp
{
    public static class Assimp
    {
        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "bin/Assimp@5.0.0-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/Assimp@5.0.0-native-bindings";
#else
        public const string LIBRARY = "bin/Assimp@5.0.0-native-bindings";
#endif
        #endregion

        [DllImport(LIBRARY, EntryPoint = "assimp_test", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AssimpTest();
    }
}
