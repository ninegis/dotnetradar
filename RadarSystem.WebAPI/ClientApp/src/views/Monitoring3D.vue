<template>
  <div class="monitoring-3d">
    <div class="page-header">
      <h2>三维监测</h2>
      <p>实时三维场景监测与数据可视化</p>
    </div>

    <div class="monitoring-container">
      <div class="cesium-container" ref="cesiumContainer">
        <!-- Cesium 三维场景容器 -->
        <div id="cesiumViewer" class="cesium-viewer"></div>
      </div>

      <div class="control-panel">
        <el-card>
          <template #header>
            <span>场景控制</span>
          </template>
          <div class="control-content">
            <el-button type="primary" @click="handleResetView">重置视角</el-button>
            <el-button @click="handleToggleFullscreen">全屏</el-button>
          </div>
        </el-card>

        <el-card style="margin-top: 10px;">
          <template #header>
            <span>实时数据</span>
          </template>
          <div class="data-content">
            <p>监测点数: {{ monitoringPoints }}</p>
            <p>告警数: {{ alarmCount }}</p>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'

const cesiumContainer = ref<HTMLElement>()
const monitoringPoints = ref(0)
const alarmCount = ref(0)

let cesiumViewer: any = null

onMounted(() => {
  initCesium()
  loadMonitoringData()
})

onUnmounted(() => {
  if (cesiumViewer) {
    cesiumViewer.destroy()
  }
})

const initCesium = () => {
  try {
    // Cesium 初始化逻辑
    // 注意：需要在 public 目录配置 Cesium 资源
    console.log('Cesium 初始化')
    ElMessage.info('三维场景加载中...')
  } catch (error) {
    console.error('Cesium 初始化失败:', error)
    ElMessage.error('三维场景初始化失败')
  }
}

const loadMonitoringData = async () => {
  try {
    // 加载监测数据
    monitoringPoints.value = 0
    alarmCount.value = 0
  } catch (error) {
    console.error('数据加载失败:', error)
  }
}

const handleResetView = () => {
  if (cesiumViewer) {
    // 重置视角逻辑
    ElMessage.success('视角已重置')
  }
}

const handleToggleFullscreen = () => {
  if (!document.fullscreenElement) {
    cesiumContainer.value?.requestFullscreen()
  } else {
    document.exitFullscreen()
  }
}
</script>

<style scoped>
.monitoring-3d {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.page-header {
  padding: 20px;
  background: #fff;
  border-bottom: 1px solid #e8e8e8;
}

.page-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 500;
}

.page-header p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.monitoring-container {
  flex: 1;
  display: flex;
  position: relative;
  overflow: hidden;
}

.cesium-container {
  flex: 1;
  position: relative;
  background: #000;
}

.cesium-viewer {
  width: 100%;
  height: 100%;
}

.control-panel {
  width: 300px;
  padding: 20px;
  background: #f5f5f5;
  overflow-y: auto;
}

