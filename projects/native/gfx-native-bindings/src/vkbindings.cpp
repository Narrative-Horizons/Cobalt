#include <VkBootstrap.h>

#define VMA_IMPLEMENTATION
#define VMA_STATIC_VULKAN_FUNCTIONS 0
#include <vma/vk_mem_alloc.h>

#include <GLFW/glfw3.h>

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

	for (size_t i = 0; i < info.enabledLayerCount; ++i)
	{
		bldr.enable_layer(info.enabledLayers[i]);
	}

	for (size_t i = 0; i < info.enabledExtensionCount; ++i)
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
		destroy_instance(device.instance);
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
		destroy_surface(device.instance, device.surface);
		destroy_instance(device.instance);
		return nullptr;
	}
	device.physicalDevice = physicalDeviceResult.value();

	const auto deviceResult = vkb::DeviceBuilder(device.physicalDevice)
		.build();

	if (!deviceResult)
	{
		destroy_surface(device.instance, device.surface);
		destroy_instance(device.instance);
		return nullptr;
	}

	device.device = deviceResult.value();

	auto gq = device.device.get_queue(vkb::QueueType::graphics);
	if (!gq.has_value())
	{
		destroy_surface(device.instance, device.surface);
		destroy_instance(device.instance);
		return nullptr;
	}
	device.graphicsQueue = gq.value();

	auto pq = device.device.get_queue(vkb::QueueType::present);
	if (!pq.has_value())
	{
		destroy_surface(device.instance, device.surface);
		destroy_instance(device.instance);
		return nullptr;
	}

	device.presentQueue = pq.value();

	auto cq = device.device.get_queue(vkb::QueueType::compute);
	if (!cq.has_value())
	{
		destroy_surface(device.instance, device.surface);
		destroy_instance(device.instance);
		return nullptr;
	}

	device.computeQueue = cq.value();

	auto tq = device.device.get_queue(vkb::QueueType::transfer);
	if (!tq.has_value())
	{
		destroy_surface(device.instance, device.surface);
		destroy_instance(device.instance);
		return nullptr;
	}

	device.tranferQueue = tq.value();

	device.device = deviceResult.value();
	device.functionTable = device.device.make_table();

	VmaAllocatorCreateInfo allocatorInfo = {};
	allocatorInfo.flags = 0;
	allocatorInfo.device = device.device;

	VmaVulkanFunctions funcs = {};
	funcs.vkGetPhysicalDeviceProperties = reinterpret_cast<PFN_vkGetPhysicalDeviceProperties>(device.instance.fp_vkGetInstanceProcAddr(
		device.instance.instance, "vkGetPhysicalDeviceProperties"));
	funcs.vkGetPhysicalDeviceMemoryProperties = reinterpret_cast<PFN_vkGetPhysicalDeviceMemoryProperties>(device.instance.fp_vkGetInstanceProcAddr(
		device.instance.instance, "vkGetPhysicalDeviceMemoryProperties"));
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
	funcs.vkGetPhysicalDeviceMemoryProperties2KHR = reinterpret_cast<PFN_vkGetPhysicalDeviceMemoryProperties2KHR>(device.instance.fp_vkGetInstanceProcAddr(
		device.instance.instance, "vkGetPhysicalDeviceMemoryProperties2KHR"));

	allocatorInfo.pVulkanFunctions = &funcs;
	allocatorInfo.instance = device.instance;
	allocatorInfo.physicalDevice = device.physicalDevice;
	
	vmaCreateAllocator(&allocatorInfo, &device.allocator);

	VkCommandPoolCreateInfo poolInfo = {};
	poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
	poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;
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
		destroy_device(device->device);
		destroy_surface(device->instance, device->surface);
		destroy_instance(device->instance);
		
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
		destroy_swapchain(swapchain->swapchain);
		delete swapchain;

		return nullptr;
	}

	return swapchain;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_swapchain(Swapchain* swapchain)
{
	if (swapchain)
	{
		destroy_swapchain(swapchain->swapchain);
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
	VkRenderPassCreateInfo renderPassInfo;
	renderPassInfo.pNext = nullptr;
	renderPassInfo.flags = 0;
	renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;

	renderPassInfo.attachmentCount = info.attachmentCount;
	renderPassInfo.pAttachments = info.attachments;

	renderPassInfo.subpassCount = info.subpassCount;
	renderPassInfo.pSubpasses = info.subpasses;
	
	renderPassInfo.dependencyCount = info.dependencyCount;
	renderPassInfo.pDependencies = info.dependencies;

	VkRenderPass renderpass;
	if (device->functionTable.createRenderPass(&renderPassInfo, device->device.allocation_callbacks, &renderpass) != VK_SUCCESS)
	{
		return nullptr;
	}

	RenderPass* pass = new RenderPass();
	pass->pass = renderpass;
	
	return pass;
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
	
	if (device->functionTable.allocateCommandBuffers(&allocInfo, buffer->buffers) != VK_SUCCESS)
	{
		delete[] buffer->buffers;
		delete buffer;
		
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
			delete[] buffer->buffers;
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
		VkCommandBufferBeginInfo beginInfo;
		beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
		beginInfo.flags = 0;
		beginInfo.pNext = nullptr;
		beginInfo.pInheritanceInfo = nullptr;
		
		device->functionTable.beginCommandBuffer(buffer->buffers[index], &beginInfo);

		return true;
	}

	return false;
}

// TODO: Use RenderPassBeginInfo & SubpassContents
VK_BINDING_EXPORT bool cobalt_vkb_command_begin_renderpass(Device* device, CommandBuffer* buffer, const uint32_t index, RenderPass* pass, Framebuffer* framebuffer)
{
	if (buffer)
	{
		VkRenderPassBeginInfo renderPassInfo;
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

VK_BINDING_EXPORT bool cobalt_vkb_command_bind_pipeline(Device* device, CommandBuffer* buffer, const uint32_t bindpoint, const uint32_t index, Shader* shader)
{
	if(buffer)
	{
		device->functionTable.cmdBindPipeline(buffer->buffers[index], static_cast<VkPipelineBindPoint>(bindpoint), shader->pipeline);
		
		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_command_draw(Device* device, CommandBuffer* buffer, const uint32_t index, const uint32_t vertexCount, const uint32_t instanceCount,
	const uint32_t firstVertex, const uint32_t firstInstance)
{
	if (buffer)
	{
		device->functionTable.cmdDraw(buffer->buffers[index], vertexCount, instanceCount, firstVertex, firstInstance);

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

VK_BINDING_EXPORT bool cobalt_vkb_command_bind_vertex_buffers(Device* device, CommandBuffer* buffer, const uint32_t index, const uint32_t firstBinding,
	const uint32_t bindingCount, Buffer** buffers, uint64_t* offsets)
{
	if (buffer)
	{
		std::vector<VkBuffer> vkBuffers;
		for(uint32_t i = 0; i < bindingCount; i++)
		{
			vkBuffers.push_back(buffers[index]->buffer);
		}
		
		device->functionTable.cmdBindVertexBuffers(buffer->buffers[index], firstBinding, bindingCount, vkBuffers.data(), offsets);

		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_command_bind_index_buffer(Device* device, CommandBuffer* buffer, const uint32_t index, Buffer* indexBuffer, 
	const uint64_t offset, uint32_t indexType)
{
	if (buffer)
	{
		device->functionTable.cmdBindIndexBuffer(buffer->buffers[index], indexBuffer->buffer, offset, static_cast<VkIndexType>(indexType));

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
	VkBufferCreateInfo info;
	
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

	VkBuffer vkbuffer = VK_NULL_HANDLE;
	VmaAllocation allocation = VMA_NULL;
	
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

VK_BINDING_EXPORT ShaderModule* cobalt_vkb_create_shadermodule(Device* device, const ShaderModuleCreateInfo info)
{
	VkShaderModuleCreateInfo createInfo;
	createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	createInfo.flags = 0;
	createInfo.pNext = nullptr;
	createInfo.codeSize = info.codeSize;
	createInfo.pCode = reinterpret_cast<const uint32_t*>(info.code);

	VkShaderModule vkshaderModule;

	if(device->functionTable.createShaderModule(&createInfo, device->device.allocation_callbacks, &vkshaderModule) != VK_SUCCESS)
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
			geometryStageInfo.pName = "main";
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
			tessControlStageInfo.pName = "main";
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
			tessEvalStageInfo.pName = "main";
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
		computeStageInfo.pName = "main";
		computeStageInfo.pSpecializationInfo = nullptr;
		computeStageInfo.stage = VK_SHADER_STAGE_COMPUTE_BIT;

		shaderStages.push_back(computeStageInfo);
	}

	shader->pass = info.pass;
	shader->subPassIndex = info.subPassIndex;
	std::vector<VkDescriptorSetLayout> vkLayouts;
	std::vector<DescriptorSetLayout> layouts;

	vkLayouts.reserve(info.layoutInfo.setCount);
	for(uint32_t i = 0; i < info.layoutInfo.setCount; i++)
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
		layouts.push_back(setLayout);
	}

	shader->pipelineLayout.sets = layouts;

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

	/*VkVertexInputBindingDescription vbBinding = {0, sizeof(float) * 14, VK_VERTEX_INPUT_RATE_VERTEX};
	vertexInfo.pVertexBindingDescriptions = &vbBinding;*/

	/*std::vector<VkVertexInputAttributeDescription> vbAttrs;
	vbAttrs.emplace_back(VkVertexInputAttributeDescription{ 0, 0, VK_FORMAT_R32G32B32_SFLOAT, 0 });
	vbAttrs.emplace_back(VkVertexInputAttributeDescription{ 1, 0, VK_FORMAT_R32G32_SFLOAT, sizeof(float) * 3 });
	vbAttrs.emplace_back(VkVertexInputAttributeDescription{ 2, 0, VK_FORMAT_R32G32B32_SFLOAT, sizeof(float) * 5 });
	vbAttrs.emplace_back(VkVertexInputAttributeDescription{ 3, 0, VK_FORMAT_R32G32B32_SFLOAT, sizeof(float) * 8 });
	vbAttrs.emplace_back(VkVertexInputAttributeDescription{ 4, 0, VK_FORMAT_R32G32B32_SFLOAT, sizeof(float) * 11 });

	vertexInfo.pVertexAttributeDescriptions = vbAttrs.data();*/

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
	auto& [layout, sets] = shader->pipelineLayout;
	auto& [pools] = shader->descPool;
	const auto setCount = sets.size();
	DescriptorSet* results = new DescriptorSet[setCount];

	std::vector<VkDescriptorSetAllocateInfo> allocate;

	for (size_t i = 0; i < setCount; ++i)
	{
		auto& [layout, bindings] = sets[i];
		std::unordered_map<VkDescriptorType, size_t> requiredCounts;
		for (auto& [bindingIndex, type, descriptorCount, stageFlags] : bindings)
		{
			requiredCounts[static_cast<VkDescriptorType>(type)] += descriptorCount;
		}

		bool successful = false;
		for (auto& [pool, samplerCapacity, combinedImageSamplerCapacity, sampledImageCapacity, 
			storageImageCapacity, uniformTexelBufferCapacity, storageTexelBufferCapacity, uniformBufferCapacity, 
			storageBufferCapacity, dynamicUniformBufferCapacity, dynamicStorageBufferCapacity, inputAttachmentCapacity, 
			samplerCount, combinedImageSamplerCount, sampledImageCount, storageImageCount, uniformTexelBufferCount, 
			storageTexelBufferCount, uniformBufferCount, storageBufferCount, dynamicUniformBufferCount, dynamicStorageBufferCount, 
			inputAttachmentCount, maxSets, allocatedSets] : pools)
		{
			if (maxSets == allocatedSets)
			{
				continue;
			}

			successful = combinedImageSamplerCount >= requiredCounts[VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER]
				&& dynamicStorageBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC]
				&& dynamicStorageBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC]
				&& inputAttachmentCount >= requiredCounts[VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT]
				&& sampledImageCount >= requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE]
				&& samplerCount >= requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLER]
				&& storageBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER]
				&& storageImageCount >= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_IMAGE]
				&& storageTexelBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER]
				&& uniformBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER]
				&& uniformTexelBufferCount >= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER];

			if (successful)
			{
				VkDescriptorSetAllocateInfo info;
				info.descriptorPool = pool;
				info.descriptorSetCount = 1;
				info.pSetLayouts = &layout;
				info.pNext = VK_NULL_HANDLE;
				info.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;

				const auto result = shader->device->functionTable.allocateDescriptorSets(&info, &(results[i].set));
				if (result != VK_SUCCESS)
				{
					// well shit
					// TODO: figure out what to do here
					break;
				}

				combinedImageSamplerCount -= requiredCounts[VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER];
				dynamicStorageBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC];
				dynamicUniformBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC];
				inputAttachmentCount -= requiredCounts[VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT];
				sampledImageCount -= requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE];
				samplerCount -= requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLER];
				storageBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER];
				storageImageCount -= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_IMAGE];
				storageTexelBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER];
				uniformBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER];
				uniformTexelBufferCount -= requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER];
				
				++allocatedSets;
				
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
				sz.descriptorCount = static_cast<uint32_t>(count);
				sz.type = type;

				sizes.push_back(sz);
			}

			poolInfo.maxSets = 32;
			poolInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
			poolInfo.pNext = VK_NULL_HANDLE;
			poolInfo.poolSizeCount = static_cast<uint32_t>(sizes.size());
			poolInfo.pPoolSizes = sizes.data();
			poolInfo.flags = 0;

			FixedDescriptorSetPool p;
			const auto result = shader->device->functionTable.createDescriptorPool(&poolInfo, shader->device->device.allocation_callbacks, &(p.pool));

			if (result != VK_SUCCESS)
			{
				// what the dog doing
				continue;
			}

			constexpr size_t multiplier = 32;
			p.allocatedSets = 0;
			p.maxSets = poolInfo.maxSets;
			p.samplerCount = p.samplerCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLER] * multiplier);
			p.combinedImageSamplerCount = p.combinedImageSamplerCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER] * multiplier);
			p.sampledImageCount = p.sampledImageCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE] * multiplier);
			p.storageImageCount = p.storageImageCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_IMAGE]) * multiplier;
			p.uniformTexelBufferCount = p.uniformTexelBufferCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER]) * multiplier;
			p.storageTexelBufferCount = p.storageTexelBufferCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER]) * multiplier;
			p.uniformBufferCount = p.uniformBufferCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER]) * multiplier;
			p.storageBufferCount = p.storageBufferCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER]) * multiplier;
			p.dynamicUniformBufferCount = p.dynamicUniformBufferCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC]) * multiplier;
			p.dynamicStorageBufferCount = p.dynamicStorageBufferCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC]) * multiplier;
			p.inputAttachmentCount = p.inputAttachmentCapacity = static_cast<uint32_t>(requiredCounts[VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT]) * multiplier;

			pools.push_back(p);
		}
	}

	return results;
}

