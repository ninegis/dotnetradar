@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

set TIMESTAMP=%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%
set REPORT=AutoTestReport_%TIMESTAMP%.txt

echo ================================================================ > %REPORT%
echo   Auto Test System - DotNetty Port Monitoring >> %REPORT%
echo   Start Time: %date% %time% >> %REPORT%
echo ================================================================ >> %REPORT%
echo. >> %REPORT%

echo.
echo ================================================================
echo   Auto Test System - DotNetty Port Monitoring
echo   Start Time: %date% %time%
echo ================================================================
echo.

rem ====================================================================
rem STEP 1: Stop all processes
rem ====================================================================
echo [STEP 1] Stopping all processes...
echo [STEP 1] Stopping all processes... >> %REPORT%

taskkill /F /IM dotnet.exe 2>nul
timeout /t 3 /nobreak >nul

echo [OK] Processes stopped
echo [OK] Processes stopped >> %REPORT%
echo.

rem ====================================================================
rem STEP 2: Build project
rem ====================================================================
echo [STEP 2] Building project...
echo [STEP 2] Building project... >> %REPORT%

dotnet build RadarSystem.sln -c Release > build_output.txt 2>&1

findstr /C:"succeeded" /C:"已成功" build_output.txt >nul
if %ERRORLEVEL% EQU 0 (
    echo [OK] Build SUCCESS
    echo [OK] Build SUCCESS >> %REPORT%
) else (
    echo [ERROR] Build FAILED
    echo [ERROR] Build FAILED >> %REPORT%
    type build_output.txt >> %REPORT%
    goto :END_FAILED
)
echo.

rem ====================================================================
rem STEP 3: Start system
rem ====================================================================
echo [STEP 3] Starting system...
echo [STEP 3] Starting system... >> %REPORT%

cd RadarSystem.WebAPI
start /B dotnet run --configuration Release --no-build > nul 2>&1
cd ..

echo [OK] System started
echo [OK] System started >> %REPORT%
echo.

rem ====================================================================
rem STEP 4: Wait for startup
rem ====================================================================
echo [STEP 4] Waiting %ERRORLEVEL% seconds for startup...
echo [STEP 4] Waiting for startup... >> %REPORT%

timeout /t 20 /nobreak >nul

echo [OK] Wait completed
echo [OK] Wait completed >> %REPORT%
echo.

rem ====================================================================
rem STEP 5: Check critical ports
rem ====================================================================
echo [STEP 5] Checking critical ports...
echo [STEP 5] Checking critical ports... >> %REPORT%
echo.

echo   Port Check Results:
echo   ----------------------------------------
echo   Port Check Results: >> %REPORT%
echo   ---------------------------------------- >> %REPORT%

set SUCCESS_COUNT=0

rem Check Port 1030 - ArcRadar
netstat -ano | findstr "LISTENING" | findstr ":1030" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port  1030 - ArcRadar
    echo     [OK] Port  1030 - ArcRadar >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port  1030 - ArcRadar [NOT LISTENING]
    echo     [--] Port  1030 - ArcRadar [NOT LISTENING] >> %REPORT%
)

rem Check Port 10305 - MIMO Lite
netstat -ano | findstr "LISTENING" | findstr ":10305" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port 10305 - MIMOLite
    echo     [OK] Port 10305 - MIMOLite >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port 10305 - MIMOLite [NOT LISTENING]
    echo     [--] Port 10305 - MIMOLite [NOT LISTENING] >> %REPORT%
)

rem Check Port 1060 - Building
netstat -ano | findstr "LISTENING" | findstr ":1060" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port  1060 - Building
    echo     [OK] Port  1060 - Building >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port  1060 - Building [NOT LISTENING]
    echo     [--] Port  1060 - Building [NOT LISTENING] >> %REPORT%
)

rem Check Port 11135 - Building2D
netstat -ano | findstr "LISTENING" | findstr ":11135" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port 11135 - Building2D
    echo     [OK] Port 11135 - Building2D >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port 11135 - Building2D [NOT LISTENING]
    echo     [--] Port 11135 - Building2D [NOT LISTENING] >> %REPORT%
)

