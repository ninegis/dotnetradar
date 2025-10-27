# 测试设备更新API
$baseUrl = "http://localhost:8099"

# 1. 获取项目列表
Write-Host "=== 1. 获取项目列表 ===" -ForegroundColor Green
$projects = Invoke-RestMethod -Uri "$baseUrl/api/Project" -Method Get
Write-Host "项目数量: $($projects.data.Count)"
if ($projects.data.Count -gt 0) {
    $project = $projects.data[0]
    Write-Host "第一个项目: $($project.projectId) - $($project.projectName)"
    Write-Host "设备数量: $($project.devices.Count)"
    
    if ($project.devices.Count -gt 0) {
        $device = $project.devices[0]
        Write-Host "`n=== 设备信息 ===" -ForegroundColor Cyan
        Write-Host "设备ID (Id): $($device.id)"
        Write-Host "设备ID (DeviceId): $($device.deviceId)"
        Write-Host "设备名称 (DeviceName): $($device.deviceName)"
        Write-Host "出厂ID (FactoryId): $($device.factoryId)"
        Write-Host "经度 (Longitude): $($device.longitude)"
        Write-Host "纬度 (Latitude): $($device.latitude)"
        Write-Host "高程 (Elevation): $($device.elevation)"
        Write-Host "零点朝向 (Orientation): $($device.orientation)"
        
        # 2. 测试更新设备
        Write-Host "`n=== 2. 测试更新设备 ===" -ForegroundColor Green
        $updateData = @{
            projectId = $project.projectId
            deviceId = $device.deviceId
            name = "测试设备名称_$(Get-Date -Format 'HHmmss')"
            factoryId = "TEST_FACTORY_001"
            longitude = 120.5
            latitude = 31.3
            height = 100.0
            orientation = 45.0
            radarOri = 45.0
        } | ConvertTo-Json
        
        Write-Host "更新数据: $updateData"
        
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        try {
            $result = Invoke-RestMethod -Uri "$baseUrl/api/protocol/update/radar/param" -Method Post -Body $updateData -Headers $headers
            Write-Host "更新结果: $($result | ConvertTo-Json -Depth 3)" -ForegroundColor Yellow
            
            # 3. 再次获取项目列表验证
            Write-Host "`n=== 3. 验证更新结果 ===" -ForegroundColor Green
            Start-Sleep -Seconds 1
            $projectsAfter = Invoke-RestMethod -Uri "$baseUrl/api/Project" -Method Get
            $projectAfter = $projectsAfter.data | Where-Object { $_.projectId -eq $project.projectId }
            $deviceAfter = $projectAfter.devices | Where-Object { $_.deviceId -eq $device.deviceId }
            
            Write-Host "更新后的设备信息:" -ForegroundColor Cyan
            Write-Host "设备名称: $($deviceAfter.deviceName)"
            Write-Host "出厂ID: $($deviceAfter.factoryId)"
            Write-Host "经度: $($deviceAfter.longitude)"
            Write-Host "纬度: $($deviceAfter.latitude)"
            Write-Host "高程: $($deviceAfter.elevation)"
            Write-Host "零点朝向: $($deviceAfter.orientation)"
            Write-Host "最后更新时间: $($deviceAfter.lastUpdateTime)"
        }
        catch {
            Write-Host "更新失败: $_" -ForegroundColor Red
            Write-Host $_.Exception.Message
        }
    }
    else {
        Write-Host "项目中没有设备" -ForegroundColor Yellow
    }
}
else {
    Write-Host "没有找到项目" -ForegroundColor Yellow
}