.control-content,
.data-content {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.data-content p {
  margin: 0;
  padding: 8px 0;
  border-bottom: 1px solid #e8e8e8;
}

.data-content p:last-child {
  border-bottom: none;
}
</style>

  <div class="monitoring-3d">
    <div class="page-header">
      <h2>三维监测</h2>
      <p>实时三维场景监测与数据可视化</p>
    </div>

    <div class="monitoring-container">
      <div class="cesium-container" ref="cesiumContainer">
        <!-- Cesium 三维场景容器 -->
        <div id="cesiumViewer" class="cesium-viewer"></div>
      </div>

      <div class="control-panel">
        <el-card>
          <template #header>
            <span>场景控制</span>
          </template>
          <div class="control-content">
            <el-button type="primary" @click="handleResetView">重置视角</el-button>
            <el-button @click="handleToggleFullscreen">全屏</el-button>
          </div>
        </el-card>

        <el-card style="margin-top: 10px;">
          <template #header>
            <span>实时数据</span>
          </template>
          <div class="data-content">
            <p>监测点数: {{ monitoringPoints }}</p>
            <p>告警数: {{ alarmCount }}</p>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'

const cesiumContainer = ref<HTMLElement>()
const monitoringPoints = ref(0)
const alarmCount = ref(0)

let cesiumViewer: any = null

onMounted(() => {
  initCesium()
  loadMonitoringData()
})

onUnmounted(() => {
  if (cesiumViewer) {
    cesiumViewer.destroy()
  }
})

const initCesium = () => {
  try {
    // Cesium 初始化逻辑
    // 注意：需要在 public 目录配置 Cesium 资源
    console.log('Cesium 初始化')
    ElMessage.info('三维场景加载中...')
  } catch (error) {
    console.error('Cesium 初始化失败:', error)
    ElMessage.error('三维场景初始化失败')
  }
}

const loadMonitoringData = async () => {
  try {
    // 加载监测数据
    monitoringPoints.value = 0
    alarmCount.value = 0
  } catch (error) {
    console.error('数据加载失败:', error)
  }
}

const handleResetView = () => {
  if (cesiumViewer) {
    // 重置视角逻辑
    ElMessage.success('视角已重置')
  }
}

const handleToggleFullscreen = () => {
  if (!document.fullscreenElement) {
    cesiumContainer.value?.requestFullscreen()
  } else {
    document.exitFullscreen()
  }
}
</script>

<style scoped>
.monitoring-3d {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.page-header {
  padding: 20px;
  background: #fff;
  border-bottom: 1px solid #e8e8e8;
}

.page-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 500;
}

.page-header p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.monitoring-container {
  flex: 1;
  display: flex;
  position: relative;
  overflow: hidden;
}

.cesium-container {
  flex: 1;
  position: relative;
  background: #000;
}

.cesium-viewer {
  width: 100%;
  height: 100%;
}

.control-panel {
  width: 300px;
  padding: 20px;
  background: #f5f5f5;
  overflow-y: auto;
}

.control-content,
.data-content {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.data-content p {
  margin: 0;
  padding: 8px 0;
  border-bottom: 1px solid #e8e8e8;
}

.data-content p:last-child {
  border-bottom: none;
}
</style>

  <div class="monitoring-3d">
    <div class="page-header">
      <h2>三维监测</h2>
      <p>实时三维场景监测与数据可视化</p>
    </div>

    <div class="monitoring-container">
      <div class="cesium-container" ref="cesiumContainer">
        <!-- Cesium 三维场景容器 -->
        <div id="cesiumViewer" class="cesium-viewer"></div>
      </div>

      <div class="control-panel">
        <el-card>
          <template #header>
            <span>场景控制</span>
          </template>
          <div class="control-content">
            <el-button type="primary" @click="handleResetView">重置视角</el-button>
            <el-button @click="handleToggleFullscreen">全屏</el-button>
          </div>
        </el-card>

        <el-card style="margin-top: 10px;">
          <template #header>
            <span>实时数据</span>
          </template>
          <div class="data-content">
            <p>监测点数: {{ monitoringPoints }}</p>
            <p>告警数: {{ alarmCount }}</p>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'

const cesiumContainer = ref<HTMLElement>()
const monitoringPoints = ref(0)
const alarmCount = ref(0)

let cesiumViewer: any = null

onMounted(() => {
  initCesium()
  loadMonitoringData()
})

onUnmounted(() => {
  if (cesiumViewer) {
    cesiumViewer.destroy()
  }
})

const initCesium = () => {
  try {
    // Cesium 初始化逻辑
    // 注意：需要在 public 目录配置 Cesium 资源
    console.log('Cesium 初始化')
    ElMessage.info('三维场景加载中...')
  } catch (error) {
    console.error('Cesium 初始化失败:', error)
    ElMessage.error('三维场景初始化失败')
  }
}

const loadMonitoringData = async () => {
  try {
    // 加载监测数据
    monitoringPoints.value = 0
    alarmCount.value = 0
  } catch (error) {
    console.error('数据加载失败:', error)
  }
}

const handleResetView = () => {
  if (cesiumViewer) {
    // 重置视角逻辑
    ElMessage.success('视角已重置')
  }
}

const handleToggleFullscreen = () => {
  if (!document.fullscreenElement) {
    cesiumContainer.value?.requestFullscreen()
  } else {
    document.exitFullscreen()
  }
}
</script>

<style scoped>
.monitoring-3d {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.page-header {
  padding: 20px;
  background: #fff;
  border-bottom: 1px solid #e8e8e8;
}

.page-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 500;
}

.page-header p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.monitoring-container {
  flex: 1;
  display: flex;
  position: relative;
  overflow: hidden;
}

.cesium-container {
  flex: 1;
  position: relative;
  background: #000;
}

.cesium-viewer {
  width: 100%;
  height: 100%;
}

.control-panel {
  width: 300px;
  padding: 20px;
  background: #f5f5f5;
  overflow-y: auto;
}

.control-content,
.data-content {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.data-content p {
  margin: 0;
  padding: 8px 0;
  border-bottom: 1px solid #e8e8e8;
}

.data-content p:last-child {
  border-bottom: none;
}
</style>

  <div class="monitoring-3d">
    <div class="page-header">
      <h2>三维监测</h2>
      <p>实时三维场景监测与数据可视化</p>
    </div>

    <div class="monitoring-container">
      <div class="cesium-container" ref="cesiumContainer">
        <!-- Cesium 三维场景容器 -->
        <div id="cesiumViewer" class="cesium-viewer"></div>
      </div>

      <div class="control-panel">
        <el-card>
          <template #header>
            <span>场景控制</span>
          </template>
          <div class="control-content">
            <el-button type="primary" @click="handleResetView">重置视角</el-button>
            <el-button @click="handleToggleFullscreen">全屏</el-button>
          </div>
        </el-card>

        <el-card style="margin-top: 10px;">
          <template #header>
            <span>实时数据</span>
          </template>
          <div class="data-content">
            <p>监测点数: {{ monitoringPoints }}</p>
            <p>告警数: {{ alarmCount }}</p>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'

const cesiumContainer = ref<HTMLElement>()
const monitoringPoints = ref(0)
const alarmCount = ref(0)

let cesiumViewer: any = null

onMounted(() => {
  initCesium()
  loadMonitoringData()
})

onUnmounted(() => {
  if (cesiumViewer) {
    cesiumViewer.destroy()
  }
})

const initCesium = () => {
  try {
    // Cesium 初始化逻辑
    // 注意：需要在 public 目录配置 Cesium 资源
    console.log('Cesium 初始化')
    ElMessage.info('三维场景加载中...')
  } catch (error) {
    console.error('Cesium 初始化失败:', error)
    ElMessage.error('三维场景初始化失败')
  }
}

const loadMonitoringData = async () => {
  try {
    // 加载监测数据
    monitoringPoints.value = 0
    alarmCount.value = 0
  } catch (error) {
    console.error('数据加载失败:', error)
  }
}

const handleResetView = () => {
  if (cesiumViewer) {
    // 重置视角逻辑
    ElMessage.success('视角已重置')
  }
}

const handleToggleFullscreen = () => {
  if (!document.fullscreenElement) {
    cesiumContainer.value?.requestFullscreen()
  } else {
    document.exitFullscreen()
  }
}
</script>

<style scoped>
.monitoring-3d {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.page-header {
  padding: 20px;
  background: #fff;
  border-bottom: 1px solid #e8e8e8;
}

.page-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 500;
}

.page-header p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.monitoring-container {
  flex: 1;
  display: flex;
  position: relative;
  overflow: hidden;
}

.cesium-container {
  flex: 1;
  position: relative;
  background: #000;
}

.cesium-viewer {
  width: 100%;
  height: 100%;
}

.control-panel {
  width: 300px;
  padding: 20px;
  background: #f5f5f5;
  overflow-y: auto;
}

.control-content,
.data-content {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.data-content p {
  margin: 0;
  padding: 8px 0;
  border-bottom: 1px solid #e8e8e8;
}

.data-content p:last-child {
  border-bottom: none;
}
</style>





