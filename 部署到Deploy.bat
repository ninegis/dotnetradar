@echo off
chcp 65001 >nul 2>&1
setlocal enabledelayedexpansion
title Deploy to Deploy Directory

echo.
echo ========================================
echo    Deploy to Deploy Directory
echo ========================================
echo.

REM Get script directory
set "SCRIPT_DIR=%~dp0"
set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"

set "DEPLOY_DIR=%SCRIPT_DIR%\Deploy"
set "WEBAPI_DIR=%SCRIPT_DIR%\RadarSystem.WebAPI"

REM Check if Deploy directory exists
if not exist "%DEPLOY_DIR%" (
    echo [ERROR] Deploy directory not found: %DEPLOY_DIR%
    echo Please create Deploy directory first
    pause
    exit /b 1
)

echo [1/5] Building frontend...
cd /d "%SCRIPT_DIR%"
call build-frontend.bat
if errorlevel 1 (
    echo [ERROR] Frontend build failed
    pause
    exit /b 1
)

echo.
echo [2/5] Building backend...
cd /d "%WEBAPI_DIR%"
dotnet build --configuration Release --verbosity minimal
if errorlevel 1 (
    echo [ERROR] Backend build failed
    pause
    exit /b 1
)

echo.
echo [3/5] Publishing backend to Deploy...
dotnet publish --configuration Release --verbosity minimal --output "%DEPLOY_DIR%" --no-build
if errorlevel 1 (
    echo [ERROR] Backend publish failed
    pause
    exit /b 1
)

echo.
echo [4/5] Copying configuration files...
REM Copy appsettings.json to Deploy/conf
if not exist "%DEPLOY_DIR%\conf" mkdir "%DEPLOY_DIR%\conf"
if exist "%WEBAPI_DIR%\appsettings.json" (
    copy /Y "%WEBAPI_DIR%\appsettings.json" "%DEPLOY_DIR%\conf\appsettings.json" >nul
    echo [OK] Configuration file copied to Deploy/conf/appsettings.json
)

REM Copy database if exists
if not exist "%DEPLOY_DIR%\db" mkdir "%DEPLOY_DIR%\db"
if exist "%WEBAPI_DIR%\Data\*.db" (
    copy /Y "%WEBAPI_DIR%\Data\*.db" "%DEPLOY_DIR%\db\" >nul
    echo [OK] Database files copied to Deploy/db
)

echo.
echo [5/5] Ensuring directory structure...
if not exist "%DEPLOY_DIR%\Data" mkdir "%DEPLOY_DIR%\Data"
if not exist "%DEPLOY_DIR%\logs" mkdir "%DEPLOY_DIR%\logs"
if not exist "%DEPLOY_DIR%\wwwroot" mkdir "%DEPLOY_DIR%\wwwroot"
echo [OK] Directory structure verified

echo.
echo ========================================
echo    Deployment Complete
echo ========================================
echo.
echo Deployment directory: %DEPLOY_DIR%
echo.
echo Directory structure:
echo   Deploy\
echo   ├── RadarSystem.WebAPI.dll
echo   ├── RadarSystem.WebAPI.exe
echo   ├── conf\appsettings.json
echo   ├── db\radar.db
echo   ├── Data\
echo   ├── logs\
echo   ├── wwwroot\
echo   ├── emqx\
echo   ├── 启动系统.bat
echo   └── 停止系统.bat
echo.
echo To start the system:
echo   cd Deploy
echo   启动系统.bat
echo.
pause

