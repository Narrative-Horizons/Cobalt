#version 460
#extension GL_ARB_bindless_texture : enable

#define MAX_TEX_COUNT 500
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

layout(std140, binding = 2) uniform SceneData
{
    mat4 view;
    mat4 projection;
    mat4 viewProjection;

    vec3 cameraPosition;
    vec3 cameraDirection;

    vec3 sunDirection;
    vec3 sunColor;

    uint aBufferCapacity;
};

layout(location = 0) out vec4 FragColor;

void main()
{
    FragColor = vec4(1.0);
}