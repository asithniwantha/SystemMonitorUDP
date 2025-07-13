# UDP System Monitor - PowerShell Build Script
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   UDP System Monitor - Release Build Script" -ForegroundColor Cyan  
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$ProjectDir = "C:\Users\asith\source\repos\SystemMonitorUDP\SystemMonitorUDP"
$OutputDir = "C:\Publish\SystemMonitorUDP"
$Version = "1.0.0"

Write-Host "Building UDP System Monitor v$Version..." -ForegroundColor Yellow
Write-Host ""

Set-Location $ProjectDir

try {
    Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Green
    dotnet clean --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Clean failed" }

    Write-Host "[2/4] Restoring NuGet packages..." -ForegroundColor Green
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

    Write-Host "[3/4] Building Release configuration..." -ForegroundColor Green
    dotnet build --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    Write-Host "[4/4] Publishing self-contained executable..." -ForegroundColor Green
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o $OutputDir
    if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

    Write-Host ""
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Release files location: $OutputDir" -ForegroundColor White
    Write-Host "Main executable: $OutputDir\SystemMonitorUDP.exe" -ForegroundColor White
    Write-Host ""
    Write-Host "You can now distribute the contents of the output folder." -ForegroundColor Yellow
    Write-Host ""
    
    # Open output folder
    if (Test-Path $OutputDir) {
        Start-Process explorer.exe $OutputDir
    }
}
catch {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Red
    Write-Host "BUILD FAILED!" -ForegroundColor Red
    Write-Host "================================================" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

Read-Host "Press Enter to continue"