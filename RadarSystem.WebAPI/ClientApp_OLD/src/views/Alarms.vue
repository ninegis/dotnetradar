<template>
  <div class="alarms-page">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>告警管理</span>
          <el-space>
            <el-badge :value="unhandledCount" :max="99" type="danger">
              <el-button>未处理告警</el-button>
            </el-badge>
          </el-space>
        </div>
      </template>
      
      <el-form :inline="true" :model="searchForm" class="search-form">
        <el-form-item label="告警级别">
          <el-select v-model="searchForm.level" placeholder="全部" clearable style="width: 150px">
            <el-option label="严重" value="严重" />
            <el-option label="警告" value="警告" />
            <el-option label="提示" value="提示" />
          </el-select>
        </el-form-item>
        <el-form-item label="处理状态">
          <el-select v-model="searchForm.status" placeholder="全部" clearable style="width: 150px">
            <el-option label="未处理" value="未处理" />
            <el-option label="已确认" value="已确认" />
            <el-option label="已处理" value="已处理" />
          </el-select>
        </el-form-item>
        <el-form-item label="时间范围">
          <el-date-picker
            v-model="searchForm.dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            style="width: 240px"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      
      <el-table :data="tableData" style="width: 100%" v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="deviceName" label="设备名称" min-width="120" />
        <el-table-column prop="alarmLevel" label="告警级别" width="100">
          <template #default="{ row }">
            <el-tag :type="getLevelType(row.alarmLevel)">
              {{ row.alarmLevel }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="alarmType" label="告警类型" width="120" />
        <el-table-column prop="message" label="告警信息" min-width="200" show-overflow-tooltip />
        <el-table-column prop="occurredAt" label="发生时间" width="180" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button
              v-if="row.status === '未处理'"
              size="small"
              type="warning"
              @click="handleAcknowledge(row)"
            >
              确认
            </el-button>
            <el-button
              v-if="row.status !== '已处理'"
              size="small"
              type="success"
              @click="handleResolve(row)"
            >
              处理
            </el-button>
            <el-button size="small" @click="handleView(row)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
      
      <el-pagination
        v-model:current-page="pagination.page"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
        style="margin-top: 20px; justify-content: flex-end;"
        @size-change="loadData"
        @current-change="loadData"
      />
    </el-card>
    
    <!-- 告警详情对话框 -->
    <el-dialog v-model="detailVisible" title="告警详情" width="600px">
      <el-descriptions :column="1" border v-if="currentAlarm">
        <el-descriptions-item label="告警ID">{{ currentAlarm.id }}</el-descriptions-item>
        <el-descriptions-item label="设备名称">{{ currentAlarm.deviceName }}</el-descriptions-item>
        <el-descriptions-item label="告警级别">
          <el-tag :type="getLevelType(currentAlarm.alarmLevel)">
            {{ currentAlarm.alarmLevel }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="告警类型">{{ currentAlarm.alarmType }}</el-descriptions-item>
        <el-descriptions-item label="告警信息">{{ currentAlarm.message }}</el-descriptions-item>
        <el-descriptions-item label="发生时间">{{ currentAlarm.occurredAt }}</el-descriptions-item>
        <el-descriptions-item label="确认时间">{{ currentAlarm.acknowledgedAt || '-' }}</el-descriptions-item>
        <el-descriptions-item label="处理时间">{{ currentAlarm.resolvedAt || '-' }}</el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="getStatusType(currentAlarm.status)">
            {{ currentAlarm.status }}
          </el-tag>
        </el-descriptions-item>
      </el-descriptions>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { alarmApi } from '@/api'
import type { AlarmRecord } from '@/types'

const loading = ref(false)
const detailVisible = ref(false)
const currentAlarm = ref<AlarmRecord | null>(null)

const tableData = ref<AlarmRecord[]>([])
const pagination = reactive({
  page: 1,
  pageSize: 10,
  total: 0
})

const searchForm = reactive({
  level: '',
  status: '',
  dateRange: null as any
})

const unhandledCount = computed(() => {
  return tableData.value.filter(item => item.status === '未处理').length
})

const getLevelType = (level: string) => {
  const typeMap: Record<string, any> = {
    '严重': 'danger',
    '警告': 'warning',
    '提示': 'info'
  }
  return typeMap[level] || 'info'
}

const getStatusType = (status: string) => {
  const typeMap: Record<string, any> = {
    '未处理': 'danger',
    '已确认': 'warning',
    '已处理': 'success'
  }
  return typeMap[status] || 'info'
}

const loadData = async () => {
  loading.value = true
  try {
    const params: any = {
      page: pagination.page,
      pageSize: pagination.pageSize,
      level: searchForm.level,
      status: searchForm.status
    }
    
    if (searchForm.dateRange && searchForm.dateRange.length === 2) {
      params.startDate = searchForm.dateRange[0]
      params.endDate = searchForm.dateRange[1]
    }
    
    const response: any = await alarmApi.getList(params)
    if (response.success) {
      tableData.value = response.data.items
      pagination.total = response.data.total
    }
  } catch (error) {
    console.error('加载数据失败:', error)
  } finally {
    loading.value = false
  }
}

const handleReset = () => {
  Object.assign(searchForm, {
    level: '',
    status: '',
    dateRange: null
  })
  loadData()
}

const handleView = (row: AlarmRecord) => {
  currentAlarm.value = row
  detailVisible.value = true
}

const handleAcknowledge = (row: AlarmRecord) => {
  ElMessageBox.confirm('确认此告警？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(async () => {
    try {
      const response: any = await alarmApi.acknowledge(row.id)
      if (response.success) {
        ElMessage.success('确认成功')
        loadData()
      }
    } catch (error) {
      console.error('确认失败:', error)
    }
  })
}

const handleResolve = (row: AlarmRecord) => {
  ElMessageBox.confirm('标记此告警为已处理？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'success'
  }).then(async () => {
    try {
      const response: any = await alarmApi.resolve(row.id)
      if (response.success) {
        ElMessage.success('处理成功')
        loadData()
      }
    } catch (error) {
      console.error('处理失败:', error)
    }
  })
}

onMounted(() => {
  loadData()
})
</script>

<style scoped>
.alarms-page {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.search-form {
  margin-bottom: 20px;
}
</style>

