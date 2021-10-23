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

struct DescriptorSet
{
	VkDescriptorSet set;

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
	
	uint32_t allocatedSamplerCount;
	uint32_t allocatedCombinedImageSamplerCount;
	uint32_t allocatedSampledImageCount;
	uint32_t allocatedStorageImageCount;
	uint32_t allocatedUniformTexelBufferCount;
	uint32_t allocatedStorageTexelBufferCount;
	uint32_t allocatedUniformBufferCount;
	uint32_t allocatedStorageBufferCount;
	uint32_t allocatedDynamicUniformBufferCount;
	uint32_t allocatedDynamicStorageBufferCount;
	uint32_t allocatedInputAttachmentCount;
	
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

	DescriptorSet* tryAllocate(const VkDescriptorSetAllocateInfo& info);
};

struct DynamicDescriptorSetPool
{
	std::vector<FixedDescriptorSetPool> pools;
};

struct Shader
{
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

