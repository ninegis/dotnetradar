@echo off
chcp 65001 >nul 2>&1
setlocal enabledelayedexpansion
title Fix Algorithm Config Table

echo.
echo ========================================
echo    Fix Algorithm Config Table
echo ========================================
echo.

set "SCRIPT_DIR=%~dp0"
set "DB_PATH=%SCRIPT_DIR%RadarSystem.WebAPI\Data\radar.db"

if not exist "%DB_PATH%" (
    echo [ERROR] Database file not found: %DB_PATH%
    echo Please start the service first to create the database
    pause
    exit /b 1
)

echo [INFO] Database path: %DB_PATH%
echo [INFO] Adding missing columns to algorithm_configs table...
echo.

REM Create a temporary SQL file
set "TEMP_SQL=%TEMP%\fix_algorithm_configs_%RANDOM%.sql"

(
echo -- Add missing old columns to algorithm_configs table
echo ALTER TABLE algorithm_configs ADD COLUMN filter_type INTEGER DEFAULT 0;
echo ALTER TABLE algorithm_configs ADD COLUMN alpha_filter INTEGER DEFAULT 0;
echo ALTER TABLE algorithm_configs ADD COLUMN beta_filter INTEGER DEFAULT 0;
echo ALTER TABLE algorithm_configs ADD COLUMN de_noise_thread INTEGER DEFAULT 0;
echo ALTER TABLE algorithm_configs ADD COLUMN sens_coef INTEGER DEFAULT 0;
echo ALTER TABLE algorithm_configs ADD COLUMN defo_image_dec TEXT DEFAULT '1';
echo ALTER TABLE algorithm_configs ADD COLUMN scat_image_dec TEXT DEFAULT '1';
echo ALTER TABLE algorithm_configs ADD COLUMN win_coheren INTEGER DEFAULT 0;
echo ALTER TABLE algorithm_configs ADD COLUMN atm_pha_err_est_func_switch TEXT DEFAULT '0';
echo ALTER TABLE algorithm_configs ADD COLUMN filter_width INTEGER DEFAULT 0;
echo ALTER TABLE algorithm_configs ADD COLUMN monitor_mode TEXT DEFAULT '0';
echo ALTER TABLE algorithm_configs ADD COLUMN ipv4 TEXT;
) > "%TEMP_SQL%"

echo [INFO] SQL script created: %TEMP_SQL%
echo [INFO] Please restart the service to apply the changes
echo.
echo The service will automatically add missing columns on startup.
echo.
echo Press any key to exit...
pause >nul

del "%TEMP_SQL%" 2>nul

