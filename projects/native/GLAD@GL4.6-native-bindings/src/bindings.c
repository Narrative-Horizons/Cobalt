#include <glad/glad.h>

#include <stdio.h>

#define GLAD_BINDING_EXPORT __declspec(dllexport)

GLAD_BINDING_EXPORT int cobalt_glad_load_gl_proc_address(void* loader_func)
{
	return gladLoadGLLoader(loader_func);
}

GLAD_BINDING_EXPORT void cobalt_glad_gl_clear_color(float r, float g, float b, float a)
{
	glClearColor(r, g, b, a);
}

GLAD_BINDING_EXPORT const unsigned char* cobalt_glad_gl_get_string(int name)
{
	return glGetString(name);
}
