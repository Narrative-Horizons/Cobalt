#pragma once

#include <vulkan/vulkan.h>
#include <GLFW/glfw3.h>

#include "graphicsobjects.hpp"

struct ApiVersion
{
	unsigned int major;
	unsigned int minor;
	unsigned int patch;
};

struct InstanceCreateInfo
{
	ApiVersion appVersion;
	const char* appName;
	ApiVersion engineVersion;
	const char* engineName;
	ApiVersion requiredVersion;
	ApiVersion desiredVersion;
	size_t enabledLayerCount;
	const char** enabledLayers;
	size_t enabledExtensionCount;
	const char** enabledExtensions;
	bool requireValidationLayers;
	bool useDefaultDebugger;
	GLFWwindow* window;
};

struct SwapchainCreateInfo
{

};

struct RenderPassCreateInfo
{
	uint32_t attachmentCount;
	const VkAttachmentDescription* attachments;

	uint32_t subpassCount;
	const VkSubpassDescription* subpasses;

	uint32_t dependencyCount;
	const VkSubpassDependency* dependencies;
};

struct CommandBufferCreateInfo
{
	uint32_t pool;
	uint32_t amount;
	bool primary;
};

struct BufferCreateInfo
{
	uint32_t usage;
	uint64_t size;
	uint32_t sharingMode;

	uint32_t indexCount;
	uint32_t* indices;
};

struct BufferMemoryCreateInfo
{
	uint32_t usage;
	uint32_t preferredFlags;
	uint32_t requiredFlags;
};

struct ShaderModuleCreateInfo
{
	const char* code;
	size_t codeSize;
};

struct ShaderLayoutBindingCreateInfo
{
	uint32_t bindingIndex;
	uint32_t type;
	uint32_t descriptorCount;
	uint32_t stageFlags;
};

struct ShaderLayoutSetCreateInfo
{
	uint32_t bindingCount = 0;
	ShaderLayoutBindingCreateInfo* bindingInfos;
};

struct ShaderLayoutCreateInfo
{
	uint32_t setCount = 0;
	ShaderLayoutSetCreateInfo* setInfos;
};

struct ShaderCreateInfo
{
	const char* vertexModulePath;
	const char* fragmentModulePath;
	const char* geometryModulePath;
	const char* tesselationEvalModulePath;
	const char* tesselationControlModulePath;
	const char* computeModulePath;

	RenderPass* pass;
	uint32_t subPassIndex;

	ShaderLayoutCreateInfo layoutInfo;
};

struct ImageCreateInfo
{
	uint32_t imageType;
	uint32_t format;
	uint32_t width;
	uint32_t height;
	uint32_t depth;
	uint32_t mipLevels;
	uint32_t arrayLayers;
	uint32_t samples;
	uint32_t tiling;
	uint32_t usage;
	uint32_t sharingMode;
	uint32_t queueFamilyIndexCount;
	uint32_t* queueFamilyIndices;
	uint32_t initialLayout;
};

struct ImageViewCreateInfo
{
	Image* image;
	uint32_t viewType;
	uint32_t format;
	uint32_t aspectMask;
	uint32_t baseMipLevel;
	uint32_t levelCount;
	uint32_t baseArrayLayer;
	uint32_t layerCount;
};

struct FramebufferCreateInfo
{
	RenderPass* pass;
	uint32_t attachmentCount;
	ImageView** attachments;
	uint32_t width;
	uint32_t height;
	uint32_t layers;
};

struct SemaphoreCreateInfo
{
	uint32_t flags;
};

struct IndexedCommandBuffers
{
	CommandBuffer* commandbuffers;
	uint32_t* bufferIndices;
	uint32_t amount;
};

struct SubmitInfo
{
	uint32_t waitSemaphoreCount;
	Semaphore** waitSemaphores;

	uint32_t* waitDstStageMask;

	uint32_t commandbufferCount;
	IndexedCommandBuffers* commandbuffer;

	uint32_t signalSemaphoreCount;
	Semaphore** signalSemaphores;
};

struct PresentInfo
{
	uint32_t waitSemaphoreCount;
	Semaphore** waitSemaphores;

	uint32_t swapchainCount;
	Swapchain** swapchains;

	uint32_t* imageIndices;
};

struct FenceCreateInfo
{
	uint32_t flags;
};

struct Rect2D
{
	float x;
	float y;
	float width;
	float height;
};

struct Vector4
{
	float x, y, z, w;
};

struct ClearValue
{
	Vector4 color;
	float depth;
};

struct Offset3D
{
	int x;
	int y;
	int z;
};

struct Extent3D
{
	int width;
	int depth;
	int height;
};

struct RenderPassBeginInfo
{
	RenderPass* renderpass;
	Framebuffer* framebuffer;

	Rect2D renderArea;
	uint32_t clearValueCount;

	ClearValue* clearValues;
};

struct BufferCopy
{
	uint64_t srcOffset;
	uint64_t dstOffset;
	uint64_t size;
};

struct ImageSubresourceLayers
{
	uint32_t aspectMask;
	uint32_t mipLevel;
	uint32_t baseArrayLayer;
	uint32_t layerCount;
};

struct BufferImageCopy
{
	uint64_t bufferOffset;
	uint32_t bufferRowLength;
	uint32_t bufferImageHeight;
	ImageSubresourceLayers imageSubresource;
	Offset3D imageOffset;
	Extent3D imageExtent;
};

struct MemoryBarrier
{
	uint32_t srcAccessMask;
	uint32_t dstAccessMask;
};

struct BufferMemoryBarrier
{
	uint32_t srcAccessMask;
	uint32_t dstAccessMask;
	uint32_t srcQueueFamilyIndex;
	uint32_t dstQueueFamilyIndex;
	Buffer* buffer;
	uint64_t offset;
	uint64_t size;
};

struct ImageSubresourceRange
{
	uint32_t aspectMask;
	uint32_t baseMipLevel;
	uint32_t levelCount;
	uint32_t baseArrayLayer;
	uint32_t layerCount;
};

struct ImageMemoryBarrier
{
	uint32_t srcAccessMask;
	uint32_t dstAccessMask;
	uint32_t oldLayout;
	uint32_t newLayout;
	uint32_t srcQueueFamilyIndex;
	uint32_t dstQueueFamilyIndex;
	Image* image;
	ImageSubresourceRange subresourceRange;
};

struct SamplerCreateInfo
{
	uint32_t flags;
	uint32_t minFilter;
	uint32_t magFilter;
	uint32_t mipmapMode;
	uint32_t addressModeU;
	uint32_t addressModeV;
	uint32_t addressModeW;
	float mipLodBias;
	bool anisotropyEnable;
	float maxAnisotropy;
	bool compareEnable;
	uint32_t compareOp;
	float minLod;
	float maxLod;
	uint32_t borderColor;
	bool unnormalizedCoordinates;
};