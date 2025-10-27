# 测试图像生成配置API
$apiUrl = "http://localhost:8099"

Write-Host "========== 测试图像生成配置API ==========" -ForegroundColor Cyan

# 1. 获取项目列表
Write-Host "`n1. 获取项目列表..." -ForegroundColor Yellow
$projects = Invoke-RestMethod -Uri "$apiUrl/api/Project" -Method Get
Write-Host "项目数量: $($projects.data.Count)" -ForegroundColor Green

if ($projects.data.Count -gt 0) {
    $projectId = $projects.data[0].projectId
    Write-Host "测试项目ID: $projectId" -ForegroundColor Cyan
    
    # 2. 获取图像配置
    Write-Host "`n2. 获取图像配置..." -ForegroundColor Yellow
    try {
        $imageConfig = Invoke-RestMethod -Uri "$apiUrl/api/protocol/project/imageAnalysisConfig/$projectId" -Method Get
        Write-Host "响应code: $($imageConfig.code)" -ForegroundColor Green
        
        if ($imageConfig.code -eq 200) {
            Write-Host "`n图像配置详情:" -ForegroundColor Cyan
            Write-Host "  genImageType: $($imageConfig.data.genImageType)"
            Write-Host "  defoInterval: $($imageConfig.data.defoInterval)"
            Write-Host "  scatInterval: $($imageConfig.data.scatInterval)"
            Write-Host "  defoNumber: $($imageConfig.data.defoNumber)"
            Write-Host "  scatNumber: $($imageConfig.data.scatNumber)"
        } elseif ($imageConfig.code -eq 404) {
            Write-Host "未找到配置（404）" -ForegroundColor Yellow
            
            # 3. 创建默认配置
            Write-Host "`n3. 创建图像配置..." -ForegroundColor Yellow
            $createBody = @{
                projectId = $projectId
                genImageType = "01"
                defoInterval = 60
                scatInterval = 60
                defoNumber = 10
                scatNumber = 10
            } | ConvertTo-Json
            
            $createRes = Invoke-RestMethod -Uri "$apiUrl/api/protocol/update/project/imageAnalysisConfig" `
                -Method Post `
                -ContentType "application/json" `
                -Body $createBody
            
            Write-Host "创建响应code: $($createRes.code)" -ForegroundColor Green
            Write-Host "创建响应message: $($createRes.message)" -ForegroundColor Green
            
            # 4. 再次获取验证
            Write-Host "`n4. 再次获取验证..." -ForegroundColor Yellow
            $imageConfig2 = Invoke-RestMethod -Uri "$apiUrl/api/protocol/project/imageAnalysisConfig/$projectId" -Method Get
            Write-Host "响应code: $($imageConfig2.code)" -ForegroundColor Green
            
            if ($imageConfig2.code -eq 200) {
                Write-Host "配置已创建:" -ForegroundColor Green
                Write-Host "  genImageType: $($imageConfig2.data.genImageType)"
                Write-Host "  defoInterval: $($imageConfig2.data.defoInterval)"
                Write-Host "  scatInterval: $($imageConfig2.data.scatInterval)"
                Write-Host "  defoNumber: $($imageConfig2.data.defoNumber)"
                Write-Host "  scatNumber: $($imageConfig2.data.scatNumber)"
            }
        }
    } catch {
        Write-Host "错误: $_" -ForegroundColor Red
    }
} else {
    Write-Host "没有项目数据！" -ForegroundColor Red
}

Write-Host "`n========== 测试完成 ==========" -ForegroundColor Cyan

