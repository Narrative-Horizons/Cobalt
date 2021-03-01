project "Cobalt Bindings"
    kind "SharedLib"
    dotnetframework "netcoreapp3.1"
    language "C#"

    targetdir (binaries)
    objdir (intermediate)

    namespace "Cobalt.Bindings"

    platformtarget "x86_64"

    dependson {
        "GLAD@GL4.6-native-bindings",
        "GLFW@3.3.3-native-bindings"
    }

    files {
        "**.cs"
    }

    filter "system:windows"
        defines {
            "COBALT_PLATFORM_WINDOWS"
        }
