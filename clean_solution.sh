#!/bin/sh

# Clean all previous generated project files
find . -name "*.vcxproj*" -type f -delete
find . -name "*.csproj*" -type f -delete
find . -name "*.sln*" -type f -delete
find . -name "Makefile" -type f -delete

# Clean all previous generated files
find . -name "bin" -type f -delete
find . -name "bin-int" -type f -delete
find . -name "obj" -type f -delete