# ===================================================================
# 边坡雷达监测系统 - 后台接口全量测试脚本
# ===================================================================

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:8099/api"
$testResults = @()

Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host "边坡雷达监测系统 - 后台接口全量测试" -ForegroundColor Cyan
Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host ""

# 等待服务启动
Write-Host "[1/4] 等待服务启动..." -ForegroundColor Yellow
$maxRetries = 30
$retryCount = 0
$serviceReady = $false

while ($retryCount -lt $maxRetries) {
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl/health" -Method GET -TimeoutSec 2 -ErrorAction Stop
        $serviceReady = $true
        Write-Host "✅ 服务已就绪！" -ForegroundColor Green
        break
    } catch {
        $retryCount++
        Write-Host "等待服务启动... ($retryCount/$maxRetries)" -ForegroundColor Gray
        Start-Sleep -Seconds 2
    }
}

if (-not $serviceReady) {
    Write-Host "❌ 服务未能在预期时间内启动，尝试继续测试..." -ForegroundColor Red
}

Write-Host ""
Write-Host "[2/4] 开始接口测试..." -ForegroundColor Yellow
Write-Host ""

# 测试函数
function Test-API {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [hashtable]$Headers = @{"Content-Type" = "application/json"}
    )
    
    try {
        $uri = "$baseUrl$Endpoint"
        Write-Host "测试: $Name" -ForegroundColor Cyan
        Write-Host "  $Method $uri" -ForegroundColor Gray
        
        $params = @{
            Uri = $uri
            Method = $Method
            Headers = $Headers
            TimeoutSec = 10
        }
        
        if ($Body -ne $null) {
            $jsonBody = $Body | ConvertTo-Json -Depth 10
            Write-Host "  请求体: $jsonBody" -ForegroundColor Gray
            $params.Body = $jsonBody
        }
        
        $response = Invoke-WebRequest @params -ErrorAction Stop
        $statusCode = $response.StatusCode
        $content = $response.Content
        
        Write-Host "  ✅ 状态码: $statusCode" -ForegroundColor Green
        
        # 尝试解析JSON响应
        try {
            $jsonResponse = $content | ConvertFrom-Json
            Write-Host "  响应: $($jsonResponse | ConvertTo-Json -Depth 2 -Compress)" -ForegroundColor Gray
        } catch {
            Write-Host "  响应: $($content.Substring(0, [Math]::Min(200, $content.Length)))" -ForegroundColor Gray
        }
        
        $script:testResults += [PSCustomObject]@{
            Name = $Name
            Endpoint = $Endpoint
            Method = $Method
            Status = "通过"
            StatusCode = $statusCode
            Error = ""
        }
        
        Write-Host ""
        return $true
    } catch {
        $statusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.value__ } else { "N/A" }
        $errorMsg = $_.Exception.Message
        
        Write-Host "  ❌ 失败: $errorMsg (状态码: $statusCode)" -ForegroundColor Red
        
        $script:testResults += [PSCustomObject]@{
            Name = $Name
            Endpoint = $Endpoint
            Method = $Method
            Status = "失败"
            StatusCode = $statusCode
            Error = $errorMsg
        }
        
        Write-Host ""
        return $false
    }
}

# ===================================================================
# 1. 项目管理接口 (Protocol Controller)
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "1. 项目管理接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 1.1 添加项目
Test-API -Name "添加项目" -Method POST -Endpoint "/protocol/add/project" -Body @{
    projectId = ""  # 让后端自动生成
    projectName = "测试项目_$(Get-Date -Format 'yyyyMMddHHmmss')"
    projectDescribe = "这是一个自动化测试项目"
    contact = "张三"
    phone = "13800138000"
    email = "test@example.com"
    lon = 104.06
    lat = 30.67
    alt = 500
}

# 1.2 查询项目列表
Test-API -Name "查询项目列表" -Method POST -Endpoint "/protocol/queryAll/project" -Body @{
    pageNum = 1
    pageSize = 10
}

# 1.3 删除项目
Test-API -Name "删除项目（模拟）" -Method POST -Endpoint "/protocol/remove/project" -Body @{
    projectId = "TEST_PROJECT_ID"
}

# 1.4 设置项目视图
Test-API -Name "设置项目视图" -Method POST -Endpoint "/protocol/set/projectView" -Body @{
    projectId = "TEST_PROJECT_ID"
    viewConfig = "{\"center\":[104.06,30.67],\"zoom\":15}"
}

