project "vk-bootstrap-native-bindings"
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
        "%{NativeIncludeDirs.vkbootstrap}",
        "%{NativeIncludeDirs.vulkan}"
    }

    links {
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