<template>
  <section id="devicelist" v-show="visible">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <UnorderedListOutlined />
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;设备列表</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle" style="margin-bottom: 10px;">
          <a-col :span="18">
            <el-select v-model="selectedProjectId" placeholder="选择项目" @change="loadDevices" style="width: 100%">
              <el-option
                v-for="item in store.projectInfo.projectData"
                :key="item.projectId"
                :label="item.projectName"
                :value="item.projectId"
              />
            </el-select>
          </a-col>
          <a-col :span="6">
            <a-button type="primary" ghost @click="loadDevices" :loading="loading">
              <template #icon><ReloadOutlined /></template>
              刷新
            </a-button>
          </a-col>
        </a-row>
        
        <a-spin :spinning="loading">
          <div v-if="devices.length === 0" style="text-align: center; padding: 20px; color: #888;">
            暂无设备数据
          </div>
          <a-list v-else :data-source="devices" :split="true">
            <template #renderItem="{ item }">
              <a-list-item>
                <a-list-item-meta>
                  <template #title>
                    <a-space>
                      <a-tag :color="item.status === 'Online' ? 'green' : 'gray'">
                        {{ item.status }}
                      </a-tag>
                      <strong>{{ item.deviceName }}</strong>
                    </a-space>
                  </template>
                  <template #description>
                    <div class="device-info">
                      <div><strong>设备ID:</strong> {{ item.deviceId }}</div>
                      <div><strong>设备类型:</strong> {{ item.deviceType }} ({{ item.deviceTypeCode }})</div>
                      <div><strong>IP地址:</strong> {{ item.ipAddress }}:{{ item.port }}</div>
                      <div><strong>MQTT主题:</strong> {{ item.mqttTopic || '未配置' }}</div>
                      <div><strong>位置:</strong> {{ item.location || '未设置' }}</div>
                      <div v-if="item.description"><strong>描述:</strong> {{ item.description }}</div>
                      <div><strong>最后更新:</strong> {{ formatDateTime(item.updateTime) }}</div>
                    </div>
                  </template>
                </a-list-item-meta>
                <template #actions>
                  <a-button type="link" size="small" @click="editDevice(item)">编辑</a-button>
                  <a-popconfirm
                    title="确定删除该设备？"
                    ok-text="确定"
                    cancel-text="取消"
                    @confirm="deleteDevice(item.deviceId)"
                  >
                    <a-button type="link" danger size="small">删除</a-button>
                  </a-popconfirm>
                </template>
              </a-list-item>
            </template>
          </a-list>
        </a-spin>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
import { defineComponent, ref, onMounted, reactive, h } from 'vue';
import { UnorderedListOutlined, ReloadOutlined } from '@ant-design/icons-vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import { useMapStore } from "@/store/index.js";
import { ApiRadar } from "@/axios/apiRadar.js";
import { showMessage } from "@/utils/tools.js";
import axios from 'axios';

defineComponent({
  name: "DeviceList",
});

const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
});

const store = useMapStore();
const devices = ref([]);
const selectedProjectId = ref('');
const loading = ref(false);

const formatDateTime = (dateTime) => {
  if (!dateTime) return '未知';
  return new Date(dateTime).toLocaleString('zh-CN');
};

const loadDevices = async () => {
  if (!selectedProjectId.value) {
    showMessage('请先选择项目', 'warning');
    return;
  }
  
  loading.value = true;
  try {
    const res = await axios.get(`${ApiRadar.apiUrl}/api/Device?projectId=${selectedProjectId.value}`);
    if (res.data.code === 200) {
      devices.value = res.data.data || [];
      showMessage(`加载了 ${devices.value.length} 个设备`, 'success');
    } else {
      showMessage(res.data.message || '加载设备失败', 'error');
    }
  } catch (error) {
    console.error('加载设备失败:', error);
    showMessage('加载设备失败: ' + (error.response?.data?.message || error.message), 'error');
  } finally {
    loading.value = false;
  }
};

const editDevice = (device) => {
  showMessage('编辑功能开发中...', 'info');
  // TODO: 打开编辑对话框，填充设备信息
};

const deleteDevice = async (deviceId) => {
  try {
    const res = await ApiRadar.DeleteDevice(deviceId);
    if (res.data.code === 200) {
      showMessage('设备删除成功', 'success');
      await loadDevices(); // 重新加载列表
    } else {
      showMessage(res.data.message || '删除设备失败', 'error');
    }
  } catch (error) {
    console.error('删除设备失败:', error);
    showMessage('删除设备失败: ' + (error.response?.data?.message || error.message), 'error');
  }
};

onMounted(() => {
  // 默认选中第一个项目
  if (store.projectInfo.projectData && store.projectInfo.projectData.length > 0) {
    selectedProjectId.value = store.projectInfo.projectSelected || store.projectInfo.projectData[0].projectId;
    loadDevices();
  }
});
</script>

<style scoped>
#devicelist {
  height: 100%;
  width: 100%;
}

.device-info {
  font-size: 12px;
  color: #666;
  line-height: 1.8;
}

.device-info div {
  margin-bottom: 4px;
}

:deep(.ant-list-item) {
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

:deep(.ant-list-item-meta-title) {
  color: #fff;
}

:deep(.ant-list-item-meta-description) {
  color: #ccc;
}
</style>

