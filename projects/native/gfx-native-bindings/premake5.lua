project "gfx-native-bindings"
    kind "SharedLib"
    language "C++"
    cppdialect "C++17"

    architecture "x64"

    targetdir (binaries)
    objdir (intermediate)

    files {
        "src/**.c",
        "src/**.cpp"
    }

    includedirs {
        "%{NativeIncludeDirs.glfw}",
        "%{NativeIncludeDirs.vkbootstrap}",
        "%{NativeIncludeDirs.vulkan}",
        "%{NativeIncludeDirs.vma}",
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