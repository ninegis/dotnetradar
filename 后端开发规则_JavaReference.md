# 后端开发规则 - Java参考实现

## 📋 参考架构说明

本C#后端实现参考了以下Java项目架构：

### 1. 圆弧雷达数据接收模块（ArcSAR）
**位置**: `C:\kotradar2025\3RadarArcsarParse`
**技术栈**: Spring Boot 3.4.1 + Java 17 + Netty 4.1.116 + MQTT
**主要功能**: 圆弧雷达TCP通信、数据解析、MQTT消息发布
**关键类**: 
- `RadarTCPNettyServer` - TCP服务器
- `RadarDecoder` - 数据解码器
- `RadarServerHandler` - 服务器处理器
- `MqttServer` - MQTT服务

### 2. 完整监测系统（除圆弧雷达外的所有功能）
**位置**: `C:\kotradar2025\kotjavrradar`
**技术栈**: Spring Boot 2.6.6 + Java 17 + 多模块Maven项目
**主要模块**:
- `canon-server` - 主服务器
- `canon-device` - 设备管理
- `canon-data-analysis` - 数据分析
- `canon-image-analysis` - 图像分析
- `canon-alarm` - 告警管理
- `canon-radar` - 雷达管理
- `canon-report` - 报表生成
- `canon-mqtt` - MQTT通信
- `canon-dao` - 数据访问层

## 🔄 C#实现对应关系

### 项目结构映射
```
Java Spring Boot          →  C# ASP.NET Core
├── canon-server          →  RadarSystem.WebAPI
├── canon-device          →  RadarSystem.Core (设备管理)
├── canon-data-analysis   →  RadarSystem.Core (数据分析)
├── canon-image-analysis  →  RadarSystem.ImageAnalysis
├── canon-alarm           →  RadarSystem.Alarm
├── canon-radar           →  RadarSystem.Radar
├── canon-report          →  RadarSystem.WebAPI (报表Controller)
├── canon-mqtt            →  RadarSystem.Communication (MQTT)
└── canon-dao            →  RadarSystem.Data
```

