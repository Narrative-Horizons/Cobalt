#version 450

layout (location = 0) out vec4 resolve;

layout (input_attachment = 0, set = 0, binding = 0) uniform subpassInput color;

void main(void)
{
    resolve = subpassLoad(color);
}