VK_BINDING_EXPORT void cobalt_vkb_destroy_descriptors(Shader* shader, DescriptorSet* sets)
{
	if (!sets)
	{
		// print error?
		return;
	}

	const size_t sz = shader->pipelineLayout.sets.size();

	for (size_t i = 0; i < sz; ++i)
	{
		DescriptorSet set = sets[i];
		shader->device->functionTable.freeDescriptorSets(set.parent->pool, 1, &(set.set));

		set.parent->allocatedSets -= 1;
		set.parent->samplerCount += set.samplerCount;
		set.parent->combinedImageSamplerCount += set.combinedImageSamplerCount;
		set.parent->sampledImageCount += set.sampledImageCount;
		set.parent->storageImageCount += set.storageImageCount;
		set.parent->uniformTexelBufferCount += set.uniformTexelBufferCount;
		set.parent->storageTexelBufferCount += set.storageTexelBufferCount;
		set.parent->uniformBufferCount += set.uniformBufferCount;
		set.parent->storageBufferCount += set.storageBufferCount;
		set.parent->dynamicUniformBufferCount += set.dynamicUniformBufferCount;
		set.parent->dynamicStorageBufferCount += set.dynamicStorageBufferCount;
		set.parent->inputAttachmentCount += set.inputAttachmentCount;
	}

	delete sets;
}

