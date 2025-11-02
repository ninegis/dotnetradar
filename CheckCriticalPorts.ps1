# Check Critical DotNetty Ports Status
# Critical Ports: 1030, 10305, 1060, 11135, 11125, 11129, 11133, 11127, 11114, 11111

param(
    [switch]$Detailed
)

$criticalPorts = @{
    1030 = "ArcRadar"
    10305 = "MIMOLite"
    1060 = "Building"
    11135 = "Building2D"
    11125 = "MIMO"
    11129 = "MIMOCommon"
    11133 = "Traffic"
    11127 = "MotorPitch"
    11114 = "Motor"
    11111 = "GPS"
}

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  DotNetty Critical Ports Check - $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

$successCount = 0

foreach ($port in $criticalPorts.Keys | Sort-Object) {
    $name = $criticalPorts[$port]
    $listening = netstat -ano | findstr "LISTENING" | findstr ":$port"
    
    if ($listening) {
        Write-Host "  [OK] Port $($port.ToString().PadLeft(5)) - $name" -ForegroundColor Green
        $successCount++
        
        if ($Detailed) {
            Write-Host "       $listening" -ForegroundColor DarkGray
        }
    } else {
        Write-Host "  [--] Port $($port.ToString().PadLeft(5)) - $name [NOT LISTENING]" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "----------------------------------------------------------------" -ForegroundColor DarkGray
Write-Host "  Success: $successCount / $($criticalPorts.Count)" -ForegroundColor $(if ($successCount -eq $criticalPorts.Count) { "Green" } elseif ($successCount -gt 0) { "Yellow" } else { "Red" })
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

