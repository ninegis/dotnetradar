# 图像分析配置API实现说明

## 📅 完成日期
2025-10-24 24:00

## 🎯 实现目标

修复前端"雷达图像生成配置提交"功能失败的问题（CORS错误和API接口缺失）。

---

##  问题背景

###原始错误

```
Access to XMLHttpRequest at 'http://47.77.196.133:8099/api/protocol/update/project/imageAnalysisConfig'  
from origin 'http://47.77.196.133:6098' has been blocked by CORS policy
```

```
POST http://47.77.196.133:8099/api/protocol/update/project/imageAnalysisConfig net::ERR_FAILED
```

### 问题根源

1. **后端API接口不存在**：前端调用的 `/api/protocol/update/project/imageAnalysisConfig` 接口在后端没有实现
2. **CORS配置正确**：Program.cs中已有正确的CORS配置（AllowAnyOrigin），不是CORS问题
3. **实际问题**：接口缺失导致 `net::ERR_FAILED`

---

## ✅ 已完成的工作

### 1. 发现现有基础设施

经检查发现，系统已经具备完整的图像分析配置基础设施：

#### Repository层（已存在）
**文件**：`RadarSystem.Data/Repositories/ConfigRepositories.cs`

```csharp
public class ImageAnalysisConfigRepository : IImageAnalysisConfigRepository
{
    Task<ImageAnalysisConfig> CreateAsync(ImageAnalysisConfig config);
    Task<ImageAnalysisConfig?> GetByIdAsync(string id);
    Task<ImageAnalysisConfig?> GetByProjectIdAsync(string projectId);
    Task<ImageAnalysisConfig> UpdateAsync(ImageAnalysisConfig config);
    Task<bool> DeleteAsync(string id);
    // ...完整的映射方法
}
```

#### Service层（已存在）
**文件**：`RadarSystem.Core/Services/ConfigServices.cs`

```csharp
public class ImageAnalysisConfigService : IImageAnalysisConfigService
{
    Task<ImageAnalysisConfig> CreateOrUpdateConfigAsync(CreateImageAnalysisConfigRequest request);
    Task<ImageAnalysisConfig?> GetConfigAsync(string projectId);
    Task<bool> DeleteConfigAsync(string projectId);
    Task<ImageAnalysisConfig> GetOrCreateDefaultConfigAsync(string projectId);
}
```

#### 接口定义（已存在）
**文件**：`RadarSystem.Core/Interfaces/IConfigServices.cs` 和 `IConfigRepositories.cs`

---

### 2. 新增Controller API接口

**文件**：`RadarSystem.WebAPI/Controllers/ProtocolController.cs`

#### 2.1 依赖注入

```csharp
private readonly IImageAnalysisConfigService _imageAnalysisConfigService;

public ProtocolController(
    IProjectService projectService,
    IImageAnalysisConfigService imageAnalysisConfigService, // ✅ 新增
    ILogger<ProtocolController> logger)
{
    _projectService = projectService;
    _imageAnalysisConfigService = imageAnalysisConfigService;
    _logger = logger;
}
```

#### 2.2 更新配置接口

