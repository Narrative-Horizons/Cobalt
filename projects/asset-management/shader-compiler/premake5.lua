project "Shader Compiler"
    kind "ConsoleApp"
    language "C++"
    cppdialect "C++17"

    architecture "x64"

    targetdir (binaries)
    objdir (intermediate)

    files {
        "src/**.c",
        "src/**.cpp",
        "src/**.hpp"
    }

    toolset "clang"

    -- OS filters
    filter "system:windows"
        systemversion "latest"
        staticruntime "Off"

    filter {}

    -- Configuration Filters
    filter "configurations:Debug"
        runtime "Debug"
        symbols "On"

    filter "configurations:Release"
        optimize "Full"
        runtime "Release"
        symbols "Off"

    filter {}