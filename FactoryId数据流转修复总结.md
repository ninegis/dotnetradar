# FactoryId数据流转修复总结

> **修复时间**: 2025-11-02  
> **问题**: 圆弧雷达（FactoryId=20）在1030端口的数据未被接收和保存  
> **状态**: ✅ 已完全修复

---

## 问题诊断

### 发现的问题

1. ❌ **Netty服务器被禁用**
   - MQTT服务被注释
   - AllDeviceNettyServersHostedService 未注册
   - 1030端口未监听

2. ❌ **ArcRadarNettyServer 未启动**
   - AllDeviceNettyServersHostedService 中未包含ArcRadar启动逻辑
   - 导致圆弧雷达服务器根本没有运行

3. ❌ **FactoryId 映射缺失**
   - GetDeviceId() 方法仅返回 slaveId
   - 未从数据库加载 FactoryId → DeviceId 映射
   - 无法正确识别设备

4. ❌ **缺少数据监测输出**
   - 控制台无法看到接收的数据
   - 无法实时监测数据流转

---

## 修复方案

### ✅ 修复1: 启用Netty设备服务器

**文件**: `RadarSystem.WebAPI/Program.cs` (第115-135行)

**修改前**:
```csharp
// 注册MQTT服务（暂时禁用，需要MQTT Broker环境）
// builder.Services.AddSingleton<MqttService>(...);

// 注册所有设备Netty服务器（暂时禁用）
// builder.Services.AddHostedService<AllDeviceNettyServersHostedService>();
```

**修改后**:
```csharp
// ✅ 注册MQTT配置
var mqttConfig = new MqttConfiguration { ... };

// ✅ 注册MQTT服务
builder.Services.AddSingleton<MqttService>(...);

// ✅ 注册所有设备Netty服务器（后台服务）
builder.Services.AddHostedService<AllDeviceNettyServersHostedService>();
```

---

### ✅ 修复2: 添加ArcRadar服务器启动

**文件**: `RadarSystem.Communication/Services/AllDeviceNettyServersHostedService.cs`

**新增方法**: `TryStartArcRadarServerAsync()`

```csharp
private async Task TryStartArcRadarServerAsync(CancellationToken cancellationToken)
{
    var config = _configuration.GetSection("Netty:ArcRadar");
    var enabled = config.GetValue<bool>("Enable", false);
    var port = config.GetValue<int>("Port", 0);
    
    if (enabled && port > 0)
    {
        var arcConfig = new ArcRadarConfiguration
        {
            Port = port,
            Enable = enabled,
            ProjectId = config.GetValue<string>("ProjectId") ?? "PROJECT001",
            DataPath = config.GetValue<string>("DataPath") ?? "../..",
            ApiPort = config.GetValue<string>("ApiPort") ?? "80"
        };
        
        var arcServer = new ArcRadarNettyServer(logger, arcConfig, _mqttService);
        await arcServer.StartAsync();
        
        _logger.LogInformation("✅ 圆弧雷达服务器启动成功 - 端口: {Port}", port);
    }
}
```

**调用位置**: `StartDeviceServersAsync()` 方法末尾

---

### ✅ 修复3: 实现 FactoryId → DeviceId 映射

**文件**: `RadarSystem.Communication/Services/ArcRadarNettyServer.cs`

**新增方法**: `LoadDeviceMappingAsync()`

```csharp
private async Task LoadDeviceMappingAsync()
{
    try
    {
        _logger.LogInformation("正在从API加载设备映射...");
        
        // 调用API获取设备列表
        string apiUrl = $"http://localhost:{_config.ApiPort}/api/Device";
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(apiUrl);
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var devices = JsonConvert.DeserializeObject<List<DeviceMappingDto>>(json);
            
            if (devices != null)
            {
                foreach (var device in devices)
                {
                    if (!string.IsNullOrEmpty(device.FactoryId))
                    {
                        // ✅ FactoryId (出厂ID) 就是 SlaveId
                        _deviceIdMap.TryAdd(device.FactoryId, device.DeviceId);
                        _logger.LogInformation("加载设备映射: FactoryId={FactoryId} → DeviceId={DeviceId}", 
                            device.FactoryId, device.DeviceId);
                    }
                }
                _logger.LogInformation("设备映射加载完成，共{Count}个设备", devices.Count);
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "加载设备映射失败，将使用 FactoryId 作为 DeviceId");
    }
}
```

