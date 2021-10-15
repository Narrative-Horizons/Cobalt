#include <VkBootstrap.h>

#define VMA_IMPLEMENTATION
#define VMA_STATIC_VULKAN_FUNCTIONS 0
#include <vma/vk_mem_alloc.h>

#include <GLFW/glfw3.h>

#include <stdio.h>
#include <stdbool.h>
#include <fstream>
#include <string>

#include "createinfos.hpp"
#include "graphicsobjects.hpp"

#define VK_BINDING_EXPORT extern "C" __declspec(dllexport) inline

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

	VkPipelineCacheCreateInfo cacheInfo = {};
	cacheInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_CACHE_CREATE_INFO;
	cacheInfo.flags = 0;
	cacheInfo.pNext = nullptr;
	cacheInfo.initialDataSize = 0;
	cacheInfo.pInitialData = nullptr;

	// TODO: Look for pipelinecache file on load, on exit save out the pipeline cache
	device.pipelineCache = VK_NULL_HANDLE;
	
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

	const auto imageViews = swapchain->swapchain.get_image_views();
	if(imageViews.has_value())
	{
		swapchain->frameViews = imageViews.value();
	}
	else
	{
		cobalt_vkb_destroy_swapchain(swapchain);
		return nullptr;
	}

	return swapchain;
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

	std::vector<VkCommandBuffer> buffers(info.amount);
	
	if (device->functionTable.allocateCommandBuffers(&allocInfo, buffers.data()))
	{
		return nullptr;
	}

	CommandBuffer* buffer = new CommandBuffer();
	
	buffer->buffers = std::move(buffers);
	buffer->pool = allocInfo.commandPool;
	buffer->queue = queue;

	return buffer;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_commandbuffer(Device* device, CommandBuffer* buffer, const uint32_t index)
{
	if (buffer)
	{
		device->functionTable.freeCommandBuffers(buffer->pool, 1, buffer->buffers.data() + index);
		buffer->buffers.erase(buffer->buffers.begin() + index);

		if (buffer->buffers.empty())
		{
			delete buffer;
		}
		
		return true;
	}

	return false;
}

