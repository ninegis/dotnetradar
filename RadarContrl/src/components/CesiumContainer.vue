<template>
  <div id="cesiumContainer">
    <Header/>
    <TreeView/>
    <ToolBar/>
  </div>
</template>

<script setup>
import {defineComponent, h, onMounted, ref, toRaw, onUnmounted} from "vue";
import 'cesium/Build/Cesium/Widgets/widgets.css'
import '@/styles/cesiumstyle.css';
import TreeView from './TreeView/TreeView.vue'
import ToolBar from './ToolBar/ToolBar.vue'
import {useMapStore} from "@/store/index.js";
import {CesiumUtils} from "@/utils/CesiumUtils.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {loadLayer, monitorLoad, staticDataBind} from "@/utils/radartool.js";
import {instanceReset} from "@/axios/apiucml.js";
import Header from "@/components/Header/Header.vue";
import {useI18n} from "vue-i18n";

defineComponent({
  name: "CesiumContainer"
})
const {locale} = useI18n();
const store = useMapStore();
onMounted(()=>{
  instanceReset(store.sysinfo.ucmlInfo.userOid).then(()=>{
    if (store.sysinfo.config.language==="1"){
      locale.value = 'en';
      store.sysinfo.title = store.sysinfo.config.i18Title;
    }
    ApiRadar.apiUrl = store.sysinfo.serverIp;
    CesiumUtils.CesiumInit().then(()=>{
      loadLayer(store.sysinfo.ucmlInfo.orgOid).then(()=>{
        ApiRadar.getRadarData().then(res=>{
          store.projectInfo.projectData = res.data.data;
          store.projectInfo.projectSelected = res.data.data[0].projectId;
          const data = res.data.data[0];
          const device = data['devices'][0];
          if (device['lowMode']!==null && device['lowMode']!==undefined){
            if (device['lowMode'].mode===1){
              if (device['lowMode']['time']===1){
                store.sysinfo.config.radarHeart = 10*60*1000;
              }else if (device['lowMode']['time']===2){
                store.sysinfo.config.radarHeart = 20*60*1000;
              }else if (device['lowMode']['time']===3){
                store.sysinfo.config.radarHeart = 30*60*1000;
              }else if (device['lowMode']['time']===4){
                store.sysinfo.config.radarHeart = 60*60*1000;
              }else if (device['lowMode']['time']===5){
                store.sysinfo.config.radarHeart = 120*60*1000;
              }
            }
          }
          staticDataBind();
          if (data['defaultCamera'] && data['defaultCamera']['lon'] !== undefined) {
            CesiumUtils.CameraFlyToPostion(data['defaultCamera']['lon'],data['defaultCamera']['lat'],data['defaultCamera']['alt'],data['defaultCamera']['heading'],data['defaultCamera']['pitch'],data['defaultCamera']['roll']);
          }
          monitorLoad(data.geoMarks||[]);
          store.startRadarMQTT();
        })
      })
    });
  })
})
</script>

<style scoped>
.gutter-box{
  text-align:center;
}
#cesiumContainer{
  position: absolute;
  height:100%;
  width: 100%;
  padding: 0;
  margin: 0;
}
</style>