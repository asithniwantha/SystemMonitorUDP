# UDP System Monitor - PowerShell Build Script with Warning Analysis
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
    Write-Host "[1/6] Creating Resources directory for icons..." -ForegroundColor Green
    $ResourcesDir = Join-Path $ProjectDir "Resources"
    if (-not (Test-Path $ResourcesDir)) {
        New-Item -ItemType Directory -Path $ResourcesDir -Force | Out-Null
    }

    Write-Host "[2/6] Cleaning previous builds..." -ForegroundColor Green
    dotnet clean --configuration Release --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Clean failed" }

    Write-Host "[3/6] Restoring NuGet packages..." -ForegroundColor Green
    dotnet restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

    Write-Host "[4/6] Building Release configuration with warning analysis..." -ForegroundColor Green
    $buildOutput = dotnet build --configuration Release --no-restore --verbosity normal 2>&1
    $warnings = $buildOutput | Select-String -Pattern "warning"
    
    if ($warnings) {
        Write-Host ""
        Write-Host "??  BUILD WARNINGS DETECTED:" -ForegroundColor Yellow
        $warnings | ForEach-Object { Write-Host "   $_" -ForegroundColor Yellow }
        Write-Host ""
    } else {
        Write-Host "? No warnings detected!" -ForegroundColor Green
    }
    
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    Write-Host "[5/6] Running build analysis..." -ForegroundColor Green
    # Check for common .NET 9 issues
    $sourceFiles = Get-ChildItem -Path $ProjectDir -Filter "*.cs" -Recurse
    $potentialIssues = @()
    
    foreach ($file in $sourceFiles) {
        $content = Get-Content $file.FullName -Raw
        if ($content -match "Assembly\.GetExecutingAssembly\(\)\.Location" -and $content -notmatch "#pragma warning disable IL3000") {
            $potentialIssues += "Potential IL3000 warning in $($file.Name)"
        }
        if ($content -match "ShowBalloonTip" -and $content -notmatch "#pragma warning disable") {
            $potentialIssues += "Potential platform compatibility warning in $($file.Name)"
        }
    }
    
    if ($potentialIssues.Count -eq 0) {
        Write-Host "? No potential warning sources detected!" -ForegroundColor Green
    } else {
        Write-Host "??  Potential issues (may be suppressed):" -ForegroundColor Cyan
        $potentialIssues | ForEach-Object { Write-Host "   $_" -ForegroundColor Cyan }
    }

    Write-Host "[6/6] Publishing self-contained executable..." -ForegroundColor Green
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o $OutputDir --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

    Write-Host ""
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Build Summary:" -ForegroundColor White
    Write-Host "   • Version: $Version" -ForegroundColor Gray
    Write-Host "   • Target: .NET 9 (win-x64)" -ForegroundColor Gray
    Write-Host "   • Configuration: Release" -ForegroundColor Gray
    Write-Host "   • Package: Single-file, self-contained" -ForegroundColor Gray
    Write-Host "   • Warnings: $($warnings.Count)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "?? Output Location:" -ForegroundColor White
    Write-Host "   $OutputDir\SystemMonitorUDP.exe" -ForegroundColor Gray
    Write-Host ""
    Write-Host "? Features Included:" -ForegroundColor White
    Write-Host "   • Real-time system monitoring" -ForegroundColor Gray
    Write-Host "   • UDP transmission with JSON format" -ForegroundColor Gray
    Write-Host "   • Modern WPF UI with dark theme" -ForegroundColor Gray
    Write-Host "   • System tray integration" -ForegroundColor Gray
    Write-Host "   • Windows startup integration" -ForegroundColor Gray
    Write-Host "   • Custom application icon" -ForegroundColor Gray
    Write-Host "   • Multi-source CPU temperature" -ForegroundColor Gray
    Write-Host ""
    Write-Host "?? Ready for distribution!" -ForegroundColor Yellow
    Write-Host ""
    
    # File size information
    $exePath = Join-Path $OutputDir "SystemMonitorUDP.exe"
    if (Test-Path $exePath) {
        $fileSize = (Get-Item $exePath).Length
        $fileSizeMB = [Math]::Round($fileSize / 1MB, 1)
        Write-Host "?? Executable Size: $fileSizeMB MB" -ForegroundColor Cyan
    }
    
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
    
    # Show last few lines of build output for diagnosis
    if ($buildOutput) {
        Write-Host "Last build output:" -ForegroundColor Yellow
        $buildOutput | Select-Object -Last 10 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    }
    Write-Host ""
}

Read-Host "Press Enter to continue"