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

layout(std140, binding = 3) uniform SceneData
{
    mat4 view;
    mat4 projection;
    mat4 viewProjection;
    mat4 skyProjection;

    vec3 cameraPosition;
    vec3 cameraDirection;

    vec3 sunDirection;
    vec3 sunColor;
};

layout (location = 0) out VertexData
{
    vec3 position;
    vec2 texcoord;
    mat3 tangentBasis;

    flat int instanceID;
} vout;

void main()
{
    int instance = gl_InstanceID + gl_BaseInstance;
    outData.instanceID = instance;

    ObjectData myData = objects[instance];

    vout.position = vec3(myData.transform * vec4(iPosition, 1.0));
    vout.texcoord = iTexCoord0;

    vout.tangentBasis = mat3(myData.transform) * mat3(iTangent, iBinormal, iNormal);

    gl_Position = viewProjection * myData.transform * vec4(iPosition, 1.0);
}