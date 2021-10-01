#include <VkBootstrap.h>

#include <stdio.h>
#include <stdbool.h>

#define VKBOOTSTRAP_BINDING_EXPORT extern "C" __declspec(dllexport) inline

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
	// TODO: custom debugger and layers
};

VKBOOTSTRAP_BINDING_EXPORT vkb::Instance* cobalt_vkb_create_instance(InstanceCreateInfo info)
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

	auto result = bldr.build();

	if (result)
	{
		return new vkb::Instance(result.value());
	}

	return nullptr;
}

VKBOOTSTRAP_BINDING_EXPORT bool cobalt_vkb_destroy_instance(vkb::Instance* instance)
{
	if (instance)
	{
		vkb::destroy_instance(*instance);
		delete instance;
	}

	return true;
}