# 立即操作指南 - 圆弧雷达端口监测

> **目标**: 启动系统并监测FactoryId=20的圆弧雷达数据  
> **端口**: 1030  
> **预计时间**: 5分钟

---

## 🚀 快速开始（3步）

### 第1步：停止现有进程
```powershell
Get-Process | Where-Object {$_.ProcessName -eq "dotnet"} | Stop-Process -Force
```

### 第2步：启动系统（前台运行）
```powershell
cd C:\kotradar2025\dotnetradar\RadarSystem.WebAPI
dotnet run --configuration Release
```

### 第3步：等待并观察（15秒）

**5秒后应看到**:
```
================================================================================
【圆弧雷达服务器】开始启动...
================================================================================
配置: Enable=True, Port=1030, ProjectId=PROJECT001
正在创建ArcRadarNettyServer实例...
正在启动Netty服务器，端口: 1030...
```

**10-15秒后应看到**:
```
╔══════════════════════════════════════════════════════════════════════════════╗
║ ✅ 圆弧雷达 服务器启动成功！
║   监听端口: 1030
║   项目ID: PROJECT001
║   数据路径: ../..
║   等待设备连接...
╚══════════════════════════════════════════════════════════════════════════════╝
```

---

## ✅ 成功标志

### 端口监听检查（另开PowerShell）

```powershell
netstat -ano | findstr ":1030"
```

**期望输出**:
```
TCP    0.0.0.0:1030    0.0.0.0:0    LISTENING    12345
```

### 使用监测脚本

```powershell
cd C:\kotradar2025\dotnetradar
.\端口监测PowerShell脚本.ps1
```

**期望看到**:
```
【雷达设备】
  ✅ 圆弧雷达 (端口1030) - DAG圆弧雷达
  ...
```

---

## 📡 设备数据接收（FactoryId=20）

当设备连接并发送数据时，控制台会显示：

```
================================================================================
【圆弧雷达数据接收】
  时间: 2025-11-02 XX:XX:XX.XXX
  端口: 1030
  唯一值(FactoryId/SlaveId): 20
  命令代码: 0x0302
  数据长度: 1,048,576 字节 (1024.00 KB)
  原始数据(HEX): 5A5A000000140302...
  设备映射: FactoryId(20) → DeviceId(RADAR_001)
================================================================================

********************************************************************************
【圆弧雷达文件保存】
  时间: 2025-11-02 XX:XX:XX.XXX
  设备ID: RADAR_001
  FactoryId: 20
  数据类型: 形变 (00)
  文件路径: ../../data/project/PROJECT001/radar/RADAR_001/00_XXXXXXXXXXXX.dat
  文件大小: 1024.00 KB
  状态: ✅ 保存成功
********************************************************************************
```

---

## ❌ 如果失败

### 如果看到错误信息

```
❌❌❌ 圆弧雷达服务器启动失败！❌❌❌
   错误: XXXX
```

请：
1. 复制完整的错误信息
2. 检查是否端口被占用
3. 检查日志文件

### 如果什么都没显示

可能原因：
1. Task.Run 的代码未执行（异步问题）
2. 配置读取失败
3. 异常被静默捕获

解决：
1. 查看日志文件最后200行
2. 使用测试程序：`dotnet run --project 测试Netty服务启动.csproj`

---

## 📁 关键文件位置

```
C:\kotradar2025\dotnetradar\
├── appsettings.json (端口配置)
├── RadarSystem.WebAPI\
│   ├── Program.cs (直接启动代码在第1222行)
│   ├── Data\radar.db (设备数据库)
│   └── logs\ (日志目录)
├── 端口监测PowerShell脚本.ps1 (监测工具)
├── 启动雷达系统并监测1030端口.bat (启动脚本)
├── 所有雷达端口监测配置.json (配置文档)
└── 所有雷达端口监测配置_中文.md (中文文档)
```

---

## 🆘 快速诊断命令

```powershell
# 1. 检查端口
netstat -ano | findstr "1030 6098 8099"

# 2. 检查进程
Get-Process | Where-Object {$_.ProcessName -eq "dotnet"}

# 3. 查看最新日志
Get-Content RadarSystem.WebAPI\logs\radar-api-*.txt -Tail 50

# 4. 查看保存的文件
Get-ChildItem data\project\PROJECT001\radar\ -Recurse -File | 
  Sort-Object LastWriteTime -Descending | Select-Object -First 5

# 5. 测试API
Invoke-WebRequest "http://localhost:8099/api/Device" -Headers @{"Authorization"="Bearer YOUR_TOKEN"}
```

---

## 📞 需要帮助？

如果遇到问题，请提供：
1. 控制台完整输出（从启动到错误）
2. 端口检查结果 (`netstat -ano | findstr ":1030"`)
3. 最新日志文件的最后100行
4. appsettings.json中的Netty.ArcRadar配置

---

**立即执行**：
```powershell
cd C:\kotradar2025\dotnetradar\RadarSystem.WebAPI
dotnet run --configuration Release
```

**等待看到绿色的✅标记！** 🎯

