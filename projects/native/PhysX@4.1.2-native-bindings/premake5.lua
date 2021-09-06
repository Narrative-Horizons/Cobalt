project "PhysX@4.1.2-native-bindings"
    kind "SharedLib"
    language "C"
    cdialect "C11"

    architecture "x64"

    targetdir (binaries)
    objdir (intermediate)

    files {
        "src/**.cpp"
    }

    includedirs {
        "%{Dependencies.PhysX.include}"
    }

    libdirs {
        "%{Dependencies.PhysX.lib}"
    }

    toolset "clang"

    -- OS filters
    filter "system:windows"
        systemversion "latest"
        staticruntime "Off"

        links {
            "PhysX_64.lib",
            "PhysXCharacterKinematic_static_64.lib",
            "PhysXCommon_64.lib",
            "PhysXCooking_64.lib",
            "PhysXExtensions_static_64.lib",
            "PhysXFoundation_64.lib",
            "PhysXPvdSDK_static_64.lib",
            "PhysXTask_static_64.lib",
            "PhysXVehicle_static_64.lib",
        }

    filter {}

    -- Configuration Filters
    filter "configurations:Debug"
        runtime "Debug"
        symbols "On"

        defines({
            "_DEBUG"
        })

    filter "configurations:Release"
        optimize "Full"
        runtime "Release"
        symbols "Off"

        defines({
            "NDEBUG"
        })

    filter {}