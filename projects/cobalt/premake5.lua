project "Cobalt"
    kind "SharedLib"
    dotnetframework "netcoreapp3.1"
    language "C#"

    targetdir (binaries)
    objdir (intermediate)

    namespace "Cobalt"

    platformtarget "x86_64"

    links {
        "Cobalt Bindings"
    }

    files {
        "**.cs"
    }

    filter "system:windows"
        defines {
            "COBALT_PLATFORM_WINDOWS"
        }

    filter "platforms:x86_64"
        architecture "x86_64"