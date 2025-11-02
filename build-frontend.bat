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

REM [4/5] 构建后端
echo.
echo [4/5] 构建后端项目...
cd /d "%~dp0RadarSystem.WebAPI"
dotnet build --configuration Release --verbosity quiet
if errorlevel 1 (
    echo ❌ 后端构建失败
    pause
    exit /b 1
)
echo ✅ 后端构建成功

REM ============================================
REM [5/5] 启动系统服务
REM ============================================
echo.
echo [5/5] 启动系统服务...
cd /d "%~dp0RadarSystem.WebAPI"

REM 检查端口是否被占用
netstat -ano | findstr ":6098" >nul 2>&1
if not errorlevel 1 (
    echo ⚠️  端口 6098 已被占用，尝试停止现有进程...
    for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":6098" ^| findstr "LISTENING"') do (
        taskkill /F /PID %%a >nul 2>&1
    )
    timeout /t 2 /nobreak >nul
)

netstat -ano | findstr ":8099" >nul 2>&1
if not errorlevel 1 (
    echo ⚠️  端口 8099 已被占用，尝试停止现有进程...
    for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":8099" ^| findstr "LISTENING"') do (
        taskkill /F /PID %%a >nul 2>&1
    )
    timeout /t 2 /nobreak >nul
)

echo.
echo ========================================
echo    🚀 系统启动中...
echo ========================================
echo.
echo 📍 访问地址:
echo    🌐 前端界面: http://localhost:6098
echo    📡 API 服务: http://localhost:8099
echo    📖 API 文档: http://localhost:8099/swagger
echo.
echo 👤 默认账户:
echo    用户名: admin
echo    密码:   admin123
echo.
echo 💡 提示: 按 Ctrl+C 停止服务
echo.
echo ========================================
echo    系统运行中...
echo ========================================
echo.

REM 等待 2 秒后打开浏览器
timeout /t 2 /nobreak >nul
start "" "http://localhost:6098"

REM 启动应用
dotnet run --configuration Release --no-build --urls "http://localhost:8099"

REM 如果程序退出，显示消息
echo.
echo ========================================
echo    ⏹️  系统已停止
echo ========================================
echo.
pause

