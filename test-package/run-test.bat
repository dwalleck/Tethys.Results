@echo off
echo Running PhoenixLabs.Results test package...
echo.

REM Restore packages
echo Restoring packages...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo Failed to restore packages.
    exit /b %ERRORLEVEL%
)

REM Build the project
echo Building project...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo Failed to build project.
    exit /b %ERRORLEVEL%
)

REM Run the test
echo Running test...
echo.
dotnet run
if %ERRORLEVEL% neq 0 (
    echo Test failed with error code %ERRORLEVEL%.
    exit /b %ERRORLEVEL%
)

echo.
echo Test completed successfully!
pause
