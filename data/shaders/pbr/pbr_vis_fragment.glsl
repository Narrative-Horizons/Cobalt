#version 460

layout (location = 0) in VertexData
{
    vec2 uv;
} i;

struct ObjectData
{
    mat4 transform;
    uint materialID;
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

layout (location = 3) uniform sampler2DMS visibility;

layout (location = 0) out vec4 fragColor;

void main(void)
{
    vec2 uv = i.uv;

    fragColor = vec4(uv, 0.0, 1.0);
}