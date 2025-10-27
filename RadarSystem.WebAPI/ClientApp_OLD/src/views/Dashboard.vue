<template>
  <div class="dashboard">
    <el-row :gutter="20">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="stat-card">
          <div class="stat-content">
            <div class="stat-icon projects">
              <el-icon :size="32"><Folder /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ stats.projects }}</div>
              <div class="stat-label">项目总数</div>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="stat-card">
          <div class="stat-content">
            <div class="stat-icon devices">
              <el-icon :size="32"><Monitor /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ stats.devices }}</div>
              <div class="stat-label">设备总数</div>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="stat-card">
          <div class="stat-content">
            <div class="stat-icon online">
              <el-icon :size="32"><View /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ stats.onlineDevices }}</div>
              <div class="stat-label">在线设备</div>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="stat-card">
          <div class="stat-content">
            <div class="stat-icon alarms">
              <el-icon :size="32"><Bell /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ stats.alarms }}</div>
              <div class="stat-label">未处理告警</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
    
    <el-row :gutter="20" style="margin-top: 20px">
      <el-col :span="16">
        <el-card>
          <template #header>
            <span>设备状态</span>
          </template>
          <div style="height: 300px; display: flex; align-items: center; justify-content: center; color: #909399;">
            图表区域 - 可集成 ECharts
          </div>
        </el-card>
      </el-col>
      
      <el-col :span="8">
        <el-card>
          <template #header>
            <span>最新告警</span>
          </template>
          <el-timeline>
            <el-timeline-item
              v-for="alarm in recentAlarms"
              :key="alarm.id"
              :timestamp="alarm.time"
              placement="top"
            >
              <el-tag :type="getAlarmType(alarm.level)" size="small">
                {{ alarm.level }}
              </el-tag>
              <span style="margin-left: 8px">{{ alarm.message }}</span>
            </el-timeline-item>
          </el-timeline>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Folder, Monitor, View, Bell } from '@element-plus/icons-vue'

const stats = ref({
  projects: 0,
  devices: 0,
  onlineDevices: 0,
  alarms: 0
})

const recentAlarms = ref([
  { id: 1, level: '警告', message: '设备 #001 位移超限', time: '2025-10-22 10:30' },
  { id: 2, level: '提示', message: '设备 #002 信号弱', time: '2025-10-22 09:15' },
  { id: 3, level: '严重', message: '设备 #003 连接中断', time: '2025-10-22 08:00' }
])

const getAlarmType = (level: string) => {
  const typeMap: Record<string, any> = {
    '严重': 'danger',
    '警告': 'warning',
    '提示': 'info'
  }
  return typeMap[level] || 'info'
}

onMounted(async () => {
  // 加载统计数据
  stats.value = {
    projects: 12,
    devices: 45,
    onlineDevices: 38,
    alarms: 5
  }
})
</script>

<style scoped>
.dashboard {
  width: 100%;
}

.stat-card {
  margin-bottom: 20px;
}

.stat-content {
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-icon {
  width: 64px;
  height: 64px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.stat-icon.projects {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.stat-icon.devices {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
}

.stat-icon.online {
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
}

.stat-icon.alarms {
  background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);
}

.stat-info {
  flex: 1;
}

.stat-value {
  font-size: 28px;
  font-weight: 600;
  color: #303133;
}

.stat-label {
  font-size: 14px;
  color: #909399;
  margin-top: 4px;
}
</style>


