# API接口清单 - 边坡雷达监测系统

## 📊 接口统计

| 分类 | 数量 | 状态 |
|------|------|------|
| 项目管理 | 5 | ⏳ 待实现 |
| 设备管理 | 4 | ⏳ 待实现 |
| 监测位置 | 3 | ⏳ 待实现 |
| 告警规则 | 6 | ⏳ 待实现 |
| 告警联系人 | 6 | ⏳ 待实现 |
| 告警记录 | 2 | ⏳ 待实现 |
| 雷达控制 | 3 | ⏳ 待实现 |
| 雷达参数 | 7 | ⏳ 待实现 |
| 雷达图像 | 6 | ⏳ 待实现 |
| 数据管理 | 3 | ⏳ 待实现 |
| 系统配置 | 10 | ⏳ 待实现 |
| 用户认证 | 2 | ✅ 已实现 |
| **总计** | **57** | |

---

## 🔗 API基础配置

### 当前配置（前端）
```javascript
// RadarContrl/src/axios/baseapi.js
const manVehicleSysApiUrl = 'https://data.kotiot.cn/api/';      // ❌ 远程
const ucmlSysUrl = 'http://8.140.201.145:6081/basic-api/';      // ❌ 远程
const manVehiclSysWebSocketUrl = 'wss://data.kotiot.cn/wss/';   // ❌ 远程
```

### 需要修改为（本地开发）
```javascript
const manVehicleSysApiUrl = 'http://localhost:8099/api/';       // ✅ 本地
const manVehiclSysWebSocketUrl = 'ws://localhost:8099/ws/';     // ✅ 本地
// ucmlSysUrl 可以继续使用远程（用户管理系统）
```

### C#后端配置
```csharp
// RadarSystem.WebAPI/Program.cs
app.MapControllers(); // API路由前缀：/api/

// 端口配置
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(8099); // API服务端口
});
```

---

## 📝 接口详细清单

---

## 1️⃣ 项目管理 (Project)

### 1.1 获取项目列表
```
GET /api/project/list
```
**前端调用：**
```javascript
// RadarContrl/src/axios/apiRadar.js:11
ApiRadar.getRadarData()
```
**C#实现：**
```csharp
// RadarSystem.WebAPI/Controllers/ProjectController.cs
[HttpGet("list")]
public async Task<IActionResult> GetProjectList()
```
**返回值：**
```json
{
  "code": 200,
  "data": [
    {
      "projectId": "string",
      "projectName": "string",
      "description": "string",
      "contact": "string",
      "phone": "string",
      "email": "string",
      "lon": 0.0,
      "lat": 0.0,
      "alt": 0.0,
      "startTime": "2025-01-01",
      "endTime": "2025-12-31"
    }
  ]
}
```

---

### 1.2 添加项目
```
POST /api/protocol/add/project
```
**前端调用：**
```javascript
// apiRadar.js:629
ApiRadar.addProject(projectId, projectName, projectDescribe, contact, phone, email, lon, lat, alt)
```
**请求参数：**
```json
{
  "projectId": "string",
  "projectName": "string",
  "projectDescribe": "string",
  "contact": "string",
  "phone": "string",
  "email": "string",
  "lon": 0.0,
  "lat": 0.0,
  "alt": 0.0,
  "startTime": "",
  "endTime": ""
}
```
**C#实现：**
```csharp
[HttpPost("protocol/add/project")]
public async Task<IActionResult> AddProject([FromBody] ProjectDto dto)
```

---

### 1.3 更新项目信息
```
POST /api/custom/updateProjectInfo
```
**前端调用：**
```javascript
// apiRadar.js:81
ApiRadar.updateProjectInfo(projectId, name, description, contact, phone, email)
```
**请求参数：**
```json
{
  "projectId": "string",
  "name": "string",
  "description": "string",
  "contact": "string",
  "phone": "string",
  "email": "string"
}
```

---

