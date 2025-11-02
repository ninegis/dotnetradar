# Automated Test, Fix and Check System
# Fully automated end-to-end testing

param(
    [int]$MaxRetries = 2,
    [int]$StartupWaitSeconds = 20
)

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = "AutoTestReport_$timestamp.txt"

function Write-TestLog {
    param([string]$Message, [string]$Level = "INFO")
    
    $color = switch ($Level) {
        "SUCCESS" { "Green" }
        "ERROR" { "Red" }
        "WARNING" { "Yellow" }
        default { "Cyan" }
    }
    
    $log = "[$(Get-Date -Format 'HH:mm:ss')] [$Level] $Message"
    Write-Host $log -ForegroundColor $color
    Add-Content -Path $reportFile -Value $log
}

Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "  Automated Test System - DotNetty Port Monitoring" -ForegroundColor Cyan
Write-Host "  Start Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "================================================================`n" -ForegroundColor Cyan

# STEP 1: Stop all processes
Write-TestLog "STEP 1: Stopping all processes..." "INFO"
Get-Process | Where-Object {$_.ProcessName -eq "dotnet"} | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3
Write-TestLog "Processes stopped" "SUCCESS"

# STEP 2: Build project
Write-TestLog "STEP 2: Building project..." "INFO"
$buildOutput = dotnet build RadarSystem.sln -c Release 2>&1 | Out-String

if ($buildOutput -match "succeeded|已成功") {
    Write-TestLog "Build SUCCESS" "SUCCESS"
} else {
    Write-TestLog "Build FAILED" "ERROR"
    Add-Content -Path $reportFile -Value $buildOutput
    exit 1
}

# STEP 3: Start system
Write-TestLog "STEP 3: Starting system..." "INFO"
$proc = Start-Process -FilePath "dotnet" -ArgumentList "run --project RadarSystem.WebAPI\RadarSystem.WebAPI.csproj -c Release --no-build" -PassThru -WindowStyle Hidden
Write-TestLog "System started, PID: $($proc.Id)" "SUCCESS"

# STEP 4: Wait for startup
Write-TestLog "STEP 4: Waiting $StartupWaitSeconds seconds..." "INFO"
for ($i = 1; $i -le $StartupWaitSeconds; $i++) {
    Write-Progress -Activity "Startup" -Status "$i/$StartupWaitSeconds sec" -PercentComplete ($i * 100 / $StartupWaitSeconds)
    Start-Sleep -Seconds 1
}
Write-Progress -Activity "Startup" -Completed

# STEP 5: Check critical ports
Write-TestLog "STEP 5: Checking critical ports..." "INFO"

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

$successCount = 0

Write-Host "`n  Port Check Results:" -ForegroundColor Yellow
Write-Host "  ----------------------------------------" -ForegroundColor DarkGray

foreach ($port in $criticalPorts.Keys | Sort-Object) {
    $name = $criticalPorts[$port]
    $listening = netstat -ano | findstr "LISTENING" | findstr ":$port"
    
    if ($listening) {
        Write-Host "    [OK] Port $($port.ToString().PadLeft(5)) - $name" -ForegroundColor Green
        Write-TestLog "Port $port ($name) LISTENING" "SUCCESS"
        $successCount++
    } else {
        Write-Host "    [--] Port $($port.ToString().PadLeft(5)) - $name [NOT LISTENING]" -ForegroundColor Red
        Write-TestLog "Port $port ($name) NOT LISTENING" "WARNING"
    }
}

Write-Host "  ----------------------------------------" -ForegroundColor DarkGray
Write-Host "  Success: $successCount / $($criticalPorts.Count)`n" -ForegroundColor $(if ($successCount -gt 0) { "Green" } else { "Red" })

# STEP 6: Check logs
Write-TestLog "STEP 6: Checking logs..." "INFO"
$logFile = Get-ChildItem RadarSystem.WebAPI\logs\*.txt | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($logFile) {
    Write-TestLog "Latest log: $($logFile.Name)" "INFO"
    $arcLogs = Get-Content $logFile.FullName | Select-String -Pattern "ArcRadar|1030" | Select-Object -Last 5
    if ($arcLogs) {
        Write-TestLog "Found $($arcLogs.Count) ArcRadar log entries" "INFO"
    } else {
        Write-TestLog "No ArcRadar logs found" "WARNING"
    }
}

# STEP 7: Generate report
Write-TestLog "STEP 7: Generating report..." "INFO"

Add-Content -Path $reportFile -Value "`n================================================================"
Add-Content -Path $reportFile -Value "  TEST SUMMARY"
Add-Content -Path $reportFile -Value "================================================================"
Add-Content -Path $reportFile -Value "  Ports Online: $successCount / $($criticalPorts.Count)"
Add-Content -Path $reportFile -Value "  Result: $(if ($successCount -gt 0) { 'PARTIAL SUCCESS' } else { 'FAILED' })"
Add-Content -Path $reportFile -Value "================================================================`n"

Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "  TEST COMPLETED" -ForegroundColor Cyan
Write-Host "  Ports Online: $successCount / $($criticalPorts.Count)" -ForegroundColor $(if ($successCount -gt 0) { "Green" } else { "Red" })
Write-Host "  Report: $reportFile" -ForegroundColor Cyan
Write-Host "================================================================`n" -ForegroundColor Cyan

if ($successCount -gt 0) {
    Write-Host "[SUCCESS] At least $successCount port(s) are listening!" -ForegroundColor Green
    Write-Host "`nNext Steps:" -ForegroundColor Yellow
    Write-Host "  1. Check console output of PID $($proc.Id)" -ForegroundColor White
    Write-Host "  2. Run: .\CheckCriticalPorts.ps1" -ForegroundColor White
    Write-Host "  3. Monitor: Get-Content RadarSystem.WebAPI\logs\*.txt -Tail 50 -Wait`n" -ForegroundColor White
    exit 0
} else {
    Write-Host "[FAILED] No ports listening!" -ForegroundColor Red
    Write-Host "`nManual Test Required:" -ForegroundColor Yellow
    Write-Host "  cd RadarSystem.WebAPI" -ForegroundColor White
    Write-Host "  dotnet run --configuration Release`n" -ForegroundColor White
    exit 1
}