VK_BINDING_EXPORT void cobalt_vkb_write_descriptors(Device* device, size_t count, DescriptorWriteInfo* infos)
{
	std::vector<VkWriteDescriptorSet> writes;
	for (size_t i = 0; i < count; ++i)
	{
		DescriptorWriteInfo info = infos[i];
		VkWriteDescriptorSet write;

		write.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
		write.pNext = VK_NULL_HANDLE;
		write.dstSet = info.sets[info.set].set;
		write.dstBinding = info.binding;
		write.descriptorCount = info.count;
		write.descriptorType = info.type;
		write.pBufferInfo = VK_NULL_HANDLE;
		write.pImageInfo = VK_NULL_HANDLE;
		write.pTexelBufferView = VK_NULL_HANDLE;

		switch (write.descriptorType)
		{
		case VK_DESCRIPTOR_TYPE_SAMPLER:
		case VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER:
		case VK_DESCRIPTOR_TYPE_STORAGE_IMAGE:
		case VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT: {
			auto images = new VkDescriptorImageInfo[write.descriptorCount];
			for (size_t j = 0; j < write.descriptorCount; ++i)
			{
				auto sampler = info.infos[j].images.sampler;
				auto view = info.infos[j].images.view;
				auto layout = info.infos[j].images.layout;

				images[j].sampler = sampler ? sampler->sampler : VK_NULL_HANDLE;
				images[j].imageView = view ? view->imageView : VK_NULL_HANDLE;
				images[j].imageLayout = layout;
			}
			write.pImageInfo = images;
		}
		case VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER:
		case VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC:
		case VK_DESCRIPTOR_TYPE_STORAGE_BUFFER:
		case VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC: {
			auto buffers = new VkDescriptorBufferInfo[write.descriptorCount];
			for (size_t j = 0; j < write.descriptorCount; ++j)
			{
				auto buffer = info.infos[j].buffers.buf;
				auto offset = info.infos[j].buffers.offset;
				auto range = info.infos[j].buffers.range;

				buffers[j].buffer = buffer ? buffer->buffer : VK_NULL_HANDLE;
				buffers[j].offset = offset;
				buffers[j].range = range;
			}
			write.pBufferInfo = buffers;
		}
		}

		writes.push_back(write);
	}

	device->functionTable.updateDescriptorSets(static_cast<uint32_t>(writes.size()), writes.data(), 0, nullptr);

	for (auto write : writes) {
		if (write.pBufferInfo) delete[] write.pBufferInfo;
		if (write.pImageInfo) delete[] write.pImageInfo;
		if (write.pTexelBufferView) delete[] write.pTexelBufferView;
	}
}

