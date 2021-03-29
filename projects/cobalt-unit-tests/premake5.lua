project "Cobalt Unit Tests"
    kind "ConsoleApp"
    dotnetframework "netcoreapp3.1"
    language "C#"

    targetdir (binaries)
    objdir (intermediate)

    namespace "Cobalt.Tests.Unit"

    platformtarget "x86_64"

    nuget {
        "Microsoft.NET.Test.Sdk:16.7.1",
        "MSTest.TestAdapter:2.1.1",
        "MSTest.TestFramework:2.1.1",
        "coverlet.collector:1.3.0"
    }

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
            "xcopy /E /C /I /Y \"%{sln.location}bin\\$(Configuration)\\%{cfg.system}\\%{cfg.platformtarget}\" \"%{sln.location}bin\\$(Configuration)\\%{cfg.system}\\%{cfg.dotnetframework}\\bin\""
        }

    filter {}

    filter "configurations:Release"
        optimize "Full"
