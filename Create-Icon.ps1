# UDP System Monitor - Icon Generator Script
# This script creates a custom icon for the UDP System Monitor application

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   UDP System Monitor - Icon Generator" -ForegroundColor Cyan  
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$ProjectDir = "C:\Users\asith\source\repos\SystemMonitorUDP\SystemMonitorUDP"
$ResourcesDir = Join-Path $ProjectDir "Resources"

# Create Resources directory if it doesn't exist
if (-not (Test-Path $ResourcesDir)) {
    New-Item -ItemType Directory -Path $ResourcesDir -Force | Out-Null
    Write-Host "Created Resources directory: $ResourcesDir" -ForegroundColor Green
}

Write-Host "Generating icon for UDP System Monitor..." -ForegroundColor Yellow
Write-Host ""

# Icon concept description
$IconDescription = @"
=== ICON DESIGN CONCEPT ===

The UDP System Monitor icon will feature:
1. ?? Chart/Graph symbol (system monitoring)
2. ?? Network/Signal waves (UDP transmission)  
3. ?? Computer/System element
4. ?? Real-time activity indicator
5. Modern flat design with professional colors

Colors: Blue (#0E639C), Dark Gray (#2D2D30), White accents
Style: Modern, minimalist, professional
Sizes: 16x16, 32x32, 48x48, 256x256 (multi-resolution ICO)

=== ICON FILES NEEDED ===

1. app.ico - Main application icon (multi-resolution)
2. tray.ico - System tray icon (16x16, 32x32)
3. app.png - High-resolution source (256x256)

"@

Write-Host $IconDescription -ForegroundColor White
Write-Host ""

# Create PowerShell script to generate a simple icon using built-in graphics
$IconGeneratorScript = @'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

function Create-SystemMonitorIcon {
    param([int]$Size = 256)
    
    # Create bitmap
    $bitmap = New-Object System.Drawing.Bitmap($Size, $Size)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    
    # Colors
    $primaryBlue = [System.Drawing.Color]::FromArgb(14, 99, 156)      # #0E639C
    $darkGray = [System.Drawing.Color]::FromArgb(45, 45, 48)          # #2D2D30
    $lightGray = [System.Drawing.Color]::FromArgb(200, 200, 200)      # Light gray
    $white = [System.Drawing.Color]::White
    
    # Background circle
    $bgBrush = New-Object System.Drawing.SolidBrush($darkGray)
    $graphics.FillEllipse($bgBrush, 10, 10, $Size-20, $Size-20)
    
    # Chart bars (system monitoring)
    $barBrush = New-Object System.Drawing.SolidBrush($primaryBlue)
    $barWidth = $Size / 12
    $baseY = $Size * 0.7
    
    # Draw 4 bars with different heights
    for ($i = 0; $i -lt 4; $i++) {
        $x = ($Size * 0.25) + ($i * $barWidth * 1.5)
        $height = ($Size * 0.1) + ($i * $Size * 0.08)
        $y = $baseY - $height
        $graphics.FillRectangle($barBrush, $x, $y, $barWidth, $height)
    }
    
    # Signal waves (UDP transmission)
    $wavePen = New-Object System.Drawing.Pen($white, 3)
    $centerX = $Size * 0.75
    $centerY = $Size * 0.3
    
    # Draw concentric arcs for signal waves
    for ($i = 1; $i -le 3; $i++) {
        $radius = $i * ($Size * 0.06)
        $rect = New-Object System.Drawing.RectangleF(($centerX - $radius), ($centerY - $radius), ($radius * 2), ($radius * 2))
        $graphics.DrawArc($wavePen, $rect, -45, 90)
    }
    
    # Signal source dot
    $dotBrush = New-Object System.Drawing.SolidBrush($white)
    $dotSize = $Size * 0.03
    $graphics.FillEllipse($dotBrush, ($centerX - $dotSize/2), ($centerY - $dotSize/2), $dotSize, $dotSize)
    
    # Border
    $borderPen = New-Object System.Drawing.Pen($lightGray, 2)
    $graphics.DrawEllipse($borderPen, 10, 10, $Size-20, $Size-20)
    
    # Cleanup
    $graphics.Dispose()
    $bgBrush.Dispose()
    $barBrush.Dispose()
    $wavePen.Dispose()
    $dotBrush.Dispose()
    $borderPen.Dispose()
    
    return $bitmap
}

function Save-Icon {
    param([System.Drawing.Bitmap]$Bitmap, [string]$FilePath)
    
    try {
        $Bitmap.Save($FilePath, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "Saved: $FilePath" -ForegroundColor Green
    }
    catch {
        Write-Host "Error saving $FilePath : $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Generate icons
$ResourcesPath = "RESOURCES_PATH_PLACEHOLDER"

Write-Host "Generating icon files..." -ForegroundColor Yellow

# Generate different sizes
$icon256 = Create-SystemMonitorIcon -Size 256
$icon48 = Create-SystemMonitorIcon -Size 48
$icon32 = Create-SystemMonitorIcon -Size 32
$icon16 = Create-SystemMonitorIcon -Size 16

# Save PNG files
Save-Icon -Bitmap $icon256 -FilePath (Join-Path $ResourcesPath "app_256.png")
Save-Icon -Bitmap $icon48 -FilePath (Join-Path $ResourcesPath "app_48.png")
Save-Icon -Bitmap $icon32 -FilePath (Join-Path $ResourcesPath "app_32.png")
Save-Icon -Bitmap $icon16 -FilePath (Join-Path $ResourcesPath "app_16.png")

Write-Host ""
Write-Host "Icon generation complete!" -ForegroundColor Green
Write-Host "Next step: Convert PNG files to ICO format using online converter" -ForegroundColor Yellow
Write-Host "Recommended: https://convertico.com/ or https://ico-converter.com/" -ForegroundColor Yellow

# Cleanup
$icon256.Dispose()
$icon48.Dispose()
$icon32.Dispose()
$icon16.Dispose()
'@

# Replace placeholder with actual path
$IconGeneratorScript = $IconGeneratorScript.Replace("RESOURCES_PATH_PLACEHOLDER", $ResourcesDir)

# Save the icon generator script
$GeneratorPath = Join-Path $ResourcesDir "Generate-Icon.ps1"
$IconGeneratorScript | Out-File -FilePath $GeneratorPath -Encoding UTF8

Write-Host "Icon generator script created: $GeneratorPath" -ForegroundColor Green
Write-Host ""
Write-Host "To generate the icon:" -ForegroundColor Yellow
Write-Host "1. Run: PowerShell -ExecutionPolicy Bypass -File '$GeneratorPath'" -ForegroundColor White
Write-Host "2. Convert the generated PNG files to ICO format" -ForegroundColor White
Write-Host "3. Place the ICO file in the Resources folder as 'app.ico'" -ForegroundColor White
Write-Host ""

# Create a simple instructions file
$InstructionsPath = Join-Path $ResourcesDir "ICON_INSTRUCTIONS.md"
$Instructions = @"
# UDP System Monitor - Icon Setup Instructions

## Quick Setup

1. **Generate Base Icons:**
   ```powershell
   PowerShell -ExecutionPolicy Bypass -File "Generate-Icon.ps1"
   ```

2. **Convert to ICO format:**
   - Use online converter: https://convertico.com/
   - Upload the generated PNG files (256px recommended)
   - Download as 'app.ico'
   - Place in Resources folder

3. **Alternative - Download Ready-Made Icons:**
   Search for free icons with these keywords:
   - "system monitor icon"
   - "network monitoring icon" 
   - "dashboard icon"
   - "analytics icon"
   
   Recommended sites:
   - https://www.flaticon.com/
   - https://icons8.com/
   - https://www.iconfinder.com/

## Icon Specifications

- **Format:** ICO (Windows Icon)
- **Sizes:** Multi-resolution (16x16, 32x32, 48x48, 256x256)
- **Style:** Modern, professional
- **Theme:** System monitoring, networking, analytics
- **Colors:** Blue (#0E639C), Dark (#2D2D30), White accents

## Files Needed

- `app.ico` - Main application icon
- `tray.ico` - System tray icon (optional, can use same as app.ico)

## After Adding Icon

Update the project file to reference the icon:
```xml
<ApplicationIcon>Resources\app.ico</ApplicationIcon>
```

The build process will automatically include the icon in the executable.
"@

$Instructions | Out-File -FilePath $InstructionsPath -Encoding UTF8

Write-Host "Instructions saved: $InstructionsPath" -ForegroundColor Green

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "ICON SETUP READY!" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan