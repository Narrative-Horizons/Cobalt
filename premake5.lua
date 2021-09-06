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
    include "dependencies/glsl-parser"
    group ""

    group "Native Bindings"
    include "projects/native/GLAD@GL4.6-native-bindings"
    include "projects/native/GLFW@3.3.3-native-bindings"
    include "projects/native/stb@b42009b-native-bindings"
    include "projects/native/glsl-parser-native-bindings"
    include "projects/native/PhysX@4.1.2-native-bindings"
    group ""

    group "Asset Management"
    include "projects/asset-management/asset-converter"
    include "projects/asset-management/asset-converter-cli"
    group ""

    include "projects/cobalt"
    include "projects/cobalt-math"
    include "projects/cobalt-bindings"
    include "projects/cobalt-sandbox"
    include "projects/cobalt-unit-tests"

    NativeIncludeDirs = {}
    NativeIncludeDirs["glad"] = "%{sln.location}/dependencies/GLAD@GL4.6/include"
    NativeIncludeDirs["glfw"] = "%{sln.location}/dependencies/GLFW@3.3.3/include"
    NativeIncludeDirs["stb"]  = "%{sln.location}/dependencies/stb@b42009b/include"
    NativeIncludeDirs["glslparser"]  = "%{sln.location}/dependencies/glsl-parser/include"
    NativeIncludeDirs["phonon"] = "%{sln.location}/dependencies/phonon/include/"

    Dependencies = {}
    Dependencies["PhysX"] = {
        bin = "%{sln.location}/dependencies/physx@4.1.2/bin/%{cfg.buildcfg}/%{cfg.system}",
        include = "%{sln.location}/dependencies/physx@4.1.2/include",
        lib = "%{sln.location}/dependencies/physx@4.1.2/lib/%{cfg.buildcfg}/%{cfg.system}"
    }