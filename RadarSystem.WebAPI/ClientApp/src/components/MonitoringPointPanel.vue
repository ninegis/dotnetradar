<template>
  <div class="monitoring-point-panel">
    <el-card class="list-card">
      <template #header>
        <div class="card-header">
          <span>监测点列表</span>
          <el-button type="primary" size="small" @click="showAddDialog = true">
            <el-icon><Plus /></el-icon> 添加
          </el-button>
        </div>
      </template>

      <!-- 搜索和统计 -->
      <div class="search-section">
        <el-input
          v-model="searchText"
          placeholder="搜索监测点..."
          clearable
          @input="handleSearch"
        >
          <template #prefix><el-icon><Search /></el-icon></template>
        </el-input>
        
        <div class="statistics">
          <el-tag type="success">正常: {{ store.statistics.normal }}</el-tag>
          <el-tag type="warning">预警: {{ store.statistics.warning }}</el-tag>
          <el-tag type="danger">报警: {{ store.statistics.alarm }}</el-tag>
          <el-tag type="info">离线: {{ store.statistics.offline }}</el-tag>
        </div>
      </div>

      <!-- 筛选器 -->
      <div class="filter-section">
        <el-radio-group v-model="filterStatus" size="small" @change="handleFilter">
          <el-radio-button label="all">全部</el-radio-button>
          <el-radio-button label="normal">正常</el-radio-button>
          <el-radio-button label="warning">预警</el-radio-button>
          <el-radio-button label="alarm">报警</el-radio-button>
          <el-radio-button label="offline">离线</el-radio-button>
        </el-radio-group>
      </div>

      <!-- 监测点树形列表 -->
      <el-tree
        :data="treeData"
        :props="{ label: 'name', children: 'children' }"
        node-key="id"
        default-expand-all
        @node-click="handleNodeClick"
        class="point-tree"
      >
        <template #default="{ node, data }">
          <div class="tree-node">
            <el-icon :color="getStatusColor(data.status)" v-if="data.status">
              <LocationFilled />
            </el-icon>
            <span>{{ data.name }}</span>
            <div class="node-actions" v-if="data.status">
              <el-button text size="small" @click.stop="handleEdit(data)">
                <el-icon><Edit /></el-icon>
              </el-button>
              <el-button text size="small" type="danger" @click.stop="handleDelete(data)">
                <el-icon><Delete /></el-icon>
              </el-button>
              <el-button text size="small" @click.stop="handleFlyTo(data)">
                <el-icon><Position /></el-icon>
              </el-button>
            </div>
          </div>
        </template>
      </el-tree>
    </el-card>

    <!-- 添加/编辑对话框 -->
    <el-dialog
      v-model="showAddDialog"
      :title="editingPoint ? '编辑监测点' : '添加监测点'"
      width="600px"
    >
      <el-form :model="formData" label-width="100px">
        <el-form-item label="点位编号">
          <el-input v-model="formData.code" placeholder="如 MP-01" />
        </el-form-item>
        <el-form-item label="点位名称">
          <el-input v-model="formData.name" placeholder="如 1号边坡监测点" />
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="8">
            <el-form-item label="经度">
              <el-input-number v-model="formData.longitude" :precision="6" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="纬度">
              <el-input-number v-model="formData.latitude" :precision="6" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="高程">
              <el-input-number v-model="formData.altitude" :precision="2" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="关联设备">
          <el-select v-model="formData.deviceId" placeholder="选择雷达设备">
            <el-option label="雷达设备01" value="RADAR001" />
            <el-option label="雷达设备02" value="RADAR002" />
          </el-select>
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="预警阈值">
              <el-input-number v-model="warningThreshold" placeholder="mm" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="报警阈值">
              <el-input-number v-model="alarmThreshold" placeholder="mm" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="showAddDialog = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useMonitoringStore } from '../stores/monitoring';
import { ElMessage, ElMessageBox } from 'element-plus';
import {
  Plus, Search, LocationFilled, Edit, Delete, Position
} from '@element-plus/icons-vue';
import type { MonitoringPoint } from '../types/monitoring';

const emit = defineEmits<{
  flyTo: [point: MonitoringPoint];
  select: [point: MonitoringPoint];
}>();

const store = useMonitoringStore();
const searchText = ref('');
const filterStatus = ref('all');
const showAddDialog = ref(false);
const editingPoint = ref<MonitoringPoint | null>(null);

