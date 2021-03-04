﻿using Cobalt.Bindings.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

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

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_create_program", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint CreateProgram();

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_create_shader", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint CreateShader(EShaderType type);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_attach_shader", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AttachShader(uint program, uint shader);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_detach_shader", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DetachShader(uint program, uint shader);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_shader_source", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShaderSource(uint shader, int count, string[] strings, int[] length);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_compile_shader", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CompileShader(uint shader);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_get_shader_iv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetShaderiv(uint shader, uint name, int[] param);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_get_shader_info_log", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetShaderInfoLog(uint shader, int maxLength, IntPtr length, StringBuilder infoLog);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_delete_shader", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteShader(uint shader);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_link_program", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LinkProgram(uint program);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_get_program_iv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetProgramiv(uint program, uint name, int[] param);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_get_program_info_log", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetProgramInfoLog(uint program, int maxLength, IntPtr length, StringBuilder infoLog);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_validate_program", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ValidateProgram(uint program);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_delete_program", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteProgram(uint program);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_create_vertex_arrays", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CreateVertexArrays(int amount, uint[] arrays);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_vertex_array_vertex_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VertexArrayVertexBuffer(uint vaobj, uint bindingIndex, uint buffer, IntPtr offset, int stride);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_enable_vertex_array_attrib", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnableVertexArrayAttrib(uint vaobj, uint index);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_disable_vertex_array_attrib", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DisableVertexArrayAttrib(uint vaobj, uint index);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_vertex_array_attrib_format", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VertexArrayAttribFormat(uint vaobj, uint attribIndex, int size, EVertexAttribFormat type, bool normalized, uint relativeOffset);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_vertex_array_attrib_binding", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VertexArrayAttribBinding(uint vaobj, uint attribIndex, uint bindingIndex);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_vertex_array_element_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VertexArrayElementBuffer(uint vaobj, uint buffer);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_delete_vertex_arrays", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteVertexArrays(int amount, uint[] arrays);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_use_program", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UseProgram(uint program);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_bind_vertex_array", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BindVertexArray(uint varray);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_draw_elements_instanced_base_vertex_base_instance", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DrawElementsInstancedBaseVertexBaseInstance(EBeginMode mode, int count, EDrawElementsType type, IntPtr indices, int instanceCount, int baseVertex, uint baseInstance);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_draw_arrays_instanced_base_instance", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DrawArraysInstancedBaseInstance(EBeginMode mode, int first, int count, int instanceCount, uint baseInstance);

        [DllImport(LIBRARY, EntryPoint = "cobalt_gl_clear_named_framebuffer_fv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ClearNamedFramebufferfv(uint framebuffer, EClearBuffer buffer, int drawbuffer, IntPtr value);

        public static void NamedBufferStorageTyped<T>(uint buffer, long size, [NotNull] [In, Out] T[] data, EBufferAccessMask flags) where T : unmanaged
        {
            unsafe
            {
                fixed (T* objPtr = &data[0])
                {
                    NamedBufferStorage(buffer, size, (IntPtr)objPtr, flags);
                }
            }
        }

        public static uint CreateVertexArrays()
        {
            uint[] arrays = new uint[] { 0 };
            CreateVertexArrays(1, arrays);

            return arrays[0];
        }

        public static void DeleteVertexArrays(uint array)
        {
            uint[] arrays = new uint[] { array };
            DeleteVertexArrays(1, arrays);
        }

        public static string GetShaderInfoLog(uint shader)
        {
            GetShaderiv(shader, EShaderParameter.InfoLogLength, out int size);

            StringBuilder builder = new StringBuilder(size);
            GetShaderInfoLog(shader, size, IntPtr.Zero, builder);

            return builder.ToString();
        }

        public static string GetProgramInfoLog(uint program)
        {
            GetProgramiv(program, EProgramParameter.InfoLogLength, out int size);

            StringBuilder builder = new StringBuilder(size);
            GetProgramInfoLog(program, size, IntPtr.Zero, builder);

            return builder.ToString();
        }

        public static void GetShaderiv(uint shader, EShaderParameter name, out int param)
        {
            int[] parameters = new int[] { 0 }; 
            GetShaderiv(shader, (uint)name, parameters);

            param = parameters[0];
        }

        public static void GetProgramiv(uint program, EProgramParameter name, out int param)
        {
            int[] parameters = new int[] { 0 };
            GetProgramiv(program, (uint)name, parameters);

            param = parameters[0];
        }

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

        public static void ShaderSource(uint shader, string contents)
        {
            string[] sources = new string[] { contents };
            int[] lengths = new int[] { contents.Length };

            ShaderSource(shader, 1, sources, lengths);
        }

        public static string GetString(EStringName name)
        {
            return Util.PtrToStringUTF8(GetStringImpl(name));
        }
    }
}
