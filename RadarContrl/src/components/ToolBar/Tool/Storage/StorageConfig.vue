<template>
  <section id="idstorageconfig" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em" height="1em" fill="currentColor" viewBox="0 0 1024 1024">
              <path d="M832 64H192c-17.7 0-32 14.3-32 32v832c0 17.7 14.3 32 32 32h640c17.7 0 32-14.3 32-32V96c0-17.7-14.3-32-32-32z m-40 824H232V687h560v201z m0-263H232V424h560v201z m0-263H232V161h560v201z"></path>
            </svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;存储配置</span>
      </template>
      <template v-slot:dragger-content>
        <!-- 按钮区域 -->
        <a-row type="flex" :gutter="8" align="middle" style="margin-bottom: 10px;">
          <a-col :span="12">
            <a-button class="custom-ant-btn custom-btn" type="default" ghost block @click="loadDiskInfo">
              刷新磁盘信息
            </a-button>
          </a-col>
          <a-col :span="12">
            <a-button class="custom-ant-btn custom-btn" type="primary" ghost block @click="saveConfig">
              保存配置
            </a-button>
          </a-col>
        </a-row>

        <a-row class="custom-row">
          <el-form label-width="140px">
            <!-- 项目选择 -->
            <el-form-item label="项目">
              <el-select v-model="store.projectInfo.projectSelected" @change="onProjectChange" placeholder="选择项目">
                <el-option v-for="item in store.projectInfo.projectData" :key="item.projectId" 
                  :label="item.projectName" :value="item.projectId" />
              </el-select>
            </el-form-item>

            <!-- 磁盘使用情况 -->
            <el-divider content-position="left">磁盘使用情况</el-divider>

            <el-card shadow="hover" style="margin-bottom: 15px;">
              <template #header>
                <div style="display: flex; justify-content: space-between; align-items: center;">
                  <span>磁盘信息</span>
                  <el-tag :type="diskInfo.usedPercent > 90 ? 'danger' : diskInfo.usedPercent > 80 ? 'warning' : 'success'">
                    {{ diskInfo.usedPercent }}% 已使用
                  </el-tag>
                </div>
              </template>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-statistic title="总空间" :value="(diskInfo.totalSpace / 1024 / 1024 / 1024).toFixed(2)" suffix="GB" />
                </el-col>
                <el-col :span="12">
                  <el-statistic title="已用空间" :value="(diskInfo.usedSpace / 1024 / 1024 / 1024).toFixed(2)" suffix="GB" />
                </el-col>
              </el-row>
              <el-row :gutter="16" style="margin-top: 20px;">
                <el-col :span="12">
                  <el-statistic title="剩余空间" :value="(diskInfo.freeSpace / 1024 / 1024 / 1024).toFixed(2)" suffix="GB" />
                </el-col>
                <el-col :span="12">
                  <el-statistic title="驱动器" :value="diskInfo.driveName" />
                </el-col>
              </el-row>
              <el-progress :percentage="diskInfo.usedPercent" :status="diskInfo.usedPercent > 90 ? 'exception' : undefined" style="margin-top: 20px;" />
            </el-card>

            <!-- 自动清理配置 -->
            <el-divider content-position="left">自动清理配置</el-divider>

            <el-form-item label="启用自动清理">
              <el-switch v-model="storageConfig.autoCleanupEnable" active-text="开启" inactive-text="关闭" />
              <el-text type="info" size="small" style="margin-left: 10px;">
                开启后磁盘空间不足时自动清理旧数据
              </el-text>
            </el-form-item>

            <div v-show="storageConfig.autoCleanupEnable">
              <el-alert title="自动清理说明" type="warning" :closable="false" style="margin-bottom: 15px;">
                <template #default>
                  <p style="margin: 0;">当磁盘使用率超过阈值时，自动删除超过保留期的数据</p>
                  <p style="margin: 5px 0 0 0;">请谨慎配置，删除的数据无法恢复！</p>
                </template>
              </el-alert>

              <el-form-item label="磁盘空间阈值">
                <el-slider v-model="storageConfig.diskThresholdPercent" :min="50" :max="95" 
                           :marks="{ 50: '50%', 70: '70%', 80: '80%', 90: '90%', 95: '95%' }" 
                           style="width: 100%" />
                <el-input-number v-model="storageConfig.diskThresholdPercent" :min="50" :max="95" 
                                 style="width: 100%; margin-top: 10px;" />
                <el-text type="warning" size="small">
                  磁盘使用率超过此值时触发自动清理
                </el-text>
              </el-form-item>

              <el-form-item label="数据保留天数">
                <el-input-number v-model="storageConfig.dataRetentionDays" :min="7" :max="3650" 
                                 placeholder="天" style="width: 100%" />
                <el-text type="info" size="small">
                  超过此天数的数据将被清理（建议：90天）
                </el-text>
              </el-form-item>

              <el-form-item label="清理数据类型">
                <el-checkbox-group v-model="deleteDataTypes">
                  <el-checkbox label="raw">原始雷达数据</el-checkbox>
                  <el-checkbox label="image">图像数据</el-checkbox>
                  <el-checkbox label="analysis">分析结果数据</el-checkbox>
                </el-checkbox-group>
              </el-form-item>
            </div>

            <!-- 图像压缩配置 -->
            <el-divider content-position="left">图像压缩配置</el-divider>

            <el-form-item label="启用图像压缩">
              <el-switch v-model="storageConfig.imageCompressionEnable" active-text="开启" inactive-text="关闭" />
              <el-text type="info" size="small" style="margin-left: 10px;">
                压缩图像可节省存储空间
              </el-text>
            </el-form-item>

            <div v-show="storageConfig.imageCompressionEnable">
              <el-form-item label="图像质量">
                <el-slider v-model="storageConfig.imageQuality" :min="1" :max="100" 
                           :marks="{ 1: '最低', 50: '中等', 85: '高', 100: '最高' }"
                           style="width: 100%" />
                <el-input-number v-model="storageConfig.imageQuality" :min="1" :max="100" 
                                 style="width: 100%; margin-top: 10px;" />
                <el-text type="info" size="small">
                  质量越高，文件越大（建议：85）
                </el-text>
              </el-form-item>
            </div>

            <!-- 存储路径配置 -->
            <el-divider content-position="left">存储路径配置</el-divider>

            <el-form-item label="数据存储路径">
              <el-input v-model="storageConfig.storagePath" placeholder="./Data">
                <template #append>
                  <el-button @click="selectPath('storage')">浏览</el-button>
                </template>
              </el-input>
            </el-form-item>

            <!-- 自动备份配置 -->
            <el-divider content-position="left">自动备份配置</el-divider>

            <el-form-item label="启用自动备份">
              <el-switch v-model="storageConfig.autoBackupEnable" active-text="开启" inactive-text="关闭" />
              <el-text type="info" size="small" style="margin-left: 10px;">
                定期备份数据库到指定位置
              </el-text>
            </el-form-item>

            <div v-show="storageConfig.autoBackupEnable">
              <el-form-item label="备份路径">
                <el-input v-model="storageConfig.backupPath" placeholder="./Backup">
                  <template #append>
                    <el-button @click="selectPath('backup')">浏览</el-button>
                  </template>
                </el-input>
              </el-form-item>

              <el-form-item label="备份间隔">
                <el-input-number v-model="storageConfig.backupIntervalDays" :min="1" :max="30" 
                                 placeholder="天" style="width: 100%" />
                <el-text type="info" size="small">
                  每隔N天自动备份一次（建议：7天）
                </el-text>
              </el-form-item>

              <el-form-item label="最大备份数量">
                <el-input-number v-model="storageConfig.maxBackupCount" :min="1" :max="20" 
                                 placeholder="个" style="width: 100%" />
                <el-text type="info" size="small">
                  保留最近N个备份文件，超过自动删除旧备份
                </el-text>
              </el-form-item>
            </div>

            <!-- 操作按钮 -->
            <el-divider content-position="left">数据管理操作</el-divider>

            <el-form-item>
              <el-button type="danger" @click="confirmCleanup" :disabled="!storageConfig.autoCleanupEnable">
                立即执行数据清理
              </el-button>
              <el-text type="warning" size="small" style="margin-left: 10px;">
                根据当前配置立即清理数据
              </el-text>
            </el-form-item>

            <el-form-item>
              <el-button type="primary" @click="confirmBackup" :disabled="!storageConfig.autoBackupEnable">
                立即执行数据备份
              </el-button>
              <el-text type="info" size="small" style="margin-left: 10px;">
                手动触发数据库备份
              </el-text>
            </el-form-item>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
