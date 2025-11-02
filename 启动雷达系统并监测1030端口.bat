@echo off
chcp 65001 >nul
echo ========================================
echo   启动边坡雷达监测系统
echo   监测端口: 1030 (圆弧雷达)
echo ========================================
echo.

echo [1/3] 停止现有进程...
taskkill /F /IM RadarSystem.WebAPI.exe 2>nul
timeout /t 2 /nobreak >nul
echo [OK] 完成

echo.
echo [2/3] 启动雷达系统...
cd RadarSystem.WebAPI
start "雷达系统" dotnet run --configuration Release --no-build

echo [OK] 启动命令已执行
echo.

echo [3/3] 等待系统启动...
timeout /t 10 /nobreak >nul

echo.
echo 检查端口监听状态...
netstat -ano | findstr ":1030" | findstr "LISTENING"
if %ERRORLEVEL% EQU 0 (
    echo [OK] 端口1030已开始监听
) else (
    echo [警告] 端口1030未监听，请查看控制台输出
)

echo.
echo ========================================
echo   系统已启动
echo ========================================
echo.
echo 服务地址:
echo   前端: http://localhost:6098
echo   API:  http://localhost:8099
echo   Swagger: http://localhost:8099/swagger
echo   圆弧雷达: 端口1030
echo.
echo 控制台输出格式:
echo   【数据接收】时间 + 端口 + FactoryId + 原始数据
echo   【文件保存】保存路径和状态
echo.
echo 按任意键退出...
pause >nul