### 1.4 删除项目
```
POST /api/protocol/remove/project?projectId={projectId}
```
**前端调用：**
```javascript
// apiRadar.js:645
ApiRadar.DeleteProject(projectId)
```

---

### 1.5 设置项目视角
```
POST /api/protocol/set/project/view
```
**前端调用：**
```javascript
// apiRadar.js:74
ApiRadar.addCameraParams(projectId, lon, lat, alt, heading, pitch, roll)
```
**请求参数：**
```json
{
  "projectId": "string",
  "lon": 0.0,
  "lat": 0.0,
  "alt": 0.0,
  "heading": 0.0,
  "pitch": 0.0,
  "roll": 0.0
}
```

---

## 2️⃣ 设备管理 (Device)

### 2.1 添加设备
```
POST /api/protocol/add/device
```
**前端调用：**
```javascript
// apiRadar.js:622
ApiRadar.addDevice(projectId, deviceName, deviceId, slaveId, ori, type, lon, lat, alt, ipv4)
```
**请求参数：**
```json
{
  "projectId": "string",
  "deviceName": "string",
  "deviceId": "string",
  "slaveId": "string",
  "ori": "string",
  "type": "string",
  "lon": 0.0,
  "lat": 0.0,
  "alt": 0.0,
  "ipv4": "string"
}
```

---

### 2.2 删除设备
```
POST /api/protocol/remove/device?deviceId={deviceId}
```
**前端调用：**
```javascript
// apiRadar.js:638
ApiRadar.DeleteDevice(deviceId)
```

---

### 2.3 获取设备最后心跳时间
```
GET /api/radar/lastheartbeat?url={url}&deviceId={deviceId}
```
**前端调用：**
```javascript
// apiRadar.js:705
ApiRadar.GetRadarLastHeartbeatTime(url, deviceId)
```

---

### 2.4 获取设备在线状态
```
GET /api/radar/lastonline?url={url}&deviceId={deviceId}&datetime={datetime}
```
**前端调用：**
```javascript
// apiRadar.js:698
ApiRadar.GetRadarOnlineStatusByTime(url, deviceId, datetime)
```

---

## 3️⃣ 监测位置管理 (GeoMark)

### 3.1 添加监测位置
```
POST /api/protocol/add/geo
```
**前端调用：**
```javascript
// apiRadar.js:16
ApiRadar.addMonitoringLocation(data, enableShieldArea)
```
**请求参数：**
```json
{
  "id": "string",
  "projectId": "string",
  "alarmLevel": 0,
  "visible": true,
  "name": "string",
  "type": "GEO-POINT | GEO-AREA",
  "devices": ["deviceId"],
  "coordinates": [lon, lat],
  "defoComputingMethod": 0,
  "enableData": true,
  "enableAlarmArea": false,
  "enableSlope": false,
  "enableShieldArea": false,
  "slopeValue": 2.0,
  "weightValue": 50.0,
  "direction": 1
}
```

---

### 3.2 删除监测位置
```
GET /api/protocol/remove/geo/{id}/{projectId}
```
**前端调用：**
```javascript
// apiRadar.js:41
ApiRadar.deleteMonitor(id, projectid)
```

---

## 4️⃣ 告警规则 (AlarmRule)

### 4.1 获取告警规则列表
```
GET /api/protocol/query/ruleBatch/{projectId}
```
**前端调用：**
```javascript
// apiRadar.js:69
ApiRadar.getAlarmRule(projectId)
```

---

