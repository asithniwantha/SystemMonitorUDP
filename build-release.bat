@echo off
echo ================================================
echo    UDP System Monitor - Release Build Script
echo ================================================
echo.

set PROJECT_DIR=C:\Users\asith\source\repos\SystemMonitorUDP\SystemMonitorUDP
set OUTPUT_DIR=C:\Publish\SystemMonitorUDP
set VERSION=1.0.0

echo Building UDP System Monitor v%VERSION%...
echo.

cd "%PROJECT_DIR%"

echo [1/4] Cleaning previous builds...
dotnet clean --configuration Release
if errorlevel 1 goto error

echo [2/4] Restoring NuGet packages...
dotnet restore
if errorlevel 1 goto error

echo [3/4] Building Release configuration...
dotnet build --configuration Release --no-restore
if errorlevel 1 goto error

echo [4/4] Publishing self-contained executable...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o "%OUTPUT_DIR%"
if errorlevel 1 goto error

echo.
echo ================================================
echo BUILD SUCCESSFUL!
echo ================================================
echo.
echo Release files location: %OUTPUT_DIR%
echo Main executable: %OUTPUT_DIR%\SystemMonitorUDP.exe
echo.
echo You can now distribute the contents of the output folder.
echo.
pause
goto end

:error
echo.
echo ================================================
echo BUILD FAILED!
echo ================================================
echo.
pause

:end