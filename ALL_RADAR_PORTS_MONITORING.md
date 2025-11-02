# ALL RADAR PORTS MONITORING CONFIGURATION

> **System**: Slope Radar Monitoring System  
> **Version**: 1.0  
> **Date**: 2025-11-02

---

## CRITICAL PORTS SUMMARY

### Radar Devices (Priority)

| Device Type | Port | Status | FactoryId | Console Output | Description |
|------------|------|--------|-----------|----------------|-------------|
| **Arc Radar** | **1030** | ✅ Enabled | Yes | Yes | DAG Arc Radar (TOP PRIORITY) |
| **Building Radar** | **1060** | ✅ Enabled | Yes | Yes | 3D Building Radar |
| **Building 2D Radar** | **11135** | ✅ Enabled | Yes | Yes | 2D Building Radar |
| **MIMO Lite Radar** | **10305** | ✅ Enabled | Yes | Yes | MIMO Lite Array Radar |
| **MIMO Radar** | **11125** | ✅ Enabled | Yes | Yes | MIMO Advanced Radar |
| **MIMO Common** | **11129** | ✅ Enabled | Yes | Yes | MIMO General Radar |

### Sensor Devices

| Device Type | Port | Status | Console Output |
|------------|------|--------|----------------|
| GPS Device | 11111 | ✅ Enabled | No |
| GPS V1 | 11109 | ✅ Enabled | No |
| BeiWei V1 | 11110 | ✅ Enabled | No |
| Inclinometer (Qxz) | 11126 | ✅ Enabled | No |
| Laser Device | 11131 | ✅ Enabled | No |
| CM Device | 11124 | ✅ Enabled | No |
| Orientation Sensor | 11128 | ✅ Enabled | No |

### Control Devices

| Device Type | Port | Status |
|------------|------|--------|
| Motor | 11114 | ✅ Enabled |
| BMotor | 11115 | ✅ Enabled |
| Motor Pitch | 11127 | ✅ Enabled |

### Alarm Devices

| Device Type | Port | Status |
|------------|------|--------|
| Alarm Device | 11113 | ✅ Enabled |
| Alarm Device General | 11130 | ✅ Enabled |

### Web Services

| Service | Port/Path | Protocol |
|---------|-----------|----------|
| Frontend | 6098 | HTTP |
| API | 8099 | HTTP |
| WebSocket | /wss | WebSocket |

---

## PORT MONITORING COMMANDS

### Windows PowerShell

```powershell
# Check ALL radar ports
netstat -ano | findstr "LISTENING" | findstr "1030 1060 10305 11125 11129 11135"

# Check specific device type
netstat -ano | findstr "LISTENING" | findstr ":1030"  # Arc Radar
netstat -ano | findstr "LISTENING" | findstr ":1060"  # Building Radar
netstat -ano | findstr "LISTENING" | findstr ":10305" # MIMO Lite

# Check ALL device ports
netstat -ano | findstr "LISTENING" | findstr "1030 1060 1110 1112 1113"

# Use monitoring script
powershell -ExecutionPolicy Bypass -File "端口监测PowerShell脚本.ps1"

# Continuous monitoring (every 5 seconds)
powershell -ExecutionPolicy Bypass -File "端口监测PowerShell脚本.ps1" -Continuous -Interval 5
```

### Linux

```bash
# Check ALL ports
netstat -tuln | grep -E ':(1030|1060|6098|8099|10305|11)'

# Check specific port
netstat -tuln | grep :1030
```

---

## CONSOLE OUTPUT FORMAT

### Data Reception Format

```
================================================================================
【Arc Radar Data Received】
  Time: 2025-11-02 18:35:12.456
  Port: 1030
  Unique ID (FactoryId/SlaveId): 20
  Command: 0x0302
  Data Length: 1,048,576 bytes (1024.00 KB)
  Raw Data (HEX): 5A5A000000140302...
  Device Mapping: FactoryId(20) → DeviceId(RADAR_001)
================================================================================
```

### File Save Format

```
********************************************************************************
【Arc Radar File Saved】
  Time: 2025-11-02 18:35:12.678
  Device ID: RADAR_001
  FactoryId: 20
  Data Type: Deformation (00)
  File Path: ../../data/project/PROJECT001/radar/RADAR_001/00_20251102183512.dat
  File Size: 1024.00 KB
  Status: ✅ Success
********************************************************************************
```

---

## FACTORY ID MAPPING

### Concept

