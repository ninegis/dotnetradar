<template>
  <div class="reports-page">
    <el-card style="margin-bottom: 20px">
      <template #header>
        <span>生成报表</span>
      </template>
      
      <el-form :model="reportForm" :rules="rules" ref="formRef" label-width="120px">
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="报表类型" prop="reportType">
              <el-select v-model="reportForm.reportType" placeholder="请选择报表类型" style="width: 100%">
                <el-option label="日报" value="日报" />
                <el-option label="周报" value="周报" />
                <el-option label="月报" value="月报" />
                <el-option label="自定义" value="自定义" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="选择项目" prop="projectId">
              <el-select v-model="reportForm.projectId" placeholder="请选择项目" style="width: 100%">
                <el-option
                  v-for="project in projects"
                  :key="project.id"
                  :label="project.projectName"
                  :value="project.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="开始日期" prop="startDate">
              <el-date-picker
                v-model="reportForm.startDate"
                type="date"
                placeholder="选择日期"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="结束日期" prop="endDate">
              <el-date-picker
                v-model="reportForm.endDate"
                type="date"
                placeholder="选择日期"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-form-item label="包含内容">
          <el-checkbox-group v-model="reportForm.content">
            <el-checkbox label="设备状态" />
            <el-checkbox label="监测数据" />
            <el-checkbox label="告警记录" />
            <el-checkbox label="数据分析" />
            <el-checkbox label="趋势图表" />
          </el-checkbox-group>
        </el-form-item>
        
        <el-form-item label="导出格式">
          <el-radio-group v-model="reportForm.format">
            <el-radio label="PDF">PDF</el-radio>
            <el-radio label="Excel">Excel</el-radio>
            <el-radio label="Word">Word</el-radio>
          </el-radio-group>
        </el-form-item>
        
        <el-form-item>
          <el-button type="primary" @click="handleGenerate" :loading="generating">
            <el-icon><Document /></el-icon>
            生成报表
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
    
    <el-card>
      <template #header>
        <span>历史报表</span>
      </template>
      
      <el-table :data="reportList" style="width: 100%" v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="reportType" label="报表类型" width="100" />
        <el-table-column prop="projectName" label="项目" min-width="150" />
        <el-table-column prop="dateRange" label="时间范围" width="200" />
        <el-table-column prop="format" label="格式" width="80" />
        <el-table-column prop="createdAt" label="生成时间" width="180" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button
              size="small"
              type="primary"
              :disabled="row.status !== '已完成'"
              @click="handleDownload(row)"
            >
              <el-icon><Download /></el-icon>
              下载
            </el-button>
            <el-button size="small" @click="handleView(row)">预览</el-button>
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
        @size-change="loadReports"
        @current-change="loadReports"
      />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Document, Download } from '@element-plus/icons-vue'
import { reportApi, projectApi } from '@/api'
import type { Project } from '@/types'

const loading = ref(false)
const generating = ref(false)
const formRef = ref()

const projects = ref<Project[]>([])
const reportList = ref<any[]>([])
const pagination = reactive({
  page: 1,
  pageSize: 10,
  total: 0
})

const reportForm = reactive({
  reportType: '日报',
  projectId: null as number | null,
  startDate: '',
  endDate: '',
  content: ['设备状态', '监测数据', '告警记录'],
  format: 'PDF'
})

const rules = {
  reportType: [{ required: true, message: '请选择报表类型', trigger: 'change' }],
  projectId: [{ required: true, message: '请选择项目', trigger: 'change' }],
  startDate: [{ required: true, message: '请选择开始日期', trigger: 'change' }],
  endDate: [{ required: true, message: '请选择结束日期', trigger: 'change' }]
}

const getStatusType = (status: string) => {
  const typeMap: Record<string, any> = {
    '生成中': 'warning',
    '已完成': 'success',
    '失败': 'danger'
  }
  return typeMap[status] || 'info'
}

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

const loadReports = async () => {
  loading.value = true
  try {
    const response: any = await reportApi.getList({
      page: pagination.page,
      pageSize: pagination.pageSize
    })
    if (response.success) {
      reportList.value = response.data.items
      pagination.total = response.data.total
    }
  } catch (error) {
    console.error('加载报表列表失败:', error)
  } finally {
    loading.value = false
  }
}

const handleGenerate = async () => {
  await formRef.value?.validate(async (valid: boolean) => {
    if (valid) {
      generating.value = true
      try {
        const response: any = await reportApi.generate(reportForm)
        if (response.success) {
          ElMessage.success('报表生成任务已提交，请稍后在列表中查看')
          loadReports()
        }
      } catch (error) {
        ElMessage.error('报表生成失败')
      } finally {
        generating.value = false
      }
    }
  })
}

const handleDownload = async (row: any) => {
  try {
    const blob = await reportApi.download(row.id)
    const url = window.URL.createObjectURL(blob as Blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `${row.reportType}_${row.projectName}_${row.id}.${row.format.toLowerCase()}`
    link.click()
    window.URL.revokeObjectURL(url)
    ElMessage.success('下载成功')
  } catch (error) {
    ElMessage.error('下载失败')
  }
}

const handleView = (row: any) => {
  ElMessage.info(`预览报表: ${row.reportType}`)
}

onMounted(() => {
  loadProjects()
  loadReports()
})
</script>

<style scoped>
.reports-page {
  width: 100%;
}
</style>

