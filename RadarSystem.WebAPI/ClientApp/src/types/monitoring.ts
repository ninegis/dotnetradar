/**
 * 监测系统类型定义
 */

// 监测点类型
export interface MonitoringPoint {
  id: string;
  name: string;
  code: string; // 编号如 MP-01
  longitude: number;
  latitude: number;
  altitude: number;
  deviceId?: string;
  deviceType?: string;
  status: 'normal' | 'warning' | 'alarm' | 'offline';
  displacement?: number; // 当前位移 mm
  velocity?: number; // 速率 mm/h
  threshold?: {
    warning: number;
    alarm: number;
  };
  createTime?: string;
  updateTime?: string;
}

// 监测面类型
export interface MonitoringArea {
  id: string;
  name: string;
  type: 'polygon' | 'rectangle' | 'circle';
  coordinates: Array<{ longitude: number; latitude: number; altitude?: number }>;
  deviceIds: string[];
  status: 'normal' | 'warning' | 'alarm';
  avgDisplacement?: number;
  maxDisplacement?: number;
  color?: string;
  opacity?: number;
  createTime?: string;
}

// 实时监测数据
export interface MonitoringData {
  pointId: string;
  timestamp: string;
  displacement: number;
  velocity: number;
  acceleration?: number;
  temperature?: number;
  humidity?: number;
}

// 历史数据查询参数
export interface HistoryQuery {
  pointId: string;
  startTime: string;
  endTime: string;
  dataType?: 'displacement' | 'velocity' | 'acceleration';
}

// 预警规则
export interface AlarmRule {
  id: string;
  name: string;
  pointIds: string[];
  areaIds: string[];
  type: 'displacement' | 'velocity' | 'acceleration';
  operator: '>' | '<' | '>=' | '<=' | '=';
  threshold: number;
  level: 1 | 2 | 3 | 4; // 1-蓝色 2-黄色 3-橙色 4-红色
  enable: boolean;
  smsTemplate?: string;
}

// 项目配置
export interface ProjectConfig {
  id: string;
  projectId: string;
  projectName: string;
  location: string;
  description?: string;
  center: { longitude: number; latitude: number; altitude: number };
  modelUrl?: string; // 三维模型URL
  terrainUrl?: string; // 地形数据URL
  imageryUrl?: string; // 影像底图URL
}

// 设备配置
export interface DeviceConfig {
  id: string;
  deviceId: string;
  deviceName: string;
  deviceType: string;
  protocol: string;
  host: string;
  port: number;
  samplingRate: number; // 采样频率 Hz
  enable: boolean;
}

// 算法配置
export interface AlgorithmConfig {
  id: string;
  name: string;
  type: 'threshold' | 'ml' | 'statistical';
  parameters: Record<string, any>;
  modelPath?: string;
  enable: boolean;
}

// Cesium视图状态
export interface CesiumViewState {
  longitude: number;
  latitude: number;
  altitude: number;
  heading: number;
  pitch: number;
  roll: number;
}

