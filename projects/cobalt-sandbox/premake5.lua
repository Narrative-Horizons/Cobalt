project "Cobalt Sandbox"
    kind "ConsoleApp"
    dotnetframework "netcoreapp3.1"
    language "C#"

    targetdir (binaries)
    objdir (intermediate)

    namespace "Cobalt.Sandbox"

    platformtarget "x86_64"

    libdirs {
        "%{sln.location}/bin/%{cfg.buildcfg}/%{cfg.system}"
    }

    links {
        "Cobalt"
    }

    files {
        "**.cs"
    }

    filter "system:windows"
        defines {
            "COBALT_PLATFORM_WINDOWS"
        }

        postbuildcommands {
            "xcopy /E /I \"%{sln.location}bin\\$(Configuration)\\%{cfg.system}\\%{cfg.platformtarget}\" \"%{sln.location}bin\\$(Configuration)\\%{cfg.system}\\%{cfg.dotnetframework}\\bin\""
        }

    filter {}

    filter "configurations:Release"
        optimize "Full"
