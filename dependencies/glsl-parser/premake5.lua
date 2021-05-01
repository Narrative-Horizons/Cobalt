project "glsl-parser"
    kind "StaticLib"
    language "C++"

    targetdir (binaries)
    objdir (intermediate)

    architecture "x64"

    toolset "clang"

    includedirs {
        "include/"
    }

    files {
        "**.cpp",
        "**.h"
    }

    filter "configurations:Debug"
        runtime "Debug"
        symbols "On"

    filter "configurations:Release"
        optimize "Full"
        runtime "Release"
        symbols "Off"

    filter {}