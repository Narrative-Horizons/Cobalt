project "Cobalt Bindings"
    kind "SharedLib"
    dotnetframework "netcoreapp3.1"
    language "C#"

    targetdir (binaries)
    objdir (intermediate)

    namespace "Cobalt.Bindings"

    platformtarget "x86_64"

    files {
        "**.cs"
    }

    filter "configurations:Debug"
        postbuildcommands {
            "{COPY} %{sln.location}/dependencies/GLFW@3.3.3/bin/Debug/GLFW.dll %{binaries}",
        }

    filter "configurations:Release"
        postbuildcommands {
            "{COPY} %{sln.location}/dependencies/GLFW@3.3.3/bin/Release/GLFW.dll %{binaries}",
        }

    filter "system:windows"
        defines {
            "COBALT_PLATFORM_WINDOWS"
        }
