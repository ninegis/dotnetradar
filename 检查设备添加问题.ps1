# 检查设备添加问题 - 查询项目和设备数据

$databasePath = "RadarSystem.WebAPI\Data\radar.db"

# 加载SQLite程序集
Add-Type -Path "RadarSystem.WebAPI\bin\Release\net8.0\Microsoft.Data.Sqlite.dll"

$connectionString = "Data Source=$databasePath"
$connection = New-Object Microsoft.Data.Sqlite.SqliteConnection($connectionString)

try {
    $connection.Open()
    
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "  数据库数据检查" -ForegroundColor Green
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host ""
    
    # 1. 查询项目
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT COUNT(*) FROM Projects WHERE IsDeleted = 0"
    $projectCount = $command.ExecuteScalar()
    
    Write-Host "项目总数: $projectCount" -ForegroundColor Yellow
    
    if ($projectCount -gt 0) {
        $command.CommandText = "SELECT ProjectId, ProjectName, Status FROM Projects WHERE IsDeleted = 0"
        $reader = $command.ExecuteReader()
        
        Write-Host "项目列表:" -ForegroundColor Green
        while ($reader.Read()) {
            Write-Host "  - ID: $($reader['ProjectId']), 名称: $($reader['ProjectName']), 状态: $($reader['Status'])" -ForegroundColor White
        }
        $reader.Close()
    } else {
        Write-Host "  ⚠ 数据库中没有项目！这可能是问题所在。" -ForegroundColor Red
    }
    
    Write-Host ""
    
    # 2. 查询设备
    $command.CommandText = "SELECT COUNT(*) FROM Devices WHERE IsDeleted = 0"
    $deviceCount = $command.ExecuteScalar()
    
    Write-Host "设备总数: $deviceCount" -ForegroundColor Yellow
    
    if ($deviceCount -gt 0) {
        $command.CommandText = "SELECT DeviceId, DeviceName, ProjectId, Status FROM Devices WHERE IsDeleted = 0"
        $reader = $command.ExecuteReader()
        
        Write-Host "设备列表:" -ForegroundColor Green
        while ($reader.Read()) {
            Write-Host "  - ID: $($reader['DeviceId']), 名称: $($reader['DeviceName']), 项目: $($reader['ProjectId']), 状态: $($reader['Status'])" -ForegroundColor White
        }
        $reader.Close()
    }
    
    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Cyan
    
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
    Write-Host $_.Exception.StackTrace -ForegroundColor Red
} finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}

Write-Host ""
Write-Host "检查完成！" -ForegroundColor Green

