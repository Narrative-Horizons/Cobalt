using System;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.GLSL_Parser
{
    public class GLSLParser
    {
        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "bin/glsl-parser-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/glsl-parser-native-bindings";
#else
        public const string LIBRARY = "bin/glsl-parser-native-bindings";
#endif
        #endregion

        [DllImport(LIBRARY, EntryPoint = "parseSource", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ParseSource(string source, string fileName, uint shaderType);
    }
}
