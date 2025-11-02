# FactoryId=20 圆弧雷达设备接收检查清单

> **设备标识**: FactoryId = 20 (SlaveId = 0x00000014)  
> **端口**: 1030  
> **日期**: 2025-11-02

---

## ✅ 已完成的修复

| 项目 | 状态 | 说明 |
|-----|------|------|
| Netty服务启用 | ✅ | Program.cs已启用后台服务 |
| 圆弧雷达服务器 | ✅ | ArcRadarNettyServer已配置启动 |
| 1030端口监听 | ✅ | 配置Enable=true |
| FactoryId映射 | ✅ | 从API自动加载设备映射 |
| 控制台输出 | ✅ | 实时显示接收和保存数据 |
| ApiPort配置 | ✅ | 修正为8099 |

---

## 🔍 系统检查步骤

### 第1步：确认数据库配置

**检查项目**: SQLite数据库中必须有设备记录

**检查方法**:
1. 启动系统后访问: http://localhost:8099/swagger
2. 执行 GET /api/Device
3. 查找 FactoryId = "20" 的记录

**期望结果**:
```json
{
  "deviceId": "RADAR_001",  // 或其他ID
  "factoryId": "20",        // ⚠️ 必须存在
  "deviceName": "圆弧雷达设备",
  "deviceType": "ARC",
  "projectId": "PROJECT001",
  "ipAddress": "192.168.1.100",
  "port": 1030
}
```

**如果不存在，请添加**:
```
POST http://localhost:8099/api/Device
Body:
{
  "deviceId": "RADAR_001",
  "factoryId": "20",
  "deviceName": "圆弧雷达#1",
  "deviceType": "ARC",
  "projectId": "PROJECT001",
  "ipAddress": "192.168.1.100",
  "port": 1030,
  "longitude": 120.123456,
  "latitude": 30.123456,
  "elevation": 100.0
}
```

---

### 第2步：启动系统

**方法1**: 使用启动脚本
```batch
启动雷达系统并监测1030端口.bat
```

**方法2**: 手动启动
```powershell
cd RadarSystem.WebAPI
dotnet run --configuration Release
```

**观察启动日志**:
```
✅ [INF] 正在启动圆弧雷达 Netty 服务器，端口: 1030
✅ [INF] 圆弧雷达 Netty 服务器启动成功，监听端口: 1030
✅ [INF] ✅ 圆弧雷达服务器启动成功 - 端口: 1030
✅ [INF] 正在从API加载设备映射...
✅ [INF] 加载设备映射: FactoryId=20 → DeviceId=RADAR_001
✅ [INF] 设备映射加载完成，共X个设备
```

---

### 第3步：检查端口监听

```powershell
netstat -ano | findstr ":1030"
```

**期望输出**:
```
TCP    0.0.0.0:1030     0.0.0.0:0    LISTENING    12345
```

---

### 第4步：等待设备数据

当FactoryId=20的设备连接并发送数据时，控制台会显示：

```
================================================================================
【数据接收】时间: 2025-11-02 XX:XX:XX.XXX
【数据接收】端口: 1030
【数据接收】唯一值(FactoryId/SlaveId): 20
【数据接收】命令代码: 0xXXXX
【数据接收】数据长度: XXXXX 字节
【数据接收】原始数据(HEX): 5A5A00000014...
【设备映射】FactoryId: 20 → DeviceId: XXXXX
================================================================================
```

---

### 第5步：验证文件保存

```powershell
# 查看数据目录
Get-ChildItem -Path "data\project\PROJECT001\radar\" -Recurse -Directory

# 查看最新保存的文件
Get-ChildItem -Path "data\project\PROJECT001\radar\" -Recurse -File | 
  Sort-Object LastWriteTime -Descending | 
  Select-Object FullName, @{Name="Size(KB)";Expression={[math]::Round($_.Length/1KB,2)}}, LastWriteTime -First 5
```

**期望输出**:
```
FullName                                                          Size(KB)  LastWriteTime
--------                                                          --------  -------------
C:\kotradar2025\dotnetradar\data\project\PROJECT001\radar\...    1024.00   2025-11-02 18:36:15
```

---

## 🔄 完整数据流（实例）

### 设备发送数据包

