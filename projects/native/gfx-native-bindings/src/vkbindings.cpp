#include <VkBootstrap.h>

#define VMA_IMPLEMENTATION
#define VMA_STATIC_VULKAN_FUNCTIONS 0
#include <vma/vk_mem_alloc.h>

#include <GLFW/glfw3.h>

#include <stdio.h>
#include <stdbool.h>

#define VK_BINDING_EXPORT extern "C" __declspec(dllexport) inline

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
	size_t attachmentCount;
	const VkAttachmentDescription* attachments;

	size_t subpassCount;
	const VkSubpassDescription* subpasses;

	size_t dependencyCount;
	const VkSubpassDependency* dependencies;
};

struct CommandBufferCreateInfo
{
	uint32_t pool;
	uint32_t amount;
	bool primary;
};

struct PhysicalDevice
{
	vkb::Instance* parent;
	vkb::PhysicalDevice device;
	VkSurfaceKHR surface;
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

VK_BINDING_EXPORT Device* cobalt_vkb_create_device(InstanceCreateInfo info)
{
	vkb::InstanceBuilder bldr = vkb::InstanceBuilder()
		.set_app_version(info.appVersion.major, info.appVersion.minor, info.appVersion.patch)
		.set_app_name(info.appName)
		.set_engine_version(info.engineVersion.major, info.engineVersion.minor, info.engineVersion.patch)
		.set_engine_name(info.engineName)
		.require_api_version(info.requiredVersion.major, info.requiredVersion.major, info.requiredVersion.patch)
		.enable_validation_layers(info.requireValidationLayers);

	for (auto i = 0; i < info.enabledLayerCount; ++i)
	{
		bldr.enable_layer(info.enabledLayers[i]);
	}

	for (auto i = 0; i < info.enabledExtensionCount; ++i)
	{
		bldr.enable_extension(info.enabledExtensions[i]);
	}

	if (info.useDefaultDebugger)
	{
		bldr.use_default_debug_messenger();
	}

	const auto instanceResult = bldr.build();
	if (!instanceResult)
	{
		return nullptr;
	}

	Device device;

	device.instance = instanceResult.value();

	// create surface
	VkSurfaceKHR surface;
	const VkResult surfaceResult = glfwCreateWindowSurface(device.instance.instance, info.window, device.instance.allocation_callbacks, &surface);

	if (surfaceResult != VK_SUCCESS)
	{
		vkb::destroy_instance(device.instance);
		return nullptr;
	}
	device.surface = surface;

	// select physical device
	vkb::PhysicalDeviceSelector physicalDeviceSelector = vkb::PhysicalDeviceSelector(device.instance)
		.prefer_gpu_device_type(vkb::PreferredDeviceType::discrete)
		.set_minimum_version(1, 2)
		.set_surface(surface);
	const auto physicalDeviceResult = physicalDeviceSelector.select();

	if (!physicalDeviceResult)
	{
		vkb::destroy_surface(device.instance, device.surface);
		vkb::destroy_instance(device.instance);
		return nullptr;
	}
	device.physicalDevice = physicalDeviceResult.value();

	const auto deviceResult = vkb::DeviceBuilder(device.physicalDevice)
		.build();

	if (!deviceResult)
	{
		vkb::destroy_surface(device.instance, device.surface);
		vkb::destroy_instance(device.instance);
		return nullptr;
	}

	device.device = deviceResult.value();

	auto gq = device.device.get_queue(vkb::QueueType::graphics);
	if (!gq.has_value())
	{
		vkb::destroy_surface(device.instance, device.surface);
		vkb::destroy_instance(device.instance);
		return nullptr;
	}
	device.graphicsQueue = gq.value();

	auto pq = device.device.get_queue(vkb::QueueType::present);
	if (!pq.has_value())
	{
		vkb::destroy_surface(device.instance, device.surface);
		vkb::destroy_instance(device.instance);
		return nullptr;
	}

	device.presentQueue = pq.value();

	auto cq = device.device.get_queue(vkb::QueueType::compute);
	if (!cq.has_value())
	{
		vkb::destroy_surface(device.instance, device.surface);
		vkb::destroy_instance(device.instance);
		return nullptr;
	}

	device.computeQueue = cq.value();

	auto tq = device.device.get_queue(vkb::QueueType::transfer);
	if (!tq.has_value())
	{
		vkb::destroy_surface(device.instance, device.surface);
		vkb::destroy_instance(device.instance);
		return nullptr;
	}

	device.tranferQueue = tq.value();

	device.device = deviceResult.value();
	device.functionTable = device.device.make_table();

	VmaAllocatorCreateInfo allocatorInfo = {};
	allocatorInfo.flags = 0;
	allocatorInfo.device = device.device;

	VmaVulkanFunctions funcs = {};
	funcs.vkGetPhysicalDeviceProperties = (PFN_vkGetPhysicalDeviceProperties)device.instance.fp_vkGetInstanceProcAddr(device.instance.instance, "vkGetPhysicalDeviceProperties");
	funcs.vkGetPhysicalDeviceMemoryProperties = (PFN_vkGetPhysicalDeviceMemoryProperties)device.instance.fp_vkGetInstanceProcAddr(device.instance.instance, "vkGetPhysicalDeviceMemoryProperties");
	funcs.vkAllocateMemory = device.functionTable.fp_vkAllocateMemory;
	funcs.vkFreeMemory = device.functionTable.fp_vkFreeMemory;
	funcs.vkMapMemory = device.functionTable.fp_vkMapMemory;
	funcs.vkUnmapMemory = device.functionTable.fp_vkUnmapMemory;
	funcs.vkFlushMappedMemoryRanges = device.functionTable.fp_vkFlushMappedMemoryRanges;
	funcs.vkInvalidateMappedMemoryRanges = device.functionTable.fp_vkInvalidateMappedMemoryRanges;
	funcs.vkBindBufferMemory = device.functionTable.fp_vkBindBufferMemory;
	funcs.vkBindImageMemory = device.functionTable.fp_vkBindImageMemory;
	funcs.vkGetBufferMemoryRequirements = device.functionTable.fp_vkGetBufferMemoryRequirements;
	funcs.vkGetImageMemoryRequirements = device.functionTable.fp_vkGetImageMemoryRequirements;
	funcs.vkCreateBuffer = device.functionTable.fp_vkCreateBuffer;
	funcs.vkDestroyBuffer = device.functionTable.fp_vkDestroyBuffer;
	funcs.vkCreateImage = device.functionTable.fp_vkCreateImage;
	funcs.vkDestroyImage = device.functionTable.fp_vkDestroyImage;
	funcs.vkCmdCopyBuffer = device.functionTable.fp_vkCmdCopyBuffer;
	funcs.vkGetBufferMemoryRequirements2KHR = device.functionTable.fp_vkGetBufferMemoryRequirements2KHR;
	funcs.vkGetImageMemoryRequirements2KHR = device.functionTable.fp_vkGetImageMemoryRequirements2KHR;
	funcs.vkBindBufferMemory2KHR = device.functionTable.fp_vkBindBufferMemory2KHR;
	funcs.vkBindImageMemory2KHR = device.functionTable.fp_vkBindImageMemory2KHR;
	funcs.vkGetPhysicalDeviceMemoryProperties2KHR = (PFN_vkGetPhysicalDeviceMemoryProperties2KHR)device.instance.fp_vkGetInstanceProcAddr(device.instance.instance, "vkGetPhysicalDeviceMemoryProperties2KHR");

	allocatorInfo.pVulkanFunctions = &funcs;
	allocatorInfo.instance = device.instance;
	allocatorInfo.physicalDevice = device.physicalDevice;
	
	vmaCreateAllocator(&allocatorInfo, &device.allocator);

	VkCommandPoolCreateInfo poolInfo = {};
	poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
	poolInfo.flags = 0;
	poolInfo.pNext = nullptr;
	poolInfo.queueFamilyIndex = device.device.get_queue_index(vkb::QueueType::graphics).value();
	device.functionTable.createCommandPool(&poolInfo, device.device.allocation_callbacks, &device.graphicsPool);

	poolInfo.queueFamilyIndex = device.device.get_queue_index(vkb::QueueType::compute).value();
	device.functionTable.createCommandPool(&poolInfo, device.device.allocation_callbacks, &device.computePool);

	poolInfo.queueFamilyIndex = device.device.get_queue_index(vkb::QueueType::present).value();
	device.functionTable.createCommandPool(&poolInfo, device.device.allocation_callbacks, &device.presentPool);
	
	poolInfo.queueFamilyIndex = device.device.get_queue_index(vkb::QueueType::transfer).value();
	device.functionTable.createCommandPool(&poolInfo, device.device.allocation_callbacks, &device.transferPool);
	
	return new Device(device);
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_device(Device* device)
{
	if (device)
	{
		vkb::destroy_device(device->device);
		vkb::destroy_surface(device->instance, device->surface);
		vkb::destroy_instance(device->instance);
		delete device;
		return true;
	}

	return false;
}

VK_BINDING_EXPORT Swapchain* cobalt_vkb_create_swapchain(Device* device, SwapchainCreateInfo info)
{
	const vkb::SwapchainBuilder bldr{ device->device };
	const auto swapchainResult = bldr.build();
	if (!swapchainResult)
	{
		return nullptr;
	}

	Swapchain* swapchain = new Swapchain();
	swapchain->swapchain = swapchainResult.value();

	return swapchain;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_swapchain(Swapchain* swapchain)
{
	if (swapchain)
	{
		vkb::destroy_swapchain(swapchain->swapchain);
		delete swapchain;

		return true;
	}

	return false;
}

VK_BINDING_EXPORT VkRenderPass cobalt_vkb_create_renderpass(Device* device, RenderPassCreateInfo info)
{
	VkRenderPassCreateInfo renderPassInfo = {};
	renderPassInfo.pNext = nullptr;
	renderPassInfo.flags = 0;
	renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;

	renderPassInfo.attachmentCount = static_cast<uint32_t>(info.attachmentCount);
	renderPassInfo.pAttachments = info.attachments;

	renderPassInfo.subpassCount = static_cast<uint32_t>(info.subpassCount);
	renderPassInfo.pSubpasses = info.subpasses;
	
	renderPassInfo.dependencyCount = static_cast<uint32_t>(info.dependencyCount);
	renderPassInfo.pDependencies = info.dependencies;

	VkRenderPass pass;
	if (device->functionTable.createRenderPass(&renderPassInfo, device->device.allocation_callbacks, &pass) != VK_SUCCESS)
	{
		return nullptr;
	}

	return pass;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_renderpass(Device* device, VkRenderPass renderpass)
{
	if (renderpass)
	{
		device->functionTable.destroyRenderPass(renderpass, device->device.allocation_callbacks);

		return true;
	}

	return false;
}

VK_BINDING_EXPORT CommandBuffer* cobalt_vkb_create_commandbuffer(Device* device, CommandBufferCreateInfo info)
{
	VkCommandBufferAllocateInfo allocInfo = {};
	allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	allocInfo.pNext = nullptr;
	allocInfo.commandBufferCount = info.amount;

	VkQueue queue = nullptr;
	
	switch (info.pool)
	{
		case 0: // Present
		{
			allocInfo.commandPool = device->presentPool;
			queue = device->presentQueue;
			break;
		}

		case 1: // Graphics
		{
			allocInfo.commandPool = device->graphicsPool;
			queue = device->graphicsQueue;
			break;
		}

		case 2: // Transfer
		{
			allocInfo.commandPool = device->transferPool;
			queue = device->tranferQueue;
			break;
		}

		case 4: // Compute
		{
			allocInfo.commandPool = device->computePool;
			queue = device->computeQueue;
			break;
		}
	}

	allocInfo.level = info.primary ? VK_COMMAND_BUFFER_LEVEL_PRIMARY : VK_COMMAND_BUFFER_LEVEL_SECONDARY;

	VkCommandBuffer* buffers = new VkCommandBuffer[info.amount];
	
	if (device->functionTable.allocateCommandBuffers(&allocInfo, buffers))
	{
		return nullptr;
	}

	CommandBuffer* buffer = new CommandBuffer();
	buffer->buffers = buffers;
	buffer->pool = allocInfo.commandPool;
	buffer->amount = info.amount;
	buffer->queue = queue;

	return buffer;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_commandbuffer(Device* device, CommandBuffer* buffer, const uint32_t index)
{
	if (buffer)
	{
		device->functionTable.freeCommandBuffers(buffer->pool, 1, buffer->buffers + index);
		buffer->amount--;

		if (buffer->amount == 0)
		{
			delete[] buffer->buffers;
			delete buffer;
		}
		
		return true;
	}

	return false;
}