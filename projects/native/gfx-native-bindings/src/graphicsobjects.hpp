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
	std::vector<VkImageView> frameViews;
};

struct RenderPass
{
	VkRenderPass pass;
};

struct CommandBuffer
{
	std::vector<VkCommandBuffer> buffers;

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

struct Shader
{
	RenderPass* pass;
	uint32_t subPassIndex;
	
	VkPipeline pipeline;
	VkPipelineLayout pipelineLayout;


	ShaderModule* vertexModule;
	ShaderModule* fragmentModule;
	ShaderModule* geometryModule;
	ShaderModule* tesselationEvalModule;
	ShaderModule* tesselationControlModule;
	
	ShaderModule* computeModule;
};

struct FixedDescriptorSetPool
{
	VkDescriptorPool pool;
	std::unordered_map<VkDescriptorType, std::uint32_t> capacity;
	std::unordered_map<VkDescriptorType, std::uint32_t> allocated;
};

struct DynamicDescriptorSetPool
{
	std::vector<FixedDescriptorSetPool> pools;
};

struct ImageView
{
	VkImageView imageView;
	uint32_t amount;
};

struct Image
{
	VkImage image;
	uint32_t amount;
};

struct Framebuffer
{
	VkFramebuffer framebuffer;
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

	std::unordered_map<std::string, Image*> images;
	std::unordered_map<std::string, ImageView*> imageViews;
};