# 1.5 更新图像分析配置
Test-API -Name "更新图像分析配置" -Method POST -Endpoint "/protocol/update/imageAnalysisConfig" -Body @{
    projectId = "TEST_PROJECT_ID"
    analysisType = "deformation"
    threshold = 0.5
}

# ===================================================================
# 2. 设备管理接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "2. 设备管理接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 2.1 添加设备
Test-API -Name "添加设备" -Method POST -Endpoint "/protocol/add/device" -Body @{
    deviceId = "RADAR_TEST_$(Get-Date -Format 'HHmmss')"
    deviceName = "测试雷达设备"
    deviceType = "MimoLite"
    projectId = "TEST_PROJECT_ID"
    ipAddress = "192.168.1.100"
    port = 8888
    longitude = 104.06
    latitude = 30.67
}

# 2.2 查询设备列表
Test-API -Name "查询设备列表" -Method POST -Endpoint "/protocol/queryAll/device" -Body @{
    projectId = "TEST_PROJECT_ID"
    pageNum = 1
    pageSize = 10
}

# 2.3 删除设备
Test-API -Name "删除设备（模拟）" -Method POST -Endpoint "/protocol/remove/device" -Body @{
    deviceId = "RADAR_TEST_001"
}

# ===================================================================
# 3. 告警管理接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "3. 告警管理接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 3.1 查询告警规则
Test-API -Name "查询告警规则" -Method POST -Endpoint "/protocol/query/alarmRules" -Body @{
    projectId = "TEST_PROJECT_ID"
}

# 3.2 添加告警规则
Test-API -Name "添加告警规则" -Method POST -Endpoint "/protocol/add/alarmRule" -Body @{
    projectId = "TEST_PROJECT_ID"
    ruleName = "位移告警规则"
    alarmType = "displacement"
    threshold = 10.0
    level = "warning"
}

# 3.3 更新告警规则
Test-API -Name "更新告警规则" -Method POST -Endpoint "/protocol/update/alarmRule" -Body @{
    ruleId = "RULE_TEST_001"
    ruleName = "更新后的告警规则"
    threshold = 15.0
}

# 3.4 删除告警规则
Test-API -Name "删除告警规则（模拟）" -Method POST -Endpoint "/protocol/delete/alarmRule" -Body @{
    ruleId = "RULE_TEST_001"
}

# 3.5 查询告警记录
Test-API -Name "查询告警记录" -Method POST -Endpoint "/alarm/query" -Body @{
    projectId = "TEST_PROJECT_ID"
    startTime = "2025-01-01 00:00:00"
    endTime = "2025-12-31 23:59:59"
    pageNum = 1
    pageSize = 10
}

# ===================================================================
# 4. 联系人管理接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "4. 联系人管理接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 4.1 添加联系人
Test-API -Name "添加告警联系人" -Method POST -Endpoint "/alarm/contact/add" -Body @{
    name = "测试联系人"
    phone = "13900139000"
    email = "contact@test.com"
    projectId = "TEST_PROJECT_ID"
}

# 4.2 查询联系人
Test-API -Name "查询告警联系人" -Method GET -Endpoint "/alarm/contact/query?projectId=TEST_PROJECT_ID"

# ===================================================================
# 5. 雷达图像接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "5. 雷达图像接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 5.1 查询图像列表
Test-API -Name "查询雷达图像列表" -Method POST -Endpoint "/image/query" -Body @{
    projectId = "TEST_PROJECT_ID"
    deviceId = "RADAR_001"
    startTime = "2025-01-01 00:00:00"
    endTime = "2025-12-31 23:59:59"
    pageNum = 1
    pageSize = 10
}

# 5.2 生成雷达图像
Test-API -Name "生成雷达图像" -Method POST -Endpoint "/image/generate" -Body @{
    projectId = "TEST_PROJECT_ID"
    deviceId = "RADAR_001"
    startTime = "2025-10-24 00:00:00"
    endTime = "2025-10-24 23:59:59"
    imageType = "deformation"
}

# ===================================================================
# 6. 数据管理接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "6. 数据管理接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 6.1 生成测试数据
Test-API -Name "生成测试数据" -Method POST -Endpoint "/data/generate" -Body @{
    projectId = "TEST_PROJECT_ID"
    deviceId = "RADAR_001"
    dataType = "radar"
    count = 100
    geoMarkId = "MARK_001"
}

