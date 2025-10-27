export interface Project {
  id: number
  projectName: string
  location: string
  description?: string
  startDate: string
  endDate?: string
  status: string
  createdAt: string
  updatedAt: string
}

export interface Device {
  id: number
  deviceName: string
  deviceType: string
  serialNumber: string
  ipAddress: string
  port: number
  projectId: number
  projectName?: string
  status: string
  isOnline: boolean
  lastOnlineTime?: string
  createdAt: string
  updatedAt: string
}

export interface Command {
  name: string
  code: string
  description: string
  parameters?: CommandParameter[]
}

export interface CommandParameter {
  name: string
  type: string
  required: boolean
  default?: any
  description?: string
}

export interface AlarmRecord {
  id: number
  deviceId: number
  deviceName?: string
  alarmType: string
  alarmLevel: string
  message: string
  status: string
  occurredAt: string
  acknowledgedAt?: string
  resolvedAt?: string
}

export interface RadarData {
  id: number
  deviceId: number
  timestamp: string
  dataType: string
  value: number
  unit: string
  quality: number
}

export interface ApiResponse<T = any> {
  success: boolean
  message: string
  data: T
  timestamp: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}


