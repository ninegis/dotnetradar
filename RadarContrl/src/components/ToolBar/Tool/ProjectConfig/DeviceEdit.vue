<template>
  <section id="deviceedit" v-show="visible">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <EditOutlined />
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;编辑设备</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn custom-btn" type="primary" ghost block @click="commitUpdate">保存修改</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <el-form-item label="项目">
              <el-select v-model="form.projectId" disabled>
                <el-option
                  v-for="item in store.projectInfo.projectData"
                  :key="item.projectId"
                  :label="item.projectName"
                  :value="item.projectId"
                />
              </el-select>
            </el-form-item>
            
            <el-form-item label="设备ID">
              <el-input v-model="form.deviceId" disabled placeholder="设备ID不可修改"/>
            </el-form-item>
            
            <el-form-item label="设备名称">
              <el-input v-model="form.deviceName" placeholder="设备名称"/>
            </el-form-item>
            
            <el-form-item label="设备类型">
              <el-select v-model="form.deviceType" disabled>
                <el-option label="圆弧雷达" value="ER"/>
                <el-option label="MIMO雷达" value="MIMOLITE"/>
              </el-select>
            </el-form-item>
            
            <el-form-item label="IP地址">
              <el-input v-model="form.ipAddress" placeholder="127.0.0.1"/>
            </el-form-item>
            
            <el-form-item label="通信端口">
              <el-input-number v-model="form.port" :min="1" :max="65535" placeholder="8888" style="width: 100%"/>
            </el-form-item>
            
            <el-divider>雷达特有信息</el-divider>
            
            <el-form-item label="出厂ID">
              <el-input v-model="form.factoryId" placeholder="出厂ID"/>
            </el-form-item>
            
            <el-form-item label="零点朝向(度)">
              <el-input-number v-model="form.orientation" :min="0" :max="360" placeholder="0" style="width: 100%"/>
            </el-form-item>
            
            <el-divider>地理位置</el-divider>
            
            <el-form-item label="经度">
              <el-input-number v-model="form.longitude" :precision="6" :step="0.000001" style="width: 100%" placeholder="120.6"/>
            </el-form-item>
            
            <el-form-item label="纬度">
              <el-input-number v-model="form.latitude" :precision="6" :step="0.000001" style="width: 100%" placeholder="31.3"/>
            </el-form-item>
            
            <el-form-item label="高度(m)">
              <el-input-number v-model="form.elevation" :precision="2" :step="0.1" style="width: 100%" placeholder="0"/>
            </el-form-item>
            
            <el-form-item label="MQTT主题">
              <el-input v-model="form.mqttTopic" placeholder="radar/xxx"/>
            </el-form-item>
            
            <el-form-item label="设备状态">
              <el-select v-model="form.status">
                <el-option label="在线" value="Online"/>
                <el-option label="离线" value="Offline"/>
                <el-option label="维护中" value="Maintenance"/>
                <el-option label="故障" value="Error"/>
              </el-select>
            </el-form-item>
            
            <el-form-item label="位置信息">
              <el-input v-model="form.location" placeholder="设备安装位置"/>
            </el-form-item>
            
            <el-form-item label="设备描述">
              <el-input v-model="form.description" type="textarea" :rows="3" placeholder="设备备注信息"/>
            </el-form-item>
            
            <el-form-item label="创建时间">
              <el-input v-model="displayCreateTime" disabled/>
            </el-form-item>
            
            <el-form-item label="最后更新">
              <el-input v-model="displayUpdateTime" disabled/>
            </el-form-item>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
import { defineComponent, ref, onMounted, computed, reactive, h } from 'vue';
import { EditOutlined } from '@ant-design/icons-vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import { useMapStore } from "@/store/index.js";
import { ApiRadar } from "@/axios/apiRadar.js";
import { showMessage } from "@/utils/tools.js";

defineComponent({
  name: "DeviceEdit",
});

const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
  deviceData: {
    type: Object,
    default: () => ({})
  }
});

const store = useMapStore();
const form = reactive({
  deviceId: '',
  projectId: '',
  deviceName: '',
  deviceType: '',
  deviceTypeCode: 0,
  ipAddress: '',
  port: 8888,
  mqttTopic: '',
  status: 'Offline',
  // 地理位置
  longitude: 0,
  latitude: 0,
  elevation: 0,
  location: '',
  // 雷达特有信息
  factoryId: '',
  orientation: 0,
  description: '',
  createTime: null,
  updateTime: null
});

const displayCreateTime = computed(() => {
  return form.createTime ? new Date(form.createTime).toLocaleString('zh-CN') : '';
});

const displayUpdateTime = computed(() => {
  return form.updateTime ? new Date(form.updateTime).toLocaleString('zh-CN') : '';
});

const loadDeviceData = (device) => {
  Object.assign(form, device);
};

const commitUpdate = async () => {
  if (!form.deviceName) {
    showMessage('设备名称不能为空', 'warning');
    return;
  }
  
  try {
    const res = await ApiRadar.UpdateDevice(form.deviceId, {
      projectId: form.projectId,
      deviceName: form.deviceName,
      type: form.deviceType,
      deviceTypeCode: form.deviceTypeCode,
      ipv4: form.ipAddress,
      port: form.port,
      mqttTopic: form.mqttTopic,
      status: form.status,
      // 地理位置
      longitude: form.longitude,
      latitude: form.latitude,
      elevation: form.elevation,
      location: form.location,
      // 雷达特有信息
      factoryId: form.factoryId,
      orientation: form.orientation,
      description: form.description
    });
    
    if (res.data.code === 200) {
      showMessage('设备更新成功', 'success');
      store.toolbarcontent = 'deviceList';
    } else {
      showMessage(res.data.message || '更新设备失败', 'error');
    }
  } catch (error) {
    console.error('更新设备失败:', error);
    showMessage('更新设备失败: ' + (error.response?.data?.message || error.message), 'error');
  }
};

onMounted(() => {
  if (props.deviceData && props.deviceData.deviceId) {
    loadDeviceData(props.deviceData);
  }
});
</script>

<style scoped>
#deviceedit {
  height: 100%;
  width: 100%;
}

.custom-row {
  max-height: 500px;
  overflow-y: auto;
}
</style>

