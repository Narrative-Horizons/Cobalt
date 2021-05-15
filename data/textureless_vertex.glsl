#version 460

vec4 gl_Position;

layout (location=0) in vec3 position;

layout (std140, binding=0) uniform Matrices
{
    mat4 projection;
    mat4 view;
    mat4 model;
};

void main()
{
    gl_Position = projection * view * model * vec4(position, 1);
}