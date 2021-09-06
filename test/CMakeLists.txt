project(moFileReaderTest)


set(SOURCE_FILES
    ${CMAKE_SOURCE_DIR}/src/moFileReader.cs
)

add_library(test SHARED ${SOURCE_FILES} moFileReader_tests.cs)
add_executable(bench ${SOURCE_FILES} moFileReader_bench.cs)
set_target_properties(bench PROPERTIES WIN32_EXECUTABLE FALSE)

set(DOTNET_REF
	System
    System.Core
    System.Data
    Microsoft.CSharp
    WindowsBase
)

set(NUGET_PKGS_TEST
    NUnit_3.13.2
    NUnit.Console_3.12.0
)

set(NUGET_PKGS_BENCH
   BenchmarkDotNet_0.13.1
)

# Set CLR assembly properties.
set_target_properties(test PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF}"
    VS_PACKAGE_REFERENCES "${NUGET_PKGS_TEST}"
    VS_GLOBAL_ROOTNAMESPACE ${PROJECT_NAME}
)

set_target_properties(bench PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF}"
    VS_PACKAGE_REFERENCES "${NUGET_PKGS_BENCH}"
    VS_GLOBAL_ROOTNAMESPACE ${PROJECT_NAME}
)