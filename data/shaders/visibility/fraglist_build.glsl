#version 430 core
#extension GL_ARB_bindless_texture : enable

#define TILE_SIZE_X 16
#define TILE_SIZE_Y 8
#define NUM_CLOSURES 22
#define SAMPLES_PER_PIXEL 4
#define DRAW_ID_BITS 9
#define TRIANGLE_ID_MASK ((1 << DRAW_ID_BITS) - 1)

struct VisibilityParameters
{
    uint draw;
    uint instance;
    uint triangle;
};

struct Fragment
{
    ivec2 coordinates;
    uint draw;
    uint instance;
    uint material;
};

struct ObjectData
{
    mat4 transform;
    uint materialID;
};

layout (local_size_x = TILE_SIZE_X, local_size_y = TILE_SIZE_Y) in;

layout(std430, binding = 0) buffer Objects
{
    ObjectData objects[];
};

layout (rg32ui, location = 3, bindless_image) uniform uimage2DMS visibility;
layout (std430, binding = 4) buffer Fragments
{
    Fragment[] fragments;
};

void memoryBarrierWithSync()
{
    groupMemoryBarrier();
    barrier();
}

VisibilityParameters ComputeVisibility(uvec2 vis)
{
    VisibilityParameters parameters;
    parameters.draw = vis.r >> DRAW_ID_BITS;
    parameters.instance = vis.g;
    parameters.triangle = vis.r & TRIANGLE_ID_MASK;

    return parameters;
}

void main(void)
{
    const uvec2 pixel = gl_WorkGroupID.xy;
    const uint globalInvocationIndex = uint(pixel.y * imageSize(visibility).x + pixel.x);
    ivec2 ipixel = ivec2(int(pixel.x), int(pixel.y));

    for (int sampleId = 0; sampleId < SAMPLES_PER_PIXEL; ++sampleId)
    {
        uvec2 vis = imageLoad(visibility, ipixel, sampleId).rg;
        if (vis.r == 0xFFFFFFFF && vis.g == 0xFFFFFFFF)
        {
            Fragment frag;
            frag.coordinates = ipixel;
            frag.draw = 0xFFFFFFFF;
            frag.instance = 0xFFFFFFFF;
            frag.material = 0xFFFFFFFF;

            fragments[globalInvocationIndex * SAMPLES_PER_PIXEL + sampleId] = frag;
        }
        else
        {
            VisibilityParameters params = ComputeVisibility(vis);
            uint id = params.instance;
            uint material = objects[id].materialID;
            uint draw = params.draw;

            Fragment frag;
            frag.coordinates = ipixel;
            frag.draw = draw;
            frag.instance = id;
            frag.material = material;

            fragments[globalInvocationIndex * SAMPLES_PER_PIXEL + sampleId] = frag;
        }
    }
}