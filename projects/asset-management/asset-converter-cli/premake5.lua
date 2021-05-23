project "Asset Converter CLI"
    kind "ConsoleApp"
    dotnetframework "netcoreapp3.1"
    language "C#"

    targetdir (binaries)
    objdir (intermediate)

    namespace "CobaltConverter"

    platformtarget "x86_64"

    nuget {
        "log4net:2.0.12",
    }

    links {
        "Asset Converter"
    }

    files {
        "**.cs"
    }

    clr "Unsafe"

    filter "system:windows"
        defines {
            "COBALT_PLATFORM_WINDOWS"
        }

    filter "platforms:x86_64"
        architecture "x86_64"