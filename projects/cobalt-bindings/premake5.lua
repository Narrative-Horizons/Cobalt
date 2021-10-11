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
        "stb@b42009b-native-bindings",
        "phonon-native-bindings",
        "gfx-native-bindings",
        "PhysX@4.1.2-native-bindings"
    }
    
    files {
        "**.cs"
    }

    clr "Unsafe"

    filter "system:windows"
        defines {
            "COBALT_PLATFORM_WINDOWS"
        }
