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
        "%{NativeIncludeDirs.vulkan}",
        "%{NativeIncludeDirs.vkbootstrap}"
    }

    toolset "clang"

    -- OS filters
    filter "system:windows"
        systemversion "latest"
        staticruntime "Off"

        links {
            "vk-bootstrap"
        }

    filter {}

    -- Configuration Filters
    filter "configurations:Debug"
        runtime "Debug"
        symbols "On"

        defines({
            "_DEBUG"
        })

    filter "configurations:Release"
        optimize "Full"
        runtime "Release"
        symbols "Off"

        defines({
            "NDEBUG"
        })

    filter {}