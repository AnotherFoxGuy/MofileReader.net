# Setup project to use C# utilities.
include(CSharpUtilities)

set(SOURCE_FILES
    moFileReader.cs
)

# Add executable project.
add_library(${PROJECT_NAME} SHARED ${SOURCE_FILES})

set(DOTNET_REF
	System
    System.Core
    System.Data
    WindowsBase
)

# Set CLR assembly properties.
set_target_properties(${PROJECT_NAME} PROPERTIES
	WIN32_EXECUTABLE TRUE
    VS_DOTNET_REFERENCES "${DOTNET_REF}"
    VS_GLOBAL_ROOTNAMESPACE ${PROJECT_NAME}
)