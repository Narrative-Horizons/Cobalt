using Cobalt.Bindings.Utils;
using System;
using System.Runtime.InteropServices;

namespace Cobalt.Bindings.GL
{
    public static class GL
    {
        #region DLL Loading
#if COBALT_PLATFORM_WINDOWS
        public const string LIBRARY = "../x86_64/GLAD@GL4.6-native-bindings.dll";
#elif COBALT_PLATFORM_MACOS
        public const string LIBRARY = "../x86_64/GLAD@GL4.6-native-bindings";
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
        private static extern IntPtr GetStringImpl(EStringName name);

        #endregion

        [DllImport(LIBRARY, EntryPoint = "cobalt_glad_load_gl_proc_address", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LoadGLProcAddress(GLProcAddressLoader func);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glad_gl_clear_color", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ClearColor(float r, float g, float b, float a);

        [DllImport(LIBRARY, EntryPoint = "cobalt_glad_gl_clear", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Clear(EClearBufferMask mask);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_create_textures", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateTextures(ETextureTarget target, uint amount, uint[] textures);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_gen_textures", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GenTextures(uint amount, uint[] textures);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_create_buffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateBuffers(uint amount, uint[] buffers);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_named_buffer_storage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void NamedBufferStorage(uint buffer, long size, IntPtr data, EBufferAccessMask flags);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_delete_buffers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteBuffers(uint amount, uint[] buffers);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_delete_textures", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteTextures(uint amount, uint[] textures);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_map_named_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MapNamedBuffer(uint buffer, EBufferAccess access);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_map_named_buffer_range", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MapNamedBufferRange(uint buffer, long offset, long length, EBufferAccess access);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_unmap_named_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnmapNamedBuffer(uint buffer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_texture_storage_2D", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TextureStorage2D(uint texture, int levels, EPixelInternalFormat internalFormat, int width, int height);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_texture_view", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TextureView(uint texture, uint target, uint origTexture, EPixelInternalFormat internalFormat, uint minLevel, uint numLevels, uint minLayer, uint numLayers);

        public static uint CreateBuffers()
        {
            uint[] buffers = new uint[1];
            CreateBuffers(1, buffers);

            return buffers[0];
        }

        public static uint CreateTextures(ETextureTarget target)
        {
            uint[] images = new uint[1];
            CreateTextures(target, 1, images);

            return images[0];
        }

        public static uint GenTextures()
        {
            uint[] images = new uint[1];
            GenTextures(1, images);

            return images[0];
        }

        public static void DeleteBuffers(uint buffer)
        {
            uint[] buffers = { buffer };
            DeleteBuffers(1, buffers);
        }

        public static void DeleteTextures(uint texture)
        {
            uint[] textures = { texture };
            DeleteBuffers(1, textures);
        }

        public static string GetString(EStringName name)
        {
            return Util.PtrToStringUTF8(GetStringImpl(name));
        }
    }
}
