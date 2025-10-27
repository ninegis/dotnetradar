<template>
  <div class="data-detail-panel">
    <el-card v-if="point">
      <template #header>
        <div class="card-header">
          <div>
            <h3>{{ point.code }} - {{ point.name }}</h3>
            <el-tag :type="getStatusType(point.status)" size="small">
              {{ getStatusText(point.status) }}
            </el-tag>
          </div>
          <el-button text @click="emit('close')">
            <el-icon><Close /></el-icon>
          </el-button>
        </div>
      </template>

      <!-- 实时数据 -->
      <div class="realtime-section">
        <h4>实时数据</h4>
        <el-row :gutter="16">
          <el-col :span="12">
            <div class="data-item">
              <span class="label">累计位移:</span>
              <span class="value" :class="{ 'alarm': isAlarm(point.displacement) }">
                {{ point.displacement?.toFixed(2) || '--' }} mm
              </span>
            </div>
          </el-col>
          <el-col :span="12">
            <div class="data-item">
              <span class="label">位移速率:</span>
              <span class="value">{{ point.velocity?.toFixed(2) || '--' }} mm/h</span>
            </div>
          </el-col>
        </el-row>
        <el-row :gutter="16" style="margin-top: 12px">
          <el-col :span="12">
            <div class="data-item">
              <span class="label">预警阈值:</span>
              <span class="value warning">{{ point.threshold?.warning || '--' }} mm</span>
            </div>
          </el-col>
          <el-col :span="12">
            <div class="data-item">
              <span class="label">报警阈值:</span>
              <span class="value alarm">{{ point.threshold?.alarm || '--' }} mm</span>
            </div>
          </el-col>
        </el-row>
      </div>

      <!-- 历史趋势图表 -->
      <div class="chart-section">
        <h4>历史趋势</h4>
        <div class="chart-controls">
          <el-radio-group v-model="chartType" size="small">
            <el-radio-button label="displacement">位移</el-radio-button>
            <el-radio-button label="velocity">速率</el-radio-button>
            <el-radio-button label="acceleration">加速度</el-radio-button>
          </el-radio-group>
          <el-date-picker
            v-model="dateRange"
            type="datetimerange"
            range-separator="至"
            start-placeholder="开始时间"
            end-placeholder="结束时间"
            size="small"
            @change="loadHistoryData"
          />
        </div>
        <div ref="chartRef" class="chart-container"></div>
      </div>

      <!-- 设备信息 -->
      <div class="device-section">
        <h4>设备信息</h4>
        <el-descriptions :column="2" size="small">
          <el-descriptions-item label="设备ID">{{ point.deviceId || '--' }}</el-descriptions-item>
          <el-descriptions-item label="设备类型">{{ point.deviceType || '--' }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ formatTime(point.createTime) }}</el-descriptions-item>
          <el-descriptions-item label="更新时间">{{ formatTime(point.updateTime) }}</el-descriptions-item>
        </el-descriptions>
      </div>

      <!-- 操作按钮 -->
      <div class="action-section">
        <el-button type="primary" @click="handleExportData">导出数据</el-button>
        <el-button @click="handleViewHistory">查看完整历史</el-button>
        <el-button type="warning" v-if="point.status !== 'normal'" @click="handleAcknowledge">
          确认预警
        </el-button>
      </div>
    </el-card>

    <el-empty v-else description="请选择监测点查看详情" />
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue';
import type { MonitoringPoint, MonitoringData } from '../types/monitoring';
import { monitoringDataApi } from '../api/monitoring';
import { Close } from '@element-plus/icons-vue';
import * as echarts from 'echarts';
import { ElMessage } from 'element-plus';

const props = defineProps<{
  point: MonitoringPoint | null;
}>();

const emit = defineEmits<{
  close: [];
}>();

const chartRef = ref<HTMLElement>();
const chartType = ref<'displacement' | 'velocity' | 'acceleration'>('displacement');
const dateRange = ref<[Date, Date]>([
  new Date(Date.now() - 7 * 24 * 60 * 60 * 1000),
  new Date()
]);

let chart: echarts.ECharts | null = null;
const historyData = ref<MonitoringData[]>([]);

// 初始化图表
onMounted(() => {
  if (chartRef.value) {
    chart = echarts.init(chartRef.value);
    updateChart();
  }
});

onUnmounted(() => {
  if (chart) {
    chart.dispose();
    chart = null;
  }
});

// 监听point变化
watch(() => props.point, (newPoint) => {
  if (newPoint) {
    loadHistoryData();
  }
}, { immediate: true });

