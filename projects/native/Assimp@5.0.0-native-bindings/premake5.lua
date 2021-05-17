project "Assimp@5.0.0-native-bindings"
    kind "SharedLib"
    language "C++"

    architecture "x64"

    targetdir (binaries)
    objdir (intermediate)

    files {
        "src/**.cpp"
    }

    libdirs {
        "%{sln.location}/dependencies/Assimp/lib/%{cfg.buildcfg}/%{cfg.system}/%{cfg.architecture}"
    }

    includedirs {
        "%{NativeIncludeDirs.assimp}"
    }

    links {
        "assimp",
        "zlib"
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