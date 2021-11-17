#version 450

const vec2 positions[6] = vec2[](
    vec2(-1.0, -1.0), // Top Left
    vec2( 1.0,  1.0), // Bottom Right
    vec2(-1.0,  1.0), // Bottom Left
    vec2(-1.0, -1.0), // Top Left
    vec2( 1.0, -1.0), // Top Right
    vec2( 1.0,  1.0)  // Bottom Right
);

void main(void)
{
    gl_Position = vec4(vertices[gl_VertexIndex], 0.0, 1.0);
}