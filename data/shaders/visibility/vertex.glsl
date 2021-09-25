#version 460

vec4 gl_Position;

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 textureCoordinate;
layout (location = 3) in vec3 tangent;
layout (location = 4) in vec3 binormal;

struct ObjectPayload
{
    mat4 transform;
    uint material;
};

layout (location = 0) out VertexData
{
    flat uint drawID;
    flat uint instanceID;
} o;

layout (std430, binding = 0) buffer ObjectPayloadBuffer
{
    ObjectPayload objects[];
};

layout (std140, binding = 2) uniform SceneData
{
    mat4 view;
    mat4 projection;
    mat4 viewProjection;
};

void main(void)
{
    o.instanceID = uint(gl_InstanceID);
    ObjectPayload payload = objects[o.instanceID];

    gl_Position = viewProjection * payload.transform * vec4(position, 1.0);
    o.drawID = uint(gl_DrawID);
}