import { defineComponent, ref, onMounted, reactive, watch, computed } from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import axios from 'axios';
import { useMapStore } from "@/store/index.js";
import { ApiRadar } from "@/axios/apiRadar.js";
import { showMessage } from "@/utils/tools.js";
import { ElMessageBox } from 'element-plus';

defineComponent({
  name: "storageconfig",
});

const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
});

const store = useMapStore();

// 存储配置
const storageConfig = reactive({
  autoCleanupEnable: false,
  diskThresholdPercent: 80,
  dataRetentionDays: 90,
  deleteRawData: false,
  deleteImageData: false,
  deleteAnalysisData: false,
  imageQuality: 85,
  imageCompressionEnable: true,
  storagePath: './Data',
  backupPath: './Backup',
  autoBackupEnable: false,
  backupIntervalDays: 7,
  maxBackupCount: 5
});

// 磁盘信息
const diskInfo = reactive({
  totalSpace: 0,
  freeSpace: 0,
  usedSpace: 0,
  usedPercent: 0,
  driveName: 'C:',
  driveFormat: 'NTFS',
  isReady: true
});

// 删除数据类型
const deleteDataTypes = computed({
  get() {
    const types = [];
    if (storageConfig.deleteRawData) types.push('raw');
    if (storageConfig.deleteImageData) types.push('image');
    if (storageConfig.deleteAnalysisData) types.push('analysis');
    return types;
  },
  set(value) {
    storageConfig.deleteRawData = value.includes('raw');
    storageConfig.deleteImageData = value.includes('image');
    storageConfig.deleteAnalysisData = value.includes('analysis');
  }
});

