# Monitor all radar data save
Write-Host "`n[Monitor All Radar Data Save]`n" -ForegroundColor Cyan
Write-Host "==============================================================================="

$dataDir = "C:\kotradar2025\dotnetradar\RadarSystem.WebAPI\Data\data"
$radarPorts = @(
    @{Port=1030; Name="ArcRadar"},
    @{Port=10305; Name="MimoLiteRadar"},
    @{Port=11125; Name="MimoRadar"},
    @{Port=11129; Name="Mimo"},
    @{Port=1060; Name="BuildingRadar"},
    @{Port=11135; Name="Building2DRadar"}
)

$checkCount = 0
$maxChecks = 120  # 20 minutes
$checkInterval = 10  # 10 seconds
$foundData = $false

Write-Host "`nMonitoring ALL radar ports..."
Write-Host "Data directory: $dataDir"
Write-Host "Check interval: $checkInterval seconds"
Write-Host "Max checks: $maxChecks`n"
Write-Host "===============================================================================`n"

while ($checkCount -lt $maxChecks -and -not $foundData) {
    $checkCount++
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    Write-Host "[$timestamp] Check #$checkCount" -ForegroundColor Gray
    
    # Check all radar ports
    foreach ($radar in $radarPorts) {
        $port = $radar.Port
        $name = $radar.Name
        
        $listening = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
        $connected = Get-NetTCPConnection -LocalPort $port -State Established -ErrorAction SilentlyContinue
        
        if ($listening) {
            if ($connected) {
                Write-Host "  [$name] Port $port : Listening, $($connected.Count) device(s) connected" -ForegroundColor Green
            } else {
                Write-Host "  [$name] Port $port : Listening, no device" -ForegroundColor Yellow
            }
        } else {
            Write-Host "  [$name] Port $port : NOT listening" -ForegroundColor Red
        }
    }
    
    # Check data files
    if (Test-Path $dataDir) {
        $files = Get-ChildItem $dataDir -Recurse -File -ErrorAction SilentlyContinue | 
                 Where-Object { $_.Extension -ne ".db" -and $_.Name -notlike "*.db-*" }
        
        if ($files -and $files.Count -gt 0) {
            $foundData = $true
            $totalSize = ($files | Measure-Object -Property Length -Sum).Sum
            
            Write-Host "`n"
            Write-Host "===============================================================================" -ForegroundColor Green
            Write-Host "[SUCCESS] Data files saved!" -ForegroundColor Green
            Write-Host "===============================================================================" -ForegroundColor Green
            Write-Host ""
            
            Write-Host "File count: $($files.Count)" -ForegroundColor Cyan
            Write-Host "Total size: $([math]::Round($totalSize/1KB,2)) KB`n" -ForegroundColor Cyan
            
            # Group by radar type
            $files | Group-Object { $_.DirectoryName } | ForEach-Object {
                Write-Host "Directory: $($_.Name)" -ForegroundColor Yellow
                $_.Group | Select-Object Name, @{N='Size(KB)';E={[math]::Round($_.Length/1KB,2)}}, LastWriteTime | Format-Table -AutoSize
            }
            
            Write-Host "===============================================================================" -ForegroundColor Green
            Write-Host "[SUCCESS] Data save verified!" -ForegroundColor Green
            Write-Host "===============================================================================" -ForegroundColor Green
            
            break
        } else {
            Write-Host "  [Data] No files yet" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  [Data] Directory not exists" -ForegroundColor Yellow
    }
    
    Write-Host ""
    
    if ($checkCount -lt $maxChecks) {
        Start-Sleep $checkInterval
    }
}

if (-not $foundData) {
    Write-Host "`n[TIMEOUT] No data after $maxChecks checks`n" -ForegroundColor Yellow
}

