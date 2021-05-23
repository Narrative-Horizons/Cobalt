project "Asset Converter"
    kind "SharedLib"
    dotnetframework "netcoreapp3.1"
    language "C#"

    targetdir (binaries)
    objdir (intermediate)

    namespace "CobaltConverter"

    platformtarget "x86_64"

    nuget {
        "log4net:2.0.12",
        "Newtonsoft.Json:13.0.1",
        "Silk.NET.Assimp:2.4.0"
    }

    links {
        "Cobalt Math"
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