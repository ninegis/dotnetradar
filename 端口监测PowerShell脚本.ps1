# 所有雷达端口监测脚本
# 实时监控所有设备端口的监听状态

param(
    [switch]$Continuous,  # 持续监控模式
    [int]$Interval = 5    # 检查间隔（秒）
)

function Show-PortStatus {
    param([string]$Name, [int]$Port, [string]$Type)
    
    $listening = netstat -ano | findstr "LISTENING" | findstr ":$Port"
    
    if ($listening) {
        Write-Host "  ✅ " -ForegroundColor Green -NoNewline
        Write-Host "$Name (端口$Port)" -ForegroundColor White -NoNewline
        Write-Host " - $Type" -ForegroundColor DarkGray
    } else {
        Write-Host "  ❌ " -ForegroundColor Red -NoNewline
        Write-Host "$Name (端口$Port)" -ForegroundColor White -NoNewline
        Write-Host " - $Type [未监听]" -ForegroundColor DarkGray
    }
}

function Check-AllPorts {
    Clear-Host
    
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  边坡雷达监测系统 - 端口监测状态" -ForegroundColor Cyan
    Write-Host "  检查时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "【雷达设备】" -ForegroundColor Yellow
    Show-PortStatus "圆弧雷达" 1030 "DAG圆弧雷达"
    Show-PortStatus "建筑物雷达" 1060 "建筑物3D雷达"
    Show-PortStatus "建筑物2D雷达" 11135 "建筑物2D雷达"
    Show-PortStatus "MIMO Lite雷达" 10305 "MIMO Lite阵列雷达"
    Show-PortStatus "MIMO雷达" 11125 "MIMO高级雷达"
    Show-PortStatus "MIMO通用" 11129 "MIMO通用雷达"
    Write-Host ""
    
    Write-Host "【定位设备】" -ForegroundColor Yellow
    Show-PortStatus "GPS设备" 11111 "GPS定位"
    Show-PortStatus "GPS V1" 11109 "GPS V1版本"
    Show-PortStatus "北纬V1" 11110 "北纬定位V1"
    Write-Host ""
    
    Write-Host "【传感器设备】" -ForegroundColor Yellow
    Show-PortStatus "倾斜仪" 11126 "倾斜角传感器"
    Show-PortStatus "激光设备" 11131 "激光测距"
    Show-PortStatus "CM设备" 11124 "CM传感器"
    Show-PortStatus "方向传感器" 11128 "方向角传感器"
    Write-Host ""
    
    Write-Host "【控制设备】" -ForegroundColor Yellow
    Show-PortStatus "电机" 11114 "电机控制"
    Show-PortStatus "B型电机" 11115 "B型电机控制"
    Show-PortStatus "俯仰电机" 11127 "俯仰角控制"
    Write-Host ""
    
    Write-Host "【报警设备】" -ForegroundColor Yellow
    Show-PortStatus "报警设备" 11113 "报警信号接收"
    Show-PortStatus "报警设备通用" 11130 "通用报警"
    Write-Host ""
    
    Write-Host "【Web服务】" -ForegroundColor Yellow
    Show-PortStatus "前端Web" 6098 "Vue 3前端"
    Show-PortStatus "API服务" 8099 "ASP.NET Core API"
    Write-Host ""
    
    # 统计
    $radarPorts = @(1030, 1060, 11135, 10305, 11125, 11129)
    $webPorts = @(6098, 8099)
    $allDevicePorts = @(1030, 1060, 11135, 10305, 11125, 11129, 11111, 11109, 11110, 11126, 11131, 11124, 11128, 11114, 11115, 11127, 11113, 11130)
    
    $radarListening = 0
    $webListening = 0
    $deviceListening = 0
    
    foreach ($port in $radarPorts) {
        if (netstat -ano | findstr "LISTENING" | findstr ":$port") { $radarListening++ }
    }
    
    foreach ($port in $webPorts) {
        if (netstat -ano | findstr "LISTENING" | findstr ":$port") { $webListening++ }
    }
    
    foreach ($port in $allDevicePorts) {
        if (netstat -ano | findstr "LISTENING" | findstr ":$port") { $deviceListening++ }
    }
    
    Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "【统计】" -ForegroundColor Cyan
    Write-Host "  雷达设备: $radarListening/$($radarPorts.Count) 个端口在线" -ForegroundColor $(if ($radarListening -eq $radarPorts.Count) { "Green" } else { "Yellow" })
    Write-Host "  Web服务: $webListening/$($webPorts.Count) 个端口在线" -ForegroundColor $(if ($webListening -eq $webPorts.Count) { "Green" } else { "Yellow" })
    Write-Host "  所有设备: $deviceListening/$($allDevicePorts.Count) 个端口在线" -ForegroundColor $(if ($deviceListening -gt 0) { "Green" } else { "Red" })
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    if ($deviceListening -eq 0) {
        Write-Host "⚠️ 警告: 没有设备端口在监听！请启动 RadarSystem.WebAPI" -ForegroundColor Red
        Write-Host "   启动命令: cd RadarSystem.WebAPI; dotnet run" -ForegroundColor Yellow
    }
}

# 执行检查
if ($Continuous) {
    Write-Host "持续监控模式（按Ctrl+C停止）`n" -ForegroundColor Yellow
    while ($true) {
        Check-AllPorts
        Start-Sleep -Seconds $Interval
    }
} else {
    Check-AllPorts
}

