#pragma once

#include <unordered_map>
#include <vma/vk_mem_alloc.h>
#include <vulkan/vulkan.h>

#include "VkBootstrap.h"

struct Device;

struct PhysicalDevice
{
	vkb::Instance* parent;
	vkb::PhysicalDevice device;
	VkSurfaceKHR surface;
};

struct ImageView
{
	VkImageView imageView;
	uint32_t amount;
};

struct Swapchain
{
	vkb::Swapchain swapchain;
	std::vector<ImageView*> frameViews;
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

struct DescriptorSetLayout
{
	VkDescriptorSetLayout layout;
	std::vector<ShaderLayoutBinding> bindings;
};

struct PipelineLayout
{
	VkPipelineLayout layout;

	std::vector<DescriptorSetLayout> sets;
};

struct FixedDescriptorSetPool;

struct DescriptorSet
{
	VkDescriptorSet set;
	FixedDescriptorSetPool* parent;

	uint32_t samplerCount;
	uint32_t combinedImageSamplerCount;
	uint32_t sampledImageCount;
	uint32_t storageImageCount;
	uint32_t uniformTexelBufferCount;
	uint32_t storageTexelBufferCount;
	uint32_t uniformBufferCount;
	uint32_t storageBufferCount;
	uint32_t dynamicUniformBufferCount;
	uint32_t dynamicStorageBufferCount;
	uint32_t inputAttachmentCount;
};

struct FixedDescriptorSetPool
{
	VkDescriptorPool pool;
	
	uint32_t samplerCapacity = 0;
	uint32_t combinedImageSamplerCapacity = 0;
	uint32_t sampledImageCapacity = 0;
	uint32_t storageImageCapacity = 0;
	uint32_t uniformTexelBufferCapacity = 0;
	uint32_t storageTexelBufferCapacity = 0;
	uint32_t uniformBufferCapacity = 0;
	uint32_t storageBufferCapacity = 0;
	uint32_t dynamicUniformBufferCapacity = 0;
	uint32_t dynamicStorageBufferCapacity = 0;
	uint32_t inputAttachmentCapacity = 0;
	
	uint32_t samplerCount = 0;
	uint32_t combinedImageSamplerCount = 0;
	uint32_t sampledImageCount = 0;
	uint32_t storageImageCount = 0;
	uint32_t uniformTexelBufferCount = 0;
	uint32_t storageTexelBufferCount = 0;
	uint32_t uniformBufferCount = 0;
	uint32_t storageBufferCount = 0;
	uint32_t dynamicUniformBufferCount = 0;
	uint32_t dynamicStorageBufferCount = 0;
	uint32_t inputAttachmentCount = 0;

	uint32_t maxSets = 0;
	uint32_t allocatedSets = 0;
};

struct DynamicDescriptorSetPool
{
	std::vector<FixedDescriptorSetPool> pools;
};

struct Shader
{
	Device* device;

	RenderPass* pass;
	uint32_t subPassIndex;

	VkPipeline pipeline;
	PipelineLayout pipelineLayout;

	ShaderModule* vertexModule;
	ShaderModule* fragmentModule;
	ShaderModule* geometryModule;
	ShaderModule* tesselationEvalModule;
	ShaderModule* tesselationControlModule;

	ShaderModule* computeModule;
	DynamicDescriptorSetPool descPool;
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

struct Sampler
{
	VkSampler sampler;
	size_t refs;
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
	std::unordered_map<std::string, Sampler*> samplers;
};

struct Semaphore
{
	VkSemaphore semaphore;
};

struct Fence
{
	VkFence fence;
};