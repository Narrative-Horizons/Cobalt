#version 460
#extension GL_ARB_bindless_texture : enable

#define MAX_TEX_COUNT 1024
layout(location = 2, bindless_sampler) uniform sampler2D texArray[MAX_TEX_COUNT];

struct ObjectData
{
    mat4 transform;
    uint materialID;
};

struct MaterialData
{
    uint albedo;
    uint normal;
    uint emission;
    uint ORM;
};

layout(std430, binding = 0) buffer ObjectDataBuffer
{
    ObjectData objects[];
};

layout(std430, binding = 1) buffer MaterialDataBuffer
{
    MaterialData materials[];
};

layout (location = 0) in VertexData
{
    vec3 position;
    vec2 texcoord0;
    flat int instanceID;
} inData;

layout(location = 0) out vec4 FragColor;

void main()
{
    int instance = inData.instanceID;

    ObjectData myData = objects[instance];
    MaterialData myMaterial = materials[myData.materialID];

    FragColor = vec4(1, 0, 1, 1);
}