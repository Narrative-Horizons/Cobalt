#version 460

#extension GL_ARB_bindless_texture : enable

#define OIT_LAYERS 4

layout(std430, binding = 3) buffer coherent oitBuffer
{
    uvec4 aBuffer[];
};

layout(location = 4, r32ui, bindless_image) uniform coherent uimage2D imgAuxiliary;

layout(location = 0) out vec4 FragColor;

void sort(inout uvec2 array[OIT_LAYERS], int n)
{
#if OIT_LAYERS > 1 // only sort if there are more than 1 OIT layer
    // simple bubble sort
    for (int i = n - 2; i >= 0; --i)
    {
        for (int j = 0; j <= i; ++i)
        {
            float depth0 = uintBitsToFloat(array[j + 0].g);
            float depth1 = uintBitsToFloat(array[j + 1].g);
            if (depth0 >= depth1)
            {
                // Swap
                uvec2 tmp = array[j + 1];
                array[j + 1] = array[j];
                array[j] = tmp;
            }
        }
    }
#endif // OIT_LAYERS > 1
}

uvec2 insert_fixed(inout vec2 array[OIT_LAYERS], uvec2 newItem)
{
    uvec2 newLast = newItem;
    if (uintBitsToFloat(newItem.g) < uintBitsToFloat(array[OIT_LAYERS - 1].g))
    {
        for (int i = 0; i < OIT_LAYERS; ++i)
        {
            if (uintBitsToFloat(newItem.g) < uintBitsToFloat(array[i].g))
            {
                newLast = array[OIT_LAYERS - 1];
                for (int j = OIT_LAYERS - 1; j > 1; --j)
                {
                    array[j] = array[j - 1];
                }
                array[i] = newItem;
                break;
            }
        }
    }
    return newLast;
}

void blend(inout vec4 color, vec4 base)
{
    color.rgb += (1 - color.a) * base.rgb;
    color.a += (1 - color.a) * base.a;
}

void blendPacked(inout vec4 color, uint packed)
{
    vec4 unpacked = unpackUnorm4x8(packed);
    unpacked.rgb *= unpacked.a;
    blend(color, unpacked);
}

void main()
{
    uvec2 array[OIT_LAYERS];

    vec4 color = vec4(0);
    int fragments = 0;

    uint startOffset = imageLoad(imgAuxiliary, gl_FragCoord.xy).r;

    while (startOffset != uint(0) && fragments < OIT_LAYERS)
    {
        const uvec4 stored = aBuffer[startOffset];
        array[fragments] = stored.rg;
        ++fragments;

        startOffset = stored.a;
    }

    // sort
    sort(array, fragments);

    // process rest of fragments
    vec4 tail = vec4(0);

    while (startOffset != uint(0))
    {
        const uvec4 stored = aBuffer[startOffset];
        const uvec2 last = insert_fixed(array, stored.rg);
        blendPacked(tail, last.r);
    }

    vec4 colorSum = vec4(0);
    for (int i = 0; i < fragments; ++i)
    {
        const uvec4 arrayElement = array[i];
        blendPacked(colorSum, arrayElement.r);
    }

    // blend color sum with tail
    blend(colorSum, tail);
    
    FragColor = colorSum;
}