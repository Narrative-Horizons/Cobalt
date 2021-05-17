#include <assimp/Importer.hpp>
#include <assimp/scene.h>
#include <assimp/postprocess.h>

#define ASSIMP_BINDING_EXPORT __declspec(dllexport)

extern "C"
{
	ASSIMP_BINDING_EXPORT void assimp_test()
	{
		Assimp::Importer importer;
		const aiScene* scene = importer.ReadFile("data/SciFiHelmet/SciFiHelmet.gltf", aiProcessPreset_TargetRealtime_Quality);

		if (scene != nullptr)
		{
			printf("Test succeeded\n");
		}
	}
}