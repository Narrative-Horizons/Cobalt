#version 460

layout (local_size_x = 1) in;

layout(std430, binding = 0) buffer ComputeData
{
    int amount;
    int result;
    int number;
};

void main()
{
    result = number;
}