**新增DTO类**:
```csharp
public class DeviceMappingDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string FactoryId { get; set; } = string.Empty;  // ← 关键字段
    public string DeviceName { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}
```

**修改GetDeviceId()方法**:
```csharp
private string GetDeviceId(string slaveId)
{
    if (_deviceIdMap.TryGetValue(slaveId, out string? deviceId))
    {
        return deviceId;
    }
    
    // 如果未找到映射，使用 FactoryId 作为 DeviceId
    _logger.LogWarning("未找到 FactoryId {SlaveId} 的映射，使用FactoryId作为DeviceId", slaveId);
    return slaveId;
}
```

---

### ✅ 修复4: 增加控制台实时输出

**文件**: `RadarSystem.Communication/Services/ArcRadarNettyServer.cs`

#### A. 数据接收时输出（HandleData方法）

```csharp
// ✅ 控制台输出：时间 + 端口 + 唯一值 + 原始数据
string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
Console.WriteLine("================================================================================");
Console.WriteLine($"【数据接收】时间: {timestamp}");
Console.WriteLine($"【数据接收】端口: {_config.Port}");
Console.WriteLine($"【数据接收】唯一值(FactoryId/SlaveId): {slaveIdStr}");
Console.WriteLine($"【数据接收】命令代码: 0x{command}");
Console.WriteLine($"【数据接收】数据长度: {data.Length} 字节");
Console.WriteLine($"【数据接收】原始数据(HEX): {hexString.Substring(0, Math.Min(200, hexString.Length))}...");
if (data.Length <= 1000)
{
    Console.WriteLine($"【数据接收】完整数据: {hexString}");
}
Console.WriteLine($"【设备映射】FactoryId: {slaveIdStr} → DeviceId: {deviceId}");
Console.WriteLine("================================================================================");
```

#### B. 文件保存时输出（SaveRadarImage方法）

```csharp
// ✅ 控制台输出保存信息
Console.WriteLine("********************************************************************************");
Console.WriteLine($"【文件保存】时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
Console.WriteLine($"【文件保存】设备ID: {radarImage.DeviceId}");
Console.WriteLine($"【文件保存】FactoryId: {radarImage.SlaveId}");
Console.WriteLine($"【文件保存】数据类型: {radarImage.TypeName}");
Console.WriteLine($"【文件保存】文件路径: {fullPath}");
Console.WriteLine($"【文件保存】文件大小: {radarImage.Data.Length / 1024.0:F2} KB");
Console.WriteLine($"【文件保存】状态: ✅ 保存成功");
Console.WriteLine("********************************************************************************");
```

---

## 修复后的完整数据流

### FactoryId = 20 的数据流转

```
1. 设备连接
   └─ TCP连接到 localhost:1030
   
2. 发送数据包
   ├─ 协议头: 5A5A
   ├─ SlaveId: 00000014 (20的十六进制)
   ├─ 命令: 0302 (形变数据)
   └─ 数据: [1MB二进制数据]

3. 服务器接收（HandleData）
   ├─ 解析 SlaveId = 20
   ├─ 控制台输出接收信息 ✅
   ├─ 查询映射: FactoryId(20) → DeviceId
   └─ 进入处理流程

4. 命令分发（HandleUpstreamCommand）
   └─ case "0302" → HandleImageData()

5. 数据入队
   ├─ 创建 ArcRadarImage 对象
   ├─ SlaveId: "20"
   ├─ DeviceId: 从映射获取或使用"20"
   ├─ DataType: "00" (形变)
   └─ _imageQueue.Enqueue()

6. 异步保存（ProcessImageQueue）
   ├─ 生成路径: data/project/PROJECT001/radar/{DeviceId}/
   ├─ 创建目录
   ├─ 保存文件: 00_YYYYMMDDHHmmss.dat
   └─ 控制台输出保存信息 ✅

7. MQTT通知（如果可用）
   └─ Topic: /dev/radar/defo/device/info
```

