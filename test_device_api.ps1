# 测试设备查询API
$apiUrl = "http://localhost:8099"

# 1. 获取项目列表
Write-Host "1. 获取项目列表..." -ForegroundColor Cyan
$projects = Invoke-RestMethod -Uri "$apiUrl/api/Project" -Method Get
Write-Host "项目数量: $($projects.data.Count)" -ForegroundColor Green

if ($projects.data.Count -gt 0) {
    $firstProject = $projects.data[0]
    Write-Host "第一个项目ID: $($firstProject.projectId)" -ForegroundColor Yellow
    Write-Host "第一个项目名称: $($firstProject.projectName)" -ForegroundColor Yellow
    Write-Host "项目的设备数量: $($firstProject.devices.Count)" -ForegroundColor Yellow
    
    # 2. 通过设备查询API获取该项目的设备
    Write-Host "`n2. 通过设备API查询项目设备..." -ForegroundColor Cyan
    $devices = Invoke-RestMethod -Uri "$apiUrl/api/Device?projectId=$($firstProject.projectId)" -Method Get
    Write-Host "设备API返回code: $($devices.code)" -ForegroundColor Green
    Write-Host "设备API返回数量: $($devices.data.Count)" -ForegroundColor Green
    
    if ($devices.data.Count -gt 0) {
        $firstDevice = $devices.data[0]
        Write-Host "`n设备详细信息:" -ForegroundColor Cyan
        Write-Host "DeviceId: $($firstDevice.deviceId)" -ForegroundColor White
        Write-Host "DeviceName: $($firstDevice.deviceName)" -ForegroundColor White
        Write-Host "DeviceType: $($firstDevice.deviceType)" -ForegroundColor White
        Write-Host "DeviceTypeCode: $($firstDevice.deviceTypeCode)" -ForegroundColor White
        Write-Host "FactoryId: $($firstDevice.factoryId)" -ForegroundColor White
        Write-Host "Longitude: $($firstDevice.longitude)" -ForegroundColor White
        Write-Host "Latitude: $($firstDevice.latitude)" -ForegroundColor White
        Write-Host "Elevation: $($firstDevice.elevation)" -ForegroundColor White
        Write-Host "Orientation: $($firstDevice.orientation)" -ForegroundColor White
        Write-Host "LastUpdateTime: $($firstDevice.lastUpdateTime)" -ForegroundColor White
        
        Write-Host "`nParams:" -ForegroundColor Cyan
        $firstDevice.params | ConvertTo-Json -Depth 3 | Write-Host
        
        Write-Host "`nAlgorithmParam:" -ForegroundColor Cyan
        $firstDevice.algorithmParam | ConvertTo-Json -Depth 3 | Write-Host
    }
} else {
    Write-Host "没有项目数据！" -ForegroundColor Red
}
