<template>
  <div class="commands-page">
    <el-row :gutter="20">
      <el-col :span="8">
        <el-card>
          <template #header>
            <span>选择设备</span>
          </template>
          
          <el-select
            v-model="selectedProject"
            placeholder="选择项目"
            style="width: 100%; margin-bottom: 12px"
            @change="loadDevices"
          >
            <el-option
              v-for="project in projects"
              :key="project.id"
              :label="project.projectName"
              :value="project.id"
            />
          </el-select>
          
          <el-tree
            :data="deviceTree"
            :props="treeProps"
            node-key="id"
            @node-click="handleDeviceSelect"
            highlight-current
          >
            <template #default="{ node, data }">
              <span class="tree-node">
                <el-icon v-if="data.type === 'device'">
                  <Monitor />
                </el-icon>
                <span>{{ node.label }}</span>
                <el-tag
                  v-if="data.type === 'device'"
                  :type="data.isOnline ? 'success' : 'danger'"
                  size="small"
                  style="margin-left: 8px"
                >
                  {{ data.isOnline ? '在线' : '离线' }}
                </el-tag>
              </span>
            </template>
          </el-tree>
        </el-card>
      </el-col>
      
      <el-col :span="16">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>指令下载</span>
              <el-tag v-if="selectedDevice" type="primary">
                当前设备: {{ selectedDevice.deviceName }}
              </el-tag>
            </div>
          </template>
          
          <el-alert
            v-if="!selectedDevice"
            title="请先选择设备"
            type="info"
            :closable="false"
            show-icon
          />
          
          <div v-else class="commands-container">
            <el-tabs v-model="activeTab">
              <el-tab-pane label="常用指令" name="common">
                <el-row :gutter="12">
                  <el-col
                    v-for="cmd in commonCommands"
                    :key="cmd.code"
                    :span="8"
                  >
                    <el-card
                      shadow="hover"
                      class="command-card"
                      @click="selectCommand(cmd)"
                    >
                      <div class="command-icon">
                        <el-icon :size="32"><Operation /></el-icon>
                      </div>
                      <div class="command-name">{{ cmd.name }}</div>
                      <div class="command-desc">{{ cmd.description }}</div>
                    </el-card>
                  </el-col>
                </el-row>
              </el-tab-pane>
              
              <el-tab-pane label="参数配置" name="params">
                <el-form :model="paramForm" label-width="120px">
                  <el-form-item label="采样间隔">
                    <el-input-number v-model="paramForm.sampleInterval" :min="1" :max="3600" />
                    <span style="margin-left: 8px">秒</span>
                  </el-form-item>
                  <el-form-item label="存储周期">
                    <el-input-number v-model="paramForm.storageInterval" :min="1" :max="86400" />
                    <span style="margin-left: 8px">秒</span>
                  </el-form-item>
                  <el-form-item label="告警阈值">
                    <el-input-number v-model="paramForm.alarmThreshold" :min="0" :max="1000" :precision="2" />
                    <span style="margin-left: 8px">mm</span>
                  </el-form-item>
                  <el-form-item label="数据上报">
                    <el-switch v-model="paramForm.dataReporting" />
                  </el-form-item>
                  <el-form-item>
                    <el-button type="primary" @click="sendParamCommand">
                      <el-icon><Upload /></el-icon>
                      下载参数
                    </el-button>
                  </el-form-item>
                </el-form>
              </el-tab-pane>
              
              <el-tab-pane label="高级指令" name="advanced">
                <el-form :model="advancedForm" label-width="120px">
                  <el-form-item label="指令代码">
                    <el-input v-model="advancedForm.commandCode" placeholder="输入指令代码" />
                  </el-form-item>
                  <el-form-item label="参数 (JSON)">
                    <el-input
                      v-model="advancedForm.params"
                      type="textarea"
                      :rows="8"
                      placeholder='{"key": "value"}'
                    />
                  </el-form-item>
                  <el-form-item>
                    <el-button type="primary" @click="sendAdvancedCommand">
                      <el-icon><Upload /></el-icon>
                      发送指令
                    </el-button>
                  </el-form-item>
                </el-form>
              </el-tab-pane>
              
              <el-tab-pane label="历史记录" name="history">
                <el-table :data="commandHistory" style="width: 100%">
                  <el-table-column prop="time" label="时间" width="180" />
                  <el-table-column prop="command" label="指令" width="150" />
                  <el-table-column prop="params" label="参数" show-overflow-tooltip />
                  <el-table-column prop="status" label="状态" width="100">
                    <template #default="{ row }">
                      <el-tag :type="row.status === '成功' ? 'success' : 'danger'">
                        {{ row.status }}
                      </el-tag>
                    </template>
                  </el-table-column>
                </el-table>
              </el-tab-pane>
            </el-tabs>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Monitor, Operation, Upload } from '@element-plus/icons-vue'