### 技术栈对应
| Java技术 | C#对应技术 | 说明 |
|---------|-----------|------|
| Spring Boot | ASP.NET Core | Web框架 |
| Spring Security | ASP.NET Core Identity | 认证授权 |
| Spring Data JPA | Entity Framework Core | ORM框架 |
| Netty | Netty (C#版本) | 网络通信 |
| MQTT Client | MQTTnet | MQTT通信 |
| Maven | NuGet | 包管理 |
| JUnit | xUnit | 单元测试 |

## 🏗️ 架构设计原则

### 1. 分层架构
```
表示层 (Controllers)     →  API接口层
业务层 (Services)       →  业务逻辑层  
数据层 (Repositories)   →  数据访问层
实体层 (Models)        →  数据模型层
```

### 2. 依赖注入
```csharp
// Java Spring方式
@Autowired
private DeviceService deviceService;

// C# ASP.NET Core方式
public DeviceController(IDeviceService deviceService)
{
    _deviceService = deviceService;
}
```

### 3. 配置管理
```csharp
// Java application.yml
// C# appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./Data/radar.db"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForRadarSystem2025"
  }
}
```

## 📡 设备通信架构

### Netty服务器实现
```csharp
// 参考Java: RadarTCPNettyServer
public class DeviceNettyServerBase
{
    protected ILogger _logger;
    protected DeviceNettyConfiguration _config;
    protected MqttService _mqttService;
    
    public async Task StartAsync()
    {
        // 启动Netty服务器
    }
}
```

### 数据处理器
```csharp
// 参考Java: RadarDecoder
public class RadarDataNettyHandler : ChannelHandlerAdapter
{
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        // 处理雷达数据
    }
}
```

## 🗄️ 数据访问层设计

### Repository模式
```csharp
// 参考Java: canon-dao模块
public interface IDeviceRepository
{
    Task<Device> GetByIdAsync(string id);
    Task<List<Device>> GetAllAsync();
    Task<Device> AddAsync(Device device);
    Task UpdateAsync(Device device);
    Task DeleteAsync(string id);
}
```

### 服务层设计
```csharp
// 参考Java: canon-device服务
public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<DeviceService> _logger;
    
    public async Task<Device> CreateDeviceAsync(CreateDeviceRequest request)
    {
        // 业务逻辑实现
    }
}
```

## 🚨 告警系统设计

### 告警规则引擎
```csharp
// 参考Java: canon-alarm模块
public class AlarmRuleService : IAlarmRuleService
{
    public async Task<bool> EvaluateAlarmRuleAsync(AlarmRule rule, RadarData data)
    {
        // 告警规则评估逻辑
    }
}
```

### 实时告警监控
```csharp
// 参考Java: RealtimeAlarmMonitorService
public class RealtimeAlarmMonitorService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 实时监控逻辑
    }
}
```

## 🖼️ 图像分析模块

### 图像处理服务
```csharp
// 参考Java: canon-image-analysis
public class ImageAnalysisService : IImageAnalysisService
{
    public async Task<AnalysisResult> AnalyzeDeformationAsync(byte[] imageData)
    {
        // 形变分析逻辑
    }
}
```

## 📊 数据存储策略

### 主数据库 (SQLite)
```csharp
// 系统配置、用户、项目等结构化数据
public class RadarDbContext : DbContext
{
    public DbSet<Device> Devices { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<AlarmRule> AlarmRules { get; set; }
}
```

### 时序数据库 (TDengine)
```csharp
// 雷达数据、传感器数据等时序数据
public class TDengineRepository : ITDengineRepository
{
    public async Task SaveRadarDataAsync(RadarData data)
    {
        // 保存到时序数据库
    }
}
```

## 🔧 开发规范

### 1. 命名规范
```csharp
// 类名: PascalCase
public class DeviceService { }

// 方法名: PascalCase  
public async Task<Device> GetDeviceAsync() { }

// 属性名: PascalCase
public string DeviceId { get; set; }

// 字段名: camelCase (private)
private readonly ILogger _logger;
```

### 2. 异常处理
```csharp
// 统一异常处理
public async Task<ApiResponse<T>> HandleRequestAsync<T>(Func<Task<T>> operation)
{
    try
    {
        var result = await operation();
        return ApiResponse<T>.Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "操作失败");
        return ApiResponse<T>.Fail(500, ex.Message);
    }
}
```

### 3. 日志记录
```csharp
// 结构化日志
_logger.LogInformation("设备 {DeviceId} 状态更新为 {Status}", deviceId, status);
_logger.LogError(ex, "处理设备数据失败: {DeviceId}", deviceId);
```

## 🚀 部署配置

### 开发环境
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./Data/radar.db"
  },
  "TDengine": {
    "Host": "localhost",
    "Port": 6030
  },
  "Mqtt": {
    "BrokerHost": "localhost",
    "BrokerPort": 1883
  }
}
```

### 生产环境
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db;Database=radar;Trusted_Connection=true"
  },
  "TDengine": {
    "Host": "tdengine-cluster",
    "Port": 6030
  }
}
```

## 📝 代码审查清单

- [ ] 遵循Java参考架构设计
- [ ] 正确实现分层架构
- [ ] 异常处理完善
- [ ] 日志记录规范
- [ ] 单元测试覆盖
- [ ] 性能优化考虑
- [ ] 安全性检查
- [ ] 文档注释完整

## 🔄 迁移指南

### 从Java到C#的关键差异
1. **依赖注入**: Spring的@Autowired → ASP.NET Core的构造函数注入
2. **配置管理**: application.yml → appsettings.json
3. **数据访问**: JPA Repository → EF Core Repository
4. **异步编程**: CompletableFuture → async/await
5. **异常处理**: Spring的@ControllerAdvice → ASP.NET Core的ExceptionFilter

---

**参考项目**: Java Spring Boot实现  
**C#实现**: ASP.NET Core 8.0  
**维护人员**: 后端开发团队  
**最后更新**: 2025-10-23  
**版本**: v1.0