rem Check Port 11125 - MIMO
netstat -ano | findstr "LISTENING" | findstr ":11125" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port 11125 - MIMO
    echo     [OK] Port 11125 - MIMO >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port 11125 - MIMO [NOT LISTENING]
    echo     [--] Port 11125 - MIMO [NOT LISTENING] >> %REPORT%
)

rem Check Port 11129 - MIMOCommon
netstat -ano | findstr "LISTENING" | findstr ":11129" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port 11129 - MIMOCommon
    echo     [OK] Port 11129 - MIMOCommon >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port 11129 - MIMOCommon [NOT LISTENING]
    echo     [--] Port 11129 - MIMOCommon [NOT LISTENING] >> %REPORT%
)

rem Check Port 11133 - Traffic
netstat -ano | findstr "LISTENING" | findstr ":11133" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port 11133 - Traffic
    echo     [OK] Port 11133 - Traffic >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port 11133 - Traffic [NOT LISTENING]
    echo     [--] Port 11133 - Traffic [NOT LISTENING] >> %REPORT%
)

rem Check Port 11127 - MotorPitch
netstat -ano | findstr "LISTENING" | findstr ":11127" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port 11127 - MotorPitch
    echo     [OK] Port 11127 - MotorPitch >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port 11127 - MotorPitch [NOT LISTENING]
    echo     [--] Port 11127 - MotorPitch [NOT LISTENING] >> %REPORT%
)

rem Check Port 11114 - Motor
netstat -ano | findstr "LISTENING" | findstr ":11114" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port 11114 - Motor
    echo     [OK] Port 11114 - Motor >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port 11114 - Motor [NOT LISTENING]
    echo     [--] Port 11114 - Motor [NOT LISTENING] >> %REPORT%
)

rem Check Port 11111 - GPS
netstat -ano | findstr "LISTENING" | findstr ":11111" >nul
if %ERRORLEVEL% EQU 0 (
    echo     [OK] Port 11111 - GPS
    echo     [OK] Port 11111 - GPS >> %REPORT%
    set /a SUCCESS_COUNT+=1
) else (
    echo     [--] Port 11111 - GPS [NOT LISTENING]
    echo     [--] Port 11111 - GPS [NOT LISTENING] >> %REPORT%
)

echo   ----------------------------------------
echo.

rem ====================================================================
rem STEP 6: Evaluate results
rem ====================================================================
echo [STEP 6] Evaluating results...
echo [STEP 6] Evaluating results... >> %REPORT%
echo Success: %SUCCESS_COUNT% / 10 >> %REPORT%
echo.

if %SUCCESS_COUNT% GTR 0 (
    echo.
    echo ================================================================
    echo   SUCCESS! %SUCCESS_COUNT% port^(s^) are listening
    echo ================================================================
    echo.
    echo Next Steps:
    echo   1. Check ports: .\CheckCriticalPorts.ps1
    echo   2. Monitor logs: Get-Content RadarSystem.WebAPI\logs\*.txt -Tail 50 -Wait
    echo   3. Report file: %REPORT%
    echo.
    echo [SUCCESS] Test completed with %SUCCESS_COUNT%/10 ports online >> %REPORT%
    goto :END_SUCCESS
) else (
    echo.
    echo ================================================================
    echo   FAILED! No ports listening
    echo ================================================================
    echo.
    echo Problem: Netty device servers not starting
    echo.
    echo Suggested Actions:
    echo   1. Run system in foreground:
    echo      cd RadarSystem.WebAPI
    echo      dotnet run --configuration Release
    echo.
    echo   2. Watch for console output:
    echo      Look for "ArcRadar server" messages
    echo.
    echo   3. Check logs:
    echo      Get-Content RadarSystem.WebAPI\logs\*.txt -Tail 200
    echo.
    echo [FAILED] No ports online after test >> %REPORT%
    goto :END_FAILED
)

:END_SUCCESS
echo [RESULT] TEST PASSED >> %REPORT%
type %REPORT%
pause
exit /b 0

:END_FAILED
echo [RESULT] TEST FAILED >> %REPORT%
type %REPORT%
pause
exit /b 1