# 6.2 恢复数据
Test-API -Name "恢复数据" -Method POST -Endpoint "/data/restore" -Body @{
    projectId = "TEST_PROJECT_ID"
    backupPath = "/backup/test.bak"
}

# ===================================================================
# 7. 图层管理接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "7. 图层管理接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 7.1 添加图层
Test-API -Name "添加图层" -Method POST -Endpoint "/layer/add" -Body @{
    layerName = "测试图层"
    layerType = "polygon"
    projectId = "TEST_PROJECT_ID"
    geometry = "{\"type\":\"Polygon\",\"coordinates\":[[[104.06,30.67],[104.07,30.67],[104.07,30.68],[104.06,30.68],[104.06,30.67]]]}"
}

# 7.2 查询图层
Test-API -Name "查询图层列表" -Method GET -Endpoint "/layer/query?projectId=TEST_PROJECT_ID"

# ===================================================================
# 8. 地理标注接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "8. 地理标注接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 8.1 添加地理标注
Test-API -Name "添加地理标注" -Method POST -Endpoint "/geomark/add" -Body @{
    markName = "测试标注点"
    markType = "point"
    projectId = "TEST_PROJECT_ID"
    longitude = 104.06
    latitude = 30.67
    description = "这是一个测试标注点"
}

# 8.2 查询地理标注
Test-API -Name "查询地理标注" -Method GET -Endpoint "/geomark/query?projectId=TEST_PROJECT_ID"

# ===================================================================
# 9. 雷达参数接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "9. 雷达参数接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 9.1 查询雷达参数
Test-API -Name "查询雷达参数" -Method POST -Endpoint "/protocol/query/radarParams" -Body @{
    deviceId = "RADAR_001"
}

# 9.2 更新算法参数
Test-API -Name "更新雷达算法参数" -Method POST -Endpoint "/protocol/update/algoParam" -Body @{
    deviceId = "RADAR_001"
    paramName = "threshold"
    paramValue = "0.8"
}

# 9.3 更新MimoLite算法参数
Test-API -Name "更新MimoLite算法参数" -Method POST -Endpoint "/protocol/update/mimoLiteAlgoParam" -Body @{
    deviceId = "RADAR_001"
    sensitivity = 0.9
    filterLevel = 2
}

# ===================================================================
# 10. 服务器管理接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "10. 服务器管理接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 10.1 添加授权人员
Test-API -Name "添加授权人员" -Method POST -Endpoint "/server/addAllowPeople" -Body @{
    name = "测试用户"
    phone = "13700137000"
    project_code = "TEST_PROJECT"
}

# 10.2 查询授权人员
Test-API -Name "查询授权人员" -Method POST -Endpoint "/server/getAllowPeople" -Body @{
    project_code = "TEST_PROJECT"
}

# 10.3 查询用户地址
Test-API -Name "查询用户地址" -Method POST -Endpoint "/server/getuseraddress" -Body @{
    phone = "13700137000"
}

# 10.4 添加雷达操作日志
Test-API -Name "添加雷达操作日志" -Method POST -Endpoint "/server/addradaroperatelog" -Body @{
    operateType = "login"
    userName = "测试用户"
    userAddress = "127.0.0.1"
    projectCode = "TEST_PROJECT"
    deviceCode = "RADAR_001"
}

# ===================================================================
# 11. 配置管理接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "11. 配置管理接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 11.1 查询系统配置
Test-API -Name "查询系统配置信息" -Method GET -Endpoint "/config/info"

# ===================================================================
# 12. 存储管理接口
# ===================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "12. 存储管理接口测试" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

# 12.1 查询磁盘空间
Test-API -Name "查询磁盘空间" -Method POST -Endpoint "/datastorage/query/discSpace" -Body @{}

# ===================================================================
# 测试结果汇总
# ===================================================================
Write-Host ""
Write-Host "[3/4] 测试完成，生成报告..." -ForegroundColor Yellow
Write-Host ""

$totalTests = $testResults.Count
$passedTests = ($testResults | Where-Object { $_.Status -eq "通过" }).Count
$failedTests = ($testResults | Where-Object { $_.Status -eq "失败" }).Count
$passRate = if ($totalTests -gt 0) { [Math]::Round(($passedTests / $totalTests) * 100, 2) } else { 0 }

Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host "测试结果汇总" -ForegroundColor Cyan
Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host "总测试数: $totalTests" -ForegroundColor White
Write-Host "通过数量: $passedTests" -ForegroundColor Green
Write-Host "失败数量: $failedTests" -ForegroundColor Red
Write-Host "通过率: $passRate%" -ForegroundColor $(if ($passRate -ge 80) { "Green" } elseif ($passRate -ge 60) { "Yellow" } else { "Red" })
Write-Host ""

