@echo off
chcp 65001 >nul
title 边坡雷达监测系统 - 一键启动

setlocal enabledelayedexpansion

echo.
echo ========================================
echo    边坡雷达监测系统
echo    编译 - 部署 - 启动
echo ========================================
echo.

REM ============================================
REM [1/6] 检查 .NET 环境
REM ============================================
echo [1/6] 检查 .NET 环境...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 错误: 未找到 .NET SDK
    echo.
    echo 💡 请安装 .NET 8.0 或更高版本的 SDK
    echo    下载地址: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

for /f "delims=" %%v in ('dotnet --version') do set DOTNET_VERSION=%%v
echo ✅ .NET SDK 版本: %DOTNET_VERSION%

REM ============================================
REM [2/6] 检查 Node.js 环境
REM ============================================
echo.
echo [2/6] 检查 Node.js 环境...
node --version >nul 2>&1
if errorlevel 1 (
    echo ⚠️  未找到 Node.js，将跳过前端构建
    echo.
    echo 💡 如需构建前端，请安装 Node.js 18+
    echo    下载地址: https://nodejs.org
    echo.
    set SKIP_FRONTEND=1
) else (
    for /f "delims=" %%v in ('node --version') do set NODE_VERSION=%%v
    echo ✅ Node.js 版本: !NODE_VERSION!
    set SKIP_FRONTEND=0
)

REM ============================================
REM [3/6] 前端构建
REM ============================================
echo.
echo [3/6] 前端构建...

REM 如果没有 Node.js，尝试使用预构建的文件
if !SKIP_FRONTEND!==1 (
    echo ⚠️  Node.js 未安装，检查预构建文件...
    if exist "RadarSystem.WebAPI\wwwroot\index.html" (
        echo ✅ 检测到现有前端文件，将使用现有构建
        goto SKIP_FRONTEND_BUILD
    ) else (
        echo ❌ 错误: 未找到前端文件且无法构建
        echo.
        echo 💡 请安装 Node.js 以构建前端项目
        pause
        exit /b 1
    )
)

REM 检查前端项目
if not exist "RadarSystem.WebAPI\ClientApp\package.json" (
    echo ❌ 错误: 找不到前端项目
    echo    位置: RadarSystem.WebAPI\ClientApp\package.json
    pause
    exit /b 1
)

REM 检查是否已有构建产物
if exist "RadarSystem.WebAPI\wwwroot\index.html" (
    echo ℹ️  检测到已有前端构建产物
    choice /C YN /M "是否重新构建前端" /T 10 /D N
    if errorlevel 2 (
        echo ⊘ 跳过前端构建
        goto SKIP_FRONTEND_BUILD
    )
    echo    清理旧的构建产物...
    rd /s /q "RadarSystem.WebAPI\wwwroot" 2>nul
)

REM 进入前端项目目录
cd /d "%~dp0RadarSystem.WebAPI\ClientApp"
echo    使用前端项目: RadarSystem.WebAPI\ClientApp

REM 检查并安装依赖
if not exist "node_modules" (
    echo    安装依赖包（首次运行需要较长时间）...
    call npm install
    if errorlevel 1 (
        echo.
        echo ❌ npm install 失败
        echo.
        echo 💡 尝试以下方法:
        echo    1. 清理缓存: npm cache clean --force
        echo    2. 删除 package-lock.json 后重试
        echo    3. 检查网络连接
        cd /d "%~dp0"
        pause
        exit /b 1
    )
) else (
    echo    ✅ 依赖包已安装
)

REM 构建前端
echo    执行构建（输出到 ../wwwroot）...
call npm run build
if errorlevel 1 (
    echo.
    echo ❌ 前端构建失败
    echo.
    echo 💡 常见问题:
    echo    1. 检查所有 .vue 文件是否完整（不为空）
    echo    2. 查看上方构建错误信息
    echo    3. 删除 node_modules 后重新安装
    echo.
    cd /d "%~dp0"
    pause
    exit /b 1
)

cd /d "%~dp0"
echo ✅ 前端构建成功

:SKIP_FRONTEND_BUILD

REM 验证前端文件
if not exist "RadarSystem.WebAPI\wwwroot\index.html" (
    echo ❌ 错误: 前端文件不存在
    echo    位置: RadarSystem.WebAPI\wwwroot\index.html
    pause
    exit /b 1
)
echo ✅ 前端文件就绪

REM ============================================
REM [4/6] 还原后端依赖
REM ============================================
echo.
echo [4/6] 还原后端依赖...
cd /d "%~dp0"
dotnet restore --verbosity quiet
if errorlevel 1 (
    echo ❌ 依赖还原失败
    echo.
    echo 💡 尝试:
    echo    1. 检查网络连接
    echo    2. 运行: dotnet restore --verbosity normal
    pause
    exit /b 1
)
echo ✅ 依赖还原成功

REM ============================================
REM [5/6] 编译后端项目
REM ============================================
echo.
echo [5/6] 编译后端项目...
cd /d "%~dp0RadarSystem.WebAPI"
dotnet build --configuration Release --no-restore --verbosity minimal
if errorlevel 1 (
    echo.
    echo ❌ 后端编译失败
    echo.
    echo 💡 查看详细错误:
    echo    cd RadarSystem.WebAPI
    echo    dotnet build --verbosity normal
    pause
    exit /b 1
)
echo ✅ 后端编译成功

REM ============================================
REM [6/6] 启动系统
REM ============================================
echo.
echo [6/6] 启动系统服务...
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
echo 💡 提示:
echo    - 按 Ctrl+C 停止服务
echo    - 日志: RadarSystem.WebAPI\logs\
echo    - 数据库: RadarSystem.WebAPI\Data\radar.db
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
