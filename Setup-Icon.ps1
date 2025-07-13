# Quick Icon Setup for UDP System Monitor
# Downloads a suitable icon from free icon sources

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   UDP System Monitor - Quick Icon Setup" -ForegroundColor Cyan  
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$ProjectDir = "C:\Users\asith\source\repos\SystemMonitorUDP\SystemMonitorUDP"
$ResourcesDir = Join-Path $ProjectDir "Resources"

# Create Resources directory if it doesn't exist
if (-not (Test-Path $ResourcesDir)) {
    New-Item -ItemType Directory -Path $ResourcesDir -Force | Out-Null
    Write-Host "Created Resources directory: $ResourcesDir" -ForegroundColor Green
}

Write-Host "Setting up icon for UDP System Monitor..." -ForegroundColor Yellow
Write-Host ""

# Create a simple SVG icon as base64 and convert to ICO
$SvgIcon = @'
<svg width="64" height="64" viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg">
  <!-- Background Circle -->
  <circle cx="32" cy="32" r="30" fill="#2D2D30" stroke="#5A5A5A" stroke-width="2"/>
  
  <!-- Chart Bars (System Monitoring) -->
  <rect x="16" y="44" width="4" height="12" fill="#0E639C"/>
  <rect x="22" y="38" width="4" height="18" fill="#0E639C"/>
  <rect x="28" y="32" width="4" height="24" fill="#0E639C"/>
  <rect x="34" y="36" width="4" height="20" fill="#0E639C"/>
  
  <!-- Signal Waves (UDP) -->
  <g fill="none" stroke="white" stroke-width="2">
    <path d="M 44 20 Q 48 16 52 20"/>
    <path d="M 42 22 Q 48 14 54 22"/>
    <path d="M 40 24 Q 48 12 56 24"/>
  </g>
  
  <!-- Signal Source -->
  <circle cx="44" cy="24" r="2" fill="white"/>
  
  <!-- Network Lines -->
  <line x1="20" y1="16" x2="28" y2="16" stroke="#0E639C" stroke-width="2"/>
  <line x1="20" y1="20" x2="32" y2="20" stroke="#0E639C" stroke-width="2"/>
  <line x1="20" y1="24" x2="26" y2="24" stroke="#0E639C" stroke-width="2"/>
</svg>
'@

# Save SVG file
$SvgPath = Join-Path $ResourcesDir "app_icon.svg"
$SvgIcon | Out-File -FilePath $SvgPath -Encoding UTF8

Write-Host "Created SVG icon: $SvgPath" -ForegroundColor Green

# Create instructions for converting SVG to ICO
$ConversionInstructions = @"
# Converting SVG to ICO Format

## Option 1: Online Converter (Recommended)
1. Go to https://convertio.co/svg-ico/
2. Upload the SVG file: app_icon.svg
3. Set size to 64x64 or multi-size
4. Download as app.ico
5. Place in Resources folder

## Option 2: Using ImageMagick (if installed)
```powershell
magick app_icon.svg -resize 64x64 app.ico
```

## Option 3: Use Free Online Icon Generators
- https://www.favicon-generator.org/
- https://realfavicongenerator.net/
- https://www.xiconeditor.com/

## Ready-Made Icon Collections
Search for these terms on free icon sites:
- "system monitor"
- "dashboard chart"
- "network analytics"
- "server monitoring"

### Recommended Free Icon Sites:
- Flaticon.com (free with attribution)
- Icons8.com (free tier available)
- Heroicons.com (completely free)
- Feathericons.com (completely free)

## After Getting the ICO File:
1. Place app.ico in the Resources folder
2. Rebuild the project
3. The icon will appear on the executable and in the system tray
"@

$InstructionsPath = Join-Path $ResourcesDir "CONVERSION_INSTRUCTIONS.md"
$ConversionInstructions | Out-File -FilePath $InstructionsPath -Encoding UTF8

Write-Host "Conversion instructions saved: $InstructionsPath" -ForegroundColor Green
Write-Host ""

# Check if we can create a simple ICO using .NET (fallback)
$FallbackIconPath = Join-Path $ResourcesDir "app.ico"

$CreateFallbackIcon = @'
Add-Type -AssemblyName System.Drawing

try {
    # Create a simple 32x32 bitmap
    $bitmap = New-Object System.Drawing.Bitmap(32, 32)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    
    # Fill background
    $bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(45, 45, 48))
    $graphics.FillEllipse($bgBrush, 2, 2, 28, 28)
    
    # Draw chart bars
    $barBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(14, 99, 156))
    $graphics.FillRectangle($barBrush, 8, 22, 2, 6)
    $graphics.FillRectangle($barBrush, 11, 19, 2, 9)
    $graphics.FillRectangle($barBrush, 14, 16, 2, 12)
    $graphics.FillRectangle($barBrush, 17, 18, 2, 10)
    
    # Draw signal waves
    $wavePen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 1)
    $graphics.DrawArc($wavePen, 20, 8, 8, 8, -45, 90)
    $graphics.DrawArc($wavePen, 19, 7, 10, 10, -45, 90)
    
    # Save as PNG first, then convert to ICO
    $pngPath = "FALLBACK_PNG_PATH"
    $bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
    
    # Convert to ICO
    $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
    $fileStream = [System.IO.File]::Create("FALLBACK_ICO_PATH")
    $icon.Save($fileStream)
    $fileStream.Close()
    
    Write-Host "Created fallback icon: FALLBACK_ICO_PATH" -ForegroundColor Green
    
    # Cleanup
    $graphics.Dispose()
    $bitmap.Dispose()
    $bgBrush.Dispose()
    $barBrush.Dispose()
    $wavePen.Dispose()
    $icon.Dispose()
}
catch {
    Write-Host "Could not create fallback icon: $($_.Exception.Message)" -ForegroundColor Yellow
}
'@

# Replace placeholders
$FallbackPngPath = Join-Path $ResourcesDir "fallback.png"
$CreateFallbackIcon = $CreateFallbackIcon.Replace("FALLBACK_PNG_PATH", $FallbackPngPath)
$CreateFallbackIcon = $CreateFallbackIcon.Replace("FALLBACK_ICO_PATH", $FallbackIconPath)

# Save and execute fallback icon creation
$FallbackScriptPath = Join-Path $ResourcesDir "Create-FallbackIcon.ps1"
$CreateFallbackIcon | Out-File -FilePath $FallbackScriptPath -Encoding UTF8

Write-Host "Creating fallback icon..." -ForegroundColor Yellow
try {
    Invoke-Expression $CreateFallbackIcon
}
catch {
    Write-Host "Fallback icon creation failed, but that's okay!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "ICON SETUP COMPLETE!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Files created in Resources folder:" -ForegroundColor White
Write-Host "- app_icon.svg (source design)" -ForegroundColor Gray
Write-Host "- CONVERSION_INSTRUCTIONS.md (how to convert)" -ForegroundColor Gray
if (Test-Path $FallbackIconPath) {
    Write-Host "- app.ico (basic fallback icon) ?" -ForegroundColor Green
} else {
    Write-Host "- app.ico (you need to create this)" -ForegroundColor Yellow
}
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Convert SVG to ICO using online converter" -ForegroundColor White
Write-Host "2. Place final app.ico in Resources folder" -ForegroundColor White
Write-Host "3. Rebuild the project" -ForegroundColor White
Write-Host ""

# Open Resources folder
if (Test-Path $ResourcesDir) {
    Start-Process explorer.exe $ResourcesDir
}