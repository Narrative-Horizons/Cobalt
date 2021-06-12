#version 460

layout (local_size_x = 1) in;

layout(std430, binding = 0) buffer ComputeData
{
    int amount;
    int result;
    int numbers[];
};

void main()
{
    int r = 0;
    for(int i = 0; i < amount; i++)
    {
        r += numbers[i];
    }

    result = r;
}
