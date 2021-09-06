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
        "Cobalt",
        "Cobalt Math"
    }

    files {
        "**.cs"
    }

    filter "system:windows"
        defines {
            "COBALT_PLATFORM_WINDOWS"
        }

        postbuildcommands {
            "xcopy /E /C /I /Y \"%{sln.location}bin\\$(Configuration)\\%{cfg.system}\\%{cfg.platformtarget}\" \"%{sln.location}bin\\$(Configuration)\\%{cfg.system}\\%{cfg.dotnetframework}\\bin\"",
            "xcopy /E /C /I /Y \"%{sln.location}dependencies\\phonon\\bin\\%{cfg.system}\" \"%{sln.location}bin\\$(Configuration)\\%{cfg.system}\\%{cfg.dotnetframework}\\bin\"",
            "xcopy /E /C /I /Y \"%{sln.location}dependencies\\physx@4.1.2\\bin\\$(Configuration)\\%{cfg.system}\" \"%{sln.location}bin\\$(Configuration)\\%{cfg.system}\\%{cfg.dotnetframework}\\bin\"",
            "xcopy /E /C /I /Y \"%{sln.location}data\" \"%{sln.location}bin\\$(Configuration)\\%{cfg.system}\\%{cfg.dotnetframework}\\data\""
        }

    filter {}

    filter "configurations:Release"
        optimize "Full"