```
Device Data Packet    →  Database FactoryId  →  System DeviceId  →  File Path
────────────────────────────────────────────────────────────────────────────
SlaveId: 0x00000014  →  FactoryId: "20"     →  DeviceId: "RADAR_001"  →  .../RADAR_001/
SlaveId: 0x00000015  →  FactoryId: "21"     →  DeviceId: "RADAR_002"  →  .../RADAR_002/
```

### Configuration Required in Database

```sql
-- Devices table MUST have FactoryId configured
INSERT INTO Devices (DeviceId, FactoryId, DeviceName, DeviceType, ProjectId, Port)
VALUES 
  ('RADAR_001', '20', 'Arc Radar #1', 'ARC', 'PROJECT001', 1030),
  ('RADAR_002', '21', 'Arc Radar #2', 'ARC', 'PROJECT001', 1030),
  ('BUILDING_001', 'BLD001', 'Building Radar #1', 'BUILDING', 'PROJECT001', 1060);
```

---

## DATA STORAGE STRUCTURE

```
data/
└── project/
    └── {ProjectId}/
        └── radar/
            ├── {DeviceId_1}/
            │   ├── 00/  (Deformation)
            │   │   └── {yyyyMMdd}/
            │   │       └── HHmmss.dat
            │   ├── 01/  (Backscatter)
            │   └── 02/  (Confidence)
            ├── {DeviceId_2}/
            └── ...
```

Example:
```
data/project/PROJECT001/radar/
├── 20/  (if no mapping found, use FactoryId)
│   └── 00_20251102183000.dat
└── RADAR_001/  (if mapping found)
    ├── 00/
    │   └── 20251102/
    │       └── 183000.dat
    ├── 01/
    └── 02/
```

---

## MONITORING TOOLS

### 1. Port Monitoring Script

**File**: `端口监测PowerShell脚本.ps1`

**Usage**:
```powershell
# Single check
.\端口监测PowerShell脚本.ps1

# Continuous monitoring
.\端口监测PowerShell脚本.ps1 -Continuous -Interval 5
```

### 2. System Startup Script

**File**: `启动雷达系统并监测1030端口.bat`

**Usage**:
```batch
.\启动雷达系统并监测1030端口.bat
```

### 3. Unified Console Logger

**File**: `RadarSystem.Communication/Utilities/ConsoleDataLogger.cs`

**Methods**:
- `LogDataReceived()` - Log when data is received
- `LogFileSaved()` - Log when file is saved
- `LogServerStarted()` - Log when server starts
- `LogDeviceMappingLoaded()` - Log when mappings load
- `LogHeartbeat()` - Log heartbeat packets

---

## CONFIGURATION FILES

### Primary Config: `appsettings.json`

```json
{
  "Netty": {
    "ArcRadar": {
      "Port": 1030,
      "Enable": true,
      "ProjectId": "PROJECT001",
      "DataPath": "../..",
      "ApiPort": "8099"
    },
    "BuildingRadar": {
      "Port": 1060,
      "Enable": true,
      "ProjectId": "PROJECT001",
      "DataPath": "../..",
      "ApiPort": "8099"
    },
    "Building2DRadar": {
      "Port": 11135,
      "Enable": true,
      "ProjectId": "PROJECT001",
      "DataPath": "../..",
      "ApiPort": "8099"
    },
    "MimoLiteRadar": {
      "Port": 10305,
      "Enable": true,
      "ProjectId": "PROJECT001",
      "DataPath": "../..",
      "ApiPort": "8099"
    },
    "MimoRadar": {
      "Port": 11125,
      "Enable": true,
      "ProjectId": "PROJECT001",
      "DataPath": "../..",
      "ApiPort": "8099"
    },
    "Mimo": {
      "Port": 11129,
      "Enable": true,
      "ProjectId": "PROJECT001",
      "DataPath": "../..",
      "ApiPort": "8099"
    }
  }
}
```

### Monitoring Config: `所有雷达端口监测配置.json`

Complete JSON configuration with all device details, mappings, and monitoring commands.

---

## QUICK START GUIDE

### Step 1: Verify Database Configuration

Ensure devices are configured in SQLite database with **FactoryId** field:

```
GET http://localhost:8099/api/Device
```

Verify response contains:
```json
{
  "deviceId": "RADAR_001",
  "factoryId": "20",  ← MUST match device SlaveId
  ...
}
```

### Step 2: Start System

```powershell
cd RadarSystem.WebAPI
dotnet run --configuration Release
```

### Step 3: Monitor Ports (in another terminal)

