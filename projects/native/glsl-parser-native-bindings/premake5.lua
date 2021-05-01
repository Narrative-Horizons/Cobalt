project "glsl-parser-native-bindings"
    kind "SharedLib"
    language "C++"

    architecture "x64"

    targetdir (binaries)
    objdir (intermediate)

    files {
        "src/**.cpp"
    }

    includedirs {
        "%{NativeIncludeDirs.glslparser}"
    }

    links {
        "glsl-parser"
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