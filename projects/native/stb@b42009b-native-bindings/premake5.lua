project "stb@b42009b-native-bindings"
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
        "%{NativeIncludeDirs.stb}"
    }

    links {
        "stb@b42009b"
    }

    toolset "clang"

    -- OS filters
    filter "system:windows"
        systemversion "latest"
        staticruntime "Off"

        defines {
            "_CRT_SECURE_NO_WARNINGS"
        }

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