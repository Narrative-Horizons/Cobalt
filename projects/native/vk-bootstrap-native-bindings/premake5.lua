project "vk-bootstrap-native-bindings"
    kind "SharedLib"
    language "C++"
    cppdialect "C++17"

    architecture "x64"

    targetdir (binaries)
    objdir (intermediate)

    files {
        "src/**.cpp"
    }

    includedirs {
        "%{NativeIncludeDirs.glfw}",
        "%{NativeIncludeDirs.vkbootstrap}",
        "%{NativeIncludeDirs.vulkan}"
    }

    links {
        "GLFW@3.3.3",
        "vk-bootstrap"
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