# 输出详细结果表格
Write-Host "详细测试结果:" -ForegroundColor Yellow
$testResults | Format-Table -Property Name, Method, Status, StatusCode, Error -AutoSize

# 保存测试报告
$reportPath = "接口测试报告_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"
$testResults | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportPath -Encoding UTF8
Write-Host ""
Write-Host "[4/4] 测试报告已保存: $reportPath" -ForegroundColor Green

# 生成Markdown报告
$mdReportPath = "接口测试报告_$(Get-Date -Format 'yyyyMMdd_HHmmss').md"
$mdContent = @"
# 边坡雷达监测系统 - 后台接口全量测试报告

**测试时间**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

## 测试概况

- **总测试数**: $totalTests
- **通过数量**: $passedTests ✅
- **失败数量**: $failedTests ❌
- **通过率**: $passRate%

## 详细测试结果

| 接口名称 | 请求方法 | 端点 | 状态 | 状态码 | 错误信息 |
|---------|---------|------|------|--------|---------|
"@

foreach ($result in $testResults) {
    $statusIcon = if ($result.Status -eq "通过") { "✅" } else { "❌" }
    $mdContent += "`n| $($result.Name) | $($result.Method) | $($result.Endpoint) | $statusIcon $($result.Status) | $($result.StatusCode) | $($result.Error) |"
}

$mdContent += @"


## 测试分类统计

### 1. 项目管理接口
- 添加项目: $(if (($testResults | Where-Object { $_.Name -eq "添加项目" }).Status -eq "通过") { "✅" } else { "❌" })
- 查询项目列表: $(if (($testResults | Where-Object { $_.Name -eq "查询项目列表" }).Status -eq "通过") { "✅" } else { "❌" })

### 2. 设备管理接口
- 添加设备: $(if (($testResults | Where-Object { $_.Name -eq "添加设备" }).Status -eq "通过") { "✅" } else { "❌" })
- 查询设备列表: $(if (($testResults | Where-Object { $_.Name -eq "查询设备列表" }).Status -eq "通过") { "✅" } else { "❌" })

### 3. 告警管理接口
- 查询告警规则: $(if (($testResults | Where-Object { $_.Name -eq "查询告警规则" }).Status -eq "通过") { "✅" } else { "❌" })
- 添加告警规则: $(if (($testResults | Where-Object { $_.Name -eq "添加告警规则" }).Status -eq "通过") { "✅" } else { "❌" })

### 4. 雷达图像接口
- 查询图像列表: $(if (($testResults | Where-Object { $_.Name -eq "查询雷达图像列表" }).Status -eq "通过") { "✅" } else { "❌" })
- 生成图像: $(if (($testResults | Where-Object { $_.Name -eq "生成雷达图像" }).Status -eq "通过") { "✅" } else { "❌" })

### 5. 数据管理接口
- 生成测试数据: $(if (($testResults | Where-Object { $_.Name -eq "生成测试数据" }).Status -eq "通过") { "✅" } else { "❌" })
- 恢复数据: $(if (($testResults | Where-Object { $_.Name -eq "恢复数据" }).Status -eq "通过") { "✅" } else { "❌" })

## 结论

$(if ($passRate -ge 80) {
    "✅ **测试通过率达到 $passRate%，系统接口基本正常！**"
} elseif ($passRate -ge 60) {
    "⚠️ **测试通过率为 $passRate%，部分接口存在问题，需要优化。**"
} else {
    "❌ **测试通过率仅为 $passRate%，系统接口存在较多问题，需要重点排查和修复。**"
})

## 备注

- 本次测试为自动化全量接口测试
- 测试数据均为模拟数据
- 失败的接口可能是因为缺少依赖数据或权限限制
- 建议重点关注核心业务接口的通过情况

---
*报告生成时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')*
"@

$mdContent | Out-File -FilePath $mdReportPath -Encoding UTF8
Write-Host "Markdown测试报告已保存: $mdReportPath" -ForegroundColor Green
Write-Host ""
Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host "测试完成！" -ForegroundColor Cyan
Write-Host "====================================================================" -ForegroundColor Cyan

