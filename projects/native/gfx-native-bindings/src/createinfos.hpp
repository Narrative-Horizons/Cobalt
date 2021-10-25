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
	size_t size;
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
	IndexedCommandBuffers** commandbuffer;

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