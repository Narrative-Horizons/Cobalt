#include <GLFW/glfw3.h>

#define GLFW_BINDING_EXPORT __declspec(dllexport)

GLFW_BINDING_EXPORT int cobalt_glfw_init()
{
	return glfwInit();
}

GLFW_BINDING_EXPORT void cobalt_glfw_terminate(void)
{
	glfwTerminate();
}

GLFW_BINDING_EXPORT void cobalt_glfw_get_monitor_content_scale(GLFWmonitor* monitor, float* x, float* y)
{
	glfwGetMonitorContentScale(monitor, x, y);
}

GLFW_BINDING_EXPORT void* cobalt_glfw_get_monitor_user_pointer(GLFWmonitor* monitor)
{
	return glfwGetMonitorUserPointer(monitor);
}

GLFW_BINDING_EXPORT void cobalt_glfw_set_monitor_user_pointer(GLFWmonitor* monitor, void* user_pointer)
{
	glfwSetMonitorUserPointer(monitor, user_pointer);
}

GLFW_BINDING_EXPORT void* cobalt_glfw_get_window_user_pointer(GLFWwindow* window)
{
	return glfwGetWindowUserPointer(window);
}

GLFW_BINDING_EXPORT void cobalt_glfw_set_window_user_pointer(GLFWwindow* window, void* user_pointer)
{
	glfwSetWindowUserPointer(window, user_pointer);
}

GLFW_BINDING_EXPORT GLFWwindow* cobalt_glfw_get_current_context(void)
{
	return glfwGetCurrentContext();
}

GLFW_BINDING_EXPORT GLFWmonitor* cobalt_glfw_get_primary_monitor(void)
{
	return glfwGetPrimaryMonitor();
}

GLFW_BINDING_EXPORT void cobalt_glfw_maximize_window(GLFWwindow* window)
{
	glfwMaximizeWindow(window);
}

GLFW_BINDING_EXPORT const char* cobalt_glfw_get_version_string(void)
{
	return glfwGetVersionString();
}

GLFW_BINDING_EXPORT double cobalt_glfw_get_time(void)
{
	return glfwGetTime();
}

GLFW_BINDING_EXPORT void cobalt_glfw_set_time(double time)
{
	glfwSetTime(time);
}

GLFW_BINDING_EXPORT uint64_t cobalt_glfw_get_timer_frequency()
{
	return glfwGetTimerFrequency();
}

GLFW_BINDING_EXPORT uint64_t cobalt_get_time_value()
{
	return glfwGetTimerValue();
}

GLFW_BINDING_EXPORT void cobalt_glfw_get_version(int32_t* major, int32_t* minor, int32_t* revision)
{
	glfwGetVersion(major, minor, revision);
}

GLFW_BINDING_EXPORT void cobalt_glfw_swap_interval(int32_t interval)
{
	glfwSwapInterval(interval);
}

GLFW_BINDING_EXPORT const GLFWvidmode* cobalt_glfw_get_video_mode(GLFWmonitor* monitor)
{
	return glfwGetVideoMode(monitor);
}

GLFW_BINDING_EXPORT const GLFWvidmode* cobalt_glfw_get_video_modes(GLFWmonitor* monitor, int32_t* count)
{
	return glfwGetVideoModes(monitor, count);
}

GLFW_BINDING_EXPORT const GLFWmonitor* cobalt_glfw_get_window_monitor(GLFWwindow* window)
{
	return glfwGetWindowMonitor(window);
}

GLFW_BINDING_EXPORT void cobalt_glfw_set_window_monitor(GLFWwindow* window, GLFWmonitor* monitor, int32_t x, int32_t y, int32_t width, int32_t height, int32_t refresh)
{
	glfwSetWindowMonitor(window, monitor, x, y, width, height, refresh);
}

GLFW_BINDING_EXPORT void cobalt_glfw_get_cursor_pos(GLFWwindow* window, double* x, double* y)
{
	glfwGetCursorPos(window, x, y);
}

GLFW_BINDING_EXPORT void cobalt_glfw_set_cursor_pos(GLFWwindow* window, double x, double y)
{
	glfwSetCursorPos(window, x, y);
}