### 4.2 添加告警规则
```
POST /api/protocol/add/ruleBatch
```
**前端调用：**
```javascript
// apiRadar.js:206
ApiRadar.addAlarmRule(data)
```
**请求参数（复杂）：**
```json
{
  "projectId": "string",
  "id": "UUID",
  "ruleName": "string",
  "ruleDescription": "string",
  "alarmRule": "> | < | >= | <=",
  "enable": true,
  "devices": ["deviceId"],
  "geoMarkArray": ["geoMarkId"],
  "dataSource": "10",
  "targetFlag": true,
  "alarmTargetThresholds": [
    {
      "name": "蓝色预警",
      "level": 1,
      "flag": true,
      "targetCheckbox": [
        {
          "label": "位移",
          "value": 10.0,
          "flag": true,
          "timeUnit": "",
          "target": "displacement"
        },
        {
          "label": "速度",
          "value": 5.0,
          "flag": true,
          "timeUnit": "03",
          "target": "speed"
        },
        {
          "label": "加速度",
          "value": 2.0,
          "flag": true,
          "timeUnit": "03",
          "target": "acceleration"
        }
      ]
    }
  ]
}
```

---

### 4.3 更新告警规则
```
POST /api/protocol/update/ruleBatch
```
**前端调用：**
```javascript
// apiRadar.js:339
ApiRadar.updateAlarmRule(data)
```
**请求参数：**同添加告警规则

---

### 4.4 删除告警规则
```
GET /api/protocol/remove/ruleBatch/{id}/{projectId}
```
**前端调用：**
```javascript
// apiRadar.js:472
ApiRadar.deleteAlarmRule(id, projectId)
```

---

## 5️⃣ 告警联系人 (AlarmContact)

### 5.1 获取联系人列表
```
GET /api/protocol/query/contact/{projectId}
```
**前端调用：**
```javascript
// apiRadar.js:524
ApiRadar.getAlarmContact(projectId)
```

---

### 5.2 添加联系人
```
POST /api/protocol/add/contact
```
**前端调用：**
```javascript
// apiRadar.js:480
ApiRadar.addAlarmContact(name, email, phone, alarmlevel, enable, projectId)
```
**请求参数：**
```json
{
  "id": "UUID",
  "name": "string",
  "email": "string",
  "phone": "string",
  "alarmLevel": 0,
  "enable": true,
  "projectId": "string"
}
```

---

### 5.3 更新联系人
```
POST /api/protocol/update/contact
```
**前端调用：**
```javascript
// apiRadar.js:509
ApiRadar.updateAlarmContact(id, name, email, phone, alarmlevel, enable, projectId)
```

---

### 5.4 删除联系人
```
GET /api/protocol/remove/contact/{id}/{projectId}
```
**前端调用：**
```javascript
// apiRadar.js:548
ApiRadar.deleteAlarmContact(id, projectId)
```

---

### 5.5 更新短信配置
```
POST /api/protocol/update/smsConfig
```
**前端调用：**
```javascript
// apiRadar.js:556
ApiRadar.addAlarmMessage(params)
```

---

## 6️⃣ 告警记录 (AlarmRecord)

### 6.1 查询告警记录数量
```
POST /api/alarmNotify/recordList/count
```
**前端调用：**
```javascript
// apiRadar.js:112
ApiRadar.queryAlarmRecordCount(params)
```

---

### 6.2 查询告警记录列表
```
POST /api/alarmNotify/recordList/count
```
**前端调用：**
```javascript
// apiRadar.js:117
ApiRadar.queryAlarmRecord(params)
```

---

## 7️⃣ 雷达控制 (RadarControl)

### 7.1 发送雷达指令
```
GET /api/{arcsar|mimoLite}/command/{projectId}/{deviceId}/{command}/{userName}
```
**前端调用：**
```javascript
// apiRadar.js:48
ApiRadar.controlRadar(projectId, deviceId, command, userName)
```
**支持的指令：**
- 11: 参数控制

---

### 7.2 设置雷达参数控制（ArcSAR）
```
POST /api/arcsar/command/{projectId}/{deviceId}/11/qingqiangjia
```
**前端调用：**
```javascript
// apiRadar.js:579
ApiRadar.setParamControl(projectId, deviceId)
```

---

