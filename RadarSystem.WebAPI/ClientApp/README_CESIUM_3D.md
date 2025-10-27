# Cesium 三维边坡雷达监测系统

## 🎯 功能概述

基于 **Cesium** 的三维可视化边坡雷达监测系统，适用于矿区边坡安全监测场景。

## 📦 核心技术栈

- **Vue 3** - 前端框架
- **TypeScript** - 类型安全
- **Cesium 1.111** - 三维地球引擎
- **Element Plus** - UI组件库
- **ECharts** - 数据图表
- **Pinia** - 状态管理
- **Vite** - 构建工具

## 🏗️ 系统架构

### 一、三维场景模块 (`CesiumViewer.vue`)

#### 功能特性
- ✅ 高精度三维地形渲染
- ✅ 卫星影像底图叠加
- ✅ 倾斜摄影模型支持（3D Tiles）
- ✅ BIM模型导入
- ✅ 多种视角控制：
  - 鼠标滚轮：缩放
  - 左键拖拽：平移
  - 右键拖拽：旋转
  - 中键拖拽：倾斜视角

#### 图层管理
- 地形图层（支持透明度调节）
- 影像图层（多种底图切换）
- 三维模型图层（LOD优化）
- 监测点图层（实时状态显示）
- 监测面图层（半透明色带）

#### 工具栏
- 🗂️ 图层控制
- 🔄 视角复位
- 📏 测量工具（距离/面积）
- 📷 场景截图
- 📍 坐标实时显示

### 二、监测要素管理 (`MonitoringPointPanel.vue`)

#### 监测点管理
```typescript
interface MonitoringPoint {
  id: string;
  code: string;        // 如 MP-01
  name: string;        // 如 "1号边坡监测点"
  longitude: number;
  latitude: number;
  altitude: number;
  deviceId: string;
  status: 'normal' | 'warning' | 'alarm' | 'offline';
  displacement: number; // 位移 mm
  velocity: number;     // 速率 mm/h
  threshold: {
    warning: number;
    alarm: number;
  };
}
```

#### 操作功能
- ➕ 添加监测点（手动输入坐标或地图点选）
- ✏️ 编辑监测点（配置阈值、关联设备）
- 🗑️ 删除监测点
- 📁 批量导入（Excel/CSV）
- 🔍 搜索与筛选（按状态、区域）
- 📊 统计看板（正常/预警/报警/离线）

#### 监测面管理
```typescript
interface MonitoringArea {
  id: string;
  name: string;
  type: 'polygon' | 'rectangle' | 'circle';
  coordinates: Array<{longitude, latitude, altitude}>;
  deviceIds: string[];
  status: 'normal' | 'warning' | 'alarm';
}
```

- 📐 KML文件导入
- ✏️ 可视化绘制（多边形/矩形/圆形）
- 🎨 自定义颜色和透明度

### 三、数据详情面板 (`DataDetailPanel.vue`)

#### 实时数据展示
- 📈 累计位移（mm）
- 🚀 位移速率（mm/h）
- ⚡ 加速度（mm/h²）
- 🌡️ 温度、湿度等环境参数

#### 历史趋势图表
- 时间范围选择（日/周/月/自定义）
- 多种数据类型切换：
  - 位移趋势
  - 速率趋势
  - 加速度趋势
- 数据缩放和区域放大
- 导出为图片/Excel

#### 设备信息
- 设备ID、类型、状态
- 创建时间、更新时间
- 数据采集频率

### 四、系统配置模块 (`SystemConfig.vue`)

#### 项目配置
```typescript
- 项目名称：新疆天隆希望矿区
- 项目位置：新疆维吾尔自治区
- 中心坐标：经度87.6278° 纬度43.7928° 高程5000m
- 三维模型URL：3D Tiles格式
- 地形URL：Terrain格式
- 影像URL：自定义瓦片服务
```

#### 设备配置
```typescript
interface DeviceConfig {
  deviceId: string;
  deviceName: string;
  deviceType: string;  // 雷达类型
  port: number;        // 监听端口
  samplingRate: number; // 采样频率 Hz
  enable: boolean;
}
```

#### 算法配置
- **阈值判断**：位移/速率阈值
- **机器学习**：模型路径配置
- **统计分析**：滑动窗口大小

#### 预警配置
```typescript
interface AlarmRule {
  name: string;
  type: 'displacement' | 'velocity' | 'acceleration';
  operator: '>' | '<' | '>=' | '<=';
  threshold: number;
  level: 1 | 2 | 3 | 4;  // 蓝/黄/橙/红
  pointIds: string[];
  smsTemplate: string;
}
```

#### 短信模板
```
【边坡雷达预警】{pointCode} 位移超限，当前位移 {displacement}mm
【边坡雷达报警】{pointCode} 位移严重超限，当前位移 {displacement}mm，请立即处理！

可用变量：
- {pointCode}：监测点编号
- {displacement}：当前位移值
- {velocity}：当前速率
- {timestamp}：触发时间
```

## 🎨 界面布局

