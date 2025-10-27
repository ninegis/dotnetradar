# Swagger API文档使用说明

## 📋 访问地址

**Swagger UI**: http://localhost:8099/swagger

## 🎯 Swagger UI界面说明

### 1. **API接口部分**（页面顶部）
这是您需要查看的主要内容，按Controller分组显示所有API接口：

```
Alarm - 告警管理
├── GET  /api/Alarm/records - 查询告警记录
├── POST /api/Alarm/records - 创建告警记录
├── GET  /api/Alarm/statistics - 告警统计
└── PUT  /api/Alarm/records/{handleId}/status - 更新告警处理状态

AlarmRecord - 告警记录管理
├── POST /api/alarmNotify/recordList/count - 查询告警记录数量
└── POST /api/alarmNotify/recordList/list - 查询告警记录列表

Analysis - 图像分析
├── POST /api/Analysis/deformation - 形变分析
├── POST /api/Analysis/scattering - 散射分析
├── POST /api/Analysis/velocity - 速度场分析
└── GET  /api/Analysis/results - 获取分析结果

Auth - 认证管理
├── POST /api/Auth/login - 用户登录
├── POST /api/Auth/logout - 用户登出
└── POST /api/Auth/change-password - 修改密码

Data - 数据管理
├── GET  /api/Data/radar - 获取雷达数据
├── GET  /api/Data/statistics - 数据统计
├── GET  /api/Data/{id}/download - 下载数据
└── GET  /api/Data/quality - 数据质量报告

Device - 设备管理
├── GET    /api/Device - 获取设备列表
├── POST   /api/Device - 创建设备
├── GET    /api/Device/{id} - 获取设备详情
├── PUT    /api/Device/{id} - 更新设备
├── DELETE /api/Device/{id} - 删除设备
└── GET    /api/Device/types - 获取设备类型

Image - 图像管理
├── POST /api/Image/generate-deformation-tiles - 生成形变图像切片
├── POST /api/Image/generate-scattering-tiles - 生成散射图像切片
├── POST /api/Image/generate-velocity-tiles - 生成速度图像切片
└── GET  /api/Image - 获取图像列表

Layer - 图层管理
├── GET /sloperadar/api/addlayer - 添加图层
├── GET /sloperadar/api/deletelayer - 删除图层
├── GET /sloperadar/api/enablelayer - 启用/禁用图层
├── GET /sloperadar/api/showlayer - 显示/隐藏图层
└── GET /sloperadar/api/getlayer - 获取图层列表

Parameter - 参数管理
├── GET /api/Parameter/system - 获取系统参数
├── PUT /api/Parameter/system - 更新系统参数
├── GET /api/Parameter/device/{deviceId} - 获取设备运行参数
├── PUT /api/Parameter/device/{deviceId} - 更新设备运行参数
├── GET /api/Parameter/algorithm/{algorithmType} - 获取算法处理参数
├── PUT /api/Parameter/algorithm/{algorithmType} - 更新算法处理参数
├── GET /api/Parameter/colormap - 获取颜色映射配置
└── PUT /api/Parameter/colormap - 更新颜色映射配置

Project - 项目管理
├── GET    /api/Project - 获取项目列表
├── POST   /api/Project - 创建项目
├── GET    /api/Project/{id} - 获取项目详情
├── PUT    /api/Project/{id} - 更新项目
└── DELETE /api/Project/{id} - 删除项目

RadarDevice - 雷达设备
├── GET  /api/radar/lastheartbeat - 获取雷达最后心跳时间
├── GET  /api/radar/lastonline - 获取雷达在线状态
└── POST /api/radar/generatedatabyinterval - 生成雷达数据

Report - 报表管理
├── GET    /api/Report - 获取报表列表
├── POST   /api/Report/generate - 生成报表
├── GET    /api/Report/{id}/download - 下载报表
├── DELETE /api/Report/{id} - 删除报表
└── GET    /api/Report/templates - 获取报表模板

Rollback - 数据回滚
└── POST /api/rollback/validate/geo/device - 验证并恢复数据

Sar - SAR雷达图像
├── POST /api/sar/image/count - 查询SAR图像数量
├── POST /api/sar/image/list - 查询SAR图像列表
└── POST /api/sar/generate/image - 生成SAR雷达图像
```

### 2. **Schemas部分**（页面底部）
这是数据模型定义，显示所有API使用的请求和响应数据结构。

**Schemas不是接口，而是数据模型！**

例如：
- `AlarmRecord` - 告警记录的数据结构
- `ApiResponse_List_AlarmRecord` - 告警记录列表响应的数据结构
- `LoginRequest` - 登录请求的数据结构
- `Device` - 设备信息的数据结构

## 🔍 如何使用Swagger UI

### 1. **查看API接口**
- 向下滚动到页面顶部的Controller分组
- 点击任意Controller展开其下的所有接口
- 点击具体接口查看详细信息（参数、响应等）

### 2. **测试API接口**
- 点击某个接口展开
- 点击"Try it out"按钮
- 填写必需的参数
- 点击"Execute"执行请求
- 查看响应结果

### 3. **查看数据模型**
- 向下滚动到Schemas部分
- 点击任意Schema查看其数据结构
- 这些模型被API接口引用

## ✅ 验证方式

1. **打开Swagger UI**: http://localhost:8099/swagger
2. **应该看到**：
   - 页面顶部：按Controller分组的所有API接口（约51个接口）
   - 页面底部：Schemas部分显示所有数据模型
3. **如果只看到Schemas**：
   - 向上滚动页面
   - 检查是否有Controller分组显示
   - 刷新浏览器

## 📊 统计信息

- **总接口数**: 51个API端点
- **Controller数**: 14个控制器
- **Schemas数**: 约60个数据模型

## 🚨 常见问题

### Q: 为什么只看到Schemas？
A: Schemas在页面底部。请向上滚动查看实际的API接口。

### Q: Schemas是什么？
A: Schemas是数据模型定义，不是API接口。它们描述了API请求和响应的数据结构。

### Q: 如何找到具体的API接口？
A: 在Swagger UI页面顶部，按Controller名称分组显示（如Alarm、Device、Project等）。

## 🎯 正确的查看顺序

1. 打开 http://localhost:8099/swagger
2. **先看页面顶部** - 这里是所有API接口（按Controller分组）
3. **再看页面底部** - 这里是Schemas（数据模型定义）

---

**最后更新**: 2025-10-23  
**文档版本**: v1.0
