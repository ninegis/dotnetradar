<template>
  <section id="iddeviceconfig" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg t="1718951875714" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="6055" width="1em" height="1em"><path d="M594.04 959.5H142.31c-41.14 0-74.6-33.46-74.6-74.6V367.47c0-21.18 17.17-38.36 38.36-38.36s38.36 17.17 38.36 38.36v515.32h449.62c21.18 0 38.36 17.17 38.36 38.36-0.01 21.18-17.18 38.35-38.37 38.35zM783.71 569.26c-21.18 0-38.36-17.17-38.36-38.36V141.21H365.4c-21.18 0-38.36-17.17-38.36-38.36S344.22 64.5 365.4 64.5h382.07c41.14 0 74.6 33.46 74.6 74.6v391.8c0 21.19-17.18 38.36-38.36 38.36z m-36.24-428.05h0.12-0.12z" p-id="6056" fill="#ffffff"></path><path d="M360.67 438.06H130.53c-21.18 0-38.36-17.17-38.36-38.36s17.17-38.36 38.36-38.36h230.14c21.18 0 38.36 17.17 38.36 38.36s-17.18 38.36-38.36 38.36zM917.93 824.76h-268.5c-21.18 0-38.36-17.17-38.36-38.36 0-21.18 17.17-38.36 38.36-38.36h268.5c21.18 0 38.36 17.17 38.36 38.36 0 21.19-17.18 38.36-38.36 38.36z" p-id="6057" fill="#ffffff"></path><path d="M783.68 959.01c-21.18 0-38.36-17.17-38.36-38.36v-268.5c0-21.18 17.17-38.36 38.36-38.36 21.18 0 38.36 17.17 38.36 38.36v268.5c0 21.19-17.18 38.36-38.36 38.36zM364.04 437.23c-21.18 0-38.36-17.17-38.36-38.36V110.64c0-21.18 17.17-38.36 38.36-38.36 21.18 0 38.36 17.17 38.36 38.36v288.23c0 21.19-17.17 38.36-38.36 38.36z" p-id="6058" fill="#ffffff"></path><path d="M106.24 398.98c-9.85 0-19.7-3.77-27.19-11.31-14.94-15.02-14.88-39.31 0.14-54.25L338.35 75.66c15.02-14.93 39.3-14.88 54.25 0.14 14.94 15.02 14.88 39.31-0.14 54.25L133.3 387.81c-7.49 7.45-17.27 11.17-27.06 11.17z" p-id="6059" fill="#ffffff"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('backend.addDevice')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn custom-btn" type="primary" ghost block @click="commitUpdate">{{$t('common.commitChange')}}</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <el-form-item :label="$t('backend.projectSelect')">
              <el-select v-model="form.projectId">
                <el-option
                    v-for="item in store.projectInfo.projectData"
                    :key="item['projectId']"
                    :label="item['projectName']"
                    :value="item['projectId']"
                />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('legend.radar')+$t('legend.radar')">
              <el-select
                  v-model="form.type"
                  :placeholder="$t('backend.radarSelectPh')"
              >
                <el-option key="ARCSAR_" :label="$t('common.oriRadar')" value="ER"/>
                <el-option key="MIMOLITE_" :label="$t('common.mimoRadar')" value="MIMOLITE"/>
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('legend.radar')+'Id'">
              <el-col :span="19">
                <el-input v-model="form.deviceId"/>
              </el-col>
              &nbsp;
              <el-col :span="4">
                <a-tooltip :title="$t('common.random')+$t('common.generate')+$t('common.device')+'ID'">
                  <a-button shape="circle" :icon="h(SyncOutlined)" @click="generateProductId"/>
                </a-tooltip>
              </el-col>
            </el-form-item>
            <el-form-item :label="$t('map.radarName')">
              <el-input v-model="form.deviceName"/>
            </el-form-item>
            <el-form-item :label="$t('common.intranet')+'IP'">
              <el-input v-model="form.ipv4" placeholder="127.0.0.1"/>
            </el-form-item>
            <el-form-item label="通信端口">
              <el-input-number v-model="form.port" :min="1" :max="65535" placeholder="8888" style="width: 100%"/>
            </el-form-item>
            <el-form-item :label="$t('backend.factoryId')">
              <el-input v-model="form.factoryId" placeholder="出厂ID"/>
            </el-form-item>
            <el-form-item :label="$t('backend.radarOriAngle')">
              <el-input-number v-model="form.orientation" :min="0" :max="360" placeholder="0" style="width: 100%"/>
            </el-form-item>
            <el-form-item label="MQTT主题">
              <el-input v-model="form.mqttTopic" placeholder="自动生成"/>
            </el-form-item>
            <el-form-item label="设备状态">
              <el-select v-model="form.status" placeholder="Offline">
                <el-option label="在线" value="Online"/>
                <el-option label="离线" value="Offline"/>
                <el-option label="维护中" value="Maintenance"/>
                <el-option label="故障" value="Error"/>
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('common.longitude')">
              <el-input v-model="form.lon"/>
            </el-form-item>
            <el-form-item :label="$t('common.latitude')">
              <el-input v-model="form.lat"/>
            </el-form-item>
            <el-form-item :label="$t('common.altitude')">
              <el-input v-model="form.alt" placeholder="0"/>
            </el-form-item>
            <el-form-item label="设备描述">
              <el-input v-model="form.description" type="textarea" :rows="3" placeholder="设备备注信息（可选）"/>
            </el-form-item>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-29 / 12:42:00 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw, h, watch} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon, {EditOutlined, SyncOutlined} from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {showMessage} from "@/utils/tools.js";
