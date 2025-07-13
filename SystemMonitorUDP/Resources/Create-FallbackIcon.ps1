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
    $pngPath = "C:\Users\asith\source\repos\SystemMonitorUDP\SystemMonitorUDP\Resources\fallback.png"
    $bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
    
    # Convert to ICO
    $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
    $fileStream = [System.IO.File]::Create("C:\Users\asith\source\repos\SystemMonitorUDP\SystemMonitorUDP\Resources\app.ico")
    $icon.Save($fileStream)
    $fileStream.Close()
    
    Write-Host "Created fallback icon: C:\Users\asith\source\repos\SystemMonitorUDP\SystemMonitorUDP\Resources\app.ico" -ForegroundColor Green
    
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
