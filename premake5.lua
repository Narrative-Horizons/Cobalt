-- Cobalt Engine Premake
workspace "Cobalt"
    configurations {
        "Debug", -- Full symbols, no optimizations
        "Release", -- Symbols on, optimizations on
        -- "Dist" -- Symbols off, optimizations on
    }

    sln = solution()
    binaries = "%{sln.location}/bin/%{cfg.buildcfg}/%{cfg.system}/%{cfg.architecture}"
    intermediate = "%{sln.location}/bin-int/%{cfg.buildcfg}/%{cfg.system}/%{cfg.architecture}"

    group "External Dependencies"
    include "dependencies/GLAD@GL4.6"
    include "dependencies/GLFW@3.3.3"
    include "dependencies/stb@b42009b"
    group ""

    group "Native Bindings"
    include "projects/native/GLAD@GL4.6-native-bindings"
    include "projects/native/GLFW@3.3.3-native-bindings"
    include "projects/native/stb@b42009b-native-bindings"
    group ""

    include "projects/cobalt"
    include "projects/cobalt-bindings"
    include "projects/cobalt-sandbox"
    include "projects/cobalt-unit-tests"

    NativeIncludeDirs = {}
    NativeIncludeDirs["glad"] = "%{sln.location}/dependencies/GLAD@GL4.6/include"
    NativeIncludeDirs["glfw"] = "%{sln.location}/dependencies/GLFW@3.3.3/include"
    NativeIncludeDirs["stb"]  = "%{sln.location}/dependencies/stb@b42009b/include"