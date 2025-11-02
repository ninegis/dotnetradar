@echo off
chcp 65001 >nul 2>&1
setlocal enabledelayedexpansion
title Build Frontend

echo.
echo ========================================
echo    Frontend Build and Deploy
echo ========================================
echo.

REM Get script directory
set "SCRIPT_DIR=%~dp0"
set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"

REM [1/4] Check Node.js
echo [1/4] Checking Node.js...
where node >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Node.js not found
    echo Please install Node.js from https://nodejs.org
    pause
    exit /b 1
)
for /f "delims=" %%v in ('node --version 2^>nul') do set NODE_VERSION=%%v
echo [OK] Node.js: %NODE_VERSION%

REM [2/4] Build frontend
echo.
echo [2/4] Building frontend project...
set "FRONTEND_DIR=%SCRIPT_DIR%\RadarContrl"

if not exist "%FRONTEND_DIR%" (
    echo [ERROR] Frontend directory not found: %FRONTEND_DIR%
    pause
    exit /b 1
)

if not exist "%FRONTEND_DIR%\package.json" (
    echo [ERROR] package.json not found: %FRONTEND_DIR%\package.json
    pause
    exit /b 1
)

cd /d "%FRONTEND_DIR%"
if errorlevel 1 (
    echo [ERROR] Cannot change to frontend directory
    pause
    exit /b 1
)

REM Check and install dependencies if needed
if not exist "node_modules" (
    echo [INFO] Installing dependencies...
    call npm install
    if errorlevel 1 (
        echo [ERROR] npm install failed
        cd /d "%SCRIPT_DIR%"
        pause
        exit /b 1
    )
)

echo [INFO] Running npm build...
call npm run build
if errorlevel 1 (
    echo [ERROR] Frontend build failed
    echo Please check the error messages above
    cd /d "%SCRIPT_DIR%"
    pause
    exit /b 1
)
echo [OK] Frontend build successful

REM [3/4] Deploy to wwwroot
echo.
echo [3/4] Deploying to wwwroot...
cd /d "%SCRIPT_DIR%"
set "WWWROOT_DIR=%SCRIPT_DIR%\RadarSystem.WebAPI\wwwroot"
set "DIST_DIR=%FRONTEND_DIR%\dist"

if not exist "%DIST_DIR%" (
    echo [ERROR] Build output directory not found: %DIST_DIR%
    echo Please confirm frontend build was successful
    pause
    exit /b 1
)

if exist "%WWWROOT_DIR%" (
    echo [INFO] Cleaning old wwwroot directory...
    rd /s /q "%WWWROOT_DIR%" 2>nul
)

echo [INFO] Copying files to wwwroot...
if not exist "%WWWROOT_DIR%" mkdir "%WWWROOT_DIR%"
xcopy /E /I /Y "%DIST_DIR%\*" "%WWWROOT_DIR%\" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Deployment failed
    echo Please check file permissions or disk space
    pause
    exit /b 1
)
echo [OK] Deployment completed

REM [4/5] Build backend
echo.
echo [4/5] Building backend project...
set "BACKEND_DIR=%SCRIPT_DIR%\RadarSystem.WebAPI"
cd /d "%BACKEND_DIR%"
if errorlevel 1 (
    echo [ERROR] Cannot change to backend directory
    pause
    exit /b 1
)

echo [INFO] Running dotnet build...
dotnet build --configuration Release --verbosity minimal
if errorlevel 1 (
    echo [ERROR] Backend build failed
    echo Please check the error messages above
    pause
    exit /b 1
)
echo [OK] Backend build successful

REM [5/5] Start system service
echo.
echo [5/5] Starting system service...

REM Check if ports are in use
netstat -ano | findstr ":6098" >nul 2>&1
if not errorlevel 1 (
    echo [WARN] Port 6098 is in use, trying to stop existing process...
    for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":6098" ^| findstr "LISTENING"') do (
        taskkill /F /PID %%a >nul 2>&1
    )
    timeout /t 2 /nobreak >nul
)

netstat -ano | findstr ":8099" >nul 2>&1
if not errorlevel 1 (
    echo [WARN] Port 8099 is in use, trying to stop existing process...
    for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":8099" ^| findstr "LISTENING"') do (
        taskkill /F /PID %%a >nul 2>&1
    )
    timeout /t 2 /nobreak >nul
)

echo.
echo ========================================
echo    System Starting...
echo ========================================
echo.
echo Access URLs:
echo    Frontend: http://localhost:6098
echo    API:      http://localhost:8099
echo    Swagger:  http://localhost:8099/swagger
echo.
echo Default Account:
echo    Username: admin
echo    Password: admin123
echo.
echo Press Ctrl+C to stop the service
echo.
echo ========================================
echo    System Running...
echo ========================================
echo.

REM Wait 2 seconds then open browser
timeout /t 2 /nobreak >nul
start "" "http://localhost:6098"

REM Start application
dotnet run --configuration Release --no-build --urls "http://localhost:8099"

REM If program exits, show message
echo.
echo ========================================
echo    System Stopped
echo ========================================
echo.
pause
