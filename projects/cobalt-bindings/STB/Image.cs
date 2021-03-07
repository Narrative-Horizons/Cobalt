using System;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.STB
{
    public class ImageLoader
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ImagePayload
        {
            public IntPtr sdr_ub_image;
            public IntPtr sdr_us_image;
            public IntPtr hdr_f_image;
            public int width;
            public int height;
            public int channels;
        }

        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "bin/stb@b42009b-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "bin/stb@b42009b-native-bindings";
#else
        public const string LIBRARY = "bin/stb@b42009b-native-bindings";
#endif
        #endregion

        [DllImport(LIBRARY, EntryPoint = "cobalt_load_image", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LoadImage(string filename, out ImagePayload payload);

        [DllImport(LIBRARY, EntryPoint = "cobalt_release_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReleaseImage(ref ImagePayload payload);

        public static ImagePayload LoadImage(string filename)
        {
            ImagePayload payload;
            bool success = LoadImage(filename, out payload);
            if (success == false)
            {
                Console.WriteLine("Failed to load image: " + filename);
            }
            return payload;
        }
    }
}
