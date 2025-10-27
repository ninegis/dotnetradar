# 雷达系统启动脚本 (Windows)
param(
    [switch]$SkipBuild,
    [switch]$DevMode
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   边坡雷达监测系统启动脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查 .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK 版本: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ 错误: 未安装 .NET 8 SDK" -ForegroundColor Red
    Write-Host "  请访问 https://dotnet.microsoft.com/download 下载安装" -ForegroundColor Yellow
    exit 1
}

# 检查 Node.js (仅在需要构建前端时)
if (-not $SkipBuild) {
    try {
        $nodeVersion = node --version
        Write-Host "✓ Node.js 版本: $nodeVersion" -ForegroundColor Green
    } catch {
        Write-Host "✗ 错误: 未安装 Node.js" -ForegroundColor Red
        Write-Host "  请访问 https://nodejs.org 下载安装" -ForegroundColor Yellow
        exit 1
    }
}

# 构建前端
if (-not $SkipBuild) {
    Write-Host ""
    Write-Host "► 构建前端项目..." -ForegroundColor Yellow
    
    if (Test-Path "RadarSystem.WebAPI\wwwroot\index.html") {
        Write-Host "  前端已构建，跳过构建步骤" -ForegroundColor Gray
        Write-Host "  如需重新构建，请先删除 wwwroot 目录或运行: .\build-frontend.ps1" -ForegroundColor Gray
    } else {
        & .\build-frontend.ps1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ 前端构建失败" -ForegroundColor Red
            exit 1
        }
    }
} else {
    Write-Host ""
    Write-Host "⊘ 跳过前端构建 (-SkipBuild)" -ForegroundColor Gray
}

# 还原后端依赖
Write-Host ""
Write-Host "► 还原后端依赖..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ 依赖还原失败" -ForegroundColor Red
    exit 1
}

# 构建后端
Write-Host ""
Write-Host "► 构建后端项目..." -ForegroundColor Yellow
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ 后端构建失败" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ 构建完成！" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   启动应用程序" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "访问地址: " -NoNewline
Write-Host "http://localhost:8099" -ForegroundColor Green
Write-Host "API 文档: " -NoNewline
Write-Host "http://localhost:8099/api" -ForegroundColor Green
Write-Host ""
Write-Host "默认账户:" -ForegroundColor Yellow
Write-Host "  用户名: admin" -ForegroundColor Gray
Write-Host "  密码:   admin123" -ForegroundColor Gray
Write-Host ""
Write-Host "按 Ctrl+C 停止应用" -ForegroundColor Gray
Write-Host ""

# 运行应用
Set-Location -Path "RadarSystem.WebAPI"
if ($DevMode) {
    dotnet watch run
} else {
    dotnet run
}

