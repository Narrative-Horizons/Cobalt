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

    vec3 sunDirection;
    vec3 sunColor;

    uint aBufferCapacity;
};

void main()
{
    int instance = gl_InstanceID + gl_BaseInstance;
    ObjectData myData = objects[instance];
    gl_Position = projection * view * myData.transform * vec4(iPosition, 1);
}