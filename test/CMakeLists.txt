project(moFileReaderTest)

set(TEST_MO_FILE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/languages")

configure_file("config.cs.in" "config.cs")

set(SOURCE_FILES
    ${CMAKE_SOURCE_DIR}/src/moFileReader.cs
    ${CMAKE_CURRENT_BINARY_DIR}/config.cs
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

# Set CLR assembly properties.
set_target_properties(test PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF};nunit.framework"
    VS_PACKAGE_REFERENCES "NUnit_3.13.2;NUnit.Console_3.12.0"
    VS_GLOBAL_ROOTNAMESPACE ${PROJECT_NAME}
)

set_target_properties(bench PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF}"
    VS_PACKAGE_REFERENCES "BenchmarkDotNet_0.13.1"
    VS_GLOBAL_ROOTNAMESPACE ${PROJECT_NAME}
)