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

layout (location = 0) out VertexData
{
    vec3 position;
    vec2 texcoord0;
    flat int instanceID;
} outData;

void main()
{
    int instance = gl_InstanceID;
    outData.instanceID = instance;

    ObjectData myData = objects[instance];

    outData.position = iPosition;
    outData.texcoord0 = iTexCoord0;

    gl_Position = myData.transform * vec4(iPosition, 1);
}