import { projectApi, deviceApi, commandApi } from '@/api'
import type { Device, Project, Command } from '@/types'

const selectedProject = ref<number | null>(null)
const selectedDevice = ref<Device | null>(null)
const activeTab = ref('common')

const projects = ref<Project[]>([])
const deviceTree = ref<any[]>([])
const treeProps = {
  children: 'children',
  label: 'label'
}

const commonCommands = ref<Command[]>([
  { name: '开始采集', code: 'START_COLLECT', description: '开始数据采集' },
  { name: '停止采集', code: 'STOP_COLLECT', description: '停止数据采集' },
  { name: '读取数据', code: 'READ_DATA', description: '读取实时数据' },
  { name: '清空缓存', code: 'CLEAR_CACHE', description: '清空设备缓存' },
  { name: '重启设备', code: 'REBOOT', description: '重启设备' },
  { name: '校准设备', code: 'CALIBRATE', description: '设备校准' }
])

const paramForm = reactive({
  sampleInterval: 60,
  storageInterval: 300,
  alarmThreshold: 10.0,
  dataReporting: true
})

const advancedForm = reactive({
  commandCode: '',
  params: ''
})

const commandHistory = ref([
  { time: '2025-10-22 10:30:45', command: '开始采集', params: '{}', status: '成功' },
  { time: '2025-10-22 09:15:23', command: '读取数据', params: '{}', status: '成功' },
  { time: '2025-10-22 08:00:12', command: '重启设备', params: '{}', status: '失败' }
])

const loadProjects = async () => {
  try {
    const response: any = await projectApi.getList({ page: 1, pageSize: 1000 })
    if (response.success) {
      projects.value = response.data.items
    }
  } catch (error) {
    console.error('加载项目列表失败:', error)
  }
}

const loadDevices = async () => {
  if (!selectedProject.value) {
    deviceTree.value = []
    return
  }
  
  try {
    const response: any = await deviceApi.getByProject(selectedProject.value)
    if (response.success) {
      deviceTree.value = response.data.map((device: Device) => ({
        id: device.id,
        label: device.deviceName,
        type: 'device',
        isOnline: device.isOnline,
        data: device
      }))
    }
  } catch (error) {
    console.error('加载设备列表失败:', error)
  }
}

const handleDeviceSelect = (data: any) => {
  if (data.type === 'device') {
    selectedDevice.value = data.data
    loadCommandHistory()
  }
}

const selectCommand = async (cmd: Command) => {
  if (!selectedDevice.value) {
    ElMessage.warning('请先选择设备')
    return
  }
  
  try {
    const response: any = await commandApi.sendCommand(
      selectedDevice.value.id,
      cmd.code
    )
    if (response.success) {
      ElMessage.success(`指令"${cmd.name}"发送成功`)
      loadCommandHistory()
    }
  } catch (error) {
    ElMessage.error('指令发送失败')
  }
}

const sendParamCommand = async () => {
  if (!selectedDevice.value) {
    ElMessage.warning('请先选择设备')
    return
  }
  
  try {
    const response: any = await commandApi.sendCommand(
      selectedDevice.value.id,
      'SET_PARAMS',
      paramForm
    )
    if (response.success) {
      ElMessage.success('参数下载成功')
      loadCommandHistory()
    }
  } catch (error) {
    ElMessage.error('参数下载失败')
  }
}

const sendAdvancedCommand = async () => {
  if (!selectedDevice.value) {
    ElMessage.warning('请先选择设备')
    return
  }
  
  if (!advancedForm.commandCode) {
    ElMessage.warning('请输入指令代码')
    return
  }
  
  try {
    let params = {}
    if (advancedForm.params) {
      params = JSON.parse(advancedForm.params)
    }
    
    const response: any = await commandApi.sendCommand(
      selectedDevice.value.id,
      advancedForm.commandCode,
      params
    )
    if (response.success) {
      ElMessage.success('指令发送成功')
      loadCommandHistory()
    }
  } catch (error) {
    ElMessage.error('指令发送失败')
  }
}

const loadCommandHistory = async () => {
  if (!selectedDevice.value) return
  
  try {
    const response: any = await commandApi.getCommandHistory(selectedDevice.value.id)
    if (response.success) {
      commandHistory.value = response.data
    }
  } catch (error) {
    console.error('加载历史记录失败:', error)
  }
}

onMounted(() => {
  loadProjects()
})
</script>

<style scoped>
.commands-page {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.tree-node {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
}

.commands-container {
  min-height: 400px;
}

.command-card {
  margin-bottom: 12px;
  cursor: pointer;
  text-align: center;
  transition: all 0.3s;
}

.command-card:hover {
  transform: translateY(-4px);
  border-color: #409EFF;
}

.command-icon {
  color: #409EFF;
  margin-bottom: 8px;
}

.command-name {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 4px;
}

.command-desc {
  font-size: 12px;
  color: #909399;
}
</style>


