#version 460
#extension GL_ARB_bindless_texture : enable

layout (location = 0) in vec2 iUV;
layout(location = 0, bindless_sampler) uniform sampler2D tex;

layout(location = 0) out vec4 FragColor;

void main()
{
    vec4 color = texture(tex, iUV);
    FragColor = color;
    //FragColor = vec4(1, 1, 1, 1);
}