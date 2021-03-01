project "GLFW@3.3.3-native-bindings"
    kind "SharedLib"
    language "C"
    cdialect "C11"

    architecture "x64"

    targetdir (binaries)
    objdir (intermediate)

    files {
        "src/**.c"
    }

    includedirs {
        "%{NativeIncludeDirs.glfw}"
    }

    links {
        "GLFW@3.3.3"
    }

    toolset "clang"

    -- OS filters
    filter "system:window"
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