#version 460
#extension GL_ARB_bindless_texture : enable

layout (local_size_x = 1) in;

layout (rg32ui, location = 3, bindless_image) uniform uimage2D pickingImage;

layout(std430, binding = 0) buffer PickingData
{
    int mx;
    int my;

    uint identifier;
    uint generation;
};

void main()
{
    uvec2 color = imageLoad(pickingImage, ivec2(mx, my)).xy;

    identifier = color.x;
    generation = color.y;
}
