import axios from 'axios'
import { ElMessage } from 'element-plus'
import type { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'

const api: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
api.interceptors.response.use(
  (response: AxiosResponse) => {
    return response.data
  },
  (error) => {
    if (error.response) {
      switch (error.response.status) {
        case 401:
          ElMessage.error('未授权，请重新登录')
          localStorage.removeItem('token')
          window.location.href = '/login'
          break
        case 403:
          ElMessage.error('拒绝访问')
          break
        case 404:
          ElMessage.error('请求资源不存在')
          break
        case 500:
          ElMessage.error('服务器错误')
          break
        default:
          ElMessage.error(error.response.data?.message || '请求失败')
      }
    } else {
      ElMessage.error('网络错误，请检查网络连接')
    }
    return Promise.reject(error)
  }
)

// API 方法
export const authApi = {
  login: (username: string, password: string) =>
    api.post('/auth/login', { username, password }),
  logout: () => api.post('/auth/logout'),
  getCurrentUser: () => api.get('/auth/me')
}

export const projectApi = {
  getList: (params?: any) => api.get('/project', { params }),
  getById: (id: number) => api.get(`/project/${id}`),
  create: (data: any) => api.post('/project', data),
  update: (id: number, data: any) => api.put(`/project/${id}`, data),
  delete: (id: number) => api.delete(`/project/${id}`)
}

export const deviceApi = {
  getList: (params?: any) => api.get('/device', { params }),
  getById: (id: number) => api.get(`/device/${id}`),
  create: (data: any) => api.post('/device', data),
  update: (id: number, data: any) => api.put(`/device/${id}`, data),
  delete: (id: number) => api.delete(`/device/${id}`),
  getByProject: (projectId: number) => api.get(`/device/project/${projectId}`),
  connect: (id: number) => api.post(`/device/${id}/connect`),
  disconnect: (id: number) => api.post(`/device/${id}/disconnect`)
}

export const commandApi = {
  sendCommand: (deviceId: number, command: string, params?: any) =>
    api.post('/parameter/command', { deviceId, command, params }),
  getCommandHistory: (deviceId: number) =>
    api.get(`/parameter/history/${deviceId}`)
}

export const alarmApi = {
  getList: (params?: any) => api.get('/alarm', { params }),
  getById: (id: number) => api.get(`/alarm/${id}`),
  acknowledge: (id: number) => api.post(`/alarm/${id}/acknowledge`),
  resolve: (id: number) => api.post(`/alarm/${id}/resolve`)
}

export const dataApi = {
  getRealTimeData: (deviceId: number) => api.get(`/data/realtime/${deviceId}`),
  getHistoryData: (deviceId: number, params: any) =>
    api.get(`/data/history/${deviceId}`, { params }),
  exportData: (params: any) => api.post('/data/export', params, { responseType: 'blob' })
}

export const reportApi = {
  getList: (params?: any) => api.get('/report', { params }),
  generate: (data: any) => api.post('/report/generate', data),
  download: (id: number) => api.get(`/report/${id}/download`, { responseType: 'blob' })
}

export default api

