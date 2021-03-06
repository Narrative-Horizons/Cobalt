project "GLAD@GL4.6"
    kind "StaticLib"
    language "C"
    cdialect "C11"

    architecture "x64"

    targetdir (binaries)
    objdir (intermediate)

    files {
        "include/**.h",
        "src/**.c"
    }

    includedirs {
        "include/"
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