```
┌─────────────────────────────────────────────────────────────┐
│  [项目名称]  [在线/离线]     [搜索]  [配置]  [用户]        │ 顶部栏
├───────┬─────────────────────────────────────────┬───────────┤
│       │                                         │           │
│ 监测点│         Cesium 三维场景                │  数据详情 │
│ 列表  │     （地形 + 模型 + 监测点）           │  面板     │
│       │                                         │           │
│ MP-01 │    ┌──────────────────────┐            │  实时数据 │
│ MP-02 │    │  图层控制   工具栏   │            │  历史趋势 │
│ MP-03 │    │  [地形] [影像] [模型]│            │  设备信息 │
│ ...   │    └──────────────────────┘            │           │
│       │         坐标显示（鼠标位置）           │  [导出]   │
│       │                                         │  [确认]   │
└───────┴─────────────────────────────────────────┴───────────┘
  320px              flex: 1                        400px
```

## 🚀 快速开始

### 1. 安装依赖

```bash
cd RadarSystem.WebAPI/ClientApp
npm install
```

### 2. 配置Cesium Token

在 `src/components/CesiumViewer.vue` 中设置您的 Cesium Ion Token：

```typescript
Cesium.Ion.defaultAccessToken = 'your-token-here';
```

获取Token：https://cesium.com/ion/tokens

### 3. 启动开发服务器

```bash
npm run dev
```

访问：http://localhost:5173

### 4. 构建生产版本

```bash
npm run build
```

输出到：`../wwwroot`

## 📊 数据流程

### 实时数据流
```
雷达设备 → DotNetty Server → WebAPI → WebSocket → 前端
                                    ↓
                              TDengine/SQLite
                                    ↓
                               历史数据存储
```

### 前端数据流
```
Pinia Store ← API Service ← Backend
     ↓
 Vue Components
     ↓
 Cesium Viewer（可视化）
```

## 🎯 使用场景

### 场景1：日常监测
1. 打开系统，查看三维场景
2. 监测点以绿色图钉显示（正常状态）
3. 点击监测点查看实时数据和历史趋势
4. 数据每5秒自动刷新

### 场景2：预警处理
1. 监测点位移超过阈值
2. 图钉颜色变为黄色/橙色/红色
3. 弹出预警弹窗
4. 发送短信通知
5. 操作员确认处理

### 场景3：离线模式
1. 网络断开，顶部显示"离线"标签
2. 显示最后缓存的数据
3. 可查看历史趋势图
4. 网络恢复后自动重连

### 场景4：数据导出
1. 选择监测点
2. 设置时间范围
3. 导出为Excel/CSV
4. 包含位移、速率等所有数据

## ⚙️ 高级配置

### LOD（层级细节）优化

```typescript
// 在CesiumViewer中配置
viewer.scene.screenSpaceCameraController.minimumZoomDistance = 100;
viewer.scene.screenSpaceCameraController.maximumZoomDistance = 20000000;

// 3D Tiles模型LOD
tileset.maximumScreenSpaceError = 16; // 降低可提高精度但影响性能
```

### 性能优化建议

1. **模型优化**
   - 使用Cesium ion处理倾斜摄影
   - 生成多级LOD
   - 压缩纹理贴图

2. **数据优化**
   - 分页加载监测点（>100个时）
   - 历史数据按需加载
   - WebSocket代替轮询

3. **渲染优化**
   - 启用抗锯齿
   - 使用GPU加速
   - 限制帧率（30-60fps）

## 📱 多端适配

### PC端（>1280px）
- 三栏布局
- 完整功能
- 最佳体验

### 平板端（768-1280px）
- 左右面板可折叠
- 工具栏简化
- 触摸优化

### 移动端（<768px）
- 单栏布局
- 抽屉式面板
- 手势控制

## 🔧 API接口

### 监测点API
```typescript
GET    /api/monitoring/points        // 获取所有监测点
GET    /api/monitoring/points/:id    // 获取单个监测点
POST   /api/monitoring/points        // 创建监测点
PUT    /api/monitoring/points/:id    // 更新监测点
DELETE /api/monitoring/points/:id    // 删除监测点
```

### 实时数据API
```typescript
GET  /api/monitoring/data/realtime/:pointId       // 单点实时数据
POST /api/monitoring/data/realtime-batch          // 批量实时数据
POST /api/monitoring/data/history                 // 历史数据查询
```

### 预警规则API
```typescript
GET    /api/alarm/rules     // 获取所有规则
POST   /api/alarm/rules     // 创建规则
PUT    /api/alarm/rules/:id // 更新规则
DELETE /api/alarm/rules/:id // 删除规则
```

## 🎨 自定义主题

### 修改顶部栏颜色
```css
.top-bar {
  background: linear-gradient(135deg, #your-color-1 0%, #your-color-2 100%);
}
```

### 修改监测点颜色
```typescript
const getPointColor = (status: string): string => {
  return {
    normal: '#自定义绿色',
    warning: '#自定义黄色',
    alarm: '#自定义红色',
    offline: '#自定义灰色'
  }[status];
};
```

## 🐛 故障排查

### Cesium无法加载
1. 检查Token是否有效
2. 检查网络连接
3. 查看浏览器控制台错误

### 监测点不显示
1. 检查坐标是否正确
2. 检查图层是否启用
3. 检查数据是否加载成功

### 性能卡顿
1. 减少监测点数量
2. 降低模型精度
3. 关闭不必要的图层
4. 使用LOD优化

## 📞 技术支持

如有问题，请查看控制台日志或联系开发团队。

## 📄 许可证

本项目仅供内部使用，禁止未经授权的复制和分发。

