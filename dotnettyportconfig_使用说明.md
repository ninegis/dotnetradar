# DotNetty端口配置使用说明

> **配置文件**: `dotnettyportconfig.json`  
> **版本**: 1.0  
> **更新**: 2025-11-02

---

## 📋 配置文件结构

### 主要字段说明

| 字段名 | 类型 | 说明 | 中文 |
|-------|------|------|------|
| `name` | string | 设备标识符（英文） | 设备名称（英文） |
| `displayName` | string | 显示名称 | 设备名称（中文） |
| `port` | int | TCP监听端口号 | 端口 |
| `enabled` | bool | 是否启用该设备 | 启用状态 |
| `startupCheck` | bool | **是否检查启动状态** | **启动检查** |
| `criticalMonitoring` | bool | 是否重点监测 | 重点监测 |
| `priority` | int | 启动优先级(1-6) | 优先级 |
| `protocol` | string | 通信协议类型 | 协议 |
| `handlerClass` | string | Handler类名 | 处理器类 |
| `dataTypes` | array | 支持的数据类型 | 数据类型 |
| `useFactoryId` | bool | 是否使用FactoryId映射 | 使用出厂ID |
| `consoleOutput` | bool | 是否详细控制台输出 | 控制台输出 |
| `comment` | string | 备注说明（中文） | 备注 |

---

## 🔑 重点字段：`startupCheck`

### 作用

控制是否在系统启动后验证该端口的监听状态。

### 配置规则

```json
{
  "name": "ArcRadar",
  "port": 1030,
  "enabled": true,
  "startupCheck": true,  ← 启用启动检查
  "comment": "圆弧雷达 - 最重要的设备，必须检查"
}
```

### 重点监测端口（`startupCheck: true`）

```
✅ 1030  - 圆弧雷达（ArcRadar）
✅ 10305 - MIMO Lite雷达
✅ 1060  - 建筑物雷达
✅ 11135 - 建筑物2D雷达
✅ 11125 - MIMO雷达
✅ 11129 - MIMO通用
✅ 11133 - 交通雷达（禁用但检查）
✅ 11127 - 俯仰电机
✅ 11114 - 电机
✅ 11111 - GPS设备
```

---

## 🛠️ 使用工具

### 1. PowerShell检查脚本

**文件**: `CheckCriticalPorts.ps1`

**用法**:
```powershell
# 简单检查
.\CheckCriticalPorts.ps1

# 详细模式（显示netstat详情）
.\CheckCriticalPorts.ps1 -Detailed
```

**输出示例**:
```
================================================================
  DotNetty Critical Ports Check - 20:53:15
================================================================

  [OK] Port  1030 - ArcRadar
  [--] Port  1060 - Building [NOT LISTENING]
  [OK] Port  6098 - Frontend
  [OK] Port  8099 - API
  [--] Port 10305 - MIMOLite [NOT LISTENING]
  [OK] Port 11111 - GPS
  [--] Port 11114 - Motor [NOT LISTENING]
  [--] Port 11125 - MIMO [NOT LISTENING]
  [--] Port 11127 - MotorPitch [NOT LISTENING]
  [--] Port 11129 - MIMOCommon [NOT LISTENING]
  [--] Port 11133 - Traffic [NOT LISTENING]
  [--] Port 11135 - Building2D [NOT LISTENING]

----------------------------------------------------------------
  Success: 1 / 10
================================================================
```

### 2. C#端口检查器

**文件**: `RadarSystem.Communication/Utilities/PortStartupChecker.cs`

**用法**:
```csharp
var checker = new PortStartupChecker(logger);

// 检查单个端口
bool isListening = await checker.WaitForPortListeningAsync(1030, "圆弧雷达");

// 检查重点端口
var results = await checker.CheckCriticalPortsAsync();

// 显示所有监听端口
checker.ShowAllListeningPorts();
```

---

## ⚙️ appsettings.json 配置

### 添加 `StartupCheck` 字段

```json
{
  "Netty": {
    "ArcRadar": {
      "Port": 1030,
      "Enable": true,
      "StartupCheck": true,  ← 新增字段
      "ProjectId": "PROJECT001",
      "DataPath": "../..",
      "ApiPort": "8099",
      "Comment": "圆弧雷达 DAG - 独立端口，重点监测"
    }
  }
}
```

**建议配置**（已在`appsettings_netty_ports_enhanced.json`中）:
- ✅ **重点设备**: `StartupCheck: true` （10个端口）
- ⊝ **普通设备**: `StartupCheck: false` （8个端口）

---

## 📊 配置示例

### 重点监测设备（必须检查）

```json
{
  "devices": [
    {
      "name": "ArcRadar",
      "port": 1030,
      "enabled": true,
      "startupCheck": true,  ← 必须检查
      "criticalMonitoring": true,
      "comment": "圆弧雷达，核心设备"
    }
  ]
}
```

### 普通设备（可选检查）

```json
{
  "devices": [
    {
      "name": "GpsV1",
      "port": 11109,
      "enabled": true,
      "startupCheck": false,  ← 不检查
      "criticalMonitoring": false,
      "comment": "GPS V1设备"
    }
  ]
}
```

---

## 🔍 端口检查命令速查

```powershell
# 方法1: 使用检查脚本（推荐）
.\CheckCriticalPorts.ps1

# 方法2: 手动检查重点端口
netstat -ano | findstr "LISTENING" | findstr "1030 10305 1060 11135 11125 11129 11133 11127 11114 11111"

# 方法3: 检查单个端口
netstat -ano | findstr ":1030"

# 方法4: 使用完整监测脚本
.\端口监测PowerShell脚本.ps1
```

---

## 📖 配置文件位置

1. **dotnettyportconfig.json** - 完整配置（机器可读）
2. **appsettings_netty_ports_enhanced.json** - 增强的appsettings配置模板
3. **appsettings.json** - 实际使用的配置（需手动添加StartupCheck字段）

---

## 🚀 应用配置

### 步骤1: 更新 appsettings.json

将 `appsettings_netty_ports_enhanced.json` 中的 Netty 配置复制到 `appsettings.json`，或手动添加 `StartupCheck` 字段。

### 步骤2: 修改启动代码

在 `AllDeviceNettyServersHostedService.cs` 的 `TryStartServer` 方法中读取 `StartupCheck` 配置：

```csharp
var startupCheck = config.GetValue<bool>("StartupCheck", false);

if (startupCheck)
{
    // 启动后验证端口监听
    await VerifyPortListening(port, configKey);
}
```

### 步骤3: 运行检查脚本

```powershell
.\CheckCriticalPorts.ps1
```

---

**已完成的配置文件和工具，等待应用到 appsettings.json！**