```
时间: 2025-11-02 18:36:15.234
设备: FactoryId = 20
数据包:
  5A5A            (命令头)
  00000014        (SlaveId = 20)
  0302            (命令 = 形变数据)
  00100000        (数据长度 = 1MB)
  [1MB二进制数据]
  CHECKSUM
```

### 服务器处理流程

```
18:36:15.234 - 接收TCP数据包
             ↓
18:36:15.235 - 解析协议头
             ├─ SlaveId: 20
             ├─ Command: 0x0302
             └─ DataLength: 1048576
             ↓
18:36:15.236 - 控制台输出接收信息 ✅
             ↓
18:36:15.237 - 查询映射
             ├─ _deviceIdMap["20"]
             └─ 返回: "RADAR_001"
             ↓
18:36:15.238 - 控制台输出映射结果 ✅
             ↓
18:36:15.239 - 进入HandleUpstreamCommand()
             ├─ case "0302": 形变数据
             └─ HandleImageData("20", "RADAR_001", "00", "形变", data)
             ↓
18:36:15.240 - 生成文件路径
             └─ "../../data/project/PROJECT001/radar/RADAR_001"
             ↓
18:36:15.241 - 创建队列对象ArcRadarImage
             ├─ SlaveId: "20"
             ├─ DeviceId: "RADAR_001"
             ├─ DataType: "00"
             ├─ TypeName: "形变"
             └─ Data: [1MB]
             ↓
18:36:15.242 - 入队列
             └─ _imageQueue.Enqueue()
             ↓
             
--- 异步处理 ---

18:36:15.345 - ProcessImageQueue()取出数据
             ↓
18:36:15.346 - SaveRadarImage()
             ├─ 创建目录
             ├─ 生成文件名: 00_20251102183615.dat
             └─ 完整路径: .../RADAR_001/00_20251102183615.dat
             ↓
18:36:15.456 - File.WriteAllBytesAsync()
             └─ 写入1MB数据到磁盘
             ↓
18:36:15.457 - 控制台输出保存信息 ✅
             ↓
18:36:15.458 - 完成
```

---

## 📋 启动前检查清单

- [ ] 已编译项目（`dotnet build -c Release`）
- [ ] 已停止现有进程
- [ ] 数据库中已配置FactoryId=20的设备
- [ ] appsettings.json中ArcRadar.Enable=true
- [ ] appsettings.json中ArcRadar.Port=1030
- [ ] appsettings.json中ArcRadar.ApiPort="8099" ✅

---

## 🚀 立即启动

```batch
# 双击运行
启动雷达系统并监测1030端口.bat
```

系统启动后：
1. 等待10-15秒
2. 观察控制台是否显示: `✅ 圆弧雷达服务器启动成功 - 端口: 1030`
3. 观察控制台是否显示: `加载设备映射: FactoryId=20 → DeviceId=...`
4. 等待设备（FactoryId=20）连接并发送数据
5. 查看控制台数据接收输出 ✅
6. 查看控制台文件保存输出 ✅

---

## 📊 监测数据

### 实时监控命令

```powershell
# 监控日志文件
Get-Content RadarSystem.WebAPI\logs\radar-system*.log -Tail 100 -Wait

# 过滤FactoryId=20的数据
Get-Content RadarSystem.WebAPI\logs\radar-system*.log -Tail 200 | Select-String "FactoryId:20|SlaveId: 20"

# 检查文件保存
Get-ChildItem "data\project\PROJECT001\radar\*\*.dat" -Recurse | 
  Sort-Object LastWriteTime -Descending | 
  Select-Object Directory, Name, Length, LastWriteTime -First 10
```

---

## 🎯 成功标志

当系统正常工作时，您会看到：

### 控制台输出（每次接收数据）
```
================================================================================
【数据接收】时间: 2025-11-02 18:36:15.234
【数据接收】端口: 1030
【数据接收】唯一值(FactoryId/SlaveId): 20
...
================================================================================
```

### 文件系统（持续增长）
```
data\project\PROJECT001\radar\RADAR_001\
├── 00_20251102183615.dat  (1024 KB)
├── 01_20251102183715.dat  (1024 KB)
├── 02_20251102183815.dat  (1024 KB)
└── ...
```

### 端口监听（持续存在）
```
TCP    0.0.0.0:1030    0.0.0.0:0    LISTENING
```

---

**准备就绪！运行启动脚本即可开始接收FactoryId=20的圆弧雷达数据。** 🚀

