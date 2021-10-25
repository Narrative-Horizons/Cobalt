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
		for(auto view : imageViews.value())
		{
			ImageView* v = new ImageView();
			v->amount = 1;
			v->imageView = view;

			swapchain->frameViews.push_back(v);
		}
	}
	else
	{
		vkb::destroy_swapchain(swapchain->swapchain);
		delete swapchain;

		return nullptr;
	}

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

VK_BINDING_EXPORT ImageView* cobalt_vkb_get_swapchain_image_view(Swapchain* swapchain, uint32_t index)
{
	if(swapchain)
	{
		return swapchain->frameViews[index];
	}

	return nullptr;
}


VK_BINDING_EXPORT RenderPass* cobalt_vkb_create_renderpass(Device* device, RenderPassCreateInfo info)
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

	VkRenderPass renderpass;
	if (!device->functionTable.createRenderPass(&renderPassInfo, device->device.allocation_callbacks, &renderpass) == VK_SUCCESS)
	{
		return nullptr;
	}

	RenderPass* pass = new RenderPass();
	pass->pass = renderpass;
	
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

	CommandBuffer* buffer = new CommandBuffer();
	buffer->amount = allocInfo.commandBufferCount;
	buffer->buffers = new VkCommandBuffer[buffer->amount];
	
	if (!device->functionTable.allocateCommandBuffers(&allocInfo, buffer->buffers) == VK_SUCCESS)
	{
		return nullptr;
	}

	
	buffer->pool = allocInfo.commandPool;
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
			delete buffer->buffers;
			delete buffer;
		}
		
		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_begin_commandbuffer(Device* device, CommandBuffer* buffer, const uint32_t index)
{
	if(buffer)
	{
		VkCommandBufferBeginInfo beginInfo = {};
		beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
		beginInfo.flags = 0;
		beginInfo.pNext = nullptr;
		beginInfo.pInheritanceInfo = nullptr;
		
		device->functionTable.beginCommandBuffer(buffer->buffers[index], &beginInfo);

		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_command_begin_renderpass(Device* device, CommandBuffer* buffer, const uint32_t index, RenderPass* pass, Framebuffer* framebuffer)
{
	if (buffer)
	{
		VkRenderPassBeginInfo renderPassInfo = {};
		renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
		renderPassInfo.pNext = nullptr;
		renderPassInfo.renderPass = pass->pass;
		renderPassInfo.framebuffer = framebuffer->framebuffer;
		renderPassInfo.renderArea.offset = { 0, 0 };
		renderPassInfo.renderArea.extent = { 1280, 720 };

		VkClearValue clearColor = { {{0.0f, 0.0f, 0.0f, 1.0}} };
		renderPassInfo.clearValueCount = 1;
		renderPassInfo.pClearValues = &clearColor;

		device->functionTable.cmdBeginRenderPass(buffer->buffers[index], &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);

		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_command_bind_pipeline(Device* device, CommandBuffer* buffer, const uint32_t index, Shader* shader)
{
	if(buffer)
	{
		device->functionTable.cmdBindPipeline(buffer->buffers[index], VK_PIPELINE_BIND_POINT_GRAPHICS, shader->pipeline);
		
		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_command_draw(Device* device, CommandBuffer* buffer, const uint32_t index)
{
	if (buffer)
	{
		device->functionTable.cmdDraw(buffer->buffers[index], 3, 1, 0, 0);

		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_command_end_renderpass(Device* device, CommandBuffer* buffer, const uint32_t index)
{
	if (buffer)
	{
		device->functionTable.cmdEndRenderPass(buffer->buffers[index]);

		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_commandbuffer_end(Device* device, CommandBuffer* buffer, const uint32_t index)
{
	if (buffer)
	{
		device->functionTable.endCommandBuffer(buffer->buffers[index]);

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

	if(!device->functionTable.createShaderModule(&createInfo, device->device.allocation_callbacks, &vkshaderModule) == VK_SUCCESS)
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
		vertexStageInfo.pName = "main";
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
			fragmentStageInfo.pName = "main";
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

	std::vector<VkDescriptorSetLayout> vkLayouts;
	std::vector<DescriptorSetLayout> layouts;

	vkLayouts.reserve(info.layoutInfo.setCount);
	for(int i = 0; i < info.layoutInfo.setCount; i++)
	{
		VkDescriptorSetLayoutCreateInfo setInfo = {};
		setInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
		setInfo.flags = 0;
		setInfo.pNext = nullptr;

		uint32_t bindingCount = info.layoutInfo.setInfos[i].bindingCount;
		ShaderLayoutBindingCreateInfo* bindings = info.layoutInfo.setInfos[i].bindingInfos;

		setInfo.bindingCount = bindingCount;
		
		std::vector<VkDescriptorSetLayoutBinding> vkBindings;
		std::vector<ShaderLayoutBinding> bnds;

		vkBindings.reserve(bindingCount);

		for(uint32_t j = 0; j < bindingCount; j++)
		{
			auto [bindingIndex, type, descriptorCount, stageFlags] = bindings[j];

			VkDescriptorSetLayoutBinding vkBinding = {};
			vkBinding.binding = bindingIndex;
			vkBinding.descriptorCount = descriptorCount;
			vkBinding.descriptorType = static_cast<VkDescriptorType>(type);
			vkBinding.stageFlags = stageFlags;

			vkBindings.push_back(vkBinding);
			
			ShaderLayoutBinding binding;
			binding.bindingIndex = bindingIndex;
			binding.descriptorCount = descriptorCount;
			binding.type = type;
			binding.stageFlags = stageFlags;

			bnds.push_back(binding);
		}
		
		setInfo.pBindings = vkBindings.data();

		VkDescriptorSetLayout layout;
		device->functionTable.createDescriptorSetLayout(&setInfo, device->device.allocation_callbacks, &layout);
		vkLayouts.push_back(layout);

		DescriptorSetLayout setLayout;
		setLayout.bindings = bnds;
		setLayout.layout = layout;
	}

	VkPipelineLayoutCreateInfo pipelineLayoutInfo = {};
	pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
	pipelineLayoutInfo.flags = 0;
	pipelineLayoutInfo.pNext = nullptr;
	pipelineLayoutInfo.setLayoutCount = static_cast<uint32_t>(vkLayouts.size());
	pipelineLayoutInfo.pSetLayouts = vkLayouts.data();
	pipelineLayoutInfo.pushConstantRangeCount = 0;

	device->functionTable.createPipelineLayout(&pipelineLayoutInfo, device->device.allocation_callbacks, &shader->pipelineLayout.layout);
	
	VkGraphicsPipelineCreateInfo pipelineInfo = {};
	pipelineInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
	pipelineInfo.flags = 0;
	pipelineInfo.pNext = nullptr;
	pipelineInfo.layout = shader->pipelineLayout.layout;
	pipelineInfo.basePipelineHandle = VK_NULL_HANDLE;
	pipelineInfo.basePipelineIndex = 0;
	pipelineInfo.stageCount = static_cast<uint32_t>(shaderStages.size());
	pipelineInfo.pStages = shaderStages.data();
	pipelineInfo.renderPass = info.pass->pass;
	pipelineInfo.subpass = info.subPassIndex;

	VkPipelineVertexInputStateCreateInfo vertexInfo = {};
	vertexInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
	vertexInfo.flags = 0;
	vertexInfo.pNext = nullptr;
	vertexInfo.vertexBindingDescriptionCount = 0;
	vertexInfo.vertexAttributeDescriptionCount = 0;

	VkPipelineInputAssemblyStateCreateInfo inputAssembly = {};
	inputAssembly.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
	inputAssembly.flags = 0;
	inputAssembly.pNext = nullptr;
	inputAssembly.topology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
	inputAssembly.primitiveRestartEnable = false;

	VkViewport viewport = {};
	viewport.x = 0.0f;
	viewport.y = 0.0f;
	viewport.width = 1280.0f;
	viewport.height = 720.0f;
	viewport.minDepth = 0.0f;
	viewport.maxDepth = 1.0f;

	VkRect2D scissor = {};
	scissor.offset = { 0, 0 };
	scissor.extent = { 1280, 720 };

	VkPipelineViewportStateCreateInfo viewportState = {};
	viewportState.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
	viewportState.flags = 0;
	viewportState.pNext = nullptr;
	viewportState.viewportCount = 1;
	viewportState.pViewports = &viewport;
	viewportState.scissorCount = 1;
	viewportState.pScissors = &scissor;

	VkPipelineRasterizationStateCreateInfo rasterizer = {};
	rasterizer.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
	rasterizer.flags = 0;
	rasterizer.pNext = nullptr;
	rasterizer.depthClampEnable = VK_FALSE;
	rasterizer.rasterizerDiscardEnable = VK_FALSE;
	rasterizer.polygonMode = VK_POLYGON_MODE_FILL;
	rasterizer.lineWidth = 1.0f;
	rasterizer.cullMode = VK_CULL_MODE_BACK_BIT;
	rasterizer.frontFace = VK_FRONT_FACE_CLOCKWISE;
	rasterizer.depthBiasEnable = VK_FALSE;

	VkPipelineMultisampleStateCreateInfo multisampling = {};
	multisampling.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
	multisampling.flags = 0;
	multisampling.pNext = nullptr;
	multisampling.sampleShadingEnable = VK_FALSE;
	multisampling.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;

	VkPipelineColorBlendAttachmentState colorBlendAttachment = {};
	colorBlendAttachment.colorWriteMask = VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT | VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
	colorBlendAttachment.blendEnable = VK_FALSE;

	VkPipelineColorBlendStateCreateInfo colorBlending = {};
	colorBlending.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
	colorBlending.flags = 0;
	colorBlending.pNext = nullptr;
	colorBlending.logicOpEnable = VK_FALSE;
	colorBlending.logicOp = VK_LOGIC_OP_COPY;
	colorBlending.attachmentCount = 1;
	colorBlending.pAttachments = &colorBlendAttachment;
	colorBlending.blendConstants[0] = 0.0f;
	colorBlending.blendConstants[1] = 0.0f;
	colorBlending.blendConstants[2] = 0.0f;
	colorBlending.blendConstants[3] = 0.0f;

	pipelineInfo.pVertexInputState = &vertexInfo;
	pipelineInfo.pInputAssemblyState = &inputAssembly;
	pipelineInfo.pViewportState = &viewportState;
	pipelineInfo.pRasterizationState = &rasterizer;
	pipelineInfo.pMultisampleState = &multisampling;
	pipelineInfo.pDepthStencilState = nullptr;
	pipelineInfo.pColorBlendState = &colorBlending;
	pipelineInfo.pDynamicState = nullptr;

	device->functionTable.createGraphicsPipelines(device->pipelineCache, 1, &pipelineInfo, device->device.allocation_callbacks, &shader->pipeline);
	shader->device = device;

	return shader;
}

VK_BINDING_EXPORT DescriptorSet* cobalt_vkb_allocate_descriptors(Shader* shader)
{
	PipelineLayout& layout = shader->pipelineLayout;
	DynamicDescriptorSetPool& pool = shader->descPool;
	const auto setCount = layout.sets.size();
	DescriptorSet* results = new DescriptorSet[setCount];

	std::vector<VkDescriptorSetAllocateInfo> allocate;

	for (size_t i = 0; i < setCount; ++i)
	{
		const auto& set = layout.sets[i];
		std::unordered_map<VkDescriptorType, size_t> requiredCounts;
		for (auto& binding : set.bindings)
		{
			requiredCounts[(VkDescriptorType)binding.type] += binding.descriptorCount;
		}

		bool successful = false;
		for (auto& fixed : pool.pools)
		{
			if (fixed.maxSets == fixed.allocatedSets)
			{
				continue;
			}

			successful = fixed.combinedImageSamplerCount >= requiredCounts[VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER]
				&& fixed.dynamicStorageBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC]
				&& fixed.dynamicStorageBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC]
				&& fixed.inputAttachmentCount >= requiredCounts[VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT]
				&& fixed.sampledImageCount >= requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE]
				&& fixed.samplerCount >= requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLER]
				&& fixed.storageBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER]
				&& fixed.storageImageCount >= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_IMAGE]
				&& fixed.storageTexelBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER]
				&& fixed.uniformBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER]
				&& fixed.uniformTexelBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER];

			if (successful)
			{
				VkDescriptorSetAllocateInfo info;
				info.descriptorPool = fixed.pool;
				info.descriptorSetCount = 1;
				info.pSetLayouts = &set.layout;
				info.pNext = VK_NULL_HANDLE;
				info.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;

				const auto result = shader->device->functionTable.allocateDescriptorSets(&info, &(results[i].set));
				if (result != VK_SUCCESS)
				{
					// well shit
					// TODO: figure out what to do here
					break;
				}

				fixed.combinedImageSamplerCount -= requiredCounts[VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER];
				fixed.dynamicStorageBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC];
				fixed.dynamicUniformBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC];
				fixed.inputAttachmentCount -= requiredCounts[VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT];
				fixed.sampledImageCount -= requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE];
				fixed.samplerCount -= requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLER];
				fixed.storageBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER];
				fixed.storageImageCount -= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_IMAGE];
				fixed.storageTexelBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER];
				fixed.uniformBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER];
				fixed.uniformTexelBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER];
				
				++fixed.allocatedSets;
				
				break;
			}
		}

		if (!successful)
		{
			// allocate new pool
			VkDescriptorPoolCreateInfo poolInfo;
			std::vector<VkDescriptorPoolSize> sizes;

			for (const auto& [type, count] : requiredCounts)
			{
				if (count == 0)
				{
					continue;
				}

				VkDescriptorPoolSize sz;
				sz.descriptorCount = count;
				sz.type = type;

				sizes.push_back(sz);
			}

			poolInfo.maxSets = 32;
			poolInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
			poolInfo.pNext = VK_NULL_HANDLE;
			poolInfo.poolSizeCount = static_cast<uint32_t>(sizes.size());
			poolInfo.pPoolSizes = sizes.data();

			FixedDescriptorSetPool p;
			const auto result = shader->device->functionTable.createDescriptorPool(&poolInfo, shader->device->device.allocation_callbacks, &(p.pool));

			if (result != VK_SUCCESS)
			{
				// what the dog doing
				continue;
			}

			constexpr uint32_t multiplier = 32;
			p.maxSets = poolInfo.maxSets;
			p.samplerCapacity = requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLER] * multiplier;
			p.combinedImageSamplerCapacity = requiredCounts[VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER] * multiplier;
			p.sampledImageCapacity = requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE] * multiplier;

			pool.pools.push_back(p);
		}
	}

	return nullptr;
}