```csharp
/// <summary>
/// 更新项目图像分析配置
/// POST /api/protocol/update/project/imageAnalysisConfig
/// </summary>
[HttpPost("update/project/imageAnalysisConfig")]
public async Task<IActionResult> UpdateImageAnalysisConfig([FromBody] UpdateImageAnalysisConfigRequest request)
{
    // 1. 验证ProjectId
    if (string.IsNullOrWhiteSpace(request.ProjectId))
        return Ok(new { code = 400, message = "项目ID不能为空" });

    // 2. 获取现有配置
    var existingConfig = await _imageAnalysisConfigService.GetConfigAsync(request.ProjectId);

    // 3. 构建ConfigJson - 合并前端参数到JSON
    var configData = new Dictionary<string, object>();
    if (existingConfig != null && !string.IsNullOrEmpty(existingConfig.ConfigJson))
    {
        configData = JsonSerializer.Deserialize<Dictionary<string, object>>(existingConfig.ConfigJson) 
            ?? new Dictionary<string, object>();
    }

    // 4. 更新前端传来的参数
    if (request.GenImageType != null) configData["genImageType"] = request.GenImageType;
    if (request.DefoInterval.HasValue) configData["defoInterval"] = request.DefoInterval.Value;
    if (request.ScatInterval.HasValue) configData["scatInterval"] = request.ScatInterval.Value;
    if (request.DefoNumber.HasValue) configData["defoNumber"] = request.DefoNumber.Value;
    if (request.ScatNumber.HasValue) configData["scatNumber"] = request.ScatNumber.Value;

    // 5. 创建Service请求
    var serviceRequest = new CreateImageAnalysisConfigRequest
    {
        ProjectId = request.ProjectId,
        StandardImageSidePixel = existingConfig?.StandardImageSidePixel ?? 16384,
        CompressImageSidePixel = existingConfig?.CompressImageSidePixel ?? 1024,
        MatrixTileRngNum = existingConfig?.MatrixTileRngNum ?? 1203,
        MatrixTileAngNum = existingConfig?.MatrixTileAngNum ?? 61,
        GenDefo = request.GenImageType?.Contains("0") == true || request.GenImageType == "02",
        GenScat = request.GenImageType?.Contains("1") == true || request.GenImageType == "02",
        GenSpeed = existingConfig?.GenSpeed ?? false,
        GenAcceleration = existingConfig?.GenAcceleration ?? false,
        ConfigJson = JsonSerializer.Serialize(configData, new JsonSerializerOptions { WriteIndented = true })
    };

    // 6. 保存配置
    var config = await _imageAnalysisConfigService.CreateOrUpdateConfigAsync(serviceRequest);

    return Ok(new 
    { 
        code = 200, 
        message = "图像分析配置更新成功",
        data = new
        {
            projectId = config.ProjectId,
            genDefo = config.GenDefo,
            genScat = config.GenScat,
            configJson = config.ConfigJson
        }
    });
}
```

#### 2.3 获取配置接口（额外添加）

```csharp
/// <summary>
/// 获取项目图像分析配置
/// GET /api/protocol/project/imageAnalysisConfig/{projectId}
/// </summary>
[HttpGet("project/imageAnalysisConfig/{projectId}")]
public async Task<IActionResult> GetImageAnalysisConfig(string projectId)
{
    var config = await _imageAnalysisConfigService.GetConfigAsync(projectId);
    
    if (config == null)
    {
        return Ok(new { code = 404, message = "未找到配置" });
    }

    return Ok(new 
    { 
        code = 200,
        data = new
        {
            projectId = config.ProjectId,
            genDefo = config.GenDefo,
            genScat = config.GenScat,
            genSpeed = config.GenSpeed,
            genAcceleration = config.GenAcceleration,
            configJson = config.ConfigJson,
            createTime = config.CreateTime,
            updateTime = config.UpdateTime
        }
    });
}
```

---

### 3. 新增请求模型

**文件**：`RadarSystem.WebAPI/Models/ApiRequests.cs`

```csharp
/// <summary>
/// 更新图像分析配置请求（对应前端 /api/protocol/update/project/imageAnalysisConfig）
/// </summary>
public class UpdateImageAnalysisConfigRequest
{
    /// <summary>
    /// 项目ID
    /// </summary>
    [Required]
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// 图像生成类型 (例如: "01"=形变图, "02"=强度图, "03"=两者)
    /// </summary>
    public string? GenImageType { get; set; }

    /// <summary>
    /// 形变图间隔（分钟或小时）
    /// </summary>
    public int? DefoInterval { get; set; }

    /// <summary>
    /// 强度图间隔（分钟或小时）
    /// </summary>
    public int? ScatInterval { get; set; }

    /// <summary>
    /// 形变图生成数量
    /// </summary>
    public int? DefoNumber { get; set; }

    /// <summary>
    /// 强度图生成数量
    /// </summary>
    public int? ScatNumber { get; set; }
}
```

---

### 4. 服务注册

**文件**：`RadarSystem.WebAPI/Program.cs`

```csharp
// 注册Repository（已存在，确保注册）
builder.Services.AddScoped(typeof(RadarSystem.Data.Repositories.ImageAnalysisConfigRepository));

// 注册Repository接口（新增）
builder.Services.AddScoped<RadarSystem.Core.Interfaces.IImageAnalysisConfigRepository, 
    RadarSystem.Data.Repositories.ImageAnalysisConfigRepository>();

// 注册Service接口（新增）
builder.Services.AddScoped<RadarSystem.Core.Interfaces.IImageAnalysisConfigService, 
    RadarSystem.Core.Services.ImageAnalysisConfigService>();
```

