@echo off
chcp 65001 >nul
title 前端构建和部署

echo.
echo ========================================
echo    前端构建和部署脚本
echo ========================================
echo.

echo [1/4] 检查Node.js环境...
node --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 错误: 未找到Node.js，请先安装Node.js
    pause
    exit /b 1
)
echo ✅ Node.js环境检查通过

echo.
echo [2/4] 构建前端项目...
cd /d "%~dp0RadarContrl"
call npm run build
if errorlevel 1 (
    echo ❌ 前端构建失败，请检查错误信息
    pause
    exit /b 1
)
echo ✅ 前端构建成功

echo.
echo [3/4] 部署到后端wwwroot目录...
cd /d "%~dp0"
if exist "RadarSystem.WebAPI\wwwroot" (
    rd /s /q "RadarSystem.WebAPI\wwwroot"
)
xcopy /E /I /Y "RadarContrl\dist\*" "RadarSystem.WebAPI\wwwroot\"
echo ✅ 部署完成

echo.
echo [4/4] 构建后端项目...
cd /d "%~dp0RadarSystem.WebAPI"
dotnet build --configuration Release --verbosity quiet
if errorlevel 1 (
    echo ❌ 后端构建失败，请检查错误信息
    pause
    exit /b 1
)
echo ✅ 后端构建成功

echo.
echo ========================================
echo    构建部署完成！
echo ========================================
echo.
echo 📦 前端已部署到: RadarSystem.WebAPI\wwwroot
echo 🚀 启动命令: cd RadarSystem.WebAPI && dotnet run --configuration Release
echo 🌐 前端地址: http://localhost:6098
echo 📡 API地址: http://localhost:8099
echo 📖 API文档: http://localhost:8099/swagger
echo.
pause