VK_BINDING_EXPORT Image* cobalt_vkb_create_image(Device* device, ImageCreateInfo info, const char* name, uint32_t frame)
{
	const std::string objectName = std::string(name) + "_" + std::to_string(frame);
	if(device->images.find(objectName) != device->images.end())
	{
		Image* i = device->images[objectName];
		i->amount++;
		return i;
	}
	
	Image* image = new Image();
	image->amount = 1;

	VkImageCreateInfo imageInfo = {};
	imageInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
	imageInfo.flags = 0;
	imageInfo.pNext = nullptr;
	imageInfo.arrayLayers = info.arrayLayers;
	imageInfo.extent = VkExtent3D{ info.width, info.height, info.depth };
	imageInfo.imageType = static_cast<VkImageType>(info.imageType);
	imageInfo.format = static_cast<VkFormat>(info.format);
	imageInfo.queueFamilyIndexCount = info.queueFamilyIndexCount;
	imageInfo.pQueueFamilyIndices = info.queueFamilyIndices;
	imageInfo.mipLevels = info.mipLevels;
	imageInfo.initialLayout = static_cast<VkImageLayout>(info.initialLayout);
	imageInfo.sharingMode = static_cast<VkSharingMode>(info.sharingMode);
	imageInfo.tiling = static_cast<VkImageTiling>(info.tiling);
	imageInfo.samples = static_cast<VkSampleCountFlagBits>(info.samples);
	imageInfo.usage = info.usage;
	
	device->functionTable.createImage(&imageInfo, device->device.allocation_callbacks, &image->image);

	device->images[objectName] = image;
	
	return image;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_image(Device* device, Image* image)
{
	if (image)
	{
		image->amount--;

		if (image->amount <= 0)
		{

			device->functionTable.destroyImage(image->image, device->device.allocation_callbacks);
			delete image;
		}

		return true;
	}

	return false;
}


VK_BINDING_EXPORT ImageView* cobalt_vkb_create_imageview(Device* device, ImageViewCreateInfo info, const char* name, uint32_t frame)
{
	const std::string objectName = std::string(name) + "_" + std::to_string(frame);
	if (device->imageViews.find(objectName) != device->imageViews.end())
	{
		ImageView* i = device->imageViews[objectName];
		i->amount++;
		return i;
	}
	
	ImageView* view = new ImageView();
	view->amount = 1;

	VkImageViewCreateInfo viewInfo = {};
	viewInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
	viewInfo.flags = 0;
	viewInfo.pNext = nullptr;
	viewInfo.format = static_cast<VkFormat>(info.format);
	viewInfo.image = info.image->image;
	viewInfo.subresourceRange = VkImageSubresourceRange
	{
		info.aspectMask,
		info.baseMipLevel,
		info.levelCount,
		info.baseArrayLayer,
		info.layerCount
	};
	
	viewInfo.viewType = static_cast<VkImageViewType>(info.viewType);
	
	device->functionTable.createImageView(&viewInfo, device->device.allocation_callbacks, &view->imageView);
	device->imageViews[objectName] = view;
	
	return view;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_imageview(Device* device, ImageView* view)
{
	if (view)
	{
		view->amount--;
		
		if (view->amount <= 0)
		{
			device->functionTable.destroyImageView(view->imageView, device->device.allocation_callbacks);
			delete view;
		}

		return true;
	}

	return false;
}

VK_BINDING_EXPORT Framebuffer* cobalt_vkb_create_framebuffer(Device* device, FramebufferCreateInfo info)
{
	Framebuffer* framebuffer = new Framebuffer();

	VkFramebufferCreateInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
	bufferInfo.flags = 0;
	bufferInfo.pNext = nullptr;
	bufferInfo.attachmentCount = info.attachmentCount;
	bufferInfo.width = info.width;
	bufferInfo.height = info.height;
	bufferInfo.layers = info.layers;
	bufferInfo.renderPass = info.pass->pass;

	std::vector<VkImageView> attachments;
	attachments.reserve(info.attachmentCount);
	for (int i = 0; i < info.attachmentCount; i++)
	{
		info.attachments[i]->amount++;
		attachments.push_back(info.attachments[i]->imageView);
	}
	bufferInfo.pAttachments = attachments.data();

	device->functionTable.createFramebuffer(&bufferInfo, device->device.allocation_callbacks, &framebuffer->framebuffer);

	return framebuffer;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_framebuffer(Device* device, Framebuffer* framebuffer)
{
	if (framebuffer)
	{
		device->functionTable.destroyFramebuffer(framebuffer->framebuffer, device->device.allocation_callbacks);

		delete framebuffer;

		return true;
	}

	return false;
}

VK_BINDING_EXPORT Semaphore* cobalt_vkb_create_semaphore(Device* device, SemaphoreCreateInfo info)
{
	VkSemaphoreCreateInfo semaphoreInfo = {};
	semaphoreInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
	semaphoreInfo.flags = info.flags;
	semaphoreInfo.pNext = nullptr;

	VkSemaphore s;

	if(!device->functionTable.createSemaphore(&semaphoreInfo, device->device.allocation_callbacks, &s) == VK_SUCCESS)
	{
		return nullptr;
	}

	Semaphore* semaphore = new Semaphore();
	semaphore->semaphore = s;

	return semaphore;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_semaphore(Device* device, Semaphore* semaphore)
{
	if (semaphore)
	{
		device->functionTable.destroySemaphore(semaphore->semaphore, device->device.allocation_callbacks);

		delete semaphore;

		return true;
	}

	return false;
}

VK_BINDING_EXPORT uint32_t cobalt_vkb_acquire_next_image(Device* device, Swapchain* swapchain, Semaphore* semaphore)
{
	if(swapchain)
	{
		uint32_t imageIndex;
		device->functionTable.acquireNextImageKHR(swapchain->swapchain, UINT64_MAX, semaphore->semaphore, VK_NULL_HANDLE, &imageIndex);

		return imageIndex;
	}

	return UINT32_MAX;
}

VK_BINDING_EXPORT bool cobalt_vkb_submit_queue(Device* device, SubmitInfo info)
{
	VkSubmitInfo submitInfo = {};
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submitInfo.pNext = nullptr;

	submitInfo.waitSemaphoreCount = info.waitSemaphoreCount;
	submitInfo.signalSemaphoreCount = info.signalSemaphoreCount;
	
	VkSemaphore* waitSems = new VkSemaphore[info.waitSemaphoreCount];
	for(int i = 0; i < info.waitSemaphoreCount; i++)
	{
		waitSems[i] = info.waitSemaphores[i]->semaphore;
	}

	VkSemaphore* signalSems = new VkSemaphore[info.signalSemaphoreCount];
	for(int i = 0; i < info.signalSemaphoreCount; i++)
	{
		signalSems[i] = info.signalSemaphores[i]->semaphore;
	}

	submitInfo.pWaitSemaphores = waitSems;
	submitInfo.pSignalSemaphores = signalSems;
	submitInfo.pWaitDstStageMask = info.waitDstStageMask;

	uint32_t amount = 0;
	std::vector<VkCommandBuffer> uploadBuffers;
	
	for(int i = 0; i < info.commandbufferCount; i++)
	{
		IndexedCommandBuffers* buffers = info.commandbuffer[i];
		amount += buffers->amount - 1;

		for(int j = 0; j < buffers->amount - 1; j++)
		{
			uploadBuffers.push_back(buffers->commandbuffers->buffers[buffers->bufferIndices[j]]);
		}
	}
	
	submitInfo.commandBufferCount = info.commandbufferCount;
	submitInfo.pCommandBuffers = uploadBuffers.data();

	if(!device->functionTable.queueSubmit(device->graphicsQueue, 1, &submitInfo, VK_NULL_HANDLE) == VK_SUCCESS)
	{
		return false;
	}
	
	return true;
}

VK_BINDING_EXPORT bool cobalt_vkb_queue_present(Device* device, PresentInfo info)
{
	VkPresentInfoKHR presentInfo = {};
	presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
	presentInfo.pNext = nullptr;
	presentInfo.waitSemaphoreCount = info.waitSemaphoreCount;

	VkSemaphore* waitSems = new VkSemaphore[info.waitSemaphoreCount];
	for (int i = 0; i < info.waitSemaphoreCount; i++)
	{
		waitSems[i] = info.waitSemaphores[i]->semaphore;
	}

	presentInfo.pWaitSemaphores = waitSems;

	VkSwapchainKHR* swaps = new VkSwapchainKHR[info.swapchainCount];
	for (int i = 0; i < info.swapchainCount; i++)
	{
		swaps[i] = info.swapchains[i]->swapchain;
	}
	
	presentInfo.pSwapchains = swaps;

	presentInfo.pImageIndices = info.imageIndices;
	presentInfo.pResults = nullptr;

	device->functionTable.queuePresentKHR(device->presentQueue, &presentInfo);

	return true;
}