VK_BINDING_EXPORT Buffer* cobalt_vkb_create_buffer(Device* device, BufferCreateInfo createInfo, BufferMemoryCreateInfo memoryInfo)
{
	VkBufferCreateInfo info = {};
	
	info.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	info.flags = 0;
	info.pNext = nullptr;
	info.usage = createInfo.usage;
	info.size = createInfo.size;
	info.sharingMode = static_cast<VkSharingMode>(createInfo.sharingMode);
	info.queueFamilyIndexCount = createInfo.indexCount;
	info.pQueueFamilyIndices = createInfo.indices;

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.flags = 0;
	allocInfo.pUserData = nullptr;
	allocInfo.usage = static_cast<VmaMemoryUsage>(memoryInfo.usage);
	allocInfo.preferredFlags = memoryInfo.preferredFlags;
	allocInfo.requiredFlags = memoryInfo.requiredFlags;

	VkBuffer vkbuffer;
	VmaAllocation allocation;
	
	vmaCreateBuffer(device->allocator, &info, &allocInfo, &vkbuffer, &allocation, nullptr);

	Buffer* buffer = new Buffer();
	buffer->buffer = vkbuffer;
	buffer->allocation = allocation;
	buffer->size = createInfo.size;

	return buffer;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_buffer(Device* device, Buffer* buffer)
{
	if (buffer)
	{
		vmaDestroyBuffer(device->allocator, buffer->buffer, buffer->allocation);
		delete buffer;
		
		return true;
	}

	return false;
}

VK_BINDING_EXPORT void* cobalt_vkb_map_buffer(Device* device, Buffer* buffer)
{
	void* data = nullptr;
	vmaMapMemory(device->allocator, buffer->allocation, &data);

	return data;
}

VK_BINDING_EXPORT void cobalt_vkb_unmap_buffer(Device* device, Buffer* buffer)
{
	vmaUnmapMemory(device->allocator, buffer->allocation);
}

VK_BINDING_EXPORT void cobalt_vkb_copy_buffer(Device* device, CommandBuffer* buffer, uint32_t index, Buffer* src, Buffer* dst, uint32_t regionCount, BufferCopy* regions)
{
	device->functionTable.cmdCopyBuffer(buffer->buffers[index], src->buffer, dst->buffer, regionCount, reinterpret_cast<VkBufferCopy*>(regions));
}

VK_BINDING_EXPORT ShaderModule* cobalt_vkb_create_shadermodule(Device* device, ShaderModuleCreateInfo info)
{
	VkShaderModuleCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	createInfo.flags = 0;
	createInfo.pNext = nullptr;
	createInfo.codeSize = info.codeSize;
	createInfo.pCode = reinterpret_cast<const uint32_t*>(info.code);

	VkShaderModule vkshaderModule;

	if(!device->functionTable.createShaderModule(&createInfo, device->device.allocation_callbacks, &vkshaderModule))
	{
		return nullptr;
	}

	ShaderModule* shaderModule = new ShaderModule();
	shaderModule->shaderModule = vkshaderModule;
	shaderModule->count = 1;

	return shaderModule;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_shadermodule(Device* device, VkShaderModule shaderModule)
{
	if(shaderModule)
	{
		device->functionTable.destroyShaderModule(shaderModule, device->device.allocation_callbacks);
		return true;
	}

	return false;
}

VK_BINDING_EXPORT Shader* cobalt_vkb_create_shader(Device* device, ShaderCreateInfo info)
{
	Shader* shader = new Shader();
	std::vector< VkPipelineShaderStageCreateInfo> shaderStages;
	
	if(info.vertexModulePath != nullptr)
	{
		// Normal shader
		if(device->shaderModules.find(info.vertexModulePath) != device->shaderModules.end())
		{
			shader->vertexModule = device->shaderModules[info.vertexModulePath];
			shader->vertexModule->count++;
		}
		else
		{
			std::ifstream vertexFile(info.vertexModulePath, std::ios::in | std::ios::binary | std::ios::ate);
			std::streamsize size = vertexFile.tellg();
			vertexFile.seekg(0, std::ios::beg);

			std::vector<char> buffer(size);
			vertexFile.read(buffer.data(), size);

			ShaderModuleCreateInfo vertexInfo = {};
			vertexInfo.code = buffer.data();
			vertexInfo.codeSize = size;

			shader->vertexModule = cobalt_vkb_create_shadermodule(device, vertexInfo);
		}

		VkPipelineShaderStageCreateInfo vertexStageInfo = {};
		vertexStageInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		vertexStageInfo.flags = 0;
		vertexStageInfo.pNext = nullptr;
		vertexStageInfo.module = shader->vertexModule->shaderModule;
		vertexStageInfo.pName = "Vertex Shader";
		vertexStageInfo.pSpecializationInfo = nullptr;
		vertexStageInfo.stage = VK_SHADER_STAGE_VERTEX_BIT;

		shaderStages.push_back(vertexStageInfo);
		
		if(info.fragmentModulePath != nullptr)
		{
			if (device->shaderModules.find(info.fragmentModulePath) != device->shaderModules.end())
			{
				shader->fragmentModule = device->shaderModules[info.fragmentModulePath];
				shader->fragmentModule->count++;
			}
			else
			{
				std::ifstream fragmentFile(info.fragmentModulePath, std::ios::in | std::ios::binary | std::ios::ate);
				std::streamsize size = fragmentFile.tellg();
				fragmentFile.seekg(0, std::ios::beg);

				std::vector<char> buffer(size);
				fragmentFile.read(buffer.data(), size);

				ShaderModuleCreateInfo fragmentInfo = {};
				fragmentInfo.code = buffer.data();
				fragmentInfo.codeSize = size;

				shader->fragmentModule = cobalt_vkb_create_shadermodule(device, fragmentInfo);
			}

			VkPipelineShaderStageCreateInfo fragmentStageInfo = {};
			fragmentStageInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
			fragmentStageInfo.flags = 0;
			fragmentStageInfo.pNext = nullptr;
			fragmentStageInfo.module = shader->fragmentModule->shaderModule;
			fragmentStageInfo.pName = "Fragment Shader";
			fragmentStageInfo.pSpecializationInfo = nullptr;
			fragmentStageInfo.stage = VK_SHADER_STAGE_FRAGMENT_BIT;

			shaderStages.push_back(fragmentStageInfo);
		}

		if (info.geometryModulePath != nullptr)
		{
			if (device->shaderModules.find(info.geometryModulePath) != device->shaderModules.end())
			{
				shader->geometryModule = device->shaderModules[info.geometryModulePath];
				shader->geometryModule->count++;
			}
			else
			{
				std::ifstream geometryFile(info.geometryModulePath, std::ios::in | std::ios::binary | std::ios::ate);
				std::streamsize size = geometryFile.tellg();
				geometryFile.seekg(0, std::ios::beg);

				std::vector<char> buffer(size);
				geometryFile.read(buffer.data(), size);

				ShaderModuleCreateInfo geometryInfo = {};
				geometryInfo.code = buffer.data();
				geometryInfo.codeSize = size;

				shader->geometryModule = cobalt_vkb_create_shadermodule(device, geometryInfo);
			}

			VkPipelineShaderStageCreateInfo geometryStageInfo = {};
			geometryStageInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
			geometryStageInfo.flags = 0;
			geometryStageInfo.pNext = nullptr;
			geometryStageInfo.module = shader->geometryModule->shaderModule;
			geometryStageInfo.pName = "Geometry Shader";
			geometryStageInfo.pSpecializationInfo = nullptr;
			geometryStageInfo.stage = VK_SHADER_STAGE_GEOMETRY_BIT;

			shaderStages.push_back(geometryStageInfo);
		}

		if (info.tesselationControlModulePath != nullptr)
		{
			if (device->shaderModules.find(info.tesselationControlModulePath) != device->shaderModules.end())
			{
				shader->tesselationControlModule = device->shaderModules[info.tesselationControlModulePath];
				shader->tesselationControlModule->count++;
			}
			else
			{
				std::ifstream tessControlFile(info.tesselationControlModulePath, std::ios::in | std::ios::binary | std::ios::ate);
				std::streamsize size = tessControlFile.tellg();
				tessControlFile.seekg(0, std::ios::beg);

				std::vector<char> buffer(size);
				tessControlFile.read(buffer.data(), size);

				ShaderModuleCreateInfo tessControlInfo = {};
				tessControlInfo.code = buffer.data();
				tessControlInfo.codeSize = size;

				shader->tesselationControlModule = cobalt_vkb_create_shadermodule(device, tessControlInfo);
			}

			VkPipelineShaderStageCreateInfo tessControlStageInfo = {};
			tessControlStageInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
			tessControlStageInfo.flags = 0;
			tessControlStageInfo.pNext = nullptr;
			tessControlStageInfo.module = shader->tesselationControlModule->shaderModule;
			tessControlStageInfo.pName = "Tesselation Control Shader";
			tessControlStageInfo.pSpecializationInfo = nullptr;
			tessControlStageInfo.stage = VK_SHADER_STAGE_TESSELLATION_CONTROL_BIT;

			shaderStages.push_back(tessControlStageInfo);
		}

		if (info.tesselationEvalModulePath != nullptr)
		{
			if (device->shaderModules.find(info.tesselationEvalModulePath) != device->shaderModules.end())
			{
				shader->tesselationEvalModule = device->shaderModules[info.tesselationEvalModulePath];
				shader->tesselationEvalModule->count++;
			}
			else
			{
				std::ifstream tessEvalFile(info.tesselationEvalModulePath, std::ios::in | std::ios::binary | std::ios::ate);
				std::streamsize size = tessEvalFile.tellg();
				tessEvalFile.seekg(0, std::ios::beg);

				std::vector<char> buffer(size);
				tessEvalFile.read(buffer.data(), size);

				ShaderModuleCreateInfo tessEvalInfo = {};
				tessEvalInfo.code = buffer.data();
				tessEvalInfo.codeSize = size;

				shader->tesselationEvalModule = cobalt_vkb_create_shadermodule(device, tessEvalInfo);
			}

			VkPipelineShaderStageCreateInfo tessEvalStageInfo = {};
			tessEvalStageInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
			tessEvalStageInfo.flags = 0;
			tessEvalStageInfo.pNext = nullptr;
			tessEvalStageInfo.module = shader->tesselationEvalModule->shaderModule;
			tessEvalStageInfo.pName = "Tesselation Eval Shader";
			tessEvalStageInfo.pSpecializationInfo = nullptr;
			tessEvalStageInfo.stage = VK_SHADER_STAGE_TESSELLATION_EVALUATION_BIT;

			shaderStages.push_back(tessEvalStageInfo);
		}
	}
	else if(info.computeModulePath != nullptr)
	{
		// Compute shader
		if (device->shaderModules.find(info.computeModulePath) != device->shaderModules.end())
		{
			shader->computeModule = device->shaderModules[info.computeModulePath];
			shader->computeModule->count++;
		}
		else
		{
			std::ifstream computeFile(info.computeModulePath, std::ios::in | std::ios::binary | std::ios::ate);
			std::streamsize size = computeFile.tellg();
			computeFile.seekg(0, std::ios::beg);

			std::vector<char> buffer(size);
			computeFile.read(buffer.data(), size);

			ShaderModuleCreateInfo computeInfo = {};
			computeInfo.code = buffer.data();
			computeInfo.codeSize = size;

			shader->computeModule = cobalt_vkb_create_shadermodule(device, computeInfo);
		}

		VkPipelineShaderStageCreateInfo computeStageInfo = {};
		computeStageInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		computeStageInfo.flags = 0;
		computeStageInfo.pNext = nullptr;
		computeStageInfo.module = shader->computeModule->shaderModule;
		computeStageInfo.pName = "Compute Shader";
		computeStageInfo.pSpecializationInfo = nullptr;
		computeStageInfo.stage = VK_SHADER_STAGE_COMPUTE_BIT;

		shaderStages.push_back(computeStageInfo);
	}

	shader->pass = info.pass;
	shader->subPassIndex = info.subPassIndex;

	std::vector<VkDescriptorSetLayout> vkLayouts(info.layoutInfo.setCount);
	for(int i = 0; i < info.layoutInfo.setCount; i++)
	{
		VkDescriptorSetLayoutCreateInfo setInfo = {};
		setInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
		setInfo.flags = 0;
		setInfo.pNext = nullptr;

		uint32_t bindingCount = info.layoutInfo.setInfos[i].bindingCount;
		ShaderLayoutBindingCreateInfo* bindings = info.layoutInfo.setInfos[i].bindingInfos;

		setInfo.bindingCount = bindingCount;
		
		std::vector<VkDescriptorSetLayoutBinding> vkBindings(bindingCount);

		for(uint32_t j = 0; j < bindingCount; j++)
		{
			auto [bindingIndex, type, descriptorCount, stageFlags] = bindings[j];

			VkDescriptorSetLayoutBinding vkBinding = {};
			vkBinding.binding = bindingIndex;
			vkBinding.descriptorCount = descriptorCount;
			vkBinding.descriptorType = static_cast<VkDescriptorType>(type);
			vkBinding.stageFlags = stageFlags;

			vkBindings.push_back(vkBinding);
		}
		
		setInfo.pBindings = vkBindings.data();

		VkDescriptorSetLayout layout;
		device->functionTable.createDescriptorSetLayout(&setInfo, device->device.allocation_callbacks, &layout);
		vkLayouts.push_back(layout);
	}

	VkPipelineLayoutCreateInfo pipelineLayoutInfo = {};
	pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
	pipelineLayoutInfo.flags = 0;
	pipelineLayoutInfo.pNext = nullptr;
	pipelineLayoutInfo.setLayoutCount = static_cast<uint32_t>(vkLayouts.size());
	pipelineLayoutInfo.pSetLayouts = vkLayouts.data();

	device->functionTable.createPipelineLayout(&pipelineLayoutInfo, device->device.allocation_callbacks, &shader->pipelineLayout);
	
	VkGraphicsPipelineCreateInfo pipelineInfo = {};
	pipelineInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
	pipelineInfo.flags = 0;
	pipelineInfo.pNext = nullptr;
	pipelineInfo.layout = shader->pipelineLayout;
	pipelineInfo.basePipelineHandle = VK_NULL_HANDLE;
	pipelineInfo.basePipelineIndex = 0;
	pipelineInfo.stageCount = static_cast<uint32_t>(shaderStages.size());
	pipelineInfo.pStages = shaderStages.data();
	pipelineInfo.renderPass = info.pass->pass;
	pipelineInfo.subpass = info.subPassIndex;

	device->functionTable.createGraphicsPipelines(device->pipelineCache, 1, &pipelineInfo, device->device.allocation_callbacks, &shader->pipeline);

	return shader;
}
