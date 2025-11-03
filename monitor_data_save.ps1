# 数据保存监控脚本
Write-Host "`n【数据保存监控脚本】`n" -ForegroundColor Cyan
Write-Host "每30秒检查一次数据保存情况...`n" -ForegroundColor Green

$dataDir = "RadarSystem.WebAPI\Data\data\project"
$checkCount = 0
$maxChecks = 20  # 最多检查20次（10分钟）

while ($checkCount -lt $maxChecks) {
    $checkCount++
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    Write-Host "[$timestamp] 第 $checkCount 次检查..." -ForegroundColor Gray
    
    if (Test-Path $dataDir) {
        $files = Get-ChildItem $dataDir -Recurse -File -ErrorAction SilentlyContinue
        
        if ($files) {
            $totalSize = ($files | Measure-Object -Property Length -Sum).Sum
            Write-Host "`n✅✅✅ 发现数据文件！ ✅✅✅`n" -ForegroundColor Green
            Write-Host "文件数量: $($files.Count)" -ForegroundColor Cyan
            Write-Host "总大小: $([math]::Round($totalSize/1KB,2)) KB" -ForegroundColor Cyan
            Write-Host "`n最新文件:" -ForegroundColor Yellow
            $files | Sort-Object LastWriteTime -Descending | Select-Object -First 5 | Format-Table FullName, @{Name='Size(KB)';Expression={[math]::Round($_.Length/1KB,2)}}, LastWriteTime -AutoSize
            Write-Host "`n✅✅✅ 数据保存成功！测试完成！ ✅✅✅`n" -ForegroundColor Green
            break
        } else {
            Write-Host "  ⚠️ 目录存在但暂无文件" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ⚠️ 数据目录不存在" -ForegroundColor Yellow
    }
    
    if ($checkCount -lt $maxChecks) {
        Start-Sleep 30
    }
}

if ($checkCount -eq $maxChecks) {
    Write-Host "`n⚠️ 监控超时，未发现数据文件`n" -ForegroundColor Yellow
    Write-Host "请检查:" -ForegroundColor Yellow
    Write-Host "1. 设备是否连接到1030端口"
    Write-Host "2. 控制台日志中的 [DATA] 标记"
    Write-Host "3. 控制台日志中的 [HandleImageData] 标记"
    Write-Host "4. 控制台日志中的 [SAVE] 标记`n"
}

