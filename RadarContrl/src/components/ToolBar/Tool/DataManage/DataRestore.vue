<template>
  <section id="iddeviceconfig" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg t="1718951875714" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="6055" width="1em" height="1em"><path d="M594.04 959.5H142.31c-41.14 0-74.6-33.46-74.6-74.6V367.47c0-21.18 17.17-38.36 38.36-38.36s38.36 17.17 38.36 38.36v515.32h449.62c21.18 0 38.36 17.17 38.36 38.36-0.01 21.18-17.18 38.35-38.37 38.35zM783.71 569.26c-21.18 0-38.36-17.17-38.36-38.36V141.21H365.4c-21.18 0-38.36-17.17-38.36-38.36S344.22 64.5 365.4 64.5h382.07c41.14 0 74.6 33.46 74.6 74.6v391.8c0 21.19-17.18 38.36-38.36 38.36z m-36.24-428.05h0.12-0.12z" p-id="6056" fill="#ffffff"></path><path d="M360.67 438.06H130.53c-21.18 0-38.36-17.17-38.36-38.36s17.17-38.36 38.36-38.36h230.14c21.18 0 38.36 17.17 38.36 38.36s-17.18 38.36-38.36 38.36zM917.93 824.76h-268.5c-21.18 0-38.36-17.17-38.36-38.36 0-21.18 17.17-38.36 38.36-38.36h268.5c21.18 0 38.36 17.17 38.36 38.36 0 21.19-17.18 38.36-38.36 38.36z" p-id="6057" fill="#ffffff"></path><path d="M783.68 959.01c-21.18 0-38.36-17.17-38.36-38.36v-268.5c0-21.18 17.17-38.36 38.36-38.36 21.18 0 38.36 17.17 38.36 38.36v268.5c0 21.19-17.18 38.36-38.36 38.36zM364.04 437.23c-21.18 0-38.36-17.17-38.36-38.36V110.64c0-21.18 17.17-38.36 38.36-38.36 21.18 0 38.36 17.17 38.36 38.36v288.23c0 21.19-17.17 38.36-38.36 38.36z" p-id="6058" fill="#ffffff"></path><path d="M106.24 398.98c-9.85 0-19.7-3.77-27.19-11.31-14.94-15.02-14.88-39.31 0.14-54.25L338.35 75.66c15.02-14.93 39.3-14.88 54.25 0.14 14.94 15.02 14.88 39.31-0.14 54.25L133.3 387.81c-7.49 7.45-17.27 11.17-27.06 11.17z" p-id="6059" fill="#ffffff"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('common.data')+$t('common.restore')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn custom-btn" type="primary" ghost block @click="commitUpdate">{{$t('common.data')+$t('common.restore')}}</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <el-form-item :label="$t('common.belongs')">
              <el-select
                  v-model="store.radarInfo.deviceId"
                  :placeholder="$t('decoration.radarDropdown')"
                  @change="radarOnChange"
              >
                <el-option
                    v-for="item in store.projectInfo.deviceData"
                    :key="item.id"
                    :label="item.name"
                    :value="item.id"
                />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('common.monitor')">
              <el-select
                  v-model="markSelected"
                  multiple
                  :placeholder="$t('decoration.placeholderMonitorDropdown')"
              >
                <el-option
                    v-for="item in markData"
                    :key="item.id"
                    :label="item.name"
                    :value="item.id"
                />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('common.startTime')">
              <el-date-picker
                  v-model="startTime"
                  type="datetime"
                  :placeholder="$t('common.placeholderSelectStartTime')"
              />
            </el-form-item>
            <el-form-item :label="$t('common.endTime')">
              <el-date-picker
                  v-model="endTime"
                  type="datetime"
                  :placeholder="$t('common.placeholderSelectEndTime')"
              />
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
import {DateTimeToStr, showMessage} from "@/utils/tools.js";
import {getUUID, monitorLoad, projectDataInit, staticDataBind} from "@/utils/radartool.js";
import Layer from "@/components/ToolBar/Layer/Layer.vue";
import {CommonUtils} from "@/utils/CommonUtils.js";
import {useRouter} from "vue-router";
import {CesiumUtils} from "@/utils/CesiumUtils.js";
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
const markData = ref([]);
const markSelected = ref(null);
const startTime = ref(new Date(new Date().setDate(new Date().getDate()-1)));
const endTime = ref(new Date())
/*-- methods --*/
const radarOnChange = ()=>{
  markSelected.value = null;
  markData.value = (store.projectInfo.projectData[0].geoMarks||[]).filter(item=>item['devices'][0]===store.radarInfo.deviceId);
}
const commitUpdate=()=>{
  for (let i=0;i<markSelected.value.length;i++){
    const tmpObj = (store.projectInfo.projectData[0].geoMarks || []).filter(item => item['id'] === markSelected.value[i]);
    ApiRadar.DataRestore(store.radarInfo.projectId,store.radarInfo.deviceId,markSelected.value[i],tmpObj[0]['type'],
        DateTimeToStr(startTime.value),DateTimeToStr(endTime.value)).then(res=>{})
  }
  CommonUtils.ShowMessage(store.sysinfo.config.language==="0"?"该区间的位移/速度/加速度数据已经重新生成成功，请耐心等待并在前端刷新查看":"The displacement/velocity/acceleration data for this interval has been successfully regenerated. Please be patient and refresh the front-end to view it","success",'50000');
}
/*-- events --*/
onMounted(() => {
  markData.value = (store.projectInfo.projectData[0].geoMarks||[]).filter(item=>item['devices'][0]===store.radarInfo.deviceId);
  //console.log('MessageTemplate.onMounted');
});
</script>

<style scoped>
#iddeviceconfig {
  height: 100%;
  width: 100%;
}
</style>