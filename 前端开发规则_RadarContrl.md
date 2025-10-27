# 前端开发规则 - RadarContrl

## 📁 项目结构

```
RadarContrl/
├── src/
│   ├── components/          # 通用组件
│   │   ├── ToolBar/        # 工具栏组件
│   │   ├── TreeView/       # 树形导航
│   │   ├── DragContainer/  # 拖拽面板
│   │   └── BaseMaps/       # 底图切换
│   ├── views/              # 页面组件
│   │   ├── Login.vue       # 登录页面
│   │   ├── Main.vue        # 主界面
│   │   └── Layer.vue       # 图层管理
│   ├── router/             # 路由配置
│   ├── store/              # 状态管理 (Pinia)
│   ├── utils/              # 工具函数
│   ├── assets/             # 静态资源
│   └── styles/             # 样式文件
├── public/                # 公共资源
└── package.json           # 依赖配置
```

## 🛠️ 技术栈

### 核心框架
- **Vue 3** - 渐进式JavaScript框架
- **Vite** - 快速构建工具
- **Pinia** - 状态管理
- **Vue Router** - 路由管理 (Hash模式)

### UI组件库
- **Ant Design Vue** - 主要UI组件库
- **Element Plus** - 辅助UI组件库
- **双UI库策略** - 根据组件特性选择最适合的库

### 地图引擎
- **Cesium 1.120.0** - 3D地球可视化
- **Turf.js** - 地理空间分析
- **D3.js** - 数据可视化

### 通信与工具
- **Axios** - HTTP客户端
- **MQTT** - 实时数据通信
- **WebSocket** - 实时通信

## 📝 开发规范

### 组件命名
```javascript
// ✅ 正确: PascalCase
export default {
  name: 'ToolBarComponent'
}

// ❌ 错误: kebab-case
export default {
  name: 'tool-bar-component'
}
```

### 文件命名
```
// ✅ 正确
Login.vue
Main.vue
ToolBar.vue

// ❌ 错误
login.vue
main.vue
tool-bar.vue
```

### 组件结构
```vue
<template>
  <!-- 模板内容 -->
</template>

<script setup>
// Composition API
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useStore } from '@/store'

// 响应式数据
const data = ref('')
const state = reactive({})

// 计算属性
const computedValue = computed(() => {})

// 生命周期
onMounted(() => {})

// 方法定义
const handleClick = () => {}
</script>

<style scoped>
/* 组件样式 */
</style>
```

## 🎨 UI设计规范

### 主题色彩
```css
:root {
  --primary-color: #1890ff;
  --success-color: #52c41a;
  --warning-color: #faad14;
  --error-color: #f5222d;
  --text-color: #262626;
  --bg-color: #f0f2f5;
}
```

### 布局规范
- **主布局**: 顶部导航 + 侧边栏 + 内容区
- **响应式**: 支持桌面端和移动端
- **栅格系统**: 使用Ant Design的24栅格系统

### 组件使用规范
```vue
<!-- Ant Design Vue 组件 -->
<a-button type="primary" @click="handleClick">
  按钮文本
</a-button>

<!-- Element Plus 组件 -->
<el-button type="primary" @click="handleClick">
  按钮文本
</el-button>
```

## 🗺️ 地图开发规范

### Cesium初始化
```javascript
import * as Cesium from 'cesium'

// 初始化Cesium Viewer
const viewer = new Cesium.Viewer('cesiumContainer', {
  terrainProvider: Cesium.createWorldTerrain(),
  timeline: false,
  animation: false,
  homeButton: false,
  sceneModePicker: false,
  baseLayerPicker: false,
  navigationHelpButton: false,
  fullscreenButton: false,
  vrButton: false
})
```

### 图层管理
```javascript
// 添加图层
const addLayer = (layerConfig) => {
  const layer = viewer.imageryLayers.addImageryProvider(
    new Cesium.WebMapTileServiceImageryProvider(layerConfig)
  )
  return layer
}

// 移除图层
const removeLayer = (layer) => {
  viewer.imageryLayers.remove(layer)
}
```

### 测量工具
```javascript
// 长度测量
const measureLength = () => {
  // 实现长度测量逻辑
}

// 面积测量
const measureArea = () => {
  // 实现面积测量逻辑
}

// 角度测量
const measureAngle = () => {
  // 实现角度测量逻辑
}
```

## 📡 API通信规范

### Axios配置
```javascript
// src/axios/index.js
import axios from 'axios'

const api = axios.create({
  baseURL: process.env.VUE_APP_API_BASE_URL || 'http://localhost:8099/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器
api.interceptors.request.use(config => {
  const token = sessionStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// 响应拦截器
api.interceptors.response.use(
  response => response.data,
  error => {
    console.error('API Error:', error)
    return Promise.reject(error)
  }
)

export default api
```

### API调用规范
```javascript
// 统一API调用格式
const apiCall = async (url, method = 'GET', data = null) => {
  try {
    const response = await api({
      url,
      method,
      data
    })
    return response
  } catch (error) {
    console.error('API调用失败:', error)
    throw error
  }
}
```

## 🔄 状态管理规范

### Pinia Store结构
```javascript
// src/store/index.js
import { defineStore } from 'pinia'

export const useMainStore = defineStore('main', {
  state: () => ({
    user: null,
    isAuthenticated: false,
    currentProject: null,
    layers: [],
    devices: []
  }),
  
  getters: {
    isLoggedIn: (state) => state.isAuthenticated,
    currentUser: (state) => state.user
  },
  
  actions: {
    async login(credentials) {
      // 登录逻辑
    },
    
    async logout() {
      // 登出逻辑
    },
    
    setCurrentProject(project) {
      this.currentProject = project
    }
  }
})
```

## 🚀 构建与部署

### 开发环境
```bash
# 安装依赖
npm install

# 启动开发服务器
npm run dev

# 构建生产版本
npm run build
```

### 环境配置
```javascript
// .env.development
VUE_APP_API_BASE_URL=http://localhost:8099/api
VUE_APP_WS_URL=ws://localhost:8099/wss

// .env.production
VUE_APP_API_BASE_URL=https://api.radar.com/api
VUE_APP_WS_URL=wss://api.radar.com/wss
```

## 📊 性能优化

### 代码分割
```javascript
// 路由懒加载
const routes = [
  {
    path: '/main',
    component: () => import('@/views/Main.vue')
  }
]
```

### 组件优化
```vue
<script setup>
// 使用shallowRef优化大对象
import { shallowRef } from 'vue'

const largeData = shallowRef({})
</script>
```

## 🧪 测试规范

### 单元测试
```javascript
// 组件测试示例
import { mount } from '@vue/test-utils'
import ToolBar from '@/components/ToolBar.vue'

describe('ToolBar', () => {
  it('renders correctly', () => {
    const wrapper = mount(ToolBar)
    expect(wrapper.exists()).toBe(true)
  })
})
```

## 📝 代码审查清单

- [ ] 组件命名规范
- [ ] 文件结构清晰
- [ ] 样式作用域正确
- [ ] API调用错误处理
- [ ] 性能优化考虑
- [ ] 可访问性支持
- [ ] 浏览器兼容性
- [ ] 移动端适配

---

**维护人员**: 前端开发团队  
**最后更新**: 2025-10-23  
**版本**: v1.0
