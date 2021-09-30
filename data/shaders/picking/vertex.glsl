#version 460

in int gl_InstanceID;

layout (location = 0) in vec3 iPosition;
layout (location = 1) in vec2 iTexCoord0;
layout (location = 2) in vec3 iNormal;
layout (location = 3) in vec3 iTangent;
layout (location = 4) in vec3 iBinormal;
layout (location = 5) in vec4 iColor0;

struct ObjectData
{
    mat4 transform;
    uint materialID;
    uint identifier;
    uint generation;
    uint _padding;
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

layout(std430, binding = 0) buffer ObjectDataBuffer
{
    ObjectData objects[];
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

layout (location = 0) out VertexData
{
    flat uint identifier;
    flat uint generation;
} outData;

void main()
{
    int instance = gl_InstanceID + gl_BaseInstance;

    ObjectData myData = objects[instance];

    outData.identifier = myData.identifier;
    outData.generation = myData.generation;

    gl_Position = viewProjection * myData.transform * vec4(iPosition, 1);
}