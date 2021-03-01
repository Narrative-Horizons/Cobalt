-- Cobalt Engine Premake
workspace "Cobalt"
    configurations {
        "Debug", -- Full symbols, no optimizations
        "Release", -- Symbols on, optimizations on
        "Dist" -- Symbols off, optimizations on
    }

    sln = solution()
    binaries = "%{sln.location}/bin/%{cfg.buildcfg}/%{cfg.system}/%{cfg.architecture}"
    intermediate = "%{sln.location}/bin-int/%{cfg.buildcfg}/%{cfg.system}/%{cfg.architecture}"

    group "External Dependencies"
    include "dependencies/GLAD@GL4.6"
    include "dependencies/GLFW@3.3.3"
    group ""

    group "Native Bindings"
    include "projects/native/GLAD@GL4.6-native-bindings"
    include "projects/native/GLFW@3.3.3-native-bindings"
    group ""

    include "projects/cobalt"
    include "projects/cobalt-sandbox"
    include "projects/cobalt-bindings"

    NativeIncludeDirs = {}
    NativeIncludeDirs["glad"] = "%{sln.location}/dependencies/GLAD@GL4.6/include"
    NativeIncludeDirs["glfw"] = "%{sln.location}/dependencies/GLFW@3.3.3/include"