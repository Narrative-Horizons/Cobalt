#version 460

vec4 gl_Position;

layout (location=0) in vec3 position;
layout (location=1) in vec2 uv;
layout (location=0) out vec2 iUV;

void main()
{
    iUV = uv;
    gl_Position = vec4(position, 1);
}