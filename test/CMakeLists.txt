project(moFileReaderTest)


set(SOURCE_FILES
    ${CMAKE_SOURCE_DIR}/src/moFileReader.cs
)

set(TEST_SOURCE_FILES
    moFileReader_tests.cs
)

source_group("source" FILES ${SOURCE_FILES})

add_library(${PROJECT_NAME} SHARED ${SOURCE_FILES} ${TEST_SOURCE_FILES})

set(DOTNET_REF
	System
    System.Core
    System.Data
    Microsoft.CSharp
    WindowsBase
)

set(NUGET_PKGS
    NUnit_3.13.1
    NUnit.Console_3.12.0
)

# Set CLR assembly properties.
set_target_properties(${PROJECT_NAME} PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF}"
    VS_PACKAGE_REFERENCES "${NUGET_PKGS}"
    VS_GLOBAL_ROOTNAMESPACE ${PROJECT_NAME}
)