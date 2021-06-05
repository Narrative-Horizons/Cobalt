#version 460
#extension GL_ARB_bindless_texture : enable

#define MAX_TEX_COUNT 500
layout(location = 2, bindless_sampler) uniform sampler2D texArray[MAX_TEX_COUNT];

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

const float PI = 3.14159265359;

vec3 GetNormalFromMap(uint normalId)
{
    vec3 tangentNormal = texture(texArray[normalId], inData.texcoord0).xyz * 2.0 - 1.0;

    vec3 Q1  = dFdx(inData.worldPos);
    vec3 Q2  = dFdy(inData.worldPos);
    vec2 st1 = dFdx(inData.texcoord0);
    vec2 st2 = dFdy(inData.texcoord0);

    vec3 N   = normalize(inData.normal);
    vec3 T  = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B  = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}
// ----------------------------------------------------------------------------
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 FresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
}

void main()
{
    int instance = inData.instanceID;

    ObjectData myData = objects[instance];
    MaterialData myMaterial = materials[myData.materialID];

    vec3 albedo = pow(texture(texArray[myMaterial.albedo], inData.texcoord0).rgb, vec3(2.2));
    float metallic = texture(texArray[myMaterial.ORM], inData.texcoord0).b;
    float roughness = texture(texArray[myMaterial.ORM], inData.texcoord0).g;
    float ao = texture(texArray[myMaterial.ORM], inData.texcoord0).r;

    vec3 N = GetNormalFromMap(myMaterial.normal);
    vec3 V = normalize(cameraPosition - inData.worldPos);

    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    // TODO: Per light
    vec3 L = normalize(sunDirection);
    vec3 H = normalize(V + L);

    float attenuation = 0;
    vec3 radiance = vec3(0.5);

    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    vec3 F = FresnelSchlick(max(dot(H, V), 0.0), F0);

    vec3 numerator = NDF * G * F;
    float denominator = max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001;
    vec3 specular = numerator / denominator;

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;

    float NdotL = max(dot(N, L), 0.0);
    vec3 Light0 = (kD * albedo / PI + specular) * radiance * NdotL;

    vec3 ambient = vec3(0.03) * albedo * ao;

    vec3 color = ambient + Light0;

    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0 / 2.2));

    color *= 2;

    FragColor = vec4(color, 1.0);
}