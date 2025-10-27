# Vue 3 前端项目初始化脚本
# 边坡雷达监测系统 Web 前端

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "边坡雷达监测系统 - Vue 3 前端初始化" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查Node.js
Write-Host "检查 Node.js..." -ForegroundColor Yellow
if (!(Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "错误: 未安装 Node.js" -ForegroundColor Red
    Write-Host "请访问 https://nodejs.org/ 下载并安装 Node.js" -ForegroundColor Red
    exit 1
}

$nodeVersion = node --version
Write-Host "✓ Node.js 版本: $nodeVersion" -ForegroundColor Green

# 检查npm
$npmVersion = npm --version
Write-Host "✓ npm 版本: $npmVersion" -ForegroundColor Green
Write-Host ""

# 创建项目
Write-Host "创建 Vue 3 项目..." -ForegroundColor Yellow
Write-Host ""

# 创建项目命令
Write-Host "执行命令: npm create vite@latest radar-web-frontend -- --template vue-ts" -ForegroundColor Cyan
npm create vite@latest radar-web-frontend -- --template vue-ts

if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 项目创建失败" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ 项目创建成功" -ForegroundColor Green
Write-Host ""

# 进入项目目录
Set-Location radar-web-frontend

# 安装依赖
Write-Host "安装依赖包..." -ForegroundColor Yellow
Write-Host ""

Write-Host "安装核心依赖..." -ForegroundColor Cyan
npm install

Write-Host ""
Write-Host "安装 Vue Router..." -ForegroundColor Cyan
npm install vue-router@4

Write-Host ""
Write-Host "安装 Pinia (状态管理)..." -ForegroundColor Cyan
npm install pinia

Write-Host ""
Write-Host "安装 Axios (HTTP客户端)..." -ForegroundColor Cyan
npm install axios

Write-Host ""
Write-Host "安装 Element Plus (UI组件库)..." -ForegroundColor Cyan
npm install element-plus

Write-Host ""
Write-Host "安装 ECharts (图表库)..." -ForegroundColor Cyan
npm install echarts

Write-Host ""
Write-Host "安装 @element-plus/icons-vue (图标)..." -ForegroundColor Cyan
npm install @element-plus/icons-vue

Write-Host ""
Write-Host "✓ 依赖安装完成" -ForegroundColor Green
Write-Host ""

# 创建目录结构
Write-Host "创建项目目录结构..." -ForegroundColor Yellow

$directories = @(
    "src\api",
    "src\stores",
    "src\views",
    "src\components",
    "src\router",
    "src\styles",
    "src\utils"
)

foreach ($dir in $directories) {
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    Write-Host "✓ 创建目录: $dir" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "初始化完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "下一步操作：" -ForegroundColor Yellow
Write-Host "1. cd radar-web-frontend" -ForegroundColor White
Write-Host "2. npm run dev" -ForegroundColor White
Write-Host ""
Write-Host "访问地址：http://localhost:5173" -ForegroundColor Cyan
Write-Host ""
Write-Host "API地址配置在 src/api/request.ts" -ForegroundColor Yellow
Write-Host "默认API地址: http://localhost:5000/api" -ForegroundColor Yellow
Write-Host ""

