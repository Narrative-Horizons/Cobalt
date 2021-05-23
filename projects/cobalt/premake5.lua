project "Cobalt"
    kind "SharedLib"
    dotnetframework "netcoreapp3.1"
    language "C#"

    targetdir (binaries)
    objdir (intermediate)

    namespace "Cobalt"

    platformtarget "x86_64"

    nuget {
        "log4net:2.0.12",
        "Newtonsoft.Json:13.0.1",
        "Silk.NET.Assimp:2.4.0"
    }

    links {
        "Cobalt Bindings",
        "Cobalt Math",
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