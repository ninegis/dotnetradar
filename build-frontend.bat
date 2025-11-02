@echo off
chcp 65001 >nul
title Build Frontend - RadarContrl

echo.
echo ========================================
echo    前端构建和部署 (RadarContrl)
echo ========================================
echo.

REM [1/4] 检查Node.js
echo [1/4] 检查Node.js环境...
node --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 错误: 未找到Node.js
    echo.
    echo 下载: https://nodejs.org
    pause
    exit /b 1
)
for /f "delims=" %%v in ('node --version') do set NODE_VERSION=%%v
echo ✅ Node.js: %NODE_VERSION%

REM [2/4] 构建前端
echo.
echo [2/4] 构建前端项目 (RadarContrl)...
cd /d "%~dp0RadarContrl"
call npm run build
if errorlevel 1 (
    echo ❌ 前端构建失败
    cd /d "%~dp0"
    pause
    exit /b 1
)
echo ✅ 前端构建成功

REM [3/4] 部署到wwwroot
echo.
echo [3/4] 部署到wwwroot...
cd /d "%~dp0"
if exist "RadarSystem.WebAPI\wwwroot" (
    rd /s /q "RadarSystem.WebAPI\wwwroot"
)
xcopy /E /I /Y /Q "RadarContrl\dist\*" "RadarSystem.WebAPI\wwwroot\"
if errorlevel 1 (
    echo ❌ 部署失败
    pause
    exit /b 1
)
echo ✅ 部署完成

REM [4/4] 构建后端
echo.
echo [4/4] 构建后端项目...
cd /d "%~dp0RadarSystem.WebAPI"
dotnet build --configuration Release --verbosity quiet
if errorlevel 1 (
    echo ❌ 后端构建失败
    pause
    exit /b 1
)
echo ✅ 后端构建成功

echo.
echo ========================================
echo    ✅ 完成！
echo ========================================
echo.
echo 前端: RadarSystem.WebAPI\wwwroot
echo 启动: cd RadarSystem.WebAPI ^&^& dotnet run
echo 访问: http://localhost:6098
echo.
pause

