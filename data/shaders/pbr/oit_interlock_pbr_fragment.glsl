#version 460

#extension GL_ARB_bindless_texture : enable
#extension GL_ARB_fragment_shader_interlock : enable

#define MAX_TEX_COUNT 500
layout(location = 2, bindless_sampler) uniform sampler2D texArray[MAX_TEX_COUNT];

const float PI = 3.14159265359;

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

layout(std140, binding = 3) uniform SceneData
{
    mat4 view;
    mat4 projection;
    mat4 viewProjection;

    vec3 cameraPosition;
    vec3 cameraDirection;

    vec3 sunDirection;
    vec3 sunColor;
};

layout (location = 0) in VertexData
{
    vec3 position;
    vec3 normal;
    vec3 tangent;
    vec3 binormal;
    vec2 texcoord0;

    vec3 worldPos;
    vec3 fragPosition;
    flat int instanceID;
} inData;

layout(location = 0) out vec4 FragColor;

void main()
{
    
}