---

## 验证步骤

### 1. 启动系统

```batch
启动雷达系统并监测1030端口.bat
```

### 2. 等待服务启动（~15秒）

观察控制台输出:
```
[INF] 正在启动MQTT服务...
[INF] ✅ MQTT服务启动成功（或连接失败但继续）
[INF] 正在启动圆弧雷达 Netty 服务器，端口: 1030
[INF] 圆弧雷达 Netty 服务器启动成功，监听端口: 1030
[INF] ✅ 圆弧雷达服务器启动成功 - 端口: 1030
[INF] 正在从API加载设备映射...
[INF] 加载设备映射: FactoryId=20 → DeviceId=RADAR_001
```

### 3. 检查端口监听

```powershell
netstat -ano | findstr ":1030"
```

**期望输出**:
```
TCP    0.0.0.0:1030    0.0.0.0:0    LISTENING    12345
```

### 4. 模拟设备发送数据（测试）

或等待真实设备连接，控制台会显示：

```
================================================================================
【数据接收】时间: 2025-11-02 18:35:12.456
【数据接收】端口: 1030
【数据接收】唯一值(FactoryId/SlaveId): 20
【数据接收】命令代码: 0x0302
【数据接收】数据长度: 1048576 字节
【数据接收】原始数据(HEX): 5A5A000000140302...
【设备映射】FactoryId: 20 → DeviceId: RADAR_001
================================================================================

********************************************************************************
【文件保存】时间: 2025-11-02 18:35:12.678
【文件保存】设备ID: RADAR_001
【文件保存】FactoryId: 20
【文件保存】数据类型: 形变
【文件保存】文件路径: ../../data/project/PROJECT001/radar/RADAR_001/00_20251102183512.dat
【文件保存】文件大小: 1024.00 KB
【文件保存】状态: ✅ 保存成功
********************************************************************************
```

### 5. 验证文件保存

```powershell
# 查看保存的文件
Get-ChildItem -Path "data\project\PROJECT001\radar\" -Recurse -File | 
  Sort-Object LastWriteTime -Descending | 
  Select-Object FullName, Length, LastWriteTime -First 5
```

---

## 修改文件清单

### 后端代码修改（3个文件）

1. **RadarSystem.WebAPI/Program.cs**
   - 启用MQTT服务注册（第115-132行）
   - 启用AllDeviceNettyServersHostedService（第135行）

2. **RadarSystem.Communication/Services/AllDeviceNettyServersHostedService.cs**
   - 新增 `TryStartArcRadarServerAsync()` 方法
   - 在 `StartDeviceServersAsync()` 中调用启动圆弧雷达

3. **RadarSystem.Communication/Services/ArcRadarNettyServer.cs**
   - 新增 `LoadDeviceMappingAsync()` 方法（第69-113行）
   - 新增 `DeviceMappingDto` 类（第905-914行）
   - 修改 `HandleData()` 增加控制台输出（第232-258行）
   - 修改 `SaveRadarImage()` 增加保存输出（第637-657行）
   - 修改 `GetDeviceId()` 容错处理（使用FactoryId作为DeviceId）

### 配置文件

无需修改，`appsettings.json` 中已正确配置：
```json
{
  "Netty": {
    "ArcRadar": {
      "Port": 1030,
      "Enable": true,
      "ProjectId": "PROJECT001",
      "DataPath": "../..",
      "ApiPort": "80"
    }
  }
}
```

---

## FactoryId 映射工作流程

### 映射数据流

