#pragma once
#include <unordered_map>
#include <vma/vk_mem_alloc.h>
#include <vulkan/vulkan.h>

#include "VkBootstrap.h"


struct BufferCopy
{
	uint64_t bufferOffset;
	uint32_t bufferRowLength;
	uint32_t bufferImageHeight;
	VkImageSubresourceLayers imageSubresource;
	VkOffset3D imageOffset;
	VkExtent3D imageExtent;
};

struct PhysicalDevice
{
	vkb::Instance* parent;
	vkb::PhysicalDevice device;
	VkSurfaceKHR surface;
};

struct Swapchain
{
	vkb::Swapchain swapchain;
};

struct RenderPass
{
	VkRenderPass pass;
};

struct CommandBuffer
{
	VkCommandBuffer* buffers;
	uint32_t amount;

	VkCommandPool pool;
	VkQueue queue;
};

struct Buffer
{
	VkBuffer buffer;
	VmaAllocation allocation;
	size_t size;
};

struct ShaderModule
{
	VkShaderModule shaderModule;

	uint32_t count;
};

struct ShaderLayoutBinding
{
	uint32_t bindingIndex;
	uint32_t type;
	uint32_t descriptorCount;
	uint32_t stageFlags;
};

struct ShaderLayoutSet
{
	uint32_t bindingCount;
	ShaderLayoutBinding* bindings;
};

struct ShaderLayout
{
	uint32_t setCount;
	ShaderLayoutSet* sets;
	
	VkPipelineLayout layout;
};

struct Shader
{
	RenderPass* pass;
	uint32_t subPassIndex;
	
	ShaderLayout* layout;
	VkPipeline pipeline;

	ShaderModule* vertexModule;
	ShaderModule* fragmentModule;
	ShaderModule* geometryModule;
	ShaderModule* tesselationEvalModule;
	ShaderModule* tesselationControlModule;
	
	ShaderModule* computeModule;
};

struct Device
{
	vkb::Instance instance;
	vkb::PhysicalDevice physicalDevice;
	vkb::Device device;
	vkb::DispatchTable functionTable;
	VkSurfaceKHR surface;
	VmaAllocator allocator;

	VkQueue graphicsQueue;
	VkQueue presentQueue;
	VkQueue computeQueue;
	VkQueue tranferQueue;

	VkCommandPool graphicsPool;
	VkCommandPool presentPool;
	VkCommandPool computePool;
	VkCommandPool transferPool;

	VkPipelineCache pipelineCache;

	std::unordered_map<std::string, ShaderModule*> shaderModules;
};