#version 460

// 9 bits of draw mask (512 draw calls)
#define DRAW_ID_MASK 9

// 23 bits of triangles (4194304 triangles per draw call)
#define TRIANGLE_ID_MASK 23

layout (location = 0) in VertexData
{
    flat uint drawID;
    flat uint instanceID;
} i;

layout (location = 0) out uvec2 visibility;

uint ComputeVisibility(uint drawCallID, int triangleID)
{
    return (uint(triangleID) << DRAW_ID_MASK) | drawCallID;
}

void main(void)
{
    visibility.r = ComputeVisibility(i.drawID, gl_PrimitiveID);
    visibility.g = i.instanceID;
}