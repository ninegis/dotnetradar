# 持续监控数据保存脚本 - 直到发现数据文件为止
Write-Host "`n【持续监控数据保存 - 直到成功】`n" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════════════════════"

$dataDir = "C:\kotradar2025\dotnetradar\RadarSystem.WebAPI\Data\data"
$checkCount = 0
$foundData = $false

Write-Host "`n监控目标目录: $dataDir"
Write-Host "检查间隔: 10秒"
Write-Host "按 Ctrl+C 停止监控`n"
Write-Host "═══════════════════════════════════════════════════════════════════════════════`n"

while (-not $foundData) {
    $checkCount++
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    Write-Host "[$timestamp] 第 $checkCount 次检查..." -ForegroundColor Gray
    
    if (Test-Path $dataDir) {
        $files = Get-ChildItem $dataDir -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Extension -ne ".db" -and $_.Extension -ne ".db-shm" -and $_.Extension -ne ".db-wal" }
        
        if ($files -and $files.Count -gt 0) {
            $foundData = $true
            $totalSize = ($files | Measure-Object -Property Length -Sum).Sum
            
            Write-Host "`n" -NoNewline
            Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Green
            Write-Host "✅✅✅ 数据文件保存成功！ ✅✅✅" -ForegroundColor Green
            Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Green
            Write-Host ""
            
            Write-Host "📊 统计信息:" -ForegroundColor Cyan
            Write-Host "  • 文件数量: $($files.Count)" -ForegroundColor White
            Write-Host "  • 总大小: $([math]::Round($totalSize/1KB,2)) KB" -ForegroundColor White
            Write-Host "  • 检查次数: $checkCount" -ForegroundColor White
            Write-Host ""
            
            Write-Host "📁 最新文件（前5个）:" -ForegroundColor Cyan
            $files | Sort-Object LastWriteTime -Descending | Select-Object -First 5 | ForEach-Object {
                Write-Host "  ✅ $($_.FullName)" -ForegroundColor Green
                Write-Host "     大小: $([math]::Round($_.Length/1KB,2)) KB | 时间: $($_.LastWriteTime.ToString('HH:mm:ss'))" -ForegroundColor Gray
            }
            
            Write-Host "`n" -NoNewline
            Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Green
            Write-Host "✅ 数据接收和保存功能验证成功！ 🎉" -ForegroundColor Green
            Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Green
            Write-Host ""
            
            break
        } else {
            Write-Host "  ⚠️ 数据目录存在但暂无数据文件" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ⚠️ 数据目录不存在: $dataDir" -ForegroundColor Yellow
    }
    
    # 每10秒检查一次
    Start-Sleep 10
}

if (-not $foundData) {
    Write-Host "`n⚠️ 监控已停止`n" -ForegroundColor Yellow
}

