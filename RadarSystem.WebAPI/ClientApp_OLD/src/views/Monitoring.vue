<template>
  <div class="monitoring-page">
    <el-card style="margin-bottom: 20px">
      <el-form :inline="true">
        <el-form-item label="选择设备">
          <el-select v-model="selectedDevice" placeholder="请选择设备" style="width: 200px" @change="loadData">
            <el-option
              v-for="device in devices"
              :key="device.id"
              :label="device.deviceName"
              :value="device.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="刷新间隔">
          <el-select v-model="refreshInterval" style="width: 120px" @change="updateInterval">
            <el-option label="5秒" :value="5000" />
            <el-option label="10秒" :value="10000" />
            <el-option label="30秒" :value="30000" />
            <el-option label="60秒" :value="60000" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button :type="autoRefresh ? 'danger' : 'success'" @click="toggleAutoRefresh">
            {{ autoRefresh ? '停止刷新' : '自动刷新' }}
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
    
    <el-row :gutter="20">
      <el-col :span="18">
        <el-card>
          <template #header>
            <span>实时数据趋势</span>
          </template>
          <div id="chart" style="height: 400px; display: flex; align-items: center; justify-content: center; color: #909399;">
            数据趋势图 - 可集成 ECharts 显示实时数据曲线
          </div>
        </el-card>
        
        <el-card style="margin-top: 20px">
          <template #header>
            <span>历史数据</span>
          </template>
          <el-table :data="historyData" style="width: 100%">
            <el-table-column prop="timestamp" label="时间" width="180" />
            <el-table-column prop="dataType" label="数据类型" width="120" />
            <el-table-column prop="value" label="数值" width="100" />
            <el-table-column prop="unit" label="单位" width="80" />
            <el-table-column prop="quality" label="质量" width="80">
              <template #default="{ row }">
                <el-progress :percentage="row.quality" :color="getQualityColor(row.quality)" />
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
      
      <el-col :span="6">
        <el-card>
          <template #header>
            <span>实时监测值</span>
          </template>
          
          <div class="monitor-item">
            <div class="monitor-label">水平位移</div>
            <div class="monitor-value">{{ realtimeData.horizontalDisplacement }} mm</div>
            <el-progress
              :percentage="Math.min((realtimeData.horizontalDisplacement / 50) * 100, 100)"
              :color="getProgressColor(realtimeData.horizontalDisplacement, 50)"
            />
          </div>
          
          <el-divider />
          
          <div class="monitor-item">
            <div class="monitor-label">垂直位移</div>
            <div class="monitor-value">{{ realtimeData.verticalDisplacement }} mm</div>
            <el-progress
              :percentage="Math.min((realtimeData.verticalDisplacement / 30) * 100, 100)"
              :color="getProgressColor(realtimeData.verticalDisplacement, 30)"
            />
          </div>
          
          <el-divider />
          
          <div class="monitor-item">
            <div class="monitor-label">速度</div>
            <div class="monitor-value">{{ realtimeData.velocity }} mm/h</div>
            <el-progress
              :percentage="Math.min((realtimeData.velocity / 10) * 100, 100)"
              :color="getProgressColor(realtimeData.velocity, 10)"
            />
          </div>
          
          <el-divider />
          
          <div class="monitor-item">
            <div class="monitor-label">加速度</div>
            <div class="monitor-value">{{ realtimeData.acceleration }} mm/h²</div>
            <el-progress
              :percentage="Math.min((realtimeData.acceleration / 5) * 100, 100)"
              :color="getProgressColor(realtimeData.acceleration, 5)"
            />
          </div>
        </el-card>
        
        <el-card style="margin-top: 20px">
          <template #header>
            <span>设备状态</span>
          </template>
          
          <el-descriptions :column="1" border>
            <el-descriptions-item label="在线状态">
              <el-tag :type="deviceStatus.isOnline ? 'success' : 'danger'">
                {{ deviceStatus.isOnline ? '在线' : '离线' }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="数据质量">
              <el-rate v-model="deviceStatus.dataQuality" disabled />
            </el-descriptions-item>
            <el-descriptions-item label="采集状态">
              <el-tag>{{ deviceStatus.collectStatus }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="最后更新">
              {{ deviceStatus.lastUpdate }}
            </el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import { deviceApi, dataApi } from '@/api'
import type { Device, RadarData } from '@/types'

const selectedDevice = ref<number | null>(null)
const devices = ref<Device[]>([])
const refreshInterval = ref(10000)
const autoRefresh = ref(false)
let timer: any = null

const realtimeData = reactive({
  horizontalDisplacement: 12.5,
  verticalDisplacement: 8.3,
  velocity: 2.1,
  acceleration: 0.5
})

const deviceStatus = reactive({
  isOnline: true,
  dataQuality: 4,
  collectStatus: '采集中',
  lastUpdate: '2025-10-22 18:55:30'
})

const historyData = ref<RadarData[]>([])

const loadDevices = async () => {
  try {
    const response: any = await deviceApi.getList({ page: 1, pageSize: 1000 })
    if (response.success) {
      devices.value = response.data.items
      if (devices.value.length > 0) {
        selectedDevice.value = devices.value[0].id
        loadData()
      }
    }
  } catch (error) {
    console.error('加载设备列表失败:', error)
  }
}

const loadData = async () => {
  if (!selectedDevice.value) return
  
  try {
    const response: any = await dataApi.getRealTimeData(selectedDevice.value)
    if (response.success) {
      // 更新实时数据
      Object.assign(realtimeData, response.data)
    }
  } catch (error) {
    console.error('加载数据失败:', error)
  }
}

const toggleAutoRefresh = () => {
  autoRefresh.value = !autoRefresh.value
  if (autoRefresh.value) {
    updateInterval()
  } else {
    if (timer) {
      clearInterval(timer)
      timer = null
    }
  }
}

const updateInterval = () => {
  if (timer) {
    clearInterval(timer)
  }
  if (autoRefresh.value) {
    timer = setInterval(loadData, refreshInterval.value)
  }
}

const getProgressColor = (value: number, threshold: number) => {
  const percentage = (value / threshold) * 100
  if (percentage < 50) return '#67C23A'
  if (percentage < 80) return '#E6A23C'
  return '#F56C6C'
}

const getQualityColor = (quality: number) => {
  if (quality >= 80) return '#67C23A'
  if (quality >= 60) return '#E6A23C'
  return '#F56C6C'
}

onMounted(() => {
  loadDevices()
})

onUnmounted(() => {
  if (timer) {
    clearInterval(timer)
  }
})
</script>

<style scoped>
.monitoring-page {
  width: 100%;
}

.monitor-item {
  margin-bottom: 16px;
}

.monitor-label {
  font-size: 14px;
  color: #606266;
  margin-bottom: 8px;
}

.monitor-value {
  font-size: 24px;
  font-weight: 600;
  color: #303133;
  margin-bottom: 8px;
}
</style>

