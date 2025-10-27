# 查看SQLite数据库表结构
$dbPath = "Data/radar_system.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "数据库文件不存在: $dbPath" -ForegroundColor Red
    exit
}

Add-Type -AssemblyName System.Data

$connectionString = "Data Source=$dbPath;Version=3;"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)

try {
    $connection.Open()
    
    # 获取所有表
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;"
    $reader = $command.ExecuteReader()
    
    $tables = @()
    while ($reader.Read()) {
        $tables += $reader["name"]
    }
    $reader.Close()
    
    Write-Host "=" * 80 -ForegroundColor Cyan
    Write-Host "SQLite数据库表结构" -ForegroundColor Green
    Write-Host "数据库: $dbPath" -ForegroundColor Yellow
    Write-Host "总表数: $($tables.Count)" -ForegroundColor Yellow
    Write-Host "=" * 80 -ForegroundColor Cyan
    Write-Host ""
    
    # 对每个表显示结构
    foreach ($table in $tables) {
        Write-Host "表名: $table" -ForegroundColor Green
        Write-Host ("-" * 80) -ForegroundColor Gray
        
        $command = $connection.CreateCommand()
        $command.CommandText = "PRAGMA table_info('$table');"
        $reader = $command.ExecuteReader()
        
        Write-Host "字段列表:" -ForegroundColor Yellow
        Write-Host ""
        Write-Host ("{0,-5} {1,-30} {2,-15} {3,-10} {4,-10} {5}" -f "序号", "字段名", "类型", "非空", "默认值", "主键") -ForegroundColor Cyan
        Write-Host ("{0,-5} {1,-30} {2,-15} {3,-10} {4,-10} {5}" -f "----", "------------------------------", "---------------", "----", "----------", "----") -ForegroundColor Gray
        
        while ($reader.Read()) {
            $cid = $reader["cid"]
            $name = $reader["name"]
            $type = $reader["type"]
            $notnull = if ($reader["notnull"] -eq 1) { "NOT NULL" } else { "" }
            $dflt = $reader["dflt_value"]
            $pk = if ($reader["pk"] -eq 1) { "PK" } else { "" }
            
            Write-Host ("{0,-5} {1,-30} {2,-15} {3,-10} {4,-10} {5}" -f $cid, $name, $type, $notnull, $dflt, $pk)
        }
        $reader.Close()
        
        # 显示索引
        $command = $connection.CreateCommand()
        $command.CommandText = "PRAGMA index_list('$table');"
        $reader = $command.ExecuteReader()
        
        $hasIndex = $false
        $indexes = @()
        while ($reader.Read()) {
            $hasIndex = $true
            $indexes += @{
                name = $reader["name"]
                unique = $reader["unique"]
            }
        }
        $reader.Close()
        
        if ($hasIndex) {
            Write-Host ""
            Write-Host "索引列表:" -ForegroundColor Yellow
            foreach ($idx in $indexes) {
                $uniqueText = if ($idx.unique -eq 1) { "[UNIQUE]" } else { "" }
                Write-Host "  - $($idx.name) $uniqueText" -ForegroundColor White
            }
        }
        
        Write-Host ""
        Write-Host ""
    }
    
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
} finally {
    $connection.Close()
}

