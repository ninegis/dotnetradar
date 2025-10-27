import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue')
  },
  {
    path: '/dashboard',
    component: () => import('@/views/Layout.vue'),
    children: [
      {
        path: '',
        name: 'Dashboard',
        component: () => import('@/views/Dashboard.vue'),
        meta: { title: '仪表盘' }
      },
      {
        path: '/projects',
        name: 'Projects',
        component: () => import('@/views/Projects.vue'),
        meta: { title: '项目管理' }
      },
      {
        path: '/devices',
        name: 'Devices',
        component: () => import('@/views/Devices.vue'),
        meta: { title: '设备管理' }
      },
      {
        path: '/commands',
        name: 'Commands',
        component: () => import('@/views/Commands.vue'),
        meta: { title: '指令下载' }
      },
      {
        path: '/monitoring',
        name: 'Monitoring',
        component: () => import('@/views/Monitoring.vue'),
        meta: { title: '实时监测' }
      },
      {
        path: '/monitoring-3d',
        name: 'Monitoring3D',
        component: () => import('@/views/Monitoring3D.vue'),
        meta: { title: '三维监测' }
      },
      {
        path: '/test',
        name: 'Test',
        component: () => import('@/views/Test.vue'),
        meta: { title: '测试页面' }
      },
      {
        path: '/simpletest',
        name: 'SimpleTest',
        component: () => import('@/views/SimpleTest.vue'),
        meta: { title: '简单测试' }
      },
      {
        path: '/alarms',
        name: 'Alarms',
        component: () => import('@/views/Alarms.vue'),
        meta: { title: '告警管理' }
      },
      {
        path: '/reports',
        name: 'Reports',
        component: () => import('@/views/Reports.vue'),
        meta: { title: '报表中心' }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  
  // 白名单路由
  const whiteList = ['/login']
  
  if (whiteList.includes(to.path)) {
    // 如果已登录且访问登录页，重定向到三维监测
    if (token) {
      next('/monitoring-3d')
    } else {
      next()
    }
  } else {
    // 访问其他页面需要token
    if (token) {
      next()
    } else {
      next('/login')
    }
  }
})

export default router


