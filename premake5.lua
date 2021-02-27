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

    --include "dependencies/GLFW@3.3.3"

    include "projects/cobalt"
    include "projects/cobalt-sandbox"
    include "projects/cobalt-bindings"