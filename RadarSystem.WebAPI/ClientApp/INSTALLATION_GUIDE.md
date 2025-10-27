# 前端安装和部署指南

## 📦 环境要求

- Node.js >= 18.0.0
- npm >= 9.0.0 或 pnpm >= 8.0.0

## 🚀 快速开始

### 1. 安装依赖

```bash
cd RadarSystem.WebAPI/ClientApp
npm install
```

或使用 pnpm（推荐，更快）：
```bash
pnpm install
```

### 2. 配置Cesium Token

编辑 `src/components/CesiumViewer.vue`，第14行：

```typescript
Cesium.Ion.defaultAccessToken = 'your-cesium-ion-token';
```

**获取Token步骤：**
1. 访问 https://cesium.com/ion/
2. 注册账号（免费）
3. 进入 "Access Tokens" 页面
4. 复制 Default Token 或创建新Token
5. 粘贴到代码中

### 3. 开发模式

```bash
npm run dev
```

访问：http://localhost:5173

**开发模式特性：**
- ✅ 热重载（修改代码立即生效）
- ✅ 源码映射（方便调试）
- ✅ API代理到后端（localhost:8099）

### 4. 生产构建

```bash
npm run build
```

构建产物：`../wwwroot`（自动集成到.NET WebAPI）

### 5. 预览生产版本

```bash
npm run preview
```

## 🔧 配置说明

### Vite配置 (`vite.config.ts`)

```typescript
{
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:8099'  // API代理
    }
  },
  build: {
    outDir: '../wwwroot',               // 输出到.NET静态文件目录
    rollupOptions: {
      output: {
        manualChunks: {
          'cesium': ['cesium']          // Cesium单独分包
        }
      }
    }
  }
}
```

### TypeScript配置 (`tsconfig.json`)

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "module": "ESNext",
    "moduleResolution": "bundler",
    "strict": true,
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

## 📂 项目结构

```
ClientApp/
├── src/
│   ├── api/                   # API接口层
│   │   ├── index.ts          # Axios配置
│   │   └── monitoring.ts     # 监测相关API
│   ├── components/           # 公共组件
│   │   ├── CesiumViewer.vue          # Cesium三维场景
│   │   ├── MonitoringPointPanel.vue  # 监测点列表
│   │   ├── DataDetailPanel.vue       # 数据详情
│   │   └── SystemConfig.vue          # 系统配置
│   ├── stores/               # Pinia状态管理
│   │   ├── index.ts
│   │   └── monitoring.ts     # 监测数据Store
│   ├── types/                # TypeScript类型
│   │   ├── index.ts
│   │   └── monitoring.ts     # 监测类型定义
│   ├── views/                # 页面视图
│   │   ├── Monitoring3D.vue  # 三维监测主界面 ⭐
│   │   ├── Dashboard.vue
│   │   ├── Login.vue
│   │   └── ...
│   ├── router/               # 路由配置
│   ├── App.vue               # 根组件
│   └── main.ts              # 入口文件
├── public/                   # 静态资源
│   ├── cesium-config.js     # Cesium配置
│   └── favicon.ico
├── package.json             # 依赖配置
├── vite.config.ts          # Vite配置
└── tsconfig.json           # TS配置
```

## 🌐 访问路由

| 路径 | 功能 | 说明 |
|------|------|------|
| `/login` | 登录页 | 用户认证 |
| `/dashboard` | 仪表盘 | 数据概览 |
| `/monitoring-3d` | **三维监测** | **主功能界面** ⭐ |
| `/devices` | 设备管理 | 设备配置 |
| `/alarms` | 告警管理 | 历史告警 |
| `/reports` | 报表中心 | 数据报表 |

## 🔑 默认账号

```
用户名：admin
密码：admin123
```

## 🎯 开发建议

### 推荐VS Code插件
- **Volar** - Vue 3支持
- **TypeScript Vue Plugin** - TS支持
- **ESLint** - 代码检查
- **Prettier** - 代码格式化

### 调试技巧

1. **Vue Devtools**
   - Chrome扩展
   - 查看组件树和状态

2. **Network面板**
   - 查看API请求
   - 检查WebSocket连接

3. **Console日志**
   ```typescript
   console.log('监测点数据:', store.points);
   console.log('Cesium Viewer:', viewer);
   ```

## 🐛 常见问题

### Q1: npm install失败？
A: 尝试清理缓存
```bash
npm cache clean --force
npm install
```

### Q2: Cesium加载慢？
A: 使用国内CDN或本地部署Cesium资源

### Q3: 生产构建后样式错乱？
A: 检查base路径配置，确保CSS路径正确

### Q4: TypeScript类型错误？
A: 运行类型检查
```bash
npm run type-check
```

## 📝 开发规范

### 组件命名
- PascalCase：`MonitoringPointPanel.vue`
- 组件名与文件名一致

### API命名
- camelCase：`monitoringPointApi.getAll()`
- RESTful风格

### 样式规范
- 使用scoped样式
- BEM命名（可选）
- 响应式设计优先

## 🔄 更新日志

### v1.0.0 (2025-10-23)
- ✅ 基础Cesium三维场景
- ✅ 监测点/面管理
- ✅ 实时数据展示
- ✅ 历史趋势图表
- ✅ 预警规则配置
- ✅ 系统配置面板
- ✅ 响应式布局

## 📧 联系方式

开发团队：边坡雷达监测系统开发组
邮箱：dev@radar.com

