#include <glad/glad.h>

#include <stdio.h>
#include <stdbool.h>

#define GLAD_BINDING_EXPORT __declspec(dllexport)

GLAD_BINDING_EXPORT int cobalt_glad_load_gl_proc_address(GLADloadproc loader_func)
{
	int r = gladLoadGLLoader(loader_func);

	glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);
	//glDebugMessageCallback(debugCallback, NULL);
	glEnable(GL_DEBUG_OUTPUT);

	return r;
}

GLAD_BINDING_EXPORT void cobalt_glad_gl_clear_color(float r, float g, float b, float a)
{
	glClearColor(r, g, b, a);
}

GLAD_BINDING_EXPORT void cobalt_glad_gl_clear(unsigned int mask)
{
	glClear(mask);
}

GLAD_BINDING_EXPORT void cobalt_gl_enable(unsigned int mask)
{
	glEnable(mask);
}

GLAD_BINDING_EXPORT void cobalt_gl_generate_texture_mipmap(unsigned int texture)
{
	glGenerateTextureMipmap(texture);
}

GLAD_BINDING_EXPORT void cobalt_gl_blend_mode(unsigned int srcMode, unsigned int dstMode)
{
	glBlendFunc(srcMode, dstMode);
}

GLAD_BINDING_EXPORT void cobalt_gl_depth_mask(unsigned int flag)
{
	glDepthMask(flag);
}

GLAD_BINDING_EXPORT void cobalt_gl_disable(unsigned int mask)
{
	glDisable(mask);
}

GLAD_BINDING_EXPORT void cobalt_gl_polygon_mode(unsigned int face, unsigned int mode)
{
	glPolygonMode(face, mode);
}

GLAD_BINDING_EXPORT void cobalt_gl_cull_face(unsigned int mode)
{
	glCullFace(mode);
}

GLAD_BINDING_EXPORT void cobalt_gl_front_face(unsigned int mode)
{
	glFrontFace(mode);
}

GLAD_BINDING_EXPORT void cobalt_gl_draw_elements(unsigned int mode, unsigned int count, unsigned int type, const void* indices)
{
	glDrawElements(mode, count, type, indices);
}

typedef void(*DebugProc)(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* message, const void* userParam);

static DebugProc _internalProc;

static void debugCallback(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* message, const void* userParam)
{
	_internalProc(source, type, id, severity, length, message, userParam);
}

GLAD_BINDING_EXPORT void cobalt_gl_debug_message_callback(DebugProc callback, const void* userParam)
{
	_internalProc = callback;

	glDebugMessageCallback(debugCallback, userParam);
}

GLAD_BINDING_EXPORT const unsigned char* cobalt_glad_gl_get_string(int name)
{
	return glGetString(name);
}

GLAD_BINDING_EXPORT void cobalt_gl_finish()
{
	glFinish();
}

GLAD_BINDING_EXPORT void cobalt_gl_create_textures(unsigned int target, unsigned int amount, unsigned int* textures)
{
	glCreateTextures(target, amount, textures);
}

GLAD_BINDING_EXPORT void cobalt_gl_create_buffers(unsigned int amount, unsigned int* buffers)
{
	glCreateBuffers(amount, buffers);
}

GLAD_BINDING_EXPORT void cobalt_gl_create_frame_buffers(unsigned int amount, unsigned int* buffers)
{
	glCreateFramebuffers(amount, buffers);
}

GLAD_BINDING_EXPORT unsigned int cobalt_gl_check_named_frame_buffer_status(unsigned int buffer, unsigned int target)
{
	return glCheckNamedFramebufferStatus(buffer, target);
}

GLAD_BINDING_EXPORT void cobalt_gl_gen_textures(unsigned int amount, unsigned int* textures)
{
	glGenTextures(amount, textures);
}

GLAD_BINDING_EXPORT void cobalt_gl_named_buffer_storage(unsigned int buffer, long long int size, const void* data, unsigned int flags)
{
	glNamedBufferStorage(buffer, size, data, flags);
}