// 监听图表类型变化
watch(chartType, () => {
  updateChart();
});

// 加载历史数据
const loadHistoryData = async () => {
  if (!props.point || !dateRange.value) return;

  try {
    const response = await monitoringDataApi.getHistory({
      pointId: props.point.id,
      startTime: dateRange.value[0].toISOString(),
      endTime: dateRange.value[1].toISOString(),
      dataType: chartType.value
    });
    historyData.value = response.data;
    updateChart();
  } catch (error) {
    console.error('加载历史数据失败:', error);
  }
};

// 更新图表
const updateChart = () => {
  if (!chart || historyData.value.length === 0) return;

  const xData = historyData.value.map(d => new Date(d.timestamp).toLocaleString());
  const yData = historyData.value.map(d => {
    switch (chartType.value) {
      case 'displacement': return d.displacement;
      case 'velocity': return d.velocity;
      case 'acceleration': return d.acceleration || 0;
      default: return 0;
    }
  });

  const unitMap = {
    displacement: 'mm',
    velocity: 'mm/h',
    acceleration: 'mm/h²'
  };

  const nameMap = {
    displacement: '位移',
    velocity: '速率',
    acceleration: '加速度'
  };

  chart.setOption({
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const param = params[0];
        return `${param.name}<br/>${nameMap[chartType.value]}: ${param.value} ${unitMap[chartType.value]}`;
      }
    },
    grid: {
      left: '10%',
      right: '10%',
      bottom: '15%',
      top: '10%'
    },
    xAxis: {
      type: 'category',
      data: xData,
      axisLabel: {
        rotate: 45,
        fontSize: 10
      }
    },
    yAxis: {
      type: 'value',
      name: `${nameMap[chartType.value]} (${unitMap[chartType.value]})`,
      axisLabel: {
        formatter: '{value}'
      }
    },
    series: [{
      name: nameMap[chartType.value],
      type: 'line',
      smooth: true,
      data: yData,
      itemStyle: {
        color: '#1890ff'
      },
      areaStyle: {
        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
          { offset: 0, color: 'rgba(24, 144, 255, 0.3)' },
          { offset: 1, color: 'rgba(24, 144, 255, 0)' }
        ])
      }
    }],
    dataZoom: [{
      type: 'inside',
      start: 0,
      end: 100
    }, {
      start: 0,
      end: 100,
      height: 20
    }]
  });
};

// 判断是否报警
const isAlarm = (value?: number): boolean => {
  if (!value || !props.point?.threshold) return false;
  return value >= props.point.threshold.alarm;
};

// 获取状态类型
const getStatusType = (status: string) => {
  const typeMap: Record<string, any> = {
    normal: 'success',
    warning: 'warning',
    alarm: 'danger',
    offline: 'info'
  };
  return typeMap[status] || 'info';
};

// 获取状态文本
const getStatusText = (status: string): string => {
  const textMap: Record<string, string> = {
    normal: '正常',
    warning: '预警',
    alarm: '报警',
    offline: '离线'
  };
  return textMap[status] || '未知';
};

// 格式化时间
const formatTime = (time?: string): string => {
  if (!time) return '--';
  return new Date(time).toLocaleString();
};

// 导出数据
const handleExportData = () => {
  ElMessage.info('数据导出功能开发中');
};

// 查看完整历史
const handleViewHistory = () => {
  ElMessage.info('历史查看功能开发中');
};

// 确认预警
const handleAcknowledge = () => {
  ElMessage.success('预警已确认');
};
</script>

<style scoped>
.data-detail-panel {
  height: 100%;
  overflow-y: auto;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.card-header h3 {
  margin: 0 0 8px 0;
  font-size: 16px;
}

.realtime-section,
.chart-section,
.device-section,
.action-section {
  margin-bottom: 24px;
}

.realtime-section h4,
.chart-section h4,
.device-section h4 {
  margin: 0 0 12px 0;
  font-size: 14px;
  color: #666;
}

.data-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f5f5f5;
  border-radius: 4px;
}

.data-item .label {
  color: #666;
  font-size: 13px;
}

.data-item .value {
  font-size: 16px;
  font-weight: bold;
  color: #1890ff;
}

.data-item .value.warning {
  color: #faad14;
}

.data-item .value.alarm {
  color: #f5222d;
}

.chart-controls {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
  gap: 12px;
}

.chart-container {
  width: 100%;
  height: 300px;
}

.action-section {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
</style>