### 7.3 设置雷达参数控制（MIMO Lite）
```
POST /api/mimoLite/command/{projectId}/{deviceId}/11/qingqiangjia
```
**前端调用：**
```javascript
// apiRadar.js:586
ApiRadar.setPushiRadarParamControl(projectId, deviceId)
```

---

## 8️⃣ 雷达参数 (RadarParams)

### 8.1 更新雷达基础参数
```
POST /api/protocol/update/radar/param
```
**前端调用：**
```javascript
// apiRadar.js:102
ApiRadar.updateRadarParams(params)
```

---

### 8.2 更新MIMO Lite雷达参数
```
POST /api/protocol/update/radar/mimolite/param
```
**前端调用：**
```javascript
// apiRadar.js:107
ApiRadar.updatePushiRadarParams(params)
```

---

### 8.3 更新雷达算法参数
```
POST /api/protocol/update/radar/algoparam
```
**前端调用：**
```javascript
// apiRadar.js:183
ApiRadar.updateRadarAlgorithmParam(params)
```

---

### 8.4 更新MIMO Lite算法参数
```
POST /api/protocol/update/radar/mimolite/algoparam
```
**前端调用：**
```javascript
// apiRadar.js:177
ApiRadar.updatePushiRadarAlgorithmParam(params)
```

---

### 8.5 更新速度目标
```
POST /api/protocol/update/speed/target
```
**前端调用：**
```javascript
// apiRadar.js:189
ApiRadar.updateSpeedTarget(projectId, timeUnit)
```

---

### 8.6 更新色标
```
POST /api/protocol/update/colorBar
```
**前端调用：**
```javascript
// apiRadar.js:196
ApiRadar.updateColorBar(params)
```

---

### 8.7 更新危险区域
```
POST /api/protocol/update/hidden/analysis
```
**前端调用：**
```javascript
// apiRadar.js:201
ApiRadar.updateDangerArea(params)
```

---

## 9️⃣ 雷达图像 (RadarImage)

### 9.1 查询图像数量
```
POST /api/sar/image/count
```
**前端调用：**
```javascript
// apiRadar.js:122
ApiRadar.queryImageCount(projectId, deviceId, startDateTime, endDateTime, type, status)
```
**请求参数：**
```json
{
  "projectId": "string",
  "devId": "string",
  "startDateTime": "2025-01-01T00:00:00",
  "endDateTime": "2025-01-31T23:59:59",
  "status": 0,
  "type": 0,
  "pageRowSize": 5
}
```

---

### 9.2 查询图像列表
```
POST /api/sar/image/list
```
**前端调用：**
```javascript
// apiRadar.js:161
ApiRadar.queryImageList(projectId, deviceId, startDateTime, endDateTime, type, status, count)
```
**请求参数：**
```json
{
  "projectId": "string",
  "devId": "string",
  "startDateTime": "2025-01-01T00:00:00",
  "endDateTime": "2025-01-31T23:59:59",
  "status": 0,
  "type": 0,
  "pageRowSize": 10,
  "page": 1
}
```

---

### 9.3 生成雷达图像
```
POST /api/sar/generate/image
```
**前端调用：**
```javascript
// apiRadar.js:144
ApiRadar.generateRadarImage(deviceId, duration, fileName, projectId, sequence, status, timeUnit, ts, type)
```
**请求参数：**
```json
{
  "deviceId": "string",
  "duration": 0,
  "fileName": "string",
  "projectId": "string",
  "sequence": 0,
  "status": 0,
  "timeUnit": "string",
  "ts": 0,
  "type": 0
}
```

---

### 9.4 获取图像资源
```
GET /api/{url}{filename}
```
**前端调用：**
```javascript
// apiRadar.js:137
ApiRadar.getImageResource(url, filename)
```

---

### 9.5 更新图像分析配置
```
POST /api/protocol/update/project/imageAnalysisConfig
```
**前端调用：**
```javascript
// apiRadar.js:89
ApiRadar.updateImageAnalysisConfig(projectId, imageDiffAnalysisConfig, imageAnalysisConfig)
```
**请求参数：**
```json
{
  "projectId": "string",
  "genImageType": 0,
  "defoInterval": 0,
  "scatInterval": 0,
  "defoNumber": 0,
  "scatNumber": 0
}
```

