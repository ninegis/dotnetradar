@echo off
chcp 65001 >nul
echo ========================================
echo   EMQX MQTT Broker 快速部署
echo ========================================
echo.

rem 检查Chocolatey
where choco >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [INFO] 安装Chocolatey包管理器...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))"
)

rem 安装EMQX
echo [INFO] 通过Chocolatey安装EMQX...
choco install emqx -y

rem 启动EMQX
echo [INFO] 启动EMQX服务...
emqx start

rem 等待启动
timeout /t 10 /nobreak >nul

rem 检查端口
echo.
echo [INFO] 检查EMQX端口...
netstat -ano | findstr ":1883 :8083 :18083"

echo.
echo ========================================
echo   EMQX 部署完成
echo ========================================
echo.
echo 服务端口:
echo   MQTT TCP: 1883
echo   MQTT WebSocket: 8083
echo   Dashboard: http://localhost:18083
echo.
echo 默认账号:
echo   用户名: admin
echo   密码: public
echo.
pause