GLAD_BINDING_EXPORT void cobalt_gl_named_frame_buffer_texture(unsigned int buffer, unsigned int attachment, unsigned int texture, int level)
{
	glNamedFramebufferTexture(buffer, attachment, texture, level);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_buffers(unsigned int amount, unsigned int* buffers)
{
	glDeleteBuffers(amount, buffers);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_frame_buffers(unsigned int amount, unsigned int* buffers)
{
	glDeleteFramebuffers(amount, buffers);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_textures(unsigned int amount, unsigned int* textures)
{
	glDeleteTextures(amount, textures);
}

GLAD_BINDING_EXPORT void* cobalt_gl_map_named_buffer(unsigned int buffer, unsigned int access)
{
	return glMapNamedBuffer(buffer, access);
}

GLAD_BINDING_EXPORT void cobalt_gl_bind_frame_buffer(unsigned int target, unsigned int buffer)
{
	glBindFramebuffer(target, buffer);
}

GLAD_BINDING_EXPORT void* cobalt_gl_map_named_buffer_range(unsigned int buffer, long long int offset, long long int length, unsigned int access)
{
	return glMapNamedBufferRange(buffer, offset, length, access);
}

GLAD_BINDING_EXPORT void cobalt_gl_unmap_named_buffer(unsigned int buffer)
{
	glUnmapNamedBuffer(buffer);
}

GLAD_BINDING_EXPORT void cobalt_gl_texture_storage_2D(unsigned int texture, int levels, unsigned int internalFormat, int width, int height)
{
	glTextureStorage2D(texture, levels, internalFormat, width, height);
}

GLAD_BINDING_EXPORT void cobalt_gl_texture_view(unsigned int texture, unsigned int target, unsigned int origtexture, unsigned int internalFormat, unsigned int minLevel, unsigned int numLevels, unsigned int minLayer, unsigned int numLayers)
{
	glTextureView(texture, target, origtexture, internalFormat, minLevel, numLevels, minLayer, numLayers);
}

GLAD_BINDING_EXPORT unsigned int cobalt_gl_create_program()
{
	return glCreateProgram();
}

GLAD_BINDING_EXPORT unsigned int cobalt_gl_create_shader(unsigned int type)
{
	return glCreateShader(type);
}

GLAD_BINDING_EXPORT void cobalt_gl_attach_shader(unsigned int program, unsigned int shader)
{
	glAttachShader(program, shader);
}

GLAD_BINDING_EXPORT void cobalt_gl_detach_shader(unsigned int program, unsigned int shader)
{
	glDetachShader(program, shader);
}

GLAD_BINDING_EXPORT void cobalt_gl_shader_source(unsigned int shader, int count, const char** string, int* length)
{
	glShaderSource(shader, count, string, length);
}

GLAD_BINDING_EXPORT void cobalt_gl_compile_shader(unsigned int shader)
{
	glCompileShader(shader);
}

GLAD_BINDING_EXPORT void cobalt_gl_get_shader_iv(unsigned int shader, unsigned int name, int* params)
{
	glGetShaderiv(shader, name, params);
}

GLAD_BINDING_EXPORT void cobalt_gl_get_shader_info_log(unsigned int shader, int maxLength, int* length, char* infoLog)
{
	glGetShaderInfoLog(shader, maxLength, length, infoLog);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_shader(unsigned int shader)
{
	glDeleteShader(shader);
}

GLAD_BINDING_EXPORT void cobalt_gl_link_program(unsigned int program)
{
	glLinkProgram(program);
}

GLAD_BINDING_EXPORT void cobalt_gl_get_program_iv(unsigned int program, unsigned int name, int* params)
{
	glGetProgramiv(program, name, params);
}

GLAD_BINDING_EXPORT void cobalt_gl_get_program_info_log(unsigned int program, int maxLength, int* length, char* infoLog)
{
glGetProgramInfoLog(program, maxLength, length, infoLog);
}

GLAD_BINDING_EXPORT void cobalt_gl_validate_program(unsigned int program)
{
	glValidateProgram(program);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_program(unsigned int program)
{
	glDeleteProgram(program);
}

GLAD_BINDING_EXPORT void cobalt_gl_create_vertex_arrays(int amount, unsigned int* arrays)
{
	glCreateVertexArrays(amount, arrays);
}

GLAD_BINDING_EXPORT void cobalt_gl_vertex_array_vertex_buffer(unsigned int vaobj, unsigned int bindingIndex, unsigned int buffer, long long int offset, int stride)
{
	glVertexArrayVertexBuffer(vaobj, bindingIndex, buffer, offset, stride);
}

GLAD_BINDING_EXPORT void cobalt_gl_enable_vertex_array_attrib(unsigned int vaobj, unsigned int index)
{
	glEnableVertexArrayAttrib(vaobj, index);
}

GLAD_BINDING_EXPORT void cobalt_gl_disable_vertex_array_attrib(unsigned int vaobj, unsigned int index)
{
	glDisableVertexArrayAttrib(vaobj, index);
}

GLAD_BINDING_EXPORT void cobalt_gl_vertex_array_attrib_format(unsigned int vaobj, unsigned int attribIndex, int size, unsigned int type, bool normalized, unsigned int relativeOffset)
{
	glVertexArrayAttribFormat(vaobj, attribIndex, size, type, normalized ? GL_TRUE : GL_FALSE, relativeOffset);
}

GLAD_BINDING_EXPORT void cobalt_gl_vertex_array_attrib_binding(unsigned int vaobj, unsigned int attribIndex, unsigned int bindingIndex)
{
	glVertexArrayAttribBinding(vaobj, attribIndex, bindingIndex);
}

GLAD_BINDING_EXPORT void cobalt_gl_vertex_array_element_buffer(unsigned int vaobj, unsigned int buffer)
{
	glVertexArrayElementBuffer(vaobj, buffer);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_vertex_arrays(int amount, const unsigned int* arrays)
{
	glDeleteVertexArrays(amount, arrays);
}

GLAD_BINDING_EXPORT void cobalt_gl_use_program(unsigned int program)
{
	glUseProgram(program);
}

GLAD_BINDING_EXPORT void cobalt_gl_bind_vertex_array(unsigned int varray)
{
	glBindVertexArray(varray);
}

GLAD_BINDING_EXPORT void cobalt_gl_draw_elements_instanced_base_vertex_base_instance(unsigned int mode, int count, unsigned int type, void* indices, int instanceCount, int baseVertex, unsigned int baseInstance)
{
	glDrawElementsInstancedBaseVertexBaseInstance(mode, count, type, indices, instanceCount, baseVertex, baseInstance);
}

GLAD_BINDING_EXPORT void cobalt_gl_multi_draw_elements_indirect(unsigned int mode, unsigned int type, const void* indirect, int drawCount, int stride)
{
	glMultiDrawElementsIndirect(mode, type, indirect, drawCount, stride);
}

GLAD_BINDING_EXPORT void cobalt_gl_draw_arrays_instanced_base_instance(unsigned int mode, int first, int count, int instanceCount, unsigned int baseInstance)
{
	glDrawArraysInstancedBaseInstance(mode, first, count, instanceCount, baseInstance);
}

GLAD_BINDING_EXPORT void cobalt_gl_clear_named_framebuffer_fv(unsigned int framebuffer, unsigned int buffer, int drawbuffer, const float* value)
{
	glClearNamedFramebufferfv(framebuffer, buffer, drawbuffer, value);
}

GLAD_BINDING_EXPORT void cobalt_gl_clear_named_framebuffer_uiv(unsigned int framebuffer, unsigned int buffer, int drawbuffer, const unsigned int* value)
{
	glClearNamedFramebufferuiv(framebuffer, buffer, drawbuffer, value);
}

GLAD_BINDING_EXPORT void cobalt_gl_clear_named_framebuffer_fi(unsigned int framebuffer, unsigned int buffer, int drawbuffer, const float depth, const float stencil)
{
	glClearNamedFramebufferfi(framebuffer, buffer, drawbuffer, depth, stencil);
}

GLAD_BINDING_EXPORT void cobalt_gl_create_samplers(unsigned int count, unsigned int* samplers)
{
	glCreateSamplers(count, samplers);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_samplers(unsigned int count, const unsigned int* samplers)
{
	glDeleteSamplers(count, samplers);
}

GLAD_BINDING_EXPORT void cobalt_gl_sampler_parameter_i(unsigned int sampler, unsigned int pname, int param)
{
	glSamplerParameteri(sampler, pname, param);
}

GLAD_BINDING_EXPORT void cobalt_gl_sampler_parameter_f(unsigned int sampler, unsigned int pname, float param)
{
	glSamplerParameterf(sampler, pname, param);
}

GLAD_BINDING_EXPORT void cobalt_gl_texture_storage_1d(unsigned int texture, int levels, unsigned int internalFormat, int width)
{
	glTextureStorage1D(texture, levels, internalFormat, width);
}

GLAD_BINDING_EXPORT void cobalt_gl_texture_storage_2d(unsigned int texture, int levels, unsigned int internalFormat, int width, int height)
{
	glTextureStorage2D(texture, levels, internalFormat, width, height);
}

GLAD_BINDING_EXPORT void cobalt_gl_texture_storage_2d_multisample(unsigned int texture, unsigned int samples, unsigned int internalFormat, unsigned int width, unsigned int height, unsigned int fixedSampleLocations)
{
	glTextureStorage2DMultisample(texture, samples, internalFormat, width, height, fixedSampleLocations ? GL_TRUE : GL_FALSE);
}

GLAD_BINDING_EXPORT void cobalt_gl_texture_storage_3d(unsigned int texture, int levels, unsigned int internalFormat, int width, int height, int depth)
{
	glTextureStorage3D(texture, levels, internalFormat, width, height, depth);
}

GLAD_BINDING_EXPORT void cobalt_gl_texture_storage_3d_multisample(unsigned int texture, unsigned int samples, unsigned int internalFormat, unsigned int width, unsigned int height, unsigned int depth, unsigned int fixedSampleLocations)
{
	glTextureStorage3DMultisample(texture, samples, internalFormat, width, height, depth, fixedSampleLocations ? GL_TRUE : GL_FALSE);
}

GLAD_BINDING_EXPORT void cobalt_gl_make_texture_handle_resident_ARB(unsigned long handle)
{
	glMakeTextureHandleResidentARB(handle);
}

GLAD_BINDING_EXPORT void cobalt_gl_uniform_handle_ui64v_ARB(int location, int count, const unsigned long long* value)
{
	glUniformHandleui64vARB(location, count, value);
}

GLAD_BINDING_EXPORT unsigned long long cobalt_gl_get_texture_sampler_handle_ARB(unsigned int texture, unsigned int sampler)
{
	return glGetTextureSamplerHandleARB(texture, sampler);
}

GLAD_BINDING_EXPORT void cobalt_gl_texture_sub_image_2D(unsigned int texture, int level, int xoffset, int yoffset, int width, int height, unsigned int format, unsigned int type, const void* pixels)
{
	glTextureSubImage2D(texture, level, xoffset, yoffset, width, height, format, type, pixels);
}

GLAD_BINDING_EXPORT void cobalt_gl_bind_buffer_range(unsigned int target, unsigned int index, unsigned int buffer, int offset, int size)
{
	glBindBufferRange(target, index, buffer, offset, size);
}

GLAD_BINDING_EXPORT void cobalt_gl_bind_buffer(unsigned int target, unsigned int index)
{
	glBindBuffer(target, index);
}

GLAD_BINDING_EXPORT unsigned int cobalt_gl_get_error()
{
	return glGetError();
}

GLAD_BINDING_EXPORT void* cobalt_gl_fence_sync() {
	const GLsync sync = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
	return sync;
}

GLAD_BINDING_EXPORT void cobalt_gl_client_wait_sync(void* sync)
{
	glClientWaitSync((GLsync)sync, GL_SYNC_FLUSH_COMMANDS_BIT, GL_TIMEOUT_IGNORED);
}

GLAD_BINDING_EXPORT void cobalt_gl_wait_sync(void* sync)
{
	glWaitSync((GLsync)sync, 0, GL_TIMEOUT_IGNORED);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_sync(void* sync)
{
	glDeleteSync(sync);
}

GLAD_BINDING_EXPORT void cobalt_gl_depth_func(unsigned int func)
{
	glDepthFunc(func);
}

GLAD_BINDING_EXPORT void cobalt_gl_memory_barrier(unsigned int barrier)
{
	glMemoryBarrier(barrier);
}

GLAD_BINDING_EXPORT void cobalt_gl_dispatch_compute(unsigned int x, unsigned int y, unsigned int z)
{
	glDispatchCompute(x, y, z);
}

GLAD_BINDING_EXPORT void cobalt_gl_blend_equation_i(unsigned int buffer, unsigned int color, unsigned int alpha)
{
	glBlendEquationSeparatei(buffer, color, alpha);
}

GLAD_BINDING_EXPORT void cobalt_gl_blend_func_i(unsigned int buffer, unsigned int colorSrc, unsigned int colorDst, unsigned int alphaSrc, unsigned int alphaDst)
{
	glBlendFuncSeparatei(buffer, colorSrc, colorDst, alphaSrc, alphaDst);
}