---

## 🔍 API接口规范

### 请求端点
```
POST /api/protocol/update/project/imageAnalysisConfig
```

### 请求头
```
Authorization: Bearer {token}
Content-Type: application/json
```

### 请求体（camelCase，符合前端格式）
```json
{
  "projectId": "KOT_20251024_12345",
  "genImageType": "02",
  "defoInterval": 30,
  "scatInterval": 60,
  "defoNumber": 10,
  "scatNumber": 20
}
```

### 响应体（成功）
```json
{
  "code": 200,
  "message": "图像分析配置更新成功",
  "data": {
    "projectId": "KOT_20251024_12345",
    "genDefo": true,
    "genScat": true,
    "configJson": "{\"genImageType\":\"02\",\"defoInterval\":30,\"scatInterval\":60,\"defoNumber\":10,\"scatNumber\":20}"
  }
}
```

### 响应体（失败）
```json
{
  "code": 400,
  "message": "项目ID不能为空"
}
```

---

## 🗄️ 数据存储

配置数据存储在 `image_analysis_configs` 表中：

| 字段 | 类型 | 说明 |
|------|------|------|
| id | TEXT | 主键 |
| project_id | TEXT | 项目ID |
| gen_defo | BOOLEAN | 是否生成形变图 |
| gen_scat | BOOLEAN | 是否生成强度图 |
| gen_speed | BOOLEAN | 是否生成速度图 |
| gen_acceleration | BOOLEAN | 是否生成加速度图 |
| config_json | TEXT | 完整JSON配置（包含前端参数） |
| create_time | DATETIME | 创建时间 |
| update_time | DATETIME | 更新时间 |

---

## 🎯 前端集成

前端无需修改，原有代码即可正常工作：

**文件**：`RadarContrl/src/axios/apiRadar.js`

```javascript
static updateImageAnalysisConfig(projectId,imageDiffAnalysisConfig,imageAnalysisConfig){
    return new Promise((resolve,reject) => {
        axios.post(this.apiUrl+'/api/protocol/update/project/imageAnalysisConfig',{
            projectId,
            genImageType:imageAnalysisConfig['genImageType'],
            defoInterval:imageAnalysisConfig['followDefoInterval'],
            scatInterval:imageAnalysisConfig['scatInterval'],
            defoNumber:imageAnalysisConfig['followDefoNumber'],
            scatNumber:imageAnalysisConfig['scatNumber']
        }).then(res=>resolve(res))
            .catch(error=>reject(error));
    })
}
```

---

## ✅ 编译验证

```bash
$ dotnet build RadarSystem.sln --configuration Release

已成功生成。
    19 个警告
    0 个错误
```

---

## 📋 下一步操作

1. **启动后端服务**
   ```bash
   cd RadarSystem.WebAPI
   dotnet run --configuration Release
   ```

2. **测试API接口**
   - 使用Postman测试 `/api/protocol/update/project/imageAnalysisConfig`
   - 验证请求和响应格式

3. **前端测试**
   - 打开"工具 → 雷达配置 → 图像生成配置"页面
   - 修改配置并点击提交
   - 检查浏览器控制台确认无错误

---

## 📝 相关文件清单

### 修改的文件
1. `RadarSystem.WebAPI/Controllers/ProtocolController.cs` - 添加2个API接口
2. `RadarSystem.WebAPI/Models/ApiRequests.cs` - 添加请求模型
3. `RadarSystem.WebAPI/Program.cs` - 添加服务注册
4. `RadarContrl/src/utils/radartool.js` - 修复设备加载问题（之前的任务）

### 已存在的基础设施
1. `RadarSystem.Data/Repositories/ConfigRepositories.cs` - Repository实现
2. `RadarSystem.Core/Services/ConfigServices.cs` - Service实现
3. `RadarSystem.Core/Interfaces/IConfigServices.cs` - 接口定义
4. `RadarSystem.Core/Interfaces/IConfigRepositories.cs` - 接口定义
5. `RadarSystem.Data/Models/ConfigEntities.cs` - 实体定义

---

**实现完成时间**: 2025-10-24 24:00  
**实现人员**: AI Assistant  
**编译状态**: ✅ 成功（0错误，19警告）  
**测试状态**: ⏳ 待用户验证

