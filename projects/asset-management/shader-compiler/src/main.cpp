#include <iostream>
#include <filesystem>
#include <vector>
#include <string>

enum class Arguments : uint32_t
{
	BaseShaderPathLocation = 1,
	ShaderExportPathLocation = 2,
	ENUM_END = 3,
};

struct Shader
{
	std::string loadPath;
	std::string exportPath;
};

int main(int argc, char** argv)
{
	if(argc < static_cast<uint32_t>(Arguments::ENUM_END))
	{
		std::cout << "Not enough arguments passed, got " << argc - 1 << " but required " << static_cast<uint32_t>(Arguments::ENUM_END) - 1 << std::endl;
	}

	const std::string baseShaderPath = argv[static_cast<uint32_t>(Arguments::BaseShaderPathLocation)];
	const std::string shaderExportPath = argv[static_cast<uint32_t>(Arguments::ShaderExportPathLocation)];

	std::vector<Shader*> shaders;

	// Get all shaders in location
	for(std::filesystem::recursive_directory_iterator i(baseShaderPath), end; i != end; ++i)
	{
		if(!is_directory(i->path()))
		{
			auto extension = i->path().filename().extension();
			if(extension == ".vert" || extension == ".frag" || extension == ".geom" || extension == ".tesc" || extension == "tese" || extension == ".comp")
			{
				Shader* shader = new Shader();
				shader->loadPath = i->path().string();
				shader->exportPath = shaderExportPath + i->path().string().substr(baseShaderPath.length());
				shader->exportPath = shader->exportPath.substr(0, shader->exportPath.length() - extension.string().length()) + ".spv";

				shaders.push_back(shader);
			}
		}
	}


	
	for(Shader* shader : shaders)
	{
		std::string command = "glslc.exe " + shader->loadPath + " -o " + shader->exportPath;
		int retcode = system(command.c_str());
		if (retcode != 0)
			return retcode;
	}

	return 0;
}