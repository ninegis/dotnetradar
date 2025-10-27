# 检查数据库中的设备和雷达参数数据
$dbPath = "RadarSystem.WebAPI\Data\radar.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "数据库文件不存在: $dbPath" -ForegroundColor Red
    exit
}

# 加载SQLite程序集
Add-Type -Path "C:\Windows\System32\System.Data.SQLite.dll" -ErrorAction SilentlyContinue

# 创建数据库连接
$connectionString = "Data Source=$dbPath;Version=3;"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
$connection.Open()

try {
    Write-Host "========== 设备表 (Devices) ==========" -ForegroundColor Cyan
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT DeviceId, DeviceName, FactoryId, Longitude, Latitude, Elevation, Orientation, LastUpdateTime FROM Devices LIMIT 5;"
    $reader = $command.ExecuteReader()
    
    $deviceCount = 0
    while ($reader.Read()) {
        $deviceCount++
        Write-Host "`n设备 $deviceCount" -ForegroundColor Yellow
        Write-Host "  DeviceId: $($reader["DeviceId"])"
        Write-Host "  DeviceName: $($reader["DeviceName"])"
        Write-Host "  FactoryId: $($reader["FactoryId"])"
        Write-Host "  Longitude: $($reader["Longitude"])"
        Write-Host "  Latitude: $($reader["Latitude"])"
        Write-Host "  Elevation: $($reader["Elevation"])"
        Write-Host "  Orientation: $($reader["Orientation"])"
        Write-Host "  LastUpdateTime: $($reader["LastUpdateTime"])"
    }
    $reader.Close()
    
    if ($deviceCount -eq 0) {
        Write-Host "没有设备数据！" -ForegroundColor Red
    }
    
    Write-Host "`n========== 雷达参数表 (radar_params) ==========" -ForegroundColor Cyan
    $command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='radar_params';"
    $tableExists = $command.ExecuteScalar()
    
    if ($tableExists) {
        Write-Host "radar_params表存在" -ForegroundColor Green
        
        $command.CommandText = "SELECT id, project_id, device_id, img_angle_start, img_angle_end, rng_min, rng_max, freq_band, ante_beam_half FROM radar_params LIMIT 5;"
        $reader = $command.ExecuteReader()
        
        $paramCount = 0
        while ($reader.Read()) {
            $paramCount++
            Write-Host "`n雷达参数 $paramCount" -ForegroundColor Yellow
            Write-Host "  Id: $($reader["id"])"
            Write-Host "  ProjectId: $($reader["project_id"])"
            Write-Host "  DeviceId: $($reader["device_id"])"
            Write-Host "  ImgAngleStart: $($reader["img_angle_start"])"
            Write-Host "  ImgAngleEnd: $($reader["img_angle_end"])"
            Write-Host "  RngMin: $($reader["rng_min"])"
            Write-Host "  RngMax: $($reader["rng_max"])"
            Write-Host "  FreqBand: $($reader["freq_band"])"
            Write-Host "  AnteBeamHalf: $($reader["ante_beam_half"])"
        }
        $reader.Close()
        
        if ($paramCount -eq 0) {
            Write-Host "radar_params表是空的！" -ForegroundColor Yellow
        }
    } else {
        Write-Host "radar_params表不存在！" -ForegroundColor Red
    }
    
} finally {
    $connection.Close()
}

Write-Host "`n检查完成！" -ForegroundColor Green

