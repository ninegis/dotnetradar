# DotNetty端口监测 - 完整配置指南

> **配置文件**: `dotnettyportconfig.json`  
> **检查脚本**: `CheckCriticalPorts.ps1`  
> **版本**: 1.0  
> **日期**: 2025-11-02

---

## ✅ 已完成的工作

### 1. 创建配置文件

| 文件 | 用途 | 格式 |
|------|------|------|
| `dotnettyportconfig.json` | 完整端口配置（英文字段） | JSON |
| `appsettings_netty_ports_enhanced.json` | 增强的appsettings模板 | JSON |
| `dotnettyportconfig_使用说明.md` | 配置使用说明 | Markdown |
| `DotNetty端口监测_完整配置指南.md` | 本文件 | Markdown |

### 2. 创建检查工具

| 工具 | 用途 | 语言 |
|------|------|------|
| `CheckCriticalPorts.ps1` | 快速检查重点端口 | PowerShell |
| `PortStartupChecker.cs` | C#端口检查器 | C# |
| `ConsoleDataLogger.cs` | 统一控制台输出 | C# |

### 3. 配置重点监测端口（10个）

| 端口 | 设备 | StartupCheck | 说明 |
|------|------|--------------|------|
| **1030** | 圆弧雷达 | ✅ true | 最高优先级 |
| **10305** | MIMO Lite | ✅ true | 高优先级 |
| **1060** | 建筑物雷达 | ✅ true | 高优先级 |
| **11135** | 建筑物2D | ✅ true | 高优先级 |
| **11125** | MIMO雷达 | ✅ true | 高优先级 |
| **11129** | MIMO通用 | ✅ true | 中优先级 |
| **11133** | 交通雷达 | ✅ true | 中优先级（禁用） |
| **11127** | 俯仰电机 | ✅ true | 中优先级 |
| **11114** | 电机 | ✅ true | 中优先级 |
| **11111** | GPS设备 | ✅ true | 中优先级 |

---

## 🔧 配置字段详解

### `enabled` vs `startupCheck`

```json
{
  "name": "ArcRadar",
  "port": 1030,
  "enabled": true,        // 控制是否启动该设备服务器
  "startupCheck": true,   // 控制是否检查端口监听状态
  "comment": "两者都为true时，会启动并检查"
}
```

**组合说明**:

| enabled | startupCheck | 行为 |
|---------|--------------|------|
| true | true | 启动服务器 + 检查端口 ✅ |
| true | false | 启动服务器但不检查 |
| false | true | 不启动但检查端口（用于调试） |
| false | false | 不启动也不检查 ⊝ |

---

## 📊 当前监测结果

### 运行 `CheckCriticalPorts.ps1` 结果

```
================================================================
  DotNetty Critical Ports Check - 21:05:25
================================================================

  [--] Port  1030 - ArcRadar [NOT LISTENING]
  [--] Port  1060 - Building [NOT LISTENING]
  [--] Port 10305 - MIMOLite [NOT LISTENING]
  [--] Port 11111 - GPS [NOT LISTENING]
  [--] Port 11114 - Motor [NOT LISTENING]
  [--] Port 11125 - MIMO [NOT LISTENING]
  [--] Port 11127 - MotorPitch [NOT LISTENING]
  [--] Port 11129 - MIMOCommon [NOT LISTENING]
  [--] Port 11133 - Traffic [NOT LISTENING]
  [--] Port 11135 - Building2D [NOT LISTENING]

----------------------------------------------------------------
  Success: 0 / 10
================================================================
```

**结论**: 所有重点端口都未监听，Netty设备服务器未启动。

---

## 🚀 启动验证流程

### 第1步：启动系统

```powershell
cd RadarSystem.WebAPI
dotnet run --configuration Release
```

### 第2步：等待15秒

观察控制台输出，应看到：
```
【圆弧雷达服务器】开始启动...
配置: Enable=True, Port=1030, ProjectId=PROJECT001
...
╔══════════════════════════════════════════════════════════════╗
║ ✅ 圆弧雷达 服务器启动成功！
║   监听端口: 1030
╚══════════════════════════════════════════════════════════════╝
```

### 第3步：在另一个终端运行检查

```powershell
.\CheckCriticalPorts.ps1
```

### 第4步：验证结果

**期望输出**（至少1030端口监听）:
```
  [OK] Port  1030 - ArcRadar
  ...
  Success: 1 / 10  或更多
```

---

## 📝 配置更新建议

### 将 StartupCheck 应用到 appsettings.json