const formData = ref({
  code: '',
  name: '',
  longitude: 108.0,
  latitude: 34.0,
  altitude: 1000,
  deviceId: '',
  status: 'normal' as const
});

const warningThreshold = ref(50);
const alarmThreshold = ref(100);

// 树形数据
const treeData = computed(() => {
  let filteredPoints = store.points;
  
  // 状态筛选
  if (filterStatus.value !== 'all') {
    filteredPoints = filteredPoints.filter(p => p.status === filterStatus.value);
  }
  
  // 搜索筛选
  if (searchText.value) {
    filteredPoints = filteredPoints.filter(p =>
      p.name.includes(searchText.value) || p.code.includes(searchText.value)
    );
  }

  // 按区域分组
  const groups = [
    { id: 'area1', name: '1号边坡区', children: [] as MonitoringPoint[] },
    { id: 'area2', name: '2号边坡区', children: [] as MonitoringPoint[] },
    { id: 'area3', name: '3号边坡区', children: [] as MonitoringPoint[] },
    { id: 'other', name: '其他区域', children: [] as MonitoringPoint[] }
  ];

  filteredPoints.forEach(point => {
    // 简单分组逻辑，实际应根据point的区域属性
    const areaIndex = parseInt(point.code.split('-')[0]?.replace('MP', '') || '0') % 3;
    if (areaIndex >= 0 && areaIndex < 3) {
      groups[areaIndex].children.push(point);
    } else {
      groups[3].children.push(point);
    }
  });

  return groups.filter(g => g.children.length > 0);
});

// 搜索
const handleSearch = () => {
  // 搜索逻辑在computed中处理
};

// 筛选
const handleFilter = () => {
  // 筛选逻辑在computed中处理
};

// 节点点击
const handleNodeClick = (data: any) => {
  if (data.status) {
    // 是监测点
    store.selectedPoint = data;
    emit('select', data);
  }
};

// 编辑
const handleEdit = (point: MonitoringPoint) => {
  editingPoint.value = point;
  formData.value = {
    code: point.code,
    name: point.name,
    longitude: point.longitude,
    latitude: point.latitude,
    altitude: point.altitude,
    deviceId: point.deviceId || '',
    status: point.status
  };
  if (point.threshold) {
    warningThreshold.value = point.threshold.warning;
    alarmThreshold.value = point.threshold.alarm;
  }
  showAddDialog.value = true;
};

// 删除
const handleDelete = async (point: MonitoringPoint) => {
  try {
    await ElMessageBox.confirm(`确定删除监测点 ${point.code} 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    });
    await store.deletePoint(point.id);
    ElMessage.success('删除成功');
  } catch (error) {
    // 取消删除
  }
};

// 飞到点位
const handleFlyTo = (point: MonitoringPoint) => {
  emit('flyTo', point);
};

// 保存
const handleSave = async () => {
  try {
    const pointData: Partial<MonitoringPoint> = {
      ...formData.value,
      threshold: {
        warning: warningThreshold.value,
        alarm: alarmThreshold.value
      }
    };

    if (editingPoint.value) {
      await store.updatePoint(editingPoint.value.id, pointData);
      ElMessage.success('更新成功');
    } else {
      await store.addPoint(pointData);
      ElMessage.success('添加成功');
    }
    
    showAddDialog.value = false;
    editingPoint.value = null;
  } catch (error) {
    ElMessage.error('保存失败');
  }
};

// 获取状态颜色
const getStatusColor = (status: string): string => {
  const colorMap: Record<string, string> = {
    normal: '#52c41a',
    warning: '#faad14',
    alarm: '#f5222d',
    offline: '#8c8c8c'
  };
  return colorMap[status] || '#1890ff';
};

onMounted(() => {
  store.loadPoints();
  store.loadAreas();
});
</script>

<style scoped>
.monitoring-point-panel {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.list-card {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.search-section {
  margin-bottom: 16px;
}

.statistics {
  margin-top: 12px;
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.filter-section {
  margin-bottom: 16px;
}

.point-tree {
  flex: 1;
  overflow-y: auto;
}

.tree-node {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 8px;
  padding-right: 8px;
}

.tree-node span {
  flex: 1;
}

.node-actions {
  display: none;
  gap: 4px;
}

.tree-node:hover .node-actions {
  display: flex;
}
</style>