// 加载磁盘信息
const loadDiskInfo = async () => {
  if (!store.radarInfo.projectId) {
    showMessage('请先选择项目', 'warning');
    return;
  }

  try {
    const res = await axios.get(ApiRadar.apiUrl + '/api/storage/diskinfo/' + store.radarInfo.projectId);
    console.log('磁盘信息:', res);

    if (res.data && res.data.code === 200 && res.data.data) {
      Object.assign(diskInfo, res.data.data);
    }
  } catch (err) {
    console.error('获取磁盘信息失败:', err);
  }
};

// 加载存储配置
const loadStorageConfig = async () => {
  if (!store.radarInfo.projectId) {
    console.warn('项目ID为空');
    return;
  }

  try {
    const res = await axios.get(ApiRadar.apiUrl + '/api/storage/config/' + store.radarInfo.projectId);
    console.log('存储配置:', res);

    if (res.data && res.data.code === 200 && res.data.data) {
      Object.assign(storageConfig, res.data.data);
    } else if (res.data && res.data.code === 404) {
      console.log('未找到存储配置，使用默认值');
    }
  } catch (err) {
    console.error('加载存储配置失败:', err);
  }
};

// 项目切换
const onProjectChange = async () => {
  console.log('项目切换:', store.projectInfo.projectSelected);
  store.radarInfo.projectId = String(store.projectInfo.projectSelected);
  await loadStorageConfig();
  await loadDiskInfo();
};

