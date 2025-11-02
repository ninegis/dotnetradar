# EMQ MQTT Broker 集成方案

## 当前使用：MQTTnet内置Broker

**优点**：
- .NET原生，无需额外部署
- 轻量级，集成简单
- 支持WebSocket

**缺点**：
- 功能相对简单
- 性能有限

## EMQ (EMQX) 专业方案

### 方案A：使用独立EMQ服务器（推荐）

#### 1. 安装EMQ
```powershell
# 下载EMQX
https://www.emqx.io/downloads

# 或使用Docker
docker run -d --name emqx -p 1883:1883 -p 8083:8083 -p 8084:8084 -p 8883:8883 -p 18083:18083 emqx/emqx:latest
```

#### 2. 修改配置
```json
// appsettings.json
{
  "Mqtt": {
    "BrokerHost": "localhost",  // EMQ地址
    "BrokerPort": 1883,
    "WebSocketPort": 8083,       // EMQ WebSocket端口
    "Username": "",
    "Password": ""
  }
}
```

#### 3. 移除内置Broker
```csharp
// Program.cs - 删除
builder.Services.AddHostedMqttServer(...)
app.UseMqttServer(...)
app.MapMqtt("/wss");

// 保留MqttService（作为客户端连接EMQ）
```

#### 4. 前端连接EMQ
```javascript
// config.js
websocketUrl: 'ws://' + window.location.hostname + ':8083/mqtt'
```

### 方案B：混合方案（当前继续用MQTTnet）

**保持现状**：
- 内置MQTTnet Broker（已集成）
- 适合中小规模部署
- 无需额外安装

**扩展选项**：
- 生产环境可切换到EMQ
- 配置文件控制使用哪个Broker

## 快速集成EMQ

### Step 1: 安装EMQ
```bash
# Windows
emqx-5.x.x-windows-amd64.zip 解压后运行
emqx/bin/emqx start

# 验证
curl http://localhost:18083  # 管理控制台
```

### Step 2: 修改Program.cs
```csharp
// 注释掉MQTTnet Broker
// builder.Services.AddHostedMqttServer(...)

// 只保留MqttService客户端
var mqttConfig = new MqttConfiguration
{
    BrokerHost = "localhost",  // EMQ地址
    BrokerPort = 1883,
    ClientId = "RadarSystem"
};
```

### Step 3: 前端配置
```javascript
// config.js
websocketUrl: 'ws://' + window.location.hostname + ':8083/mqtt'
// EMQ默认WebSocket端口是8083
```

### Step 4: 重启系统测试
```powershell
dotnet run
# 访问 http://localhost:6098
```

## 对比

| 特性 | MQTTnet内置 | EMQ (EMQX) |
|-----|------------|-----------|
| 部署 | 无需额外安装 | 需要单独部署 |
| 性能 | 中等 | 高性能 |
| 功能 | 基础MQTT | 完整MQTT 5.0 |
| 管理界面 | 无 | 完整Dashboard |
| 集群 | 不支持 | 支持 |
| 规则引擎 | 无 | 支持 |
| 适用场景 | 中小规模 | 企业级 |

## 当前建议

**保持MQTTnet内置Broker**：
- 已经工作正常
- 无需额外部署
- 满足当前需求

**未来升级EMQ**：
- 生产环境部署时
- 需要高性能和集群
- 需要规则引擎和管理界面

## 当前状态

✅ MQTTnet Broker已启动
✅ /wss端点可用
✅ 前端mqtt.connect()支持
✅ Port 1030数据接收正常

系统已完全可用！

