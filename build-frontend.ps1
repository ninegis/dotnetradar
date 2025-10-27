# 前端构建脚本
Write-Host "开始构建前端项目..." -ForegroundColor Green

# 检查 Node.js 是否安装
try {
    $nodeVersion = node --version
    Write-Host "Node.js 版本: $nodeVersion" -ForegroundColor Cyan
} catch {
    Write-Host "错误: 未安装 Node.js，请先安装 Node.js" -ForegroundColor Red
    exit 1
}

# 进入前端项目目录
Set-Location -Path "RadarSystem.WebAPI\ClientApp"

# 检查 node_modules 是否存在
if (-not (Test-Path "node_modules")) {
    Write-Host "安装依赖包..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "依赖安装失败" -ForegroundColor Red
        Set-Location -Path "..\..\"
        exit 1
    }
}

# 构建项目
Write-Host "构建 Vue 项目..." -ForegroundColor Yellow
npm run build

if ($LASTEXITCODE -eq 0) {
    Write-Host "前端构建成功！" -ForegroundColor Green
    Write-Host "输出目录: RadarSystem.WebAPI\wwwroot" -ForegroundColor Cyan
} else {
    Write-Host "前端构建失败" -ForegroundColor Red
    Set-Location -Path "..\..\"
    exit 1
}

# 返回根目录
Set-Location -Path "..\..\"

Write-Host "完成！" -ForegroundColor Green