import {getUUID, staticDataBind} from "@/utils/radartool.js";
import Layer from "@/components/ToolBar/Layer/Layer.vue";
import {CommonUtils} from "@/utils/CommonUtils.js";
/*-- name --*/
defineComponent({
  name: "messagetemplate",
});
/*-- props  --*/
const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
});
/*-- reactive --*/
const form = reactive({})
/*-- store --*/
const store = useMapStore();
/*-- vars --*/

/*-- methods --*/
const generateProductId=()=>{
  if (form.type===undefined){
    showMessage('需选择设备类型','warning');
    return;
  }
  // 生成设备ID：雷达类型_日期_3位随机数
  const dateStr = new Date().toISOString().slice(0,10).replace(/-/g, ''); // yyyyMMdd
  const random3Digits = Math.floor(100 + Math.random() * 900); // 100-999
  const prefix = form.type === "ER" ? 'ARC' : 'MIMO';
  form.deviceId = `${prefix}_${dateStr}_${random3Digits}`;
  
  // ✅ 设置默认值
  form.deviceName = form.deviceName || '默认雷达';
  form.ipv4 = form.ipv4 || '127.0.0.1';
  form.port = form.port || 8888;
  // 出厂ID：首位不为0的5位随机数 (10000-99999)
  form.factoryId = form.factoryId || String(Math.floor(10000 + Math.random() * 90000));
  // 苏州坐标（经度120.6, 纬度31.3）
  form.lon = form.lon || '120.6';
  form.lat = form.lat || '31.3';
  form.alt = form.alt || '0';
  // 零点朝向
  form.orientation = form.orientation || '0';
  // MQTT主题自动生成
  form.mqttTopic = form.mqttTopic || `radar/${form.deviceId}`;
  // 默认状态
  form.status = form.status || 'Offline';
  
  showMessage(`已生成设备ID: ${form.deviceId}`, 'success');
}
const commitUpdate=()=>{
  if (form['projectId']===undefined||form['type']===undefined||form['deviceName']===undefined||!form.factoryId||!form.orientation||!form.lon||!form.lat||!form.alt){
    showMessage('需填写完整','warning');
    return;
  }
  ApiRadar.addDevice(
    form.projectId, form.deviceName, form.deviceId, form.factoryId, form.orientation, 
    form.type, form.lon, form.lat, form.alt, form.ipv4, 
    form.port, form.mqttTopic, form.status, form.description
  ).then(res=>{
    console.log('添加设备响应:', res);
    if(res.data.code === 200){
      showMessage('设备新增成功！','success');
      ApiRadar.AddRadarLog("新增雷达设备"+form.deviceName,store.sysinfo.config.username,store.sysinfo.address,store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
      ApiRadar.getRadarData().then(res=> {
        store.projectInfo.projectData = res.data.data;
        staticDataBind();
        store.toolbarcontent = 'radarParams';
      });
    } else {
      showMessage(res.data.message || '添加设备失败','error');
    }
  }).catch(error => {
    console.error('添加设备失败:', error);
    showMessage('添加设备失败: ' + (error.response?.data?.message || error.message || '未知错误'), 'error');
  })
}
/*-- events --*/
// ✅ 监听雷达类型变化，自动生成设备ID和默认值
watch(() => form.type, (newType) => {
  if (newType) {
    generateProductId();
  }
});

onMounted(() => {
  //console.log('MessageTemplate.onMounted');
});
</script>

<style scoped>
#iddeviceconfig {
  height: 100%;
  width: 100%;
}
</style>