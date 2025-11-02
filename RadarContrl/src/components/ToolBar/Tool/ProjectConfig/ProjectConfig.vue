<template>
  <section id="idprojectconfig" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('common.project')+$t('common.conf')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn" type="dashed" ghost block @click="setCamera">{{$t('backend.initCameraSetting')}}</a-button>
          <a-button class="executeBtn" type="primary" ghost block @click="commitUpdate">{{$t('common.commitChange')}}</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <el-form-item :label="$t('common.project')">
              <el-col :span="14">
                <el-select v-model="store.projectInfo.projectSelected"  @change="staticDataBind()">
                  <el-option v-for="item in store.projectInfo.projectData" :label="item.projectName" :value="item.projectId" :key="item.projectId"/>
                </el-select>
              </el-col>
              &nbsp;
              <el-col :span="4">
                <a-tooltip :title="$t('common.add')+$t('common.project')">
                  <a-button shape="circle" :icon="h(PlusOutlined)" @click="store.toolbarcontent = 'addproject'"/>
                </a-tooltip>
              </el-col>
              &nbsp;
              <el-col :span="4">
                <a-tooltip :title="$t('common.delete')+$t('common.project')">
                  <el-popconfirm
                      :title="$t('backend.delProjectTip')"
                      :confirm-button-text="$t('common.delete')"
                      :cancel-button-text="$t('common.cancel')"
                      @confirm="deleteProject"
                      @cancel="showMessage($t('common.operateCancel'),'info');"
                  >
                    <template #reference>
                      <a-button shape="circle" :icon="h(DeleteOutlined)"/>
                    </template>
                  </el-popconfirm>

                </a-tooltip>
              </el-col>
            </el-form-item>
            <el-form-item :label="$t('common.project')+'Id'">
                <el-input v-model="currentProjectInfo.projectId" disabled/>
            </el-form-item>
            <el-form-item :label="$t('common.project')+$t('common.name')">
              <el-input v-model="currentProjectInfo.projectName"/>
            </el-form-item>
            <el-form-item :label="$t('common.project')+$t('common.describe')">
              <el-input v-model="currentProjectInfo.description"/>
            </el-form-item>
            
            <el-divider content-position="left">地理位置信息</el-divider>
            <el-form-item :label="$t('common.longitude')">
              <el-input-number v-model="currentProjectInfo.longitude" :precision="6" :step="0.000001" style="width: 100%" placeholder="120.6"/>
            </el-form-item>
            <el-form-item :label="$t('common.latitude')">
              <el-input-number v-model="currentProjectInfo.latitude" :precision="6" :step="0.000001" style="width: 100%" placeholder="31.3"/>
            </el-form-item>
            <el-form-item label="高程(m)">
              <el-input-number v-model="currentProjectInfo.elevation" :precision="2" :step="0.1" style="width: 100%" placeholder="0"/>
            </el-form-item>
            
            <el-divider content-position="left">联系信息</el-divider>
            <el-form-item :label="$t('common.contact')">
              <el-input v-model="currentProjectInfo.contactPerson"/>
            </el-form-item>
            <el-form-item :label="$t('common.phone')">
              <el-input v-model="currentProjectInfo.contactPhone"/>
            </el-form-item>
            <el-form-item :label="$t('common.email')">
              <el-input v-model="currentProjectInfo.contactEmail"/>
            </el-form-item>
            
            <!-- ✅ 新增：设备列表展示 -->
            <el-divider content-position="left">项目设备列表</el-divider>
            <el-form-item label="设备管理">
              <a-space direction="vertical" style="width: 100%">
                <a-button type="dashed" ghost block @click="store.toolbarcontent = 'adddevice'">
                  <template #icon><PlusOutlined /></template>
                  添加设备
                </a-button>
                <a-button type="default" ghost block @click="store.toolbarcontent = 'deviceList'">
                  <template #icon><UnorderedListOutlined /></template>
                  查看设备列表
                </a-button>
              </a-space>
            </el-form-item>
            
            <!-- 简化设备列表展示 -->
            <el-form-item label="当前设备">
              <div v-if="currentProjectDevices.length === 0" style="color: #888; text-align: center; padding: 10px;">
                该项目暂无设备
              </div>
              <a-list v-else :data-source="currentProjectDevices" size="small" :split="false">
                <template #renderItem="{ item }">
                  <a-list-item style="padding: 8px 0;">
                    <a-space>
                      <a-tag :color="item.status === 'Online' ? 'green' : 'gray'" size="small">
                        {{ item.status || 'Offline' }}
                      </a-tag>
                      <span style="color: #fff;">{{ item.deviceName }}</span>
                      <span style="color: #888; font-size: 12px;">({{ item.deviceId }})</span>
                    </a-space>
                  </a-list-item>
                </template>
              </a-list>
            </el-form-item>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-28 / 18:06:05 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw, h} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon, {DeleteOutlined, EditOutlined, PlusOutlined, UnorderedListOutlined} from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {CesiumUtils} from "@/utils/CesiumUtils.js";