GLFW_BINDING_EXPORT void cobalt_glfw_window_hint(int32_t hint, int32_t value)
{
	glfwWindowHint(hint, value);
}

GLFW_BINDING_EXPORT int cobalt_glfw_window_should_close(GLFWwindow* window)
{
	return glfwWindowShouldClose(window);
}

GLFW_BINDING_EXPORT void cobalt_glfw_set_window_should_close(GLFWwindow* window, int should_close)
{
	glfwSetWindowShouldClose(window, should_close != 0 ? GLFW_TRUE : GLFW_FALSE);
}

GLFW_BINDING_EXPORT void cobalt_glfw_make_context_current(GLFWwindow* window)
{
	glfwMakeContextCurrent(window);
}

GLFW_BINDING_EXPORT float cobalt_glfw_get_window_opacity(GLFWwindow* window)
{
	return glfwGetWindowOpacity(window);
}

GLFW_BINDING_EXPORT void cobalt_glfw_set_window_opacity(GLFWwindow* window, float opacity)
{
	glfwSetWindowOpacity(window, opacity);
}

GLFW_BINDING_EXPORT void cobalt_glfw_get_monitor_workarea(GLFWmonitor* monitor, int32_t* x, int32_t* y, int32_t* width, int32_t* height)
{
	glfwGetMonitorWorkarea(monitor, x, y, width, height);
}

GLFW_BINDING_EXPORT GLFWwindow* cobalt_glfw_create_window(int width, int height, const char* title, GLFWmonitor* monitor, GLFWwindow* share)
{
	return glfwCreateWindow(width, height, title, monitor, share);
}

GLFW_BINDING_EXPORT void cobalt_glfw_set_window_title(GLFWwindow* window, const char* title)
{
	glfwSetWindowTitle(window, title);
}

GLFW_BINDING_EXPORT void cobalt_glfw_show_window(GLFWwindow* window)
{
	glfwShowWindow(window);
}

GLFW_BINDING_EXPORT void cobalt_glfw_swap_buffers(GLFWwindow* window)
{
	glfwSwapBuffers(window);
}

GLFW_BINDING_EXPORT void cobalt_glfw_poll_events(void)
{
	glfwPollEvents();
}

GLFW_BINDING_EXPORT void* cobalt_glfw_get_proc_address(const char* procname)
{
	return glfwGetProcAddress(procname);
}

GLFW_BINDING_EXPORT int32_t cobalt_glfw_get_error(const char** description)
{
	return glfwGetError(description);
}

GLFW_BINDING_EXPORT GLFWerrorfun cobalt_glfw_set_error_callback(GLFWerrorfun fun)
{
	return glfwSetErrorCallback(fun);
}

GLFW_BINDING_EXPORT GLFWwindowsizefun cobalt_glfw_set_window_size_callback(GLFWwindow* window, GLFWwindowsizefun fun)
{
	return glfwSetWindowSizeCallback(window, fun);
}

GLFW_BINDING_EXPORT GLFWcursorposfun cobalt_glfw_set_cursor_pos_callback(GLFWwindow* window, GLFWcursorposfun fun)
{
	return glfwSetCursorPosCallback(window, fun);
}

GLFW_BINDING_EXPORT GLFWmousebuttonfun cobalt_glfw_set_mouse_button_callback(GLFWwindow* window, GLFWmousebuttonfun fun)
{
	return glfwSetMouseButtonCallback(window, fun);
}

GLFW_BINDING_EXPORT GLFWmousebuttonfun cobalt_glfw_set_mouse_scroll_callback(GLFWwindow* window, GLFWscrollfun fun)
{
	return glfwSetScrollCallback(window, fun);
}

GLFW_BINDING_EXPORT GLFWcharfun cobalt_glfw_set_char_callback(GLFWwindow* window, GLFWcharfun fun)
{
	return glfwSetCharCallback(window, fun);
}

GLFW_BINDING_EXPORT GLFWkeyfun cobalt_glfw_set_key_callback(GLFWwindow* window, GLFWkeyfun fun)
{
	return glfwSetKeyCallback(window, fun);
}
