@echo off
chcp 65001 >nul
title 前端构建和部署

echo.
echo ========================================
echo    前端构建和部署脚本
echo ========================================
echo.

REM ============================================
REM [1/5] 检查Node.js环境
REM ============================================
echo [1/5] 检查Node.js环境...
node --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 错误: 未找到Node.js
    echo.
    echo 💡 请安装 Node.js 18+ 以构建前端
    echo    下载地址: https://nodejs.org
    pause
    exit /b 1
)

for /f "delims=" %%v in ('node --version') do set NODE_VERSION=%%v
echo ✅ Node.js 版本: %NODE_VERSION%

REM ============================================
REM [2/5] 检查前端项目
REM ============================================
echo.
echo [2/5] 检查前端项目...
if not exist "RadarSystem.WebAPI\ClientApp\package.json" (
    echo ❌ 错误: 找不到前端项目
    echo    位置: RadarSystem.WebAPI\ClientApp\package.json
    pause
    exit /b 1
)
echo ✅ 前端项目: RadarSystem.WebAPI\ClientApp

REM ============================================
REM [3/5] 构建前端项目
REM ============================================
echo.
echo [3/5] 构建前端项目...

REM 清理旧的构建产物
if exist "RadarSystem.WebAPI\wwwroot" (
    echo    清理旧的构建产物...
    rd /s /q "RadarSystem.WebAPI\wwwroot" 2>nul
)

REM 进入前端项目目录
cd /d "%~dp0RadarSystem.WebAPI\ClientApp"

REM 检查并安装依赖
if not exist "node_modules" (
    echo    安装依赖包（首次运行需要较长时间）...
    call npm install
    if errorlevel 1 (
        echo ❌ npm install 失败
        echo.
        echo 💡 尝试:
        echo    npm cache clean --force
        echo    然后重新运行脚本
        cd /d "%~dp0"
        pause
        exit /b 1
    )
) else (
    echo    ✅ 依赖包已安装
)

REM 执行构建
echo    执行构建（输出到 ../wwwroot）...
call npm run build
if errorlevel 1 (
    echo.
    echo ❌ 前端构建失败
    echo.
    echo 💡 常见问题:
    echo    1. 检查所有 .vue 文件是否完整
    echo    2. 查看上方构建错误信息
    echo    3. 删除 node_modules 后重新安装
    cd /d "%~dp0"
    pause
    exit /b 1
)

cd /d "%~dp0"
echo ✅ 前端构建成功

REM ============================================
REM [4/5] 验证构建产物
REM ============================================
echo.
echo [4/5] 验证构建产物...
if not exist "RadarSystem.WebAPI\wwwroot\index.html" (
    echo ❌ 错误: 构建产物不完整
    echo    未找到: RadarSystem.WebAPI\wwwroot\index.html
    pause
    exit /b 1
)
echo ✅ 构建产物验证通过

REM ============================================
REM [5/5] 构建后端项目
REM ============================================
echo.
echo [5/5] 构建后端项目...
cd /d "%~dp0RadarSystem.WebAPI"
dotnet build --configuration Release --verbosity minimal
if errorlevel 1 (
    echo.
    echo ❌ 后端构建失败
    echo.
    echo 💡 查看详细错误:
    echo    cd RadarSystem.WebAPI
    echo    dotnet build --verbosity normal
    pause
    exit /b 1
)
echo ✅ 后端构建成功

REM ============================================
REM 完成
REM ============================================
echo.
echo ========================================
echo    ✅ 构建部署完成！
echo ========================================
echo.
echo 📦 前端已部署到: RadarSystem.WebAPI\wwwroot
echo 📝 构建配置: RadarSystem.WebAPI\ClientApp\vite.config.ts
echo.
echo 🚀 启动命令:
echo    cd RadarSystem.WebAPI
echo    dotnet run --configuration Release
echo.
echo 🌐 访问地址:
echo    前端界面: http://localhost:6098
echo    API 服务: http://localhost:8099
echo    API 文档: http://localhost:8099/swagger
echo.
pause
