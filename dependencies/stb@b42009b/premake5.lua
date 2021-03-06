project "stb@b42009b"
    kind "StaticLib"
    language "C"
    cdialect "C11"

    targetdir (binaries)
    objdir (intermediate)

    architecture "x64"

    toolset "clang"

    files {
        "include/**.h",
        "src/**.c"
    }

    includedirs {
        "include/"
    }

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