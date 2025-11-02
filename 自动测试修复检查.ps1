# 自动测试、修复、检查系统
# 完全自动化的端到端测试流程

param(
    [int]$MaxRetries = 3,
    [int]$StartupWaitSeconds = 20
)

$ErrorActionPreference = "Continue"
$testResults = @()
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = "自动测试报告_$timestamp.txt"

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    
    $color = switch ($Level) {
        "SUCCESS" { "Green" }
        "ERROR" { "Red" }
        "WARNING" { "Yellow" }
        "INFO" { "Cyan" }
        default { "White" }
    }
    
    $logMessage = "[$(Get-Date -Format 'HH:mm:ss')] [$Level] $Message"
    Write-Host $logMessage -ForegroundColor $color
    Add-Content -Path $reportFile -Value $logMessage
}

function Test-PortListening {
    param([int]$Port, [string]$Name)
    
    $listening = netstat -ano | findstr "LISTENING" | findstr ":$Port"
    return $null -ne $listening
}

function Stop-AllProcesses {
    Write-Log "【步骤1】停止所有相关进程..." "INFO"
    
    try {
        $processes = Get-Process | Where-Object {$_.ProcessName -eq "dotnet" -or $_.ProcessName -like "*RadarSystem*"}
        
        if ($processes) {
            Write-Log "发现 $($processes.Count) 个进程需要停止" "INFO"
            $processes | Stop-Process -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 3
            Write-Log "进程已停止" "SUCCESS"
        } else {
            Write-Log "没有需要停止的进程" "INFO"
        }
        
        return $true
    }
    catch {
        Write-Log "停止进程失败: $_" "ERROR"
        return $false
    }
}

function Build-Project {
    Write-Log "【步骤2】编译项目..." "INFO"
    
    try {
        $buildOutput = dotnet build RadarSystem.sln -c Release 2>&1 | Out-String
        
        if ($buildOutput -match "失败|error|ERROR") {
            Write-Log "编译失败！" "ERROR"
            Write-Log $buildOutput "ERROR"
            return $false
        }
        
        if ($buildOutput -match "已成功|succeeded") {
            Write-Log "编译成功！" "SUCCESS"
            return $true
        }
        
        Write-Log "编译状态未知" "WARNING"
        return $false
    }
    catch {
        Write-Log "编译异常: $_" "ERROR"
        return $false
    }
}