// 保存配置
const saveConfig = async () => {
  if (!store.radarInfo.projectId) {
    showMessage('请先选择项目', 'error');
    return;
  }

  const params = {
    projectId: store.radarInfo.projectId,
    autoCleanupEnable: storageConfig.autoCleanupEnable,
    diskThresholdPercent: storageConfig.diskThresholdPercent,
    dataRetentionDays: storageConfig.dataRetentionDays,
    deleteRawData: storageConfig.deleteRawData,
    deleteImageData: storageConfig.deleteImageData,
    deleteAnalysisData: storageConfig.deleteAnalysisData,
    imageQuality: storageConfig.imageQuality,
    imageCompressionEnable: storageConfig.imageCompressionEnable,
    storagePath: storageConfig.storagePath,
    backupPath: storageConfig.backupPath,
    autoBackupEnable: storageConfig.autoBackupEnable,
    backupIntervalDays: storageConfig.backupIntervalDays,
    maxBackupCount: storageConfig.maxBackupCount
  };

  console.log('保存存储配置:', params);

  try {
    const res = await axios.post(ApiRadar.apiUrl + '/api/storage/config', params);
    console.log('保存响应:', res);

    if (res.data && res.data.code === 200) {
      showMessage('存储配置保存成功');
    } else {
      showMessage(res.data?.message || '保存失败', 'error');
    }
  } catch (err) {
    console.error('保存存储配置失败:', err);
    showMessage('保存失败: ' + err.message, 'error');
  }
};

// 选择路径（占位）
const selectPath = (type) => {
  showMessage('路径选择功能待实现', 'info');
};

// 确认清理
const confirmCleanup = () => {
  ElMessageBox.confirm(
    `确定要执行数据清理吗？将删除 ${storageConfig.dataRetentionDays} 天前的数据，此操作不可撤销！`,
    '确认清理',
    {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    }
  ).then(async () => {
    await executeCleanup();
  }).catch(() => {
    showMessage('已取消清理', 'info');
  });
};

// 执行清理
const executeCleanup = async () => {
  try {
    const res = await axios.post(ApiRadar.apiUrl + '/api/storage/cleanup', {
      projectId: store.radarInfo.projectId
    });

    console.log('清理响应:', res);

    if (res.data && res.data.code === 200) {
      showMessage(`数据清理完成！共删除 ${res.data.data.deletedCount} 条记录`, 'success');
      await loadDiskInfo(); // 刷新磁盘信息
    } else {
      showMessage(res.data?.message || '清理失败', 'error');
    }
  } catch (err) {
    console.error('执行清理失败:', err);
    showMessage('清理失败: ' + err.message, 'error');
  }
};

// 确认备份
const confirmBackup = () => {
  ElMessageBox.confirm(
    '确定要立即备份数据库吗？',
    '确认备份',
    {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'info',
    }
  ).then(async () => {
    await executeBackup();
  }).catch(() => {
    showMessage('已取消备份', 'info');
  });
};

// 执行备份
const executeBackup = async () => {
  showMessage('备份功能待实现', 'info');
};

onMounted(async () => {
  console.log('StorageConfig.onMounted');

  if (store.projectInfo.projectSelected) {
    store.radarInfo.projectId = String(store.projectInfo.projectSelected);
    await loadStorageConfig();
    await loadDiskInfo();
  }
});

// 监听项目切换
watch(() => store.projectInfo.projectSelected, async (newVal) => {
  if (newVal) {
    store.radarInfo.projectId = String(newVal);
    await loadStorageConfig();
    await loadDiskInfo();
  }
});
</script>

<style scoped>
#idstorageconfig {
  height: 100%;
  width: 100%;
  overflow-y: auto;
}

:deep(.el-statistic__head) {
  font-size: 14px;
  color: #666;
}

:deep(.el-statistic__content) {
  font-size: 24px;
  font-weight: bold;
}
</style>

