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
};

struct Swapchain
{
	vkb::Swapchain swapchain;
};

struct RenderPass
{
	VkRenderPass pass;
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
	if(swapchain)
	{
		vkb::destroy_swapchain(swapchain->swapchain);
		delete swapchain;

		return true;
	}

	return false;
}

VK_BINDING_EXPORT RenderPass* cobalt_vkb_create_renderpass(Device* device, RenderPassCreateInfo info)
{
	VkRenderPassCreateInfo renderPassInfo = {};
	renderPassInfo.pNext = nullptr;
	renderPassInfo.flags = 0;
	renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;

	renderPassInfo.attachmentCount = static_cast<uint32_t>(info.attachmentCount);
	renderPassInfo.pAttachments = info.attachments;

	renderPassInfo.dependencyCount = 0;

	VkSubpassDescription subPass = {};
	subPass.flags = 0;

	renderPassInfo.subpassCount = 1;
	renderPassInfo.pSubpasses = &subPass;

	VkRenderPass pass;
	if(device->functionTable.createRenderPass(&renderPassInfo, device->device.allocation_callbacks, &pass) != VK_SUCCESS)
	{
		return nullptr;
	}

	RenderPass* renderpass = new RenderPass();
	renderpass->pass = pass;

	return renderpass;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_renderpass(Device* device, RenderPass* renderpass)
{
	if (renderpass)
	{
		device->functionTable.destroyRenderPass(renderpass->pass, device->device.allocation_callbacks);
		delete renderpass;

		return true;
	}

	return false;
}