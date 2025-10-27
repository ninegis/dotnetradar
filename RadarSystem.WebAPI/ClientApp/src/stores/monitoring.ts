/**
 * 监测数据状态管理
 */
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { MonitoringPoint, MonitoringArea, MonitoringData, AlarmRule } from '../types/monitoring';
import { monitoringPointApi, monitoringAreaApi, monitoringDataApi, alarmRuleApi } from '../api/monitoring';

export const useMonitoringStore = defineStore('monitoring', () => {
  // 状态
  const points = ref<MonitoringPoint[]>([]);
  const areas = ref<MonitoringArea[]>([]);
  const realtimeData = ref<Map<string, MonitoringData>>(new Map());
  const alarmRules = ref<AlarmRule[]>([]);
  const selectedPoint = ref<MonitoringPoint | null>(null);
  const selectedArea = ref<MonitoringArea | null>(null);
  const isOnline = ref(true);
  const loading = ref(false);

  // 计算属性
  const normalPoints = computed(() => points.value.filter(p => p.status === 'normal'));
  const warningPoints = computed(() => points.value.filter(p => p.status === 'warning'));
  const alarmPoints = computed(() => points.value.filter(p => p.status === 'alarm'));
  const offlinePoints = computed(() => points.value.filter(p => p.status === 'offline'));

  // 统计信息
  const statistics = computed(() => ({
    total: points.value.length,
    normal: normalPoints.value.length,
    warning: warningPoints.value.length,
    alarm: alarmPoints.value.length,
    offline: offlinePoints.value.length
  }));

  // 加载监测点
  const loadPoints = async () => {
    loading.value = true;
    try {
      const response = await monitoringPointApi.getAll();
      points.value = response.data;
    } catch (error) {
      console.error('加载监测点失败:', error);
    } finally {
      loading.value = false;
    }
  };

  // 加载监测面
  const loadAreas = async () => {
    try {
      const response = await monitoringAreaApi.getAll();
      areas.value = response.data;
    } catch (error) {
      console.error('加载监测面失败:', error);
    }
  };

  // 添加监测点
  const addPoint = async (point: Partial<MonitoringPoint>) => {
    try {
      const response = await monitoringPointApi.create(point);
      points.value.push(response.data);
      return response.data;
    } catch (error) {
      console.error('添加监测点失败:', error);
      throw error;
    }
  };

  // 更新监测点
  const updatePoint = async (id: string, data: Partial<MonitoringPoint>) => {
    try {
      const response = await monitoringPointApi.update(id, data);
      const index = points.value.findIndex(p => p.id === id);
      if (index !== -1) {
        points.value[index] = response.data;
      }
      return response.data;
    } catch (error) {
      console.error('更新监测点失败:', error);
      throw error;
    }
  };

  // 删除监测点
  const deletePoint = async (id: string) => {
    try {
      await monitoringPointApi.delete(id);
      points.value = points.value.filter(p => p.id !== id);
    } catch (error) {
      console.error('删除监测点失败:', error);
      throw error;
    }
  };

  // 添加监测面
  const addArea = async (area: Partial<MonitoringArea>) => {
    try {
      const response = await monitoringAreaApi.create(area);
      areas.value.push(response.data);
      return response.data;
    } catch (error) {
      console.error('添加监测面失败:', error);
      throw error;
    }
  };

  // 更新监测面
  const updateArea = async (id: string, data: Partial<MonitoringArea>) => {
    try {
      const response = await monitoringAreaApi.update(id, data);
      const index = areas.value.findIndex(a => a.id === id);
      if (index !== -1) {
        areas.value[index] = response.data;
      }
      return response.data;
    } catch (error) {
      console.error('更新监测面失败:', error);
      throw error;
    }
  };

  // 删除监测面
  const deleteArea = async (id: string) => {
    try {
      await monitoringAreaApi.delete(id);
      areas.value = areas.value.filter(a => a.id !== id);
    } catch (error) {
      console.error('删除监测面失败:', error);
      throw error;
    }
  };

  // 获取实时数据
  const loadRealtimeData = async (pointIds: string[]) => {
    try {
      const response = await monitoringDataApi.getRealtimeBatch(pointIds);
      response.data.forEach(data => {
        realtimeData.value.set(data.pointId, data);
        // 更新点的状态
        const point = points.value.find(p => p.id === data.pointId);
        if (point) {
          point.displacement = data.displacement;
          point.velocity = data.velocity;
          // 根据阈值更新状态
          if (point.threshold) {
            if (data.displacement >= point.threshold.alarm) {
              point.status = 'alarm';
            } else if (data.displacement >= point.threshold.warning) {
              point.status = 'warning';
            } else {
              point.status = 'normal';
            }
          }
        }
      });
    } catch (error) {
      console.error('获取实时数据失败:', error);
      isOnline.value = false;
    }
  };

  // 加载预警规则
  const loadAlarmRules = async () => {
    try {
      const response = await alarmRuleApi.getAll();
      alarmRules.value = response.data;
    } catch (error) {
      console.error('加载预警规则失败:', error);
    }
  };

  // 启动实时数据轮询
  let realtimeTimer: number | null = null;
  const startRealtimePolling = (interval = 5000) => {
    if (realtimeTimer) return;
    
    realtimeTimer = window.setInterval(() => {
      const pointIds = points.value.map(p => p.id);
      if (pointIds.length > 0) {
        loadRealtimeData(pointIds);
      }
    }, interval);
  };

  // 停止实时数据轮询
  const stopRealtimePolling = () => {
    if (realtimeTimer) {
      clearInterval(realtimeTimer);
      realtimeTimer = null;
    }
  };

  return {
    // 状态
    points,
    areas,
    realtimeData,
    alarmRules,
    selectedPoint,
    selectedArea,
    isOnline,
    loading,
    
    // 计算属性
    statistics,
    normalPoints,
    warningPoints,
    alarmPoints,
    offlinePoints,
    
    // 方法
    loadPoints,
    loadAreas,
    addPoint,
    updatePoint,
    deletePoint,
    addArea,
    updateArea,
    deleteArea,
    loadRealtimeData,
    loadAlarmRules,
    startRealtimePolling,
    stopRealtimePolling
  };
});

