#-------------------------------------------------------
# moFileReader Main Build Script
#-------------------------------------------------------
cmake_minimum_required(VERSION 3.0)

project(moFileReader LANGUAGES CSharp)

# Set global .NET Framework version.
set(CMAKE_DOTNET_TARGET_FRAMEWORK_VERSION "v4.8")

# Set the C# language version for all projects.
set(CMAKE_CSharp_FLAGS "/langversion:9")

# setup paths
set(RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/")
set(LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/")
set(ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/")
set(EXECUTABLE_OUTPUT_PATH ${RUNTIME_OUTPUT_DIRECTORY})
set(LIBRARY_OUTPUT_PATH ${RUNTIME_OUTPUT_DIRECTORY})

add_subdirectory(src)
add_subdirectory(test)