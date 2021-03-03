#include <glad/glad.h>

#include <stdio.h>

#define GLAD_BINDING_EXPORT __declspec(dllexport)

GLAD_BINDING_EXPORT int cobalt_glad_load_gl_proc_address(GLADloadproc loader_func)
{
	return gladLoadGLLoader(loader_func);
}

GLAD_BINDING_EXPORT void cobalt_glad_gl_clear_color(float r, float g, float b, float a)
{
	glClearColor(r, g, b, a);
}

GLAD_BINDING_EXPORT void cobalt_glad_gl_clear(unsigned int mask)
{
	glClear(mask);
}

GLAD_BINDING_EXPORT const unsigned char* cobalt_glad_gl_get_string(int name)
{
	return glGetString(name);
}

GLAD_BINDING_EXPORT void cobalt_gl_create_textures(unsigned int target, unsigned int amount, unsigned int* textures)
{
	glCreateTextures(target, amount, textures);
}

GLAD_BINDING_EXPORT void cobalt_gl_create_buffers(unsigned int amount, unsigned int* buffers)
{
	glCreateBuffers(amount, buffers);
}

GLAD_BINDING_EXPORT void cobalt_gl_gen_textures(unsigned int amount, unsigned int* textures)
{
	glGenTextures(amount, textures);
}

GLAD_BINDING_EXPORT void cobalt_gl_named_buffer_storage(unsigned int buffer, long long int size, const void* data, unsigned int flags)
{
	glNamedBufferStorage(buffer, size, data, flags);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_buffers(unsigned int amount, unsigned int* buffers)
{
	glDeleteBuffers(amount, buffers);
}

GLAD_BINDING_EXPORT void cobalt_gl_delete_textures(unsigned int amount, unsigned int* textures)
{
	glDeleteTextures(amount, textures);
}

GLAD_BINDING_EXPORT void* cobalt_gl_map_named_buffer(unsigned int buffer, unsigned int access)
{
	return glMapNamedBuffer(buffer, access);
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