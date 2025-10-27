# 查询SQLite数据库中的项目信息

$databasePath = "Data\radar.db"

Add-Type -AssemblyName System.Data

$connectionString = "Data Source=$databasePath"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)

try {
    $connection.Open()
    
    # 查询项目数量
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT COUNT(*) FROM Projects"
    $count = $command.ExecuteScalar()
    
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "  项目数据统计" -ForegroundColor Green
    Write-Host "==============================================" -ForegroundColor Cyan
    Write-Host "项目总数: $count 条" -ForegroundColor Yellow
    Write-Host ""
    
    # 查询所有项目详情
    if ($count -gt 0) {
        $command.CommandText = "SELECT ProjectId, ProjectName, Contact, Phone, Email, CreateTime FROM Projects"
        $reader = $command.ExecuteReader()
        
        Write-Host "项目列表:" -ForegroundColor Green
        Write-Host "----------------------------------------------" -ForegroundColor Cyan
        
        $index = 1
        while ($reader.Read()) {
            Write-Host "$index. 项目ID: $($reader['ProjectId'])" -ForegroundColor White
            Write-Host "   项目名称: $($reader['ProjectName'])" -ForegroundColor White
            Write-Host "   联系人: $($reader['Contact'])" -ForegroundColor White
            Write-Host "   电话: $($reader['Phone'])" -ForegroundColor White
            Write-Host "   邮箱: $($reader['Email'])" -ForegroundColor White
            Write-Host "   创建时间: $($reader['CreateTime'])" -ForegroundColor White
            Write-Host "----------------------------------------------" -ForegroundColor Cyan
            $index++
        }
        $reader.Close()
    } else {
        Write-Host "数据库中暂无项目数据" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
} finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}

Write-Host ""
Write-Host "查询完成！" -ForegroundColor Green

