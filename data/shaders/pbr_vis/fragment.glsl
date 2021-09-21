#version 460
#extension GL_ARB_bindless_texture : enable

// 9 bits of draw mask (512 draw calls)
#define DRAW_ID_MASK 9

// 23 bits of triangles (4194304 triangles per draw call)
#define TRIANGLE_ID_MASK 23

#define MAX_TEX_COUNT 500

layout (location = 0) in VertexData
{
    vec2 uv;
} i;

struct ObjectData
{
    mat4 transform;
    uint materialInstanceID;
};

struct DirectionalLight
{
    vec3 direction;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float _padding;
    float intensity;
};

struct MaterialData
{
    uint albedo;
    uint normal;
    uint emission;
    uint ORM;
};

struct FragmentParameters
{
    uint objectID;
    uint fragmentID;
    uint drawCallID;
};

struct Vertex
{
    vec3 position;
    vec2 uv;
    vec3 normal;
    vec3 tangent;
    vec3 binormal;
};

struct DrawCommand
{
    uint count;
    uint instanceCount;
    uint firstIndex;
    uint baseVertex;
    uint baseInstance;
};

layout(std430, binding = 0) buffer ObjectDataBuffer
{
    ObjectData objects[];
};

layout(std430, binding = 1) buffer MaterialDataBuffer
{
    MaterialData materials[];
};

layout(std140, binding = 2) uniform SceneData
{
    mat4 view;
    mat4 projection;
    mat4 viewProjection;

    vec3 cameraPosition;
    vec3 cameraDirection;

    DirectionalLight directionalLighting;
};

layout (binding = 4) uniform VisibilityParameters
{
    uint materialID;
    uint sampleCount;
    ivec2 dimensions;
};

layout (std430, binding = 5) buffer VertexBuffer
{
    Vertex vertices[];
};

layout (std430, binding = 6) buffer IndexBuffer
{
    uint indices[];
};

layout (std430, binding = 7) buffer Commands
{
    DrawCommand draws[];
};

layout (location = 3, bindless_sampler) uniform usampler2DMS visibility;
layout (location = 8, bindless_sampler) uniform sampler2D texArray[MAX_TEX_COUNT];

layout (location = 0) out vec4 fragColor;

FragmentParameters FetchFragment(uvec2 s)
{   
    FragmentParameters parameters;
    parameters.objectID = s.g;

    uint drawData = s.r;
    parameters.fragmentID = drawData >> DRAW_ID_MASK;
    parameters.drawCallID = drawData & (1 << DRAW_ID_MASK - 1);

 
    return parameters;
}

void main(void)
{
    vec2 uv = i.uv;
    ivec2 iuv = ivec2(int(uv.x * dimensions.x), int(uv.y * dimensions.y));
    // TODO: compute each sample
    uvec2 s = texelFetch(visibility, iuv, 0).rg; // just take the first sample for now
    FragmentParameters fragment = FetchFragment(s);

    fragColor = vec4(uv, 0.0, 1.0);
}