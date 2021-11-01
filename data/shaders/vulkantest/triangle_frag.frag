#version 450

layout(location = 0) in vec3 fragColor;

layout(location = 0) out vec4 outColor;

layout(binding = 0) uniform ColorBlock
{
    vec4 color;
} blocks;

void main() {
    outColor = blocks.color;
}