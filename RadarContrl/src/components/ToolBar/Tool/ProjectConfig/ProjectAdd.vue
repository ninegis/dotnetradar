<template>
  <section id="iddeviceconfig" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg t="1718951875714" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="6055" width="1em" height="1em"><path d="M594.04 959.5H142.31c-41.14 0-74.6-33.46-74.6-74.6V367.47c0-21.18 17.17-38.36 38.36-38.36s38.36 17.17 38.36 38.36v515.32h449.62c21.18 0 38.36 17.17 38.36 38.36-0.01 21.18-17.18 38.35-38.37 38.35zM783.71 569.26c-21.18 0-38.36-17.17-38.36-38.36V141.21H365.4c-21.18 0-38.36-17.17-38.36-38.36S344.22 64.5 365.4 64.5h382.07c41.14 0 74.6 33.46 74.6 74.6v391.8c0 21.19-17.18 38.36-38.36 38.36z m-36.24-428.05h0.12-0.12z" p-id="6056" fill="#ffffff"></path><path d="M360.67 438.06H130.53c-21.18 0-38.36-17.17-38.36-38.36s17.17-38.36 38.36-38.36h230.14c21.18 0 38.36 17.17 38.36 38.36s-17.18 38.36-38.36 38.36zM917.93 824.76h-268.5c-21.18 0-38.36-17.17-38.36-38.36 0-21.18 17.17-38.36 38.36-38.36h268.5c21.18 0 38.36 17.17 38.36 38.36 0 21.19-17.18 38.36-38.36 38.36z" p-id="6057" fill="#ffffff"></path><path d="M783.68 959.01c-21.18 0-38.36-17.17-38.36-38.36v-268.5c0-21.18 17.17-38.36 38.36-38.36 21.18 0 38.36 17.17 38.36 38.36v268.5c0 21.19-17.18 38.36-38.36 38.36zM364.04 437.23c-21.18 0-38.36-17.17-38.36-38.36V110.64c0-21.18 17.17-38.36 38.36-38.36 21.18 0 38.36 17.17 38.36 38.36v288.23c0 21.19-17.17 38.36-38.36 38.36z" p-id="6058" fill="#ffffff"></path><path d="M106.24 398.98c-9.85 0-19.7-3.77-27.19-11.31-14.94-15.02-14.88-39.31 0.14-54.25L338.35 75.66c15.02-14.93 39.3-14.88 54.25 0.14 14.94 15.02 14.88 39.31-0.14 54.25L133.3 387.81c-7.49 7.45-17.27 11.17-27.06 11.17z" p-id="6059" fill="#ffffff"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('common.add')+$t('common.project')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn custom-btn" type="primary" ghost block @click="commitUpdate">{{$t('common.commitChange')}}</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <el-form-item :label="$t('common.project')+'id'">
                <el-input v-model="form.projectId" placeholder="自动生成(可修改)">
                  <template #append>
                    <el-button @click="form.projectId = generateProjectId()">重新生成</el-button>
                  </template>
                </el-input>
            </el-form-item>
            <el-form-item :label="$t('common.project')+$t('common.name')">
              <el-input v-model="form.projectName"/>
            </el-form-item>
            <el-form-item :label="$t('common.project')+$t('common.describe')">
              <el-input v-model="form.projectDescribe"/>
            </el-form-item>
            <el-form-item :label="$t('common.project')+$t('common.contact')">
              <el-input v-model="form.contact"/>
            </el-form-item>
            <el-form-item :label="$t('common.phone')">
              <el-input v-model="form.phone"/>
            </el-form-item>
            <el-form-item :label="$t('common.email')">
              <el-input v-model="form.email"/>
            </el-form-item>
            <el-form-item :label="$t('common.longitude')">
              <el-input :placeholder="$t('common.optional')" v-model="form.lon"/>
            </el-form-item>
            <el-form-item :label="$t('common.latitude')">
              <el-input :placeholder="$t('common.optional')" v-model="form.lat"/>
            </el-form-item>
            <el-form-item :label="$t('common.altitude')">
              <el-input :placeholder="$t('common.optional')" v-model="form.alt"/>
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
import {defineComponent, ref, onMounted, computed, reactive, toRaw, h} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon, {EditOutlined, SyncOutlined} from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {showMessage} from "@/utils/tools.js";
import {getUUID, monitorLoad, projectDataInit, staticDataBind} from "@/utils/radartool.js";
import Layer from "@/components/ToolBar/Layer/Layer.vue";
import {CommonUtils} from "@/utils/CommonUtils.js";
import {useRouter} from "vue-router";
import {CesiumUtils} from "@/utils/CesiumUtils.js";
import {useI18n} from "vue-i18n";
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
const form = reactive({
  projectId: '' // 初始化projectId
})
/*-- store --*/
const store = useMapStore();
/*-- vars --*/
const {t} = useI18n();

// 生成项目ID：KOT_日期_随机5位数
const generateProjectId = () => {
  const dateStr = new Date().toISOString().slice(0,10).replace(/-/g, ''); // yyyyMMdd
  const random5Digits = Math.floor(10000 + Math.random() * 90000); // 10000-99999
  return `KOT_${dateStr}_${random5Digits}`;
};

/*-- methods --*/
const commitUpdate=()=>{
  // 检查项目名称是否填写
  if (!form['projectName']){
    showMessage(t('backend.needFillProjectName') || '请填写项目名称','warning');
    return;
  }
  
  // 项目ID为空时，后端会自动生成：KOT_日期_随机5位数
  const projectId = form['projectId'] || '';
  
  ApiRadar.addProject(projectId,form.projectName,form.projectDescribe,form.contact,form.phone,form.email,form.lon,form.lat,form.alt)
    .then(res=>{
      console.log('添加项目响应:', res);
      if(res.data.code === 200){
        showMessage('项目创建成功！项目ID: ' + (res.data.data?.projectId || projectId),'success');
        ApiRadar.AddRadarLog("新增项目",store.sysinfo.config.username,store.sysinfo.address,store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
        store.projectInfo.projectSelected = store.projectInfo.projectData[0].projectId;
        projectDataInit();
        store.toolbarcontent = 'projectConfig';
      } else {
        showMessage(res.data.message || '添加项目失败','error');
      }
    })
    .catch(error => {
      console.error('添加项目失败:', error);
      showMessage('添加项目失败: ' + (error.response?.data?.message || error.message || '未知错误'), 'error');
    })
}
/*-- events --*/
onMounted(() => {
  // 组件挂载时自动生成项目ID
  form.projectId = generateProjectId();
  console.log('自动生成项目ID:', form.projectId);
});
</script>

<style scoped>
#iddeviceconfig {
  height: 100%;
  width: 100%;
}
</style>