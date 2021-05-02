#version 460

vec4 gl_Position;

layout (location=0) in vec3 position;
layout (location=1) in vec2 uv;
layout (location=0) out vec2 iUV;

layout (std140, binding=0) uniform Matrices
{
    mat4 projection;
    mat4 view;
    mat4 model;
};

void main()
{
    iUV = uv;
    gl_Position = projection * view * model * vec4(position, 1);
}