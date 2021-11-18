#version 460

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

layout(std430, set = 1, binding = 0) buffer ObjectDataBuffer
{
    ObjectData objects[];
};

layout(std140, set = 0, binding = 0) uniform SceneData
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
    vec3 position;
    vec3 normal;
    vec3 tangent;
    vec3 binormal;
    vec2 texcoord0;

    vec3 worldPos;
    vec3 fragPosition;
    flat int instanceID;
} outData;

void main()
{
    int instance = gl_InstanceIndex + gl_BaseInstance;
    outData.instanceID = instance;

    ObjectData myData = objects[instance];

    outData.position = iPosition;
    outData.texcoord0 = iTexCoord0;
    outData.normal = mat3(myData.transform) * iNormal;
    outData.tangent = iTangent;
    outData.binormal = iBinormal;
    outData.worldPos = vec3(myData.transform * vec4(iPosition, 1.0));
    gl_Position = viewProjection * myData.transform * vec4(iPosition, 1);

    outData.fragPosition = gl_Position.xyz;
}