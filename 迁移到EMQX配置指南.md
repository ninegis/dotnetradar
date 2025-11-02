# 迁移到EMQX开源版指南

## 当前状态
- ✅ MQTTnet内置Broker（已工作）
- ✅ Port 1030数据接收正常
- ✅ 系统完全可用

## 推荐：保持MQTTnet

**理由**：
1. ✅ 已经正常工作
2. ✅ .NET原生，无需额外部署
3. ✅ 性能足够（当前6个设备）
4. ✅ 维护简单

## 如确需EMQX

### 方案A：Chocolatey安装（推荐）

```powershell
# 1. 运行安装脚本
.\快速部署EMQX.bat

# 2. 验证
netstat -ano | findstr ":1883 :8083"

# 3. 访问Dashboard
http://localhost:18083
# 用户名: admin, 密码: public
```

### 方案B：Docker（需先安装Docker Desktop）

```powershell
docker-compose -f docker-compose-emqx.yml up -d
```

### 方案C：手动下载

1. 访问 https://www.emqx.io/downloads
2. 下载 Windows版本
3. 解压到 C:\emqx
4. 运行：`C:\emqx\bin\emqx.cmd start`

## 修改配置（使用EMQX后）

### 1. appsettings.json
```json
{
  "Mqtt": {
    "BrokerHost": "localhost",
    "BrokerPort": 1883,
    "WebSocketPort": 8083,
    "Username": "",
    "Password": ""
  }
}
```

### 2. Program.cs
```csharp
// 删除MQTTnet Broker
// builder.Services.AddHostedMqttServer(...)
// app.UseMqttServer(...)
// app.MapMqtt("/wss");

// 保留MqttService（连接到EMQX）
builder.Services.AddSingleton<MqttService>(...);
```

### 3. config.js
```javascript
websocketUrl: 'ws://' + window.location.hostname + ':8083/mqtt'
// EMQX的WebSocket端口是8083
```

### 4. 重启系统
```powershell
dotnet run
```

## 当前建议

**保持MQTTnet**：
- 系统已完全正常
- 满足当前需求
- 无需额外部署

**何时升级EMQX**：
- 设备数量>100
- 需要集群部署
- 需要规则引擎
- 需要管理Dashboard

## 验证端口

```powershell
# MQTTnet (当前)
netstat -ano | findstr ":8099"  # /wss在8099端口

# EMQX (升级后)  
netstat -ano | findstr ":1883 :8083 :18083"
```