---

## 🔟 数据管理 (DataManage)

### 10.1 数据恢复
```
POST /api/rollback/validate/geo/device
```
**前端调用：**
```javascript
// apiRadar.js:666
ApiRadar.DataRestore(projectId, deviceId, geoMaskId, geoMaskType, startTime, endTime)
```
**请求参数：**
```json
{
  "projectId": "string",
  "deviceId": "string",
  "geoMaskId": "string",
  "geoMaskType": "string",
  "startTime": "2025-01-01T00:00:00",
  "endTime": "2025-01-31T23:59:59",
  "rollbackStatus": "unstart",
  "dataType": "10",
  "deleteStatus": "false"
}
```

---

### 10.2 数据生成
```
POST /api/radar/generatedatabyinterval
```
**URL：** `http://218.4.141.234:25599` （独立服务器）

**前端调用：**
```javascript
// apiRadar.js:681
ApiRadar.DataGenerate(url, projectId, deviceId, startTime, endTime, interval, maxValue, minValue, markId, target, currentValue)
```

---

## 1️⃣1️⃣ 系统配置 (SystemConfig)

### 11.1 获取磁盘存储信息
```
GET /api/datastorage/query/discSpace
```
**前端调用：**
```javascript
// apiRadar.js:55
ApiRadar.getDiskStorage()
```

---

### 11.2 获取磁盘阈值配置
```
GET /api/config/info
```
**前端调用：**
```javascript
// apiRadar.js:62
ApiRadar.getDiskThreshold()
```

---

### 11.3 更新磁盘存储配置
```
POST /api/custom/updateDiskStorage
```
**前端调用：**
```javascript
// apiRadar.js:561
ApiRadar.updateDiskStorage(discSpacePercentage, deleteFile)
```
**请求参数：**
```json
{
  "discSpacePercentage": 80,
  "deleteFile": true
}
```

---

### 11.4 更新倾斜电机俯仰角
```
POST /api/custom/updateTiltMotorPitch
```
**前端调用：**
```javascript
// apiRadar.js:571
ApiRadar.updateTiltMotorPitch(projectId, deviceId, pitch)
```

---

### 11.5 添加图层
```
GET /api/addlayer
```
**URL：** `http://8.140.201.145:6086/sloperadar`

**前端调用：**
```javascript
// apiRadar.js:593
ApiRadar.addLayer(oid, name, type, url, userid, postid, divisionid, orgid, treeid)
```

---

### 11.6 删除图层
```
GET /api/deletelayer?oid={oid}
```

---

### 11.7 启用/禁用图层
```
GET /api/enablelayer?oid={oid}&enable={enable}
```

---

### 11.8 显示/隐藏图层
```
GET /api/showlayer?oid={oid}&show={show}
```

---

### 11.9 获取图层
```
GET /api/getlayer?orgid={orgid}
```

---

### 11.10 添加操作日志
```
POST /api/server/addradaroperatelog
```
**URL：** `http://218.4.141.234:25559`

**前端调用：**
```javascript
// apiRadar.js:655
ApiRadar.AddRadarLog(operate_content, operate_username, address, project_code, project_name)
```

---

## 1️⃣2️⃣ 用户认证 (Auth) ✅ 已实现