import {ApiRadar} from '@/axios/apiRadar';
import {showMessage} from "@/utils/tools.js";
import {useRouter} from "vue-router";
import {projectDataInit, staticDataBind} from "@/utils/radartool.js";
import {ElMessageBox} from "element-plus";
import {useI18n} from "vue-i18n";
/*-- name --*/
defineComponent({
  name: "projectconfig",
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
const {t} = useI18n();

/*-- computed --*/
// 当前选中项目的信息
const currentProjectInfo = computed(() => {
  const currentProject = store.projectInfo.projectData.find(
    p => p.projectId === store.projectInfo.projectSelected
  );
  return currentProject || {
    projectId: '',
    projectName: '',
    description: '',
    longitude: 0,
    latitude: 0,
    elevation: 0,
    contactPerson: '',
    contactPhone: '',
    contactEmail: '',
    devices: []
  };
});

// 当前选中项目的设备列表
const currentProjectDevices = computed(() => {
  return currentProjectInfo.value.devices || [];
});

/*-- methods --*/
const deleteProject = ()=>{
  ElMessageBox.confirm(
      (store.sysinfo.config.language==='0'?'当前项目将被删除，所有与项目有关的雷达、监测位、预警规则和联系人将会被删除，请再次确认':'The current project will be deleted, and all radars, monitoring positions, warning rules, and contacts related to the project will be deleted. Please confirm again'),
      t('common.warn'),
      {
        confirmButtonText: t('common.commitDelete'),
        cancelButtonText: t('common.operateCancel'),
        type: 'warning',
      }
  )
      .then(() => {
        ApiRadar.DeleteProject(store.radarInfo.projectId).then(res=>{
          store.projectInfo.projectSelected = store.projectInfo.projectData[0].projectId;
          showMessage((store.sysinfo.config.language==='0'?'该项目已经删除':'The project has been deleted'))
          ApiRadar.AddRadarLog("删除项目"+store.radarInfo.projectId,store.sysinfo.config.username,store.sysinfo.address,store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
          projectDataInit();
        })
      })
      .catch(() => {
        showMessage(t('common.operateCancel'),'info');
      })
}
const commitUpdate = async ()=>{
  if (!currentProjectInfo.value.projectName) {
    showMessage('项目名称不能为空', 'warning');
    return;
  }
  
  try {
    // ✅ 使用新的字段结构提交
    const res = await ApiRadar.UpdateProject(currentProjectInfo.value.projectId, {
      projectName: currentProjectInfo.value.projectName,
      projectDescribe: currentProjectInfo.value.description,
      contact: currentProjectInfo.value.contactPerson,
      phone: currentProjectInfo.value.contactPhone,
      email: currentProjectInfo.value.contactEmail,
      lon: currentProjectInfo.value.longitude,
      lat: currentProjectInfo.value.latitude,
      alt: currentProjectInfo.value.elevation
    });
    
    if (res.data.code === 200){
      showMessage(t('common.operateSuccess'));
      ApiRadar.AddRadarLog("修改项目信息"+currentProjectInfo.value.projectName, store.sysinfo.config.username, store.sysinfo.address, store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
      
      // ✅ 重新加载项目数据
      const projectRes = await ApiRadar.getRadarData();
      if (projectRes.data && projectRes.data.data) {
        // ✅ 映射项目数据（与projectDataInit保持一致）
        const projects = projectRes.data.data || [];
        store.projectInfo.projectData = projects.map(p => ({
          projectId: p.projectId,
          projectName: p.projectName,
          id: p.projectId,
          name: p.projectName,
          description: p.description,
          contact: p.contactPerson,
          phone: p.contactPhone,
          email: p.contactEmail,
          contactPerson: p.contactPerson,
          contactPhone: p.contactPhone,
          contactEmail: p.contactEmail,
          longitude: p.longitude,
          latitude: p.latitude,
          elevation: p.elevation,
          devices: p.devices || [] // 初始化devices数组
        }));
        
        // ✅ 重新加载当前项目的设备信息
        const currentProjectId = currentProjectInfo.value.projectId;
        const devRes = await ApiRadar.getDevicesByProjectId(currentProjectId);
        
        if (devRes.data && devRes.data.code === 200 && devRes.data.data) {
          const projectIndex = store.projectInfo.projectData.findIndex(
            p => p.projectId === currentProjectId
          );
          
          if (projectIndex !== -1) {
            // ✅ 映射设备数据（与其他地方保持一致）
            store.projectInfo.projectData[projectIndex].devices = devRes.data.data.map(d => {
              let deviceTypeStr = 'ER';
              
              if (d.deviceTypeCode) {
                switch (d.deviceTypeCode) {
                  case 1: deviceTypeStr = 'ER'; break;
                  case 2: deviceTypeStr = 'MIMOLITE'; break;
                  case 5: deviceTypeStr = 'ER'; break;
                  case 6: deviceTypeStr = 'ER'; break;
                  case 7: deviceTypeStr = 'MIMOLITE'; break;
                  case 8: deviceTypeStr = 'MIMOLITE'; break;
                  default: deviceTypeStr = 'ER'; break;
                }
              }
              
              return {
                deviceId: d.deviceId,
                deviceName: d.deviceName,
                id: d.deviceId,
                name: d.deviceName,
                type: deviceTypeStr,
                status: d.status,
                coordinates: [d.longitude || 0, d.latitude || 0, d.elevation || 0],
                longitude: d.longitude || 0,
                latitude: d.latitude || 0,
                elevation: d.elevation || 0,
                factoryId: d.factoryId || '',
                orientation: d.orientation || 0,
                ipAddress: d.ipAddress,
                port: d.port,
                params: d.params || {},
                dataVersion: (d.params && d.params.dataVersion) ? String(d.params.dataVersion) : '0',
                algorithmParam: d.algorithmParam || {}
              };
            });
            
            console.log('ProjectConfig保存后设备列表已更新:', store.projectInfo.projectData[projectIndex].devices);
          }
        }
      }
      
      // ✅ 重新绑定数据
      staticDataBind();
    } else {
      showMessage(res.data.message || t('common.operateFailed'),'error');
    }
  } catch (error) {
    console.error('更新项目失败:', error);
    showMessage('更新项目失败: ' + (error.response?.data?.message || error.message), 'error');
  }
}
const setCamera=()=>{
  const params = CesiumUtils.GetCameraParams();
  ApiRadar.addCameraParams(store.radarInfo.projectId,params.longitude,
  params.latitude,params.altitude,params.heading,params.pitch,params.roll).then((res)=>{
    showMessage(t('common.setSuccess'));
    ApiRadar.AddRadarLog("设置初始化场景",store.sysinfo.config.username,store.sysinfo.address,store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
  })
}
/*-- events --*/
onMounted(async () => {
  if (window.config){
    store.radarInfo.projectConfig['name'] = config['projectName'];
    store.radarInfo.projectConfig['description'] = config['projectDesc'];
    store.radarInfo.projectConfig['contact'] = config['projectContact'];
    store.radarInfo.projectConfig['phone'] = config['projectPhone'];
    store.radarInfo.projectConfig['email'] = config['projectEmail'];
  }
  
  // ✅ 修复：自动加载当前项目的设备列表
  if (store.projectInfo.projectSelected) {
    try {
      const res = await ApiRadar.getDevicesByProjectId(store.projectInfo.projectSelected);
      console.log('ProjectConfig加载设备:', res);
      
      if (res.data && res.data.code === 200 && res.data.data) {
        const projectIndex = store.projectInfo.projectData.findIndex(
          p => p.projectId === store.projectInfo.projectSelected
        );
        
        if (projectIndex !== -1) {
          // 映射设备数据
          store.projectInfo.projectData[projectIndex].devices = res.data.data.map(d => {
            let deviceTypeStr = 'ER';
            
            if (d.deviceTypeCode) {
              switch (d.deviceTypeCode) {
                case 1: deviceTypeStr = 'ER'; break;
                case 2: deviceTypeStr = 'MIMOLITE'; break;
                case 5: deviceTypeStr = 'ER'; break;
                case 6: deviceTypeStr = 'ER'; break;
                case 7: deviceTypeStr = 'MIMOLITE'; break;
                case 8: deviceTypeStr = 'MIMOLITE'; break;
                default: deviceTypeStr = 'ER'; break;
              }
            }
            
            return {
              deviceId: d.deviceId,
              deviceName: d.deviceName,
              id: d.deviceId,
              name: d.deviceName,
              type: deviceTypeStr,
              status: d.status,
              coordinates: [d.longitude || 0, d.latitude || 0, d.elevation || 0],
              longitude: d.longitude || 0,
              latitude: d.latitude || 0,
              elevation: d.elevation || 0,
              factoryId: d.factoryId || '',
              orientation: d.orientation || 0,
              ipAddress: d.ipAddress,
              port: d.port,
              params: d.params || {},
              dataVersion: (d.params && d.params.dataVersion) ? String(d.params.dataVersion) : '0',
              algorithmParam: d.algorithmParam || {}
            };
          });
          
          console.log('ProjectConfig设备列表已更新:', store.projectInfo.projectData[projectIndex].devices);
        }
      }
    } catch (error) {
      console.error('ProjectConfig加载设备失败:', error);
    }
  }
});
</script>

<style scoped>
#idprojectconfig {
  height: 100%;
  width: 100%;
}
</style>