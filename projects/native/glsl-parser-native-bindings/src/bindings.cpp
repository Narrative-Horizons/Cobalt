#include <glsl-parser/parser.h>

#include <stdio.h>
#include <stdbool.h>
#include <string>

#define GLSL_PARSER_BINDING_EXPORT __declspec(dllexport)

extern "C"
{
    GLSL_PARSER_BINDING_EXPORT void parseSource(const char* source, const char* fileName, unsigned int shaderType)
    {

        /*
            enum {
                kCompute,
                kVertex,
                kTessControl,
                kTessEvaluation,
                kGeometry,
                kFragment
            };
        */

        glsl::parser parser(source, fileName);
        glsl::astTU* tu = parser.parse(shaderType);

        if (tu)
        {
            for (size_t i = 0; i < tu->globals.size(); i++)
            {
                glsl::astGlobalVariable* variable = tu->globals[i];
                //printf("%s\n", variable->name);
            }

            for (size_t i = 0; i < tu->interfaceBlocks.size(); i++)
            {
                glsl::astInterfaceBlock* interfaceBlock = tu->interfaceBlocks[i];
                //printf("%s\n", interfaceBlock->name);

                for (size_t j = 0; j < interfaceBlock->fields.size(); j++)
                {
                    //printf("%s\n", interfaceBlock->fields[j]->name);
                }
            }
        }
        else
        {
            //printf("%s\n", parser.error());
        }
    }
}