<template>
  <section id="idcommand" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('backend.commandSend')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <el-form-item :label="$t('common.device')" style="padding: 0 10px 0 10px">
            <el-select
                v-model="store.radarInfo.deviceId"
                :placeholder="$t('decoration.radarDropdown')"
                style="width: 200px"
                @change="selectOnChange"
            >
              <el-option
                  v-for="item in store.projectInfo.deviceData"
                  :key="item.id"
                  :label="item.name"
                  :value="item.id"
              />
            </el-select>
          </el-form-item>
          <el-form-item :label="$t('decoration.devStatusTitle')"  style="padding: 0 10px 0 10px">
            <el-text class="mx-1" :type="onlineStatus?'success':'error'">{{onlineStatus?$t('common.online'):$t('common.offline')}}</el-text>
          </el-form-item>
          <el-form-item :label="$t('backend.workStatus')"  style="padding: 0 10px 0 10px">
            <el-text class="mx-1" :type="runStatus==='01'?'warning':runStatus==='00'?'error':'success'">{{runStatus==='01'?$t('common.idle'):runStatus==='00'?$t('common.stop'):$t('common.work')}}</el-text>
          </el-form-item>
          <a-button class="custom-ant-btn" type="primary" ghost block @click="sendCommand(3)">{{$t('common.start')+$t('common.work')}}</a-button>
          <a-button class="custom-ant-btn" type="primary" danger ghost block @click="sendCommand(4)">{{$t('common.stop')+$t('common.work')}}</a-button>
          <a-button class="custom-ant-btn" type="dashed" ghost block @click="resetRadar">{{$t('legend.radar')+$t('common.reset')}}</a-button>
          <a-button class="custom-ant-btn" type="primary" danger  ghost block @click="sendCommand(6)">{{$t('common.shutdown')+$t('common.restart')}}</a-button>

          <a-button class="custom-ant-btn" type="dashed" ghost block @click="sendCommand(7)"  v-show="currentRadar==='MIMOLITE'">{{$t('common.auto')+$t('common.work')}}</a-button>
          <a-button class="custom-ant-btn" type="dashed" ghost block @click="sendCommand(8)" v-show="currentRadar==='MIMOLITE'">{{$t('common.manual')+$t('common.work')}}</a-button>
          <a-button class="custom-ant-btn" type="dashed" ghost block @click="sendCommand(14)" v-show="currentRadar==='MIMOLITE'">{{$t('common.close')+$t('common.laser')}}</a-button>
          <a-button class="custom-ant-btn" type="dashed" ghost block @click="sendCommand(15)" v-show="currentRadar==='MIMOLITE'">{{$t('common.open')+$t('common.laser')}}</a-button>
          <a-button class="custom-ant-btn" type="dashed" ghost block @click="sendCommand(16)" v-show="currentRadar==='MIMOLITE'">{{$t('common.open')+$t('common.low')+$t('common.power')}}</a-button>
          <a-button class="custom-ant-btn" type="dashed" ghost block @click="sendCommand(17)" v-show="currentRadar==='MIMOLITE'">{{$t('common.open')+$t('common.high')+$t('common.power')}}</a-button>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-28 / 14:22:29 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw} from 'vue';
import Icon from "@ant-design/icons-vue";
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import {useMapStore} from "@/store/index.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {showMessage} from "@/utils/tools.js";
import {CommonUtils} from "@/utils/CommonUtils.js";
import {staticDataBind} from "@/utils/radartool.js";
import {CesiumUtils} from "@/utils/CesiumUtils.js";
import {useI18n} from "vue-i18n";

/*-- name --*/
defineComponent({
  name: "command",
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
const onlineStatus = ref(true);
const runStatus = ref('01');
const { t } = useI18n();
const currentRadar = ref('');
/*-- methods --*/
const selectOnChange=()=>{
  const index = CommonUtils.FindIndexOfArray('id',store.radarInfo.deviceId,store.projectInfo.deviceData);
  if (index===-1)return;
  store.radarInfo.coordinates = store.projectInfo.deviceData[index]['coordinates'];
  store.radarInfo.params = store.projectInfo.deviceData[index]['params'];
  currentRadar.value = store.radarInfo.deviceId.substring(0,8);
  deviceStatusOnChange();
}
const resetRadar=()=>{
  const params = store.radarInfo.params;
  if (params['radarOri']<0||params['rngMax']<0||params['rngMin']<0||params['ImgAngleStart']<0||params['ImgAngleEnd']<0){
    showMessage(store.sysinfo.config.language==="0"?'探测角度不可小于0':"The detection angle cannot be less than 0",'error');
    return;
  }
  params['rngMax'] = parseInt(params['RngMax'])-1;
  params['rngMin'] = params['RngMin'];
  params['imgAngleEnd'] = params['ImgAngleEnd'];
  params['imgAngleStart'] = params['ImgAngleStart'];
  params['projectId'] = store.radarInfo.projectId;
  params['deviceId'] = store.radarInfo.deviceId;
  params['longitude'] = store.radarInfo.coordinates[0];
  params['latitude'] = store.radarInfo.coordinates[1];
  params['height'] = store.radarInfo.coordinates[2];
  params['freqBand'] = store.radarInfo.params['FreqBand'];
  ApiRadar.updateRadarParams(params).then(res=>{
    ApiRadar.setParamControl(store.radarInfo.projectId,store.radarInfo.deviceId).then(result=>{
      showMessage(t('map.operateSuccess'));
      ApiRadar.AddRadarLog(t('legend.radar')+t('common.reset')+t('common.operate'),store.sysinfo.config.username,store.sysinfo.address,store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
      ApiRadar.getRadarData().then(res=> {
        store.projectInfo.projectData = res.data.data;
        staticDataBind();
      })
    })
  })
}
const sendCommand=(value)=>{
  ApiRadar.controlRadar(store.radarInfo.projectId,store.radarInfo.deviceId,value,'jiaqingqiang').then((data)=>{
    showMessage(data.data.msg);
    ApiRadar.AddRadarLog(t("backend.commandSend")+store.radarInfo.deviceId+(value===3?t('common.start')+t('common.work'):value===4?t('common.stop')+t('common.work'):t('common.shutdown')+t('common.restart')),store.sysinfo.config.username,store.sysinfo.address,store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
    if (value===3){
      runStatus.value = '02';
      store.projectInfo.deviceData[CommonUtils.FindIndexOfArray('id',store.radarInfo.deviceId,store.projectInfo.deviceData)]['runStatus'] = '02';
    }else if (value===4){
      runStatus.value = '01';
      store.projectInfo.deviceData[CommonUtils.FindIndexOfArray('id',store.radarInfo.deviceId,store.projectInfo.deviceData)]['runStatus'] = '01';
    }
  })
}
const deviceStatusOnChange = ()=>{
  const data = store.projectInfo.deviceData.filter(item=>item['id']===store.radarInfo.deviceId)[0];
  onlineStatus.value = data['online'];
  runStatus.value = data['runStatus'];
}
/*-- events --*/
onMounted(() => {
  currentRadar.value = store.radarInfo.deviceId.substring(0,8);
  deviceStatusOnChange();
  // console.log('Command.onMounted');
});
</script>

<style scoped>
#idcommand {
  height: 100%;
  width: 100%;
}
.custom-ant-btn{
  margin:2px 5px;
}
</style>