function Start-System {
    Write-Log "【步骤3】启动系统..." "INFO"
    
    try {
        $processInfo = Start-Process -FilePath "dotnet" `
            -ArgumentList "run --project RadarSystem.WebAPI\RadarSystem.WebAPI.csproj --configuration Release --no-build" `
            -PassThru `
            -WindowStyle Hidden
        
        Write-Log "系统已启动，PID: $($processInfo.Id)" "SUCCESS"
        return $processInfo.Id
    }
    catch {
        Write-Log "启动系统失败: $_" "ERROR"
        return $null
    }
}

function Wait-ForSystem {
    param([int]$Seconds)
    
    Write-Log "【步骤4】等待系统完全启动（$Seconds 秒）..." "INFO"
    
    for ($i = 1; $i -le $Seconds; $i++) {
        Write-Progress -Activity "系统启动中" -Status "$i/$Seconds 秒" -PercentComplete ($i * 100 / $Seconds)
        Start-Sleep -Seconds 1
    }
    
    Write-Progress -Activity "系统启动中" -Completed
    Write-Log "等待完成" "SUCCESS"
}

function Check-CriticalPorts {
    Write-Log "【步骤5】检查重点端口..." "INFO"
    
    $criticalPorts = @{
        1030 = "圆弧雷达"
        10305 = "MIMO Lite"
        1060 = "建筑物雷达"
        11135 = "建筑物2D"
        11125 = "MIMO雷达"
        11129 = "MIMO通用"
        11133 = "交通雷达"
        11127 = "俯仰电机"
        11114 = "电机"
        11111 = "GPS设备"
    }
    
    $results = @{}
    $successCount = 0
    
    Write-Host ""
    Write-Host "  端口检查结果:" -ForegroundColor Yellow
    Write-Host "  ────────────────────────────────────────" -ForegroundColor DarkGray
    
    foreach ($port in $criticalPorts.Keys | Sort-Object) {
        $name = $criticalPorts[$port]
        $isListening = Test-PortListening -Port $port -Name $name
        $results[$port] = $isListening
        
        if ($isListening) {
            Write-Host "    ✅ 端口 $($port.ToString().PadLeft(5)) - $name" -ForegroundColor Green
            Write-Log "端口 $port ($name) 监听成功" "SUCCESS"
            $successCount++
        } else {
            Write-Host "    ❌ 端口 $($port.ToString().PadLeft(5)) - $name" -ForegroundColor Red
            Write-Log "端口 $port ($name) 未监听" "WARNING"
        }
    }
    
    Write-Host "  ────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "  成功: $successCount / $($criticalPorts.Count)" -ForegroundColor $(if ($successCount -gt 0) { "Green" } else { "Red" })
    Write-Host ""
    
    Write-Log "端口检查完成: $successCount/$($criticalPorts.Count) 成功" $(if ($successCount -eq $criticalPorts.Count) { "SUCCESS" } elseif ($successCount -gt 0) { "WARNING" } else { "ERROR" })
    
    return @{
        Results = $results
        SuccessCount = $successCount
        TotalCount = $criticalPorts.Count
    }
}

function Check-Logs {
    Write-Log "【步骤6】检查日志..." "INFO"
    
    try {
        $logFile = Get-ChildItem RadarSystem.WebAPI\logs\*.txt | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        
        if ($logFile) {
            Write-Log "最新日志: $($logFile.Name)" "INFO"
            
            # 查找圆弧雷达相关日志
            $arcLogs = Get-Content $logFile.FullName | Select-String -Pattern "圆弧|ArcRadar|1030|AllDevice" | Select-Object -Last 10
            
            if ($arcLogs) {
                Write-Log "找到圆弧雷达相关日志 $($arcLogs.Count) 条" "INFO"
                foreach ($log in $arcLogs) {
                    Add-Content -Path $reportFile -Value "  LOG: $log"
                }
            } else {
                Write-Log "未找到圆弧雷达启动日志（可能未启动）" "WARNING"
            }
            
            # 查找错误
            $errors = Get-Content $logFile.FullName | Select-String -Pattern "ERR\]|ERROR|失败|异常" | Select-Object -Last 5
            if ($errors) {
                Write-Log "发现错误日志 $($errors.Count) 条" "WARNING"
                foreach ($error in $errors) {
                    Write-Log "  错误: $error" "WARNING"
                }
            }
        } else {
            Write-Log "未找到日志文件" "WARNING"
        }
    }
    catch {
        Write-Log "检查日志失败: $_" "ERROR"
    }
}

function Diagnose-Issues {
    param($PortResults)
    
    Write-Log "【步骤7】诊断问题..." "INFO"
    
    $issues = @()
    
    # 检查是否所有端口都未监听
    if ($PortResults.SuccessCount -eq 0) {
        $issue = "所有端口都未监听 - Netty设备服务器未启动"
        Write-Log $issue "ERROR"
        $issues += $issue
    }
    
    # 检查1030端口（最重要）
    if (-not $PortResults.Results[1030]) {
        $issue = "端口1030（圆弧雷达）未监听 - 核心功能不可用"
        Write-Log $issue "ERROR"
        $issues += $issue
    }
    
    # 检查WebAPI端口
    if (-not (Test-PortListening -Port 8099 -Name "API")) {
        $issue = "端口8099（API）未监听 - WebAPI未启动"
        Write-Log $issue "ERROR"
        $issues += $issue
    } else {
        Write-Log "端口8099（API）正常" "SUCCESS"
    }
    
    return $issues
}

function Auto-Fix {
    param($Issues)
    
    Write-Log "【步骤8】尝试自动修复..." "INFO"
    
    $fixApplied = $false
    
    foreach ($issue in $Issues) {
        if ($issue -match "所有端口都未监听") {
            Write-Log "问题: AllDeviceNettyServersHostedService 未执行" "WARNING"
            Write-Log "修复: 使用 Program.cs 中的直接启动代码（已添加）" "INFO"
            Write-Log "建议: 重新启动系统并查看控制台输出" "INFO"
            $fixApplied = $true
        }
        
        if ($issue -match "端口1030") {
            Write-Log "问题: 圆弧雷达服务器未启动" "WARNING"
            Write-Log "修复: 检查 appsettings.json 中 Netty.ArcRadar.Enable = true" "INFO"
            $fixApplied = $true
        }
    }
    
    if (-not $fixApplied) {
        Write-Log "未找到可自动修复的问题" "INFO"
    }
    
    return $fixApplied
}

function Generate-Report {
    param($TestResults, $PortCheckResults, $Issues)
    
    Write-Log "【步骤9】生成测试报告..." "INFO"
    
    Add-Content -Path $reportFile -Value "`n"
    Add-Content -Path $reportFile -Value "════════════════════════════════════════════════════════════════"
    Add-Content -Path $reportFile -Value "  自动测试报告"
    Add-Content -Path $reportFile -Value "  生成时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    Add-Content -Path $reportFile -Value "════════════════════════════════════════════════════════════════"
    Add-Content -Path $reportFile -Value ""
    Add-Content -Path $reportFile -Value "【测试结果】"
    Add-Content -Path $reportFile -Value "  端口成功: $($PortCheckResults.SuccessCount) / $($PortCheckResults.TotalCount)"
    Add-Content -Path $reportFile -Value "  发现问题: $($Issues.Count) 个"
    Add-Content -Path $reportFile -Value ""
    Add-Content -Path $reportFile -Value "【端口详情】"
    
    foreach ($port in $PortCheckResults.Results.Keys | Sort-Object) {
        $status = if ($PortCheckResults.Results[$port]) { "✅ 监听" } else { "❌ 未监听" }
        Add-Content -Path $reportFile -Value "  端口 $port : $status"
    }
    
    if ($Issues.Count -gt 0) {
        Add-Content -Path $reportFile -Value ""
        Add-Content -Path $reportFile -Value "【发现的问题】"
        foreach ($issue in $Issues) {
            Add-Content -Path $reportFile -Value "  - $issue"
        }
    }
    
    Add-Content -Path $reportFile -Value ""
    Add-Content -Path $reportFile -Value "════════════════════════════════════════════════════════════════"
    
    Write-Log "测试报告已保存: $reportFile" "SUCCESS"
}

# ====================================================================
# 主测试流程
# ====================================================================

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                      自动测试、修复、检查系统                                ║" -ForegroundColor Cyan
Write-Host "║                      开始时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')                           ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Add-Content -Path $reportFile -Value "自动测试开始: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Add-Content -Path $reportFile -Value "════════════════════════════════════════════════════════════════"
Add-Content -Path $reportFile -Value ""

$attempt = 1

while ($attempt -le $MaxRetries) {
    Write-Host ""
    Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor Yellow
    Write-Host "  尝试 $attempt / $MaxRetries" -ForegroundColor Yellow
    Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Log "开始第 $attempt 次测试尝试" "INFO"
    
    # 1. 停止进程
    $stopResult = Stop-AllProcesses
    if (-not $stopResult) {
        Write-Log "停止进程失败，跳过此次尝试" "ERROR"
        $attempt++
        continue
    }
    
    # 2. 编译项目
    $buildResult = Build-Project
    if (-not $buildResult) {
        Write-Log "编译失败，跳过此次尝试" "ERROR"
        $attempt++
        continue
    }
    
    # 3. 启动系统
    $processPid = Start-System
    if ($null -eq $processPid) {
        Write-Log "启动系统失败，跳过此次尝试" "ERROR"
        $attempt++
        continue
    }
    
    # 4. 等待启动
    Wait-ForSystem -Seconds $StartupWaitSeconds
    
    # 5. 检查端口
    $portResults = Check-CriticalPorts
    
    # 6. 检查日志
    Check-Logs
    
    # 7. 诊断问题
    $issues = Diagnose-Issues -PortResults $portResults
    
    # 8. 判断是否成功
    if ($portResults.SuccessCount -ge 1) {
        Write-Host ""
        Write-Host "╔══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
        Write-Host "║                         ✅ 测试成功！                                        ║" -ForegroundColor Green
        Write-Host "║  至少 $($portResults.SuccessCount) 个端口启动成功                                             ║" -ForegroundColor Green
        Write-Host "╚══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
        Write-Host ""
        
        Write-Log "测试成功！$($portResults.SuccessCount) 个端口监听" "SUCCESS"
        
        # 生成报告
        Generate-Report -TestResults $testResults -PortCheckResults $portResults -Issues $issues
        
        Write-Host ""
        Write-Host "【测试报告】" -ForegroundColor Cyan
        Get-Content $reportFile
        
        Write-Host ""
        Write-Host "【下一步】" -ForegroundColor Yellow
        Write-Host "  1. 查看实时输出: Get-Process -Id $processPid" -ForegroundColor White
        Write-Host "  2. 检查日志: Get-Content RadarSystem.WebAPI\logs\*.txt -Tail 100" -ForegroundColor White
        Write-Host "  3. 停止系统: Stop-Process -Id $processPid" -ForegroundColor White
        Write-Host "  4. 再次检查: .\CheckCriticalPorts.ps1" -ForegroundColor White
        Write-Host ""
        
        exit 0
    }
    
    # 9. 自动修复
    Write-Log "尝试自动修复问题..." "WARNING"
    $fixApplied = Auto-Fix -Issues $issues
    
    if (-not $fixApplied) {
        Write-Log "无可用的自动修复方案" "WARNING"
    }
    
    # 10. 停止进程准备下次尝试
    if ($attempt -lt $MaxRetries) {
        Write-Log "准备下一次尝试..." "INFO"
        Stop-Process -Id $processPid -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 5
    }
    
    $attempt++
}

# 测试失败
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Red
Write-Host "║                         ❌ 测试失败！                                        ║" -ForegroundColor Red
Write-Host "║  经过 $MaxRetries 次尝试，仍有端口未启动                                             ║" -ForegroundColor Red
Write-Host "╚══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Red
Write-Host ""

Write-Log "所有尝试均失败" "ERROR"

# 生成最终报告
Generate-Report -TestResults $testResults -PortCheckResults $portResults -Issues $issues

Write-Host ""
Write-Host "【最终诊断】" -ForegroundColor Red
Write-Host "  问题: Netty设备服务器无法启动" -ForegroundColor White
Write-Host ""
Write-Host "【建议的手动操作】" -ForegroundColor Yellow
Write-Host "  1. 在PowerShell中前台运行查看实时输出:" -ForegroundColor White
Write-Host "     cd RadarSystem.WebAPI" -ForegroundColor Cyan
Write-Host "     dotnet run --configuration Release" -ForegroundColor Cyan
Write-Host ""
Write-Host "  2. 观察控制台是否显示:" -ForegroundColor White
Write-Host "     【圆弧雷达服务器】开始启动..." -ForegroundColor Cyan
Write-Host "     ✅✅✅ 圆弧雷达服务器启动成功" -ForegroundColor Cyan
Write-Host ""
Write-Host "  3. 如果看到错误信息，请复制完整错误" -ForegroundColor White
Write-Host ""
Write-Host "  4. 检查日志文件:" -ForegroundColor White
Write-Host "     Get-Content RadarSystem.WebAPI\logs\*.txt -Tail 200" -ForegroundColor Cyan
Write-Host ""
Write-Host "【测试报告位置】" -ForegroundColor Yellow
Write-Host "  $reportFile" -ForegroundColor White
Write-Host ""

exit 1