```powershell
# 备份当前配置
Copy-Item appsettings.json appsettings.json.backup

# 使用增强配置
Copy-Item appsettings_netty_ports_enhanced.json appsettings.json
```

或手动编辑 `appsettings.json`，为每个Netty设备添加 `StartupCheck` 字段：

```json
{
  "Netty": {
    "ArcRadar": {
      "Port": 1030,
      "Enable": true,
      "StartupCheck": true,  ← 添加此字段
      ...
    }
  }
}
```

---

## 🎯 重点端口快速检查

### 命令行快速检查（不依赖配置文件）

```powershell
# 检查10个重点端口
$ports = @(1030, 10305, 1060, 11135, 11125, 11129, 11133, 11127, 11114, 11111)
$listening = 0
foreach ($p in $ports) {
    if (netstat -ano | findstr "LISTENING" | findstr ":$p") { $listening++ }
}
Write-Host "Success: $listening / $($ports.Count)" -ForegroundColor $(if ($listening -eq $ports.Count) { "Green" } elseif ($listening -gt 0) { "Yellow" } else { "Red" })
```

### 单行检查命令

```powershell
netstat -ano | findstr "LISTENING" | findstr "1030 10305 1060 11135 11125 11129 11133 11127 11114 11111"
```

---

## 📂 文件总览

### 配置文件

```
dotnettyportconfig.json                    # 主配置文件（英文字段）
├─ version: "1.0"
├─ criticalPorts: [1030, 10305, ...]       # 重点监测端口列表
├─ devices: [...]                          # 设备配置数组
│   ├─ name: "ArcRadar"                    # 英文标识
│   ├─ displayName: "圆弧雷达"              # 中文显示名
│   ├─ port: 1030                          # 端口号
│   ├─ enabled: true                       # 是否启用
│   ├─ startupCheck: true                  # 是否检查启动 ⭐
│   ├─ criticalMonitoring: true            # 是否重点监测
│   └─ comment: "..."                      # 中文备注
├─ webServices: [...]                      # Web服务配置
├─ startupCheckConfig: {...}               # 检查参数配置
├─ factoryIdMapping: {...}                 # FactoryId映射配置
└─ consoleOutputConfig: {...}              # 控制台输出配置
```

### 工具脚本

```
CheckCriticalPorts.ps1                     # 快速检查脚本（简洁版）
端口监测PowerShell脚本.ps1                  # 完整监测脚本
启动雷达系统并监测1030端口.bat              # 启动脚本
```

### 代码工具

```
RadarSystem.Communication/Utilities/
├─ PortStartupChecker.cs                   # C#端口检查器
└─ ConsoleDataLogger.cs                    # 统一控制台输出
```

---

## 💡 使用场景

### 场景1：系统启动验证

```powershell
# 1. 启动系统
cd RadarSystem.WebAPI
dotnet run

# 2. 等待15秒

# 3. 检查端口（另一终端）
.\CheckCriticalPorts.ps1

# 4. 查看结果
#    期望: Success: 6/10 或更多（至少雷达端口在线）
```

### 场景2：故障诊断

```powershell
# 1. 运行检查
.\CheckCriticalPorts.ps1

# 2. 如果某个端口未监听
#    检查日志查找原因
Get-Content RadarSystem.WebAPI\logs\*.txt -Tail 200 | 
  Select-String "端口\|Port\|1030\|启动\|失败"

# 3. 检查配置
Get-Content appsettings.json | Select-String "ArcRadar" -Context 5,5
```

### 场景3：持续监控

```powershell
# 在单独的PowerShell窗口运行
while ($true) {
    Clear-Host
    .\CheckCriticalPorts.ps1
    Start-Sleep -Seconds 5
}
```

---

## 📋 配置检查清单

- [ ] `dotnettyportconfig.json` 文件已创建
- [ ] `CheckCriticalPorts.ps1` 脚本可正常运行
- [ ] `appsettings.json` 中所有重点设备的 `StartupCheck` 设置为 `true`
- [ ] `PortStartupChecker.cs` 已编译
- [ ] `ConsoleDataLogger.cs` 已编译
- [ ] 系统可以正常启动
- [ ] 至少1030端口可以监听
- [ ] 设备数据可以正确接收和保存

---

**所有配置和工具已准备完成！** ✅

**下一步**: 
1. 运行 `.\CheckCriticalPorts.ps1` 查看当前状态
2. 启动系统并观察控制台输出
3. 再次运行检查脚本验证端口状态

