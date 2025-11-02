# 检查重点DotNetty端口启动状态
# 基于 dotnettyportconfig.json 配置

$configFile = "dotnettyportconfig.json"

if (!(Test-Path $configFile)) {
    Write-Host "错误: 找不到配置文件 $configFile" -ForegroundColor Red
    exit 1
}

# 读取配置
$config = Get-Content $configFile -Encoding UTF8 | ConvertFrom-Json

# 提取重点监测端口
$criticalPorts = $config.criticalPorts

Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  DotNetty重点端口启动检查                                    ║" -ForegroundColor Cyan
Write-Host "║  检查时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')                     ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# 检查每个重点端口
$successCount = 0
$totalCount = $criticalPorts.Count

Write-Host "【重点端口检查】共 $totalCount 个端口" -ForegroundColor Yellow
Write-Host ""

foreach ($port in $criticalPorts) {
    # 从配置中查找对应设备
    $device = $config.devices | Where-Object { $_.port -eq $port }
    
    if ($device) {
        $deviceName = $device.displayName
        $enabled = $device.enabled
        
        # 检查端口是否监听
        $listening = netstat -ano | findstr "LISTENING" | findstr ":$port"
        
        if ($listening) {
            Write-Host "  ✅ 端口 $($port.ToString().PadLeft(5)) - $deviceName [监听中]" -ForegroundColor Green
            $successCount++
        } else {
            if ($enabled) {
                Write-Host "  ❌ 端口 $($port.ToString().PadLeft(5)) - $deviceName [未监听] 配置:已启用" -ForegroundColor Red
            } else {
                Write-Host "  ⊝  端口 $($port.ToString().PadLeft(5)) - $deviceName [已禁用]" -ForegroundColor DarkGray
            }
        }
    } else {
        Write-Host "  ?  端口 $port - 未知设备" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray

# 统计信息
$enabledPorts = ($config.devices | Where-Object { $_.enabled -and $_.port -in $criticalPorts }).Count

Write-Host "【统计信息】" -ForegroundColor Cyan
Write-Host "  已配置启用: $enabledPorts 个端口" -ForegroundColor White
if ($enabledPorts -gt 0) {
    Write-Host "  实际监听: $successCount 个端口" -ForegroundColor $(if ($successCount -eq $enabledPorts) { "Green" } elseif ($successCount -gt 0) { "Yellow" } else { "Red" })
    Write-Host "  成功率: $([math]::Round($successCount * 100 / $enabledPorts, 1))%" -ForegroundColor $(if ($successCount -eq $enabledPorts) { "Green" } elseif ($successCount -gt 0) { "Yellow" } else { "Red" })
}

Write-Host ""

if ($successCount -eq 0) {
    Write-Host "❌ 所有端口都未监听！" -ForegroundColor Red
    Write-Host ""
    Write-Host "可能原因:" -ForegroundColor Yellow
    Write-Host "  1. RadarSystem.WebAPI 未启动" -ForegroundColor White
    Write-Host "  2. Netty设备服务器启动失败" -ForegroundColor White
    Write-Host "  3. 端口被其他程序占用" -ForegroundColor White
    Write-Host ""
    Write-Host "解决方案:" -ForegroundColor Yellow
    Write-Host "  cd RadarSystem.WebAPI" -ForegroundColor White
    Write-Host "  dotnet run --configuration Release" -ForegroundColor White
} elseif ($successCount -lt $enabledPorts) {
    Write-Host "⚠️  部分端口未启动" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "建议检查:" -ForegroundColor Yellow
    Write-Host "  1. 查看日志: Get-Content RadarSystem.WebAPI\logs\*.txt -Tail 100" -ForegroundColor White
    Write-Host "  2. 查看哪些设备启动失败" -ForegroundColor White
    Write-Host "  3. 检查appsettings.json中的Enable配置" -ForegroundColor White
} else {
    Write-Host "✅ 所有配置启用的端口都已成功启动！" -ForegroundColor Green
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# 显示详细监听信息（仅已启用的端口）
$hasDetail = $false
foreach ($port in $criticalPorts) {
    $device = $config.devices | Where-Object { $_.port -eq $port -and $_.enabled }
    if ($device) {
        $listening = netstat -ano | findstr "LISTENING" | findstr ":$port"
        if ($listening) {
            if (-not $hasDetail) {
                Write-Host "【详细监听信息】" -ForegroundColor Yellow
                $hasDetail = $true
            }
            Write-Host "  端口 $port ($($device.displayName)):" -ForegroundColor White
            Write-Host "    $listening" -ForegroundColor DarkGray
        }
    }
}

if (-not $hasDetail) {
    Write-Host "【详细监听信息】无端口监听" -ForegroundColor DarkGray
}

Write-Host ""
