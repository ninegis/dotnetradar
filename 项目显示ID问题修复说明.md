# 项目显示ID问题修复说明

## 问题描述
前端登录后首页中"当前项目"和"选择项目"显示的是ID序号，而不是项目名称。

## 问题原因
**字段命名不匹配**：后端返回的是PascalCase命名（`ProjectId`、`ProjectName`），前端期望的是camelCase命名（`id`、`name`）。

## 解决方案

### 1. 后端修改（Program.cs）
在 `RadarSystem.WebAPI\Program.cs` 中添加了JSON序列化配置，将所有返回的JSON字段转换为camelCase：

```csharp
// 配置JSON序列化：使用camelCase命名（前端JavaScript标准）
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true; // 便于调试
    });
```

**效果**：
- `ProjectId` → `projectId`
- `ProjectName` → `projectName`
- `DeviceId` → `deviceId`
- `DeviceName` → `deviceName`
- 所有其他字段同理

### 2. 前端修改
更新了所有使用项目和设备数据的组件，将字段名从小写改为camelCase：

#### 修改的文件：
1. **Header.vue**
   - 项目选择：`item.id` → `item.projectId`，`item.name` → `item.projectName`
   - 设备显示：`item.name` → `item.deviceName`

2. **CesiumContainer.vue**
   - 默认选中项目：`res.data.data[0].id` → `res.data.data[0].projectId`

3. **ProjectConfig.vue**
   - 项目选择：`item.id` → `item.projectId`，`item.name` → `item.projectName`
   - 设备列表：`item.id` → `item.deviceId`，`item.name` → `item.deviceName`
   - 查找项目：`p.id` → `p.projectId`
   - 删除后默认选中：`projectData[0].id` → `projectData[0].projectId`

4. **DeviceList.vue**
   - 项目选择：`item.id` → `item.projectId`，`item.name` → `item.projectName`
   - 默认选中：`projectData[0].id` → `projectData[0].projectId`

5. **DeviceConfig.vue**
   - 项目选择：`item['id']` → `item['projectId']`，`item['name']` → `item['projectName']`

6. **DeviceEdit.vue**
   - 项目选择：`item.id` → `item.projectId`，`item.name` → `item.projectName`

7. **ProjectAdd.vue**
   - 创建后默认选中：`projectData[0].id` → `projectData[0].projectId`

## 数据结构对比

### 修改前（后端返回PascalCase）
```json
{
  "code": 200,
  "data": [
    {
      "Id": 1,
      "ProjectId": "PRJ001",
      "ProjectName": "测试项目",
      "Devices": [
        {
          "Id": 1,
          "DeviceId": "RADAR001",
          "DeviceName": "雷达1"
        }
      ]
    }
  ]
}
```

### 修改后（后端返回camelCase）
```json
{
  "code": 200,
  "data": [
    {
      "id": 1,
      "projectId": "PRJ001",
      "projectName": "测试项目",
      "devices": [
        {
          "id": 1,
          "deviceId": "RADAR001",
          "deviceName": "雷达1"
        }
      ]
    }
  ]
}
```

## 验证步骤
1. 重新编译后端：`dotnet build RadarSystem.WebAPI\RadarSystem.WebAPI.csproj --configuration Release`
2. 启动系统：运行 `启动雷达系统.bat`
3. 登录系统
4. 检查首页"当前项目"下拉框是否正确显示项目名称
5. 切换项目验证是否正常工作

## 影响范围
- ✅ 项目选择下拉框：显示项目名称而非ID
- ✅ 设备显示：显示设备名称而非ID
- ✅ 所有API返回的JSON字段统一使用camelCase（符合JavaScript规范）
- ✅ 前端代码与后端数据结构完全匹配

## 技术细节
- 使用了ASP.NET Core的 `System.Text.Json.JsonNamingPolicy.CamelCase`
- 符合前端JavaScript的camelCase命名惯例
- 后端C#代码保持PascalCase（C#标准），仅在JSON序列化时转换

## 注意事项
- 所有后端返回的JSON字段都会自动转换为camelCase
- 前端请求中的字段名应使用camelCase
- 数据库层和C#代码层仍然使用PascalCase

