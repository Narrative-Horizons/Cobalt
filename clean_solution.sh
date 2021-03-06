#!/bin/sh

# Clean all previous generated project files
find . -name "*.vcxproj*" -type f -delete
find . -name "*.csproj*" -type f -delete
find . -name "*.sln*" -type f -delete
find . -name "Makefile" -type f -delete

# Clean all previous generated files
find . -name "bin" -type d -exec rm -rf  {} +
find . -name "bin-int" -type d -exec rm -rf  {} +
find . -name "obj" -type d -exec rm -rf  {} +