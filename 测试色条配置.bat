@echo off
chcp 65001 >nul
echo.
echo ========================================
echo   色条配置页面测试脚本
echo ========================================
echo.

echo [步骤1] 进入前端目录...
cd RadarContrl
if errorlevel 1 (
    echo ❌ 错误：无法进入RadarContrl目录
    pause
    exit /b 1
)

echo.
echo [步骤2] 检查node_modules...
if not exist "node_modules" (
    echo ⚠ 警告：node_modules不存在，正在安装依赖...
    call npm install
)

echo.
echo [步骤3] 构建前端...
echo 这可能需要几分钟时间...
call npm run build

if errorlevel 1 (
    echo.
    echo ❌ 构建失败！请检查错误信息
    pause
    exit /b 1
)

echo.
echo ✅ 构建成功！
echo.
echo ========================================
echo   测试说明
echo ========================================
echo.
echo 1. 刷新浏览器页面（Ctrl + F5）
echo 2. 打开浏览器控制台（F12）
echo 3. 点击"工具" → "色条配置"
echo 4. 查看控制台日志输出
echo.
echo 期望看到的日志：
echo   - "=== ColorConfig组件已挂载 ==="
echo   - "visible prop: true"
echo   - "store.toolbarcontent: colorConfig"
echo.
echo 如果仍然没有显示，请查看：
echo   色条配置页面显示问题排查指南.md
echo.
echo ========================================

cd ..
pause