```powershell
.\端口监测PowerShell脚本.ps1 -Continuous
```

### Step 4: Verify Port Listening

Expected output after 15 seconds:
```
【Radar Devices】
  ✅ Arc Radar (Port 1030) - DAG Arc Radar
  ✅ Building Radar (Port 1060) - 3D Building Radar
  ✅ Building 2D Radar (Port 11135) - 2D Building Radar
  ✅ MIMO Lite Radar (Port 10305) - MIMO Lite Array Radar
  ✅ MIMO Radar (Port 11125) - MIMO Advanced Radar
  ✅ MIMO Common (Port 11129) - MIMO General Radar

【Statistics】
  Radar Devices: 6/6 ports online
  Web Services: 2/2 ports online
  All Devices: 18/18 ports online
```

### Step 5: Wait for Device Connection

When FactoryId=20 device sends data:
```
================================================================================
【Arc Radar Data Received】
  Time: 2025-11-02 18:35:12.456
  Port: 1030
  Unique ID (FactoryId/SlaveId): 20
  Command: 0x0302
  Data Length: 1,048,576 bytes (1024.00 KB)
  Device Mapping: FactoryId(20) → DeviceId(RADAR_001)
================================================================================
```

---

## TROUBLESHOOTING

### Issue: No Radar Ports Listening

**Symptoms**:
```
❌ Arc Radar (Port 1030) [Not Listening]
❌ All radar ports show offline
✅ Web services (6098, 8099) are online
```

**Root Cause**:
- Netty device servers not starting
- AllDeviceNettyServersHostedService not triggered
- Or MQTT connection blocks startup

**Solution**:
1. Check logs: `Get-Content RadarSystem.WebAPI\logs\radar-api-*.txt -Tail 200`
2. Look for: "边坡雷达设备通信系统启动" or "AllDevice"
3. If not found → Use direct startup code in Program.cs (already added)
4. Run system in foreground to see console output:
   ```powershell
   cd RadarSystem.WebAPI
   dotnet run --configuration Release --no-build
   ```

### Issue: FactoryId Not Mapping

**Symptoms**:
```
Device Mapping: FactoryId(20) → DeviceId(20)
```
(DeviceId same as FactoryId = no mapping found)

**Solution**:
1. Ensure API is running (port 8099)
2. Verify device exists in database
3. Check FactoryId field is not empty
4. Wait 15 seconds for mapping to load
5. Check logs for: "加载设备映射: FactoryId=20 → DeviceId=..."

---

## FILES CREATED

### Configuration
1. `所有雷达端口监测配置.json` - Complete port configuration in JSON
2. `ALL_RADAR_PORTS_MONITORING.md` - This file (English documentation)

### Utilities
1. `RadarSystem.Communication/Utilities/ConsoleDataLogger.cs` - Unified console logger

### Scripts  
1. `端口监测PowerShell脚本.ps1` - Port monitoring script
2. `启动雷达系统并监测1030端口.bat` - System startup script

### Documentation
1. `系统数据流转完整分析.md` - Complete data flow analysis
2. `设备端口快速参考表.md` - Quick port reference
3. `FactoryId数据流转修复总结.md` - FactoryId mapping fix summary
4. Multiple other analysis and guide documents

---

## CURRENT STATUS

```
System: RadarSystem.WebAPI running (PID: 11076)
Ports:
  ✅ 6098 (Frontend) - LISTENING
  ✅ 8099 (API) - LISTENING
  ❌ 1030 (Arc Radar) - NOT LISTENING
  ❌ All other device ports - NOT LISTENING
  
Issue: Netty device servers not starting
Action Needed: Check startup console output or logs
```

---

## NEXT STEPS

1. **Stop current process**:
   ```powershell
   Get-Process | Where-Object {$_.ProcessName -eq "dotnet"} | Stop-Process -Force
   ```

2. **Run in foreground** to see console output:
   ```powershell
   cd RadarSystem.WebAPI
   dotnet run --configuration Release
   ```

3. **Watch for console output**:
   - Look for "圆弧雷达服务器" startup messages
   - Look for "✅✅✅" or "❌❌❌" markers
   - Check if port 1030 binds successfully

4. **Monitor ports** in another terminal:
   ```powershell
   .\端口监测PowerShell脚本.ps1 -Continuous
   ```

5. **Test with real device** (FactoryId=20):
   - Connect device to port 1030
   - Send data packet
   - Observe console output
   - Verify file saved in `data/project/PROJECT001/radar/`

---

**All code modifications complete. Ready for final testing with real device connection.**

