#version 460

layout (location = 0) in VertexData
{
    flat uint identifier;
    flat uint generation;
} inData;


layout(location = 0) out uvec2 FragColor;

void main()
{
    FragColor = uvec2(inData.identifier, inData.generation);
}