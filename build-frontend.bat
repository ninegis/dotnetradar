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

REM [3/4] Deploy to wwwroot (both development and Deploy)
echo.
echo [3/4] Deploying to wwwroot...
cd /d "%SCRIPT_DIR%"
set "WWWROOT_DIR=%SCRIPT_DIR%\RadarSystem.WebAPI\wwwroot"
set "DEPLOY_WWWROOT_DIR=%SCRIPT_DIR%\Deploy\wwwroot"
set "DIST_DIR=%FRONTEND_DIR%\dist"

if not exist "%DIST_DIR%" (
    echo [ERROR] Build output directory not found: %DIST_DIR%
    echo Please confirm frontend build was successful
    pause
    exit /b 1
)

REM Deploy to development wwwroot
if exist "%WWWROOT_DIR%" (
    echo [INFO] Cleaning old wwwroot directory...
    rd /s /q "%WWWROOT_DIR%" 2>nul
)
echo [INFO] Copying files to development wwwroot...
if not exist "%WWWROOT_DIR%" mkdir "%WWWROOT_DIR%"
xcopy /E /I /Y "%DIST_DIR%\*" "%WWWROOT_DIR%\" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Development deployment failed
    pause
    exit /b 1
)
echo [OK] Development wwwroot deployment completed

REM Deploy to Deploy/wwwroot
if exist "%DEPLOY_WWWROOT_DIR%" (
    echo [INFO] Cleaning old Deploy wwwroot directory...
    rd /s /q "%DEPLOY_WWWROOT_DIR%" 2>nul
)
echo [INFO] Copying files to Deploy/wwwroot...
if not exist "%DEPLOY_WWWROOT_DIR%" mkdir "%DEPLOY_WWWROOT_DIR%"
xcopy /E /I /Y "%DIST_DIR%\*" "%DEPLOY_WWWROOT_DIR%\" >nul 2>&1
if errorlevel 1 (
    echo [WARN] Deploy wwwroot deployment failed (may not exist yet)
) else (
    echo [OK] Deploy wwwroot deployment completed
)

echo.
echo ========================================
echo    Frontend Build Complete
echo ========================================
echo.
echo Deployment completed to:
echo   - RadarSystem.WebAPI\wwwroot (Development)
echo   - Deploy\wwwroot (Production)
echo.
echo To deploy full system to Deploy directory:
echo   Run: 部署到Deploy.bat
echo.
pause
