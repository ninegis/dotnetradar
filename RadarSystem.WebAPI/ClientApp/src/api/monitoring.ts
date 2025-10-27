/**
 * 监测相关API接口
 */
import axios from './index';
import type {
  MonitoringPoint,
  MonitoringArea,
  MonitoringData,
  HistoryQuery,
  AlarmRule,
  ProjectConfig,
  DeviceConfig,
  AlgorithmConfig
} from '../types/monitoring';

// 监测点管理
export const monitoringPointApi = {
  // 获取所有监测点
  getAll: () => axios.get<MonitoringPoint[]>('/api/monitoring/points'),
  
  // 获取单个监测点
  getById: (id: string) => axios.get<MonitoringPoint>(`/api/monitoring/points/${id}`),
  
  // 创建监测点
  create: (data: Partial<MonitoringPoint>) => axios.post<MonitoringPoint>('/api/monitoring/points', data),
  
  // 更新监测点
  update: (id: string, data: Partial<MonitoringPoint>) => 
    axios.put<MonitoringPoint>(`/api/monitoring/points/${id}`, data),
  
  // 删除监测点
  delete: (id: string) => axios.delete(`/api/monitoring/points/${id}`),
  
  // 批量导入
  importBatch: (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return axios.post('/api/monitoring/points/import', formData);
  }
};

// 监测面管理
export const monitoringAreaApi = {
  getAll: () => axios.get<MonitoringArea[]>('/api/monitoring/areas'),
  getById: (id: string) => axios.get<MonitoringArea>(`/api/monitoring/areas/${id}`),
  create: (data: Partial<MonitoringArea>) => axios.post<MonitoringArea>('/api/monitoring/areas', data),
  update: (id: string, data: Partial<MonitoringArea>) => 
    axios.put<MonitoringArea>(`/api/monitoring/areas/${id}`, data),
  delete: (id: string) => axios.delete(`/api/monitoring/areas/${id}`),
  
  // 导入KML文件
  importKml: (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return axios.post('/api/monitoring/areas/import-kml', formData);
  }
};

// 实时数据
export const monitoringDataApi = {
  // 获取实时数据
  getRealtime: (pointId: string) => axios.get<MonitoringData>(`/api/monitoring/data/realtime/${pointId}`),
  
  // 获取批量实时数据
  getRealtimeBatch: (pointIds: string[]) => 
    axios.post<MonitoringData[]>('/api/monitoring/data/realtime-batch', { pointIds }),
  
  // 获取历史数据
  getHistory: (query: HistoryQuery) => 
    axios.post<MonitoringData[]>('/api/monitoring/data/history', query),
  
  // 获取统计数据
  getStatistics: (pointId: string, startTime: string, endTime: string) =>
    axios.get(`/api/monitoring/data/statistics/${pointId}`, { params: { startTime, endTime } })
};

// 预警规则
export const alarmRuleApi = {
  getAll: () => axios.get<AlarmRule[]>('/api/alarm/rules'),
  getById: (id: string) => axios.get<AlarmRule>(`/api/alarm/rules/${id}`),
  create: (data: Partial<AlarmRule>) => axios.post<AlarmRule>('/api/alarm/rules', data),
  update: (id: string, data: Partial<AlarmRule>) => 
    axios.put<AlarmRule>(`/api/alarm/rules/${id}`, data),
  delete: (id: string) => axios.delete(`/api/alarm/rules/${id}`)
};

// 项目配置
export const projectConfigApi = {
  getCurrent: () => axios.get<ProjectConfig>('/api/config/project/current'),
  getAll: () => axios.get<ProjectConfig[]>('/api/config/projects'),
  update: (id: string, data: Partial<ProjectConfig>) => 
    axios.put<ProjectConfig>(`/api/config/projects/${id}`, data),
  switchProject: (projectId: string) => axios.post(`/api/config/project/switch/${projectId}`)
};

// 设备配置
export const deviceConfigApi = {
  getAll: () => axios.get<DeviceConfig[]>('/api/config/devices'),
  update: (id: string, data: Partial<DeviceConfig>) => 
    axios.put<DeviceConfig>(`/api/config/devices/${id}`, data)
};

// 算法配置
export const algorithmConfigApi = {
  getAll: () => axios.get<AlgorithmConfig[]>('/api/config/algorithms'),
  update: (id: string, data: Partial<AlgorithmConfig>) => 
    axios.put<AlgorithmConfig>(`/api/config/algorithms/${id}`, data)
};

