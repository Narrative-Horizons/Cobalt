project "Cobalt"
    kind "SharedLib"
    dotnetframework "netcoreapp3.1"
    language "C#"

    targetdir (binaries)
    objdir (intermediate)

    namespace "Cobalt"

    platformtarget "x86_64"

    nuget {
        "OpenTK:4.6.3"
    }

    links {
        "Cobalt Bindings"
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