@echo off
chcp 65001 >nul
title 边坡雷达监测系统

echo.
echo ========================================
echo    边坡雷达监测系统启动程序
echo ========================================
echo.

echo [1/4] 检查.NET环境...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 错误: 未找到.NET运行时，请先安装.NET 8.0
    pause
    exit /b 1
)
echo ✅ .NET环境检查通过

echo.
echo [2/4] 编译项目...
cd /d "%~dp0RadarSystem.WebAPI"
dotnet build --configuration Release --verbosity quiet
if errorlevel 1 (
    echo ❌ 编译失败，请检查代码错误
    pause
    exit /b 1
)
echo ✅ 项目编译成功

echo.
echo [3/4] 启动系统服务...
echo 🚀 正在启动边坡雷达监测系统...
echo 🌐 前端访问地址: http://localhost:6098
echo 📡 API服务地址: http://localhost:8099
echo 📖 Swagger文档: http://localhost:8099/swagger
echo.

start "" "http://localhost:6098"

echo [4/4] 系统启动完成！
echo.
echo ========================================
echo    系统运行中，按 Ctrl+C 停止服务
echo ========================================
echo.

dotnet run --configuration Release --urls "http://localhost:8099"