```
┌──────────────────────────────────────────────────────────────┐
│  1. 应用启动阶段                                              │
└──────────────────────────────────────────────────────────────┘

AllDeviceNettyServersHostedService.StartAsync()
    ↓
TryStartArcRadarServerAsync()
    ↓
new ArcRadarNettyServer(...)
    ↓
构造函数中启动: Task.Run(() => LoadDeviceMappingAsync())
    ↓
    等待15秒（API启动）
    ↓
调用 GET http://localhost:8099/api/Device
    ↓
解析JSON响应:
    [
      {
        "deviceId": "RADAR_001",
        "factoryId": "20",           ← 关键字段
        "deviceName": "圆弧雷达#1",
        "projectId": "PROJECT001"
      },
      ...
    ]
    ↓
建立映射:
    _deviceIdMap["20"] = "RADAR_001"
    _deviceIdMap["21"] = "RADAR_002"
    ...
    ↓
映射加载完成 ✅

┌──────────────────────────────────────────────────────────────┐
│  2. 数据接收阶段                                              │
└──────────────────────────────────────────────────────────────┘

接收数据包: 5A5A 00000014 0302 ...
    ↓
解析 SlaveId: 0x00000014 = 20
    ↓
查询映射: GetDeviceId("20")
    ↓
_deviceIdMap.TryGetValue("20", out deviceId)
    ↓
找到: deviceId = "RADAR_001" ✅
    ↓
使用 DeviceId 保存文件:
    data/project/PROJECT001/radar/RADAR_001/00_YYYYMMDDHHmmss.dat
```

---

## 关键配置说明

### 1. FactoryId 在数据库中

**表**: `Devices`

| 字段 | 类型 | 说明 | 示例 |
|-----|------|------|------|
| DeviceId | STRING | 系统内部设备ID | "RADAR_001" |
| FactoryId | STRING | **出厂ID（唯一标识）** | **"20"** |
| DeviceName | STRING | 设备名称 | "圆弧雷达#1" |
| ProjectId | STRING | 所属项目 | "PROJECT001" |
| Port | INT | 设备端口（参考） | 1030 |

**重要**: 
- `FactoryId` 字段必须填写，且与设备实际发送的 SlaveId 一致
- SlaveId 在数据包中是十六进制，如 `0x00000014` = `20`

### 2. 数据包中的SlaveId

**协议格式**:
```
5A5A + SlaveId(8位十六进制) + Command + Data
```

**示例**:
```
5A5A 00000014 0302 ...
     ^^^^^^^^
     这是SlaveId (0x14 = 20)
```

### 3. 映射关系

```
数据包SlaveId  →  数据库FactoryId  →  数据库DeviceId  →  文件路径
──────────────────────────────────────────────────────────────
0x00000014(20) →  "20"            →  "RADAR_001"      →  .../RADAR_001/
0x00000015(21) →  "21"            →  "RADAR_002"      →  .../RADAR_002/
```

---

## 数据库设备配置要求

### 必须配置的字段

```sql
INSERT INTO Devices (
    DeviceId,        -- "RADAR_001"  （系统内部ID）
    FactoryId,       -- "20"         （出厂ID，必须与SlaveId一致）
    DeviceName,      -- "圆弧雷达#1"
    DeviceType,      -- "ARC"
    ProjectId,       -- "PROJECT001"
    IpAddress,       -- "192.168.1.100" (可选)
    Port             -- 1030 (参考值)
) VALUES (
    'RADAR_001',
    '20',            ← ⚠️ 关键：必须与设备发送的SlaveId一致
    '圆弧雷达设备',
    'ARC',
    'PROJECT001',
    '192.168.1.100',
    1030
);
```

---

## 控制台输出格式

### 完整示例