VK_BINDING_EXPORT Image* cobalt_vkb_create_image(Device* device, ImageCreateInfo info, const char* name, const uint32_t frame)
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

	VkImageCreateInfo imageInfo;
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


VK_BINDING_EXPORT ImageView* cobalt_vkb_create_imageview(Device* device, ImageViewCreateInfo info, const char* name, const uint32_t frame)
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

VK_BINDING_EXPORT Sampler* cobalt_vkb_create_sampler(Device* device, const SamplerCreateInfo info, const char* name)
{
	if (device->samplers.find(name) != device->samplers.end())
	{
		Sampler* i = device->samplers[name];
		i->refs++;
		return i;
	}

	Sampler* sampler = new Sampler();
	sampler->refs = 1;

	VkSamplerCreateInfo samplerInfo = {};
	samplerInfo.sType = VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO;
	samplerInfo.flags = info.flags;
	samplerInfo.pNext = nullptr;
	samplerInfo.minFilter = static_cast<VkFilter>(info.minFilter);
	samplerInfo.magFilter = static_cast<VkFilter>(info.magFilter);
	samplerInfo.mipmapMode = static_cast<VkSamplerMipmapMode>(info.mipmapMode);
	samplerInfo.addressModeU = static_cast<VkSamplerAddressMode>(info.addressModeU);
	samplerInfo.addressModeV = static_cast<VkSamplerAddressMode>(info.addressModeV);
	samplerInfo.addressModeW = static_cast<VkSamplerAddressMode>(info.addressModeW);
	samplerInfo.mipLodBias = info.mipLodBias;
	samplerInfo.anisotropyEnable = info.anisotropyEnable;
	samplerInfo.maxAnisotropy = info.maxAnisotropy;
	samplerInfo.compareEnable = info.compareEnable;
	samplerInfo.compareOp = static_cast<VkCompareOp>(info.compareOp);
	samplerInfo.minLod = info.minLod;
	samplerInfo.maxLod = info.maxLod;
	samplerInfo.borderColor = static_cast<VkBorderColor>(info.borderColor);
	samplerInfo.unnormalizedCoordinates = info.unnormalizedCoordinates;

	device->functionTable.createSampler(&samplerInfo, device->device.allocation_callbacks, &sampler->sampler);
	device->samplers[name] = sampler;

	return sampler;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_sampler(Device* device, Sampler* sampler)
{
	if (sampler)
	{
		sampler->refs--;

		if (sampler->refs <= 0)
		{
			device->functionTable.destroySampler(sampler->sampler, device->device.allocation_callbacks);
			delete sampler;
		}

		return true;
	}

	return false;
}


VK_BINDING_EXPORT Framebuffer* cobalt_vkb_create_framebuffer(Device* device, const FramebufferCreateInfo info)
{
	Framebuffer* framebuffer = new Framebuffer();

	VkFramebufferCreateInfo bufferInfo;
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
	for (uint32_t i = 0; i < info.attachmentCount; i++)
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
	VkSemaphoreCreateInfo semaphoreInfo;
	semaphoreInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
	semaphoreInfo.flags = info.flags;
	semaphoreInfo.pNext = nullptr;

	VkSemaphore s;

	if(device->functionTable.createSemaphore(&semaphoreInfo, device->device.allocation_callbacks, &s) != VK_SUCCESS)
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

VK_BINDING_EXPORT uint32_t cobalt_vkb_acquire_next_image_fenced(Device* device, Swapchain* swapchain, const uint64_t timeout, Semaphore* semaphore, Fence* fence)
{
	if (swapchain)
	{
		uint32_t imageIndex;
		device->functionTable.acquireNextImageKHR(swapchain->swapchain, timeout, semaphore->semaphore, fence != nullptr ? fence->fence : VK_NULL_HANDLE, &imageIndex);

		return imageIndex;
	}

	return UINT32_MAX;
}

VK_BINDING_EXPORT uint32_t cobalt_vkb_acquire_next_image(Device* device, Swapchain* swapchain, const uint64_t timeout, Semaphore* semaphore)
{
	return cobalt_vkb_acquire_next_image_fenced(device, swapchain, timeout, semaphore, nullptr);
}

VK_BINDING_EXPORT bool cobalt_vkb_submit_queue(Device* device, const SubmitInfo info, Fence* fence)
{
	VkSubmitInfo submitInfo;
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submitInfo.pNext = nullptr;

	submitInfo.waitSemaphoreCount = info.waitSemaphoreCount;
	submitInfo.signalSemaphoreCount = info.signalSemaphoreCount;
	
	VkSemaphore* waitSems = new VkSemaphore[info.waitSemaphoreCount];
	for(uint32_t i = 0; i < info.waitSemaphoreCount; i++)
	{
		waitSems[i] = info.waitSemaphores[i]->semaphore;
	}

	VkSemaphore* signalSems = new VkSemaphore[info.signalSemaphoreCount];
	for(uint32_t i = 0; i < info.signalSemaphoreCount; i++)
	{
		signalSems[i] = info.signalSemaphores[i]->semaphore;
	}

	submitInfo.pWaitSemaphores = waitSems;
	submitInfo.pSignalSemaphores = signalSems;
	submitInfo.pWaitDstStageMask = info.waitDstStageMask;

	uint32_t amount = 0;
	std::vector<VkCommandBuffer> uploadBuffers;
	
	for(uint32_t i = 0; i < info.commandbufferCount; i++)
	{
		auto [commandbuffers, bufferIndices, bufferamount] = info.commandbuffer[i];
		amount += bufferamount;

		for(uint32_t j = 0; j < bufferamount; j++)
		{
			uploadBuffers.push_back(commandbuffers->buffers[bufferIndices[j]]);
		}
	}
	
	submitInfo.commandBufferCount = amount;
	submitInfo.pCommandBuffers = uploadBuffers.data();

	if(device->functionTable.queueSubmit(device->graphicsQueue, 1, &submitInfo, fence != nullptr ? fence->fence : VK_NULL_HANDLE) != VK_SUCCESS)
	{
		return false;
	}
	
	return true;
}

VK_BINDING_EXPORT bool cobalt_vkb_queue_present(Device* device, const PresentInfo info)
{
	VkPresentInfoKHR presentInfo;
	presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
	presentInfo.pNext = nullptr;
	presentInfo.waitSemaphoreCount = info.waitSemaphoreCount;

	VkSemaphore* waitSems = new VkSemaphore[info.waitSemaphoreCount];
	for (uint32_t i = 0; i < info.waitSemaphoreCount; i++)
	{
		waitSems[i] = info.waitSemaphores[i]->semaphore;
	}

	presentInfo.pWaitSemaphores = waitSems;

	VkSwapchainKHR* swaps = new VkSwapchainKHR[info.swapchainCount];
	for (uint32_t i = 0; i < info.swapchainCount; i++)
	{
		swaps[i] = info.swapchains[i]->swapchain;
	}

	presentInfo.swapchainCount = info.swapchainCount;
	presentInfo.pSwapchains = swaps;

	presentInfo.pImageIndices = info.imageIndices;
	presentInfo.pResults = nullptr;

	device->functionTable.queuePresentKHR(device->presentQueue, &presentInfo);

	return true;
}

VK_BINDING_EXPORT Fence* cobalt_vkb_create_fence(Device* device, const FenceCreateInfo info)
{
	VkFenceCreateInfo fenceInfo;
	fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
	fenceInfo.flags = info.flags;
	fenceInfo.pNext = nullptr;

	Fence* fence = new Fence();
	device->functionTable.createFence(&fenceInfo, device->device.allocation_callbacks, &fence->fence);
	return fence;
}

VK_BINDING_EXPORT bool cobalt_vkb_destroy_fence(Device* device, Fence* fence)
{
	if (fence)
	{
		device->functionTable.destroyFence(fence->fence, device->device.allocation_callbacks);

		delete fence;
		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_wait_for_fences(Device* device, const uint32_t count, Fence** fence, const bool waitAll, const uint64_t timeout)
{
	if(fence != nullptr)
	{
		std::vector<VkFence> fences;
		for(uint32_t i = 0; i < count; i++)
		{
			fences.push_back(fence[i]->fence);
		}
		
		if(device->functionTable.waitForFences(count, fences.data(), waitAll, timeout) != VK_SUCCESS)
		{
			return false;
		}

		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_reset_fences(Device* device, const uint32_t count, Fence** fence)
{
	if (fence != nullptr)
	{
		std::vector<VkFence> fences;
		for (uint32_t i = 0; i < count; i++)
		{
			fences.push_back(fence[i]->fence);
		}

		if (device->functionTable.resetFences(count, fences.data()) != VK_SUCCESS)
		{
			return false;
		}

		return true;
	}

	return false;
}

VK_BINDING_EXPORT bool cobalt_vkb_copy_buffer(Device* device, CommandBuffer* buffer, const uint32_t index, Buffer* srcBuffer, Buffer* dstBuffer,
	const uint32_t regionCount, BufferCopy* regions)
{
	std::vector<VkBufferCopy> reg;
	for(uint32_t i = 0; i < regionCount; i++)
	{
		VkBufferCopy r = {
			regions[i].srcOffset,
			regions[i].dstOffset,
			regions[i].size
		};

		reg.push_back(r);
	}
	
	device->functionTable.cmdCopyBuffer(buffer->buffers[index], srcBuffer->buffer, dstBuffer->buffer, regionCount, reg.data());
	
	return true;
}

VK_BINDING_EXPORT bool cobalt_vkb_copy_buffer_to_image(Device* device, CommandBuffer* buffer, const uint32_t index, Buffer* srcBuffer, Image* dstImage,
	uint32_t dstImageLayout, const uint32_t regionCount, BufferImageCopy* regions)
{
	std::vector<VkBufferImageCopy> reg;
	for (uint32_t i = 0; i < regionCount; i++)
	{
		const VkImageSubresourceLayers layers = {
			regions[i].imageSubresource.aspectMask,
			regions[i].imageSubresource.mipLevel,
			regions[i].imageSubresource.baseArrayLayer,
			regions[i].imageSubresource.layerCount
		};
		
		VkBufferImageCopy r = {
			regions[i].bufferOffset,
			regions[i].bufferRowLength,
			regions[i].bufferImageHeight,
		layers,
		{
				regions[i].imageOffset.x,
				regions[i].imageOffset.y,
				regions[i].imageOffset.z,
			},
		{
				static_cast<uint32_t>(regions[i].imageExtent.width),
				static_cast<uint32_t>(regions[i].imageExtent.height),
				static_cast<uint32_t>(regions[i].imageExtent.depth)
			}
		};

		reg.push_back(r);
	}

	device->functionTable.cmdCopyBufferToImage(buffer->buffers[index], srcBuffer->buffer, dstImage->image, static_cast<VkImageLayout>(dstImageLayout), 
		regionCount, reg.data());

	return true;
}

VK_BINDING_EXPORT bool cobalt_vkb_pipeline_barrier(Device* device, CommandBuffer* buffer, const uint32_t index, const uint32_t srcStageMask, const uint32_t dstStageMask,
	const uint32_t dependencyFlags, const uint32_t memoryBarrierCount, MemoryBarrier* memoryBarriers, const uint32_t bufferMemoryBarrierCount, BufferMemoryBarrier* bufferMemoryBarriers,
	const uint32_t imageMemoryBarrierCount, ImageMemoryBarrier* imageMemoryBarriers)
{
	std::vector<VkMemoryBarrier> memBars;
	for(uint32_t i = 0; i < memoryBarrierCount; i++)
	{
		VkMemoryBarrier bar = {
			VK_STRUCTURE_TYPE_MEMORY_BARRIER,
			nullptr,
			memoryBarriers[i].srcAccessMask,
			memoryBarriers[i].dstAccessMask
		};

		memBars.push_back(bar);
	}

	std::vector<VkBufferMemoryBarrier> bufferMemBars;
	for (uint32_t i = 0; i < bufferMemoryBarrierCount; i++)
	{
		VkBufferMemoryBarrier bar = {
			VK_STRUCTURE_TYPE_MEMORY_BARRIER,
			nullptr,
			bufferMemoryBarriers[i].srcAccessMask,
			bufferMemoryBarriers[i].dstAccessMask,
			bufferMemoryBarriers[i].srcQueueFamilyIndex,
			bufferMemoryBarriers[i].dstQueueFamilyIndex,
			bufferMemoryBarriers[i].buffer->buffer,
			bufferMemoryBarriers[i].offset,
			bufferMemoryBarriers[i].size
		};

		bufferMemBars.push_back(bar);
	}

	std::vector<VkImageMemoryBarrier> imageMemBars;
	for (uint32_t i = 0; i < imageMemoryBarrierCount; i++)
	{
		const VkImageSubresourceRange range = {
			imageMemoryBarriers[i].subresourceRange.aspectMask,
			imageMemoryBarriers[i].subresourceRange.baseMipLevel,
			imageMemoryBarriers[i].subresourceRange.levelCount,
			imageMemoryBarriers[i].subresourceRange.baseArrayLayer,
			imageMemoryBarriers[i].subresourceRange.layerCount
		};
		
		VkImageMemoryBarrier bar = {
			VK_STRUCTURE_TYPE_MEMORY_BARRIER,
			nullptr,
			imageMemoryBarriers[i].srcAccessMask,
			imageMemoryBarriers[i].dstAccessMask,
			static_cast<VkImageLayout>(imageMemoryBarriers[i].oldLayout),
			static_cast<VkImageLayout>(imageMemoryBarriers[i].newLayout),
			imageMemoryBarriers[i].srcQueueFamilyIndex,
			imageMemoryBarriers[i].dstQueueFamilyIndex,
			imageMemoryBarriers[i].image->image,
			range
		};

		imageMemBars.push_back(bar);
	}
	
	device->functionTable.cmdPipelineBarrier(buffer->buffers[index], srcStageMask, dstStageMask, dependencyFlags, memoryBarrierCount, memBars.data(),
		bufferMemoryBarrierCount, bufferMemBars.data(), imageMemoryBarrierCount, imageMemBars.data());

	return true;
}

VK_BINDING_EXPORT bool cobalt_vkb_draw_indirect(Device* device, CommandBuffer* buffer, const uint32_t index,
	Buffer* srcBuffer, const uint64_t offset, const uint32_t drawCount, const uint32_t stride)
{
	device->functionTable.cmdDrawIndirect(buffer->buffers[index], srcBuffer->buffer, offset, drawCount, stride);

	return true;
}

VK_BINDING_EXPORT bool cobalt_vkb_draw_indexed_indirect_count(Device* device, CommandBuffer* buffer, const uint32_t index, Buffer* srcBuffer,
	const uint64_t offset, Buffer* countBuffer, const uint64_t countBufferOffset, const uint32_t maxDrawCount, const uint32_t stride)
{
	device->functionTable.cmdDrawIndexedIndirectCount(buffer->buffers[index], srcBuffer->buffer, offset, countBuffer->buffer,
		countBufferOffset, maxDrawCount, stride);

	return true;
}

VK_BINDING_EXPORT bool cobalt_vkb_bind_descriptor_sets(Device* device, CommandBuffer* buffer, const  uint32_t index, uint32_t pipelineBindPoint,
	Shader* pipeline, const uint32_t firstSet, const uint32_t descriptorSetCount, DescriptorSet** sets, const uint32_t dynamicOffsetCount, uint32_t* dynamicOffsets)
{
	std::vector<VkDescriptorSet> descSets;
	for(uint32_t i = 0; i < descriptorSetCount; i++)
	{
		descSets.push_back(sets[i]->set);
	}
	
	device->functionTable.cmdBindDescriptorSets(buffer->buffers[index], static_cast<VkPipelineBindPoint>(pipelineBindPoint), pipeline->pipelineLayout.layout, firstSet,
		descriptorSetCount, descSets.data(), dynamicOffsetCount, dynamicOffsets);
	
	return true;
}