### 12.1 用户登录
```
POST /api/auth/login
```
**C#实现：**
```csharp
// RadarSystem.WebAPI/Controllers/AuthController.cs
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
```
**请求参数：**
```json
{
  "username": "admin",
  "password": "admin123"
}
```
**返回值：**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGc...",
    "user": {
      "id": 1,
      "username": "admin",
      "email": "admin@example.com"
    }
  }
}
```

---

### 12.2 获取当前用户信息
```
GET /api/auth/me
```
**C#实现：**
```csharp
[HttpGet("me")]
[Authorize]
public async Task<IActionResult> GetCurrentUser()
```

---

## 📚 其他第三方API

### 高德地图API

#### 获取省份列表
```
GET https://restapi.amap.com/v3/config/district?key={key}
```

#### 获取城市列表
```
GET https://restapi.amap.com/v3/config/district?key={key}&keywords={adcode}
```

#### 获取街道列表
```
GET https://restapi.amap.com/v3/config/district?key={key}&keywords={adcode}
```

---

## 🎯 API实现优先级

### Phase 1: 核心功能（高优先级）⭐⭐⭐
```
✅ 用户认证（已完成）
⏳ 项目管理（5个接口）
⏳ 设备管理（4个接口）
⏳ 监测位置管理（3个接口）
```

### Phase 2: 告警功能（高优先级）⭐⭐⭐
```
⏳ 告警规则（6个接口）
⏳ 告警联系人（6个接口）
⏳ 告警记录（2个接口）
```

### Phase 3: 雷达控制（中优先级）⭐⭐
```
⏳ 雷达控制（3个接口）
⏳ 雷达参数（7个接口）
⏳ 雷达图像（6个接口）
```

### Phase 4: 数据管理（中优先级）⭐⭐
```
⏳ 数据管理（3个接口）
```

### Phase 5: 系统配置（低优先级）⭐
```
⏳ 系统配置（10个接口）
```

---

## 📝 C#后端实现建议

### Controller组织结构
```
RadarSystem.WebAPI/Controllers/
├── AuthController.cs              ✅ 已实现（用户认证）
├── ProjectController.cs           ⏳ 项目管理
├── DeviceController.cs            ⏳ 设备管理
├── GeoMarkController.cs           ⏳ 监测位置
├── AlarmRuleController.cs         ⏳ 告警规则
├── AlarmContactController.cs      ⏳ 告警联系人
├── AlarmRecordController.cs       ⏳ 告警记录
├── RadarControlController.cs      ⏳ 雷达控制
├── RadarParamsController.cs       ⏳ 雷达参数
├── RadarImageController.cs        ⏳ 雷达图像
├── DataManageController.cs        ⏳ 数据管理
└── SystemConfigController.cs      ⏳ 系统配置
```

---

## 🔍 接口测试工具

### Postman Collection
接口已导出为Postman Collection格式（见下一个文件）

### Swagger文档
访问：`http://localhost:8099/swagger`

### API测试步骤
1. 启动后端：`cd RadarSystem.WebAPI && dotnet run`
2. 访问Swagger：http://localhost:8099/swagger
3. 测试登录接口
4. 获取Token
5. 在后续请求中添加 `Authorization: Bearer {token}` 头

---

## 📊 总结

| 类别 | 统计 |
|------|------|
| **总接口数** | 57 |
| **已实现** | 2 (用户认证) |
| **待实现** | 55 |
| **高优先级** | 20 (项目、设备、监测位置、告警) |
| **中优先级** | 19 (雷达控制、参数、图像、数据管理) |
| **低优先级** | 10 (系统配置) |
| **第三方API** | 6 (高德地图等) |

---

## 🚀 下一步行动

1. ✅ **修改前端API地址**
   ```javascript
   // RadarContrl/src/axios/baseapi.js
   const manVehicleSysApiUrl = 'http://localhost:8099/api/';
   ```

2. ⏳ **实现高优先级接口**
   - ProjectController（项目管理）
   - DeviceController（设备管理）
   - GeoMarkController（监测位置）

3. ⏳ **实现告警功能**
   - AlarmRuleController
   - AlarmContactController
   - AlarmRecordController

4. ⏳ **实现雷达控制**
   - RadarControlController
   - RadarParamsController
   - RadarImageController

---

**接口清单已完成！可用于指导后端API开发！** 🎯