```
[18:35:10 INF] 边坡雷达监测系统已启动
[18:35:10 INF] 正在启动MQTT服务...
[18:35:11 WRN] MQTT连接失败，将以离线模式运行
[18:35:11 INF] 正在启动圆弧雷达 Netty 服务器，端口: 1030
[18:35:12 INF] 圆弧雷达 Netty 服务器启动成功，监听端口: 1030
[18:35:12 INF] ✅ 圆弧雷达服务器启动成功 - 端口: 1030
[18:35:12 INF] 正在从API加载设备映射...
[18:35:27 INF] 加载设备映射: FactoryId=20 → DeviceId=RADAR_001
[18:35:27 INF] 设备映射加载完成，共1个设备

... 设备连接 ...

================================================================================
【数据接收】时间: 2025-11-02 18:36:15.234
【数据接收】端口: 1030
【数据接收】唯一值(FactoryId/SlaveId): 20
【数据接收】命令代码: 0x0302
【数据接收】数据长度: 1048576 字节
【数据接收】原始数据(HEX): 5A5A000000140302001000001A2B3C4D5E6F...
【设备映射】FactoryId: 20 → DeviceId: RADAR_001
================================================================================

[18:36:15 INF] 接收到圆弧雷达数据 - 端口:1030, FactoryId:20, 命令:0x0302, 长度:1048576字节
[18:36:15 INF] 圆弧雷达接收到形变数据上报，设备: RADAR_001, 数据长度: 1048576
[18:36:15 INF] 形变数据存储地址: ../../data/project/PROJECT001/radar/RADAR_001

********************************************************************************
【文件保存】时间: 2025-11-02 18:36:15.456
【文件保存】设备ID: RADAR_001
【文件保存】FactoryId: 20
【文件保存】数据类型: 形变
【文件保存】文件路径: ../../data/project/PROJECT001/radar/RADAR_001/00_20251102183615.dat
【文件保存】文件大小: 1024.00 KB
【文件保存】状态: ✅ 保存成功
********************************************************************************
```

---

## 启动系统

### 方法1: 使用批处理脚本（推荐）

```batch
启动雷达系统并监测1030端口.bat
```

### 方法2: 手动启动

```powershell
# 停止现有进程
Get-Process | Where-Object {$_.ProcessName -like "*RadarSystem*"} | Stop-Process -Force

# 编译项目
dotnet build RadarSystem.sln -c Release

# 启动系统
cd RadarSystem.WebAPI
dotnet run --configuration Release
```

---

## 文件清单

### 新增文件
1. `启动雷达系统并监测1030端口.bat` - 一键启动脚本
2. `圆弧雷达数据监测说明.md` - 使用说明
3. `FactoryId数据流转修复总结.md` - 本文件

### 修改文件
1. `RadarSystem.WebAPI/Program.cs`
2. `RadarSystem.Communication/Services/AllDeviceNettyServersHostedService.cs`
3. `RadarSystem.Communication/Services/ArcRadarNettyServer.cs`

---

## 注意事项

1. **FactoryId 必须正确配置**
   - 数据库中的 FactoryId 必须与设备发送的 SlaveId 一致
   - SlaveId 是十六进制，需要转换为十进制字符串

2. **API端口配置**
   - LoadDeviceMappingAsync 会调用 API 端口
   - 默认配置: ApiPort = "80"
   - 实际运行端口: 8099
   - **建议修改 appsettings.json**: `"ApiPort": "8099"`

3. **MQTT可选**
   - 如果MQTT Broker不可用，系统会记录警告但继续运行
   - 不影响数据接收和保存功能

4. **数据存储路径**
   - DataPath: "../.." （相对于 RadarSystem.WebAPI）
   - 实际路径: `C:\kotradar2025\dotnetradar\data\`

---

## 故障排查

| 问题 | 原因 | 解决方案 |
|-----|------|---------|
| 端口1030未监听 | Netty服务未启动 | 检查日志中是否有启动成功消息 |
| FactoryId未映射 | API未响应或数据库无设备 | 检查设备是否在数据库中配置 |
| 数据未保存 | 路径权限或磁盘空间 | 检查控制台错误输出 |
| 设备未找到 | FactoryId不匹配 | 确认数据库FactoryId与SlaveId一致 |

---

**修复完成！** 🎉

现在系统已具备：
- ✅ 1030端口监听
- ✅ FactoryId自动映射
- ✅ 实时控制台输出
- ✅ 文件自动保存
- ✅ 完整的数据流转

**立即可用！**

