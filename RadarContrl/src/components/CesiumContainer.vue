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
  // ✅ 监听项目场景加载事件
  window.addEventListener('project-scene-loaded', (event) => {
    const scene = event.detail;
    if (window.viewer && scene) {
      console.log('[Cesium] 定位到项目场景:', scene);
      window.viewer.camera.flyTo({
        destination: window.Cesium.Cartesian3.fromDegrees(
          scene.longitude,
          scene.latitude,
          scene.height
        ),
        orientation: {
          heading: window.Cesium.Math.toRadians(scene.heading),
          pitch: window.Cesium.Math.toRadians(scene.pitch),
          roll: window.Cesium.Math.toRadians(scene.roll)
        },
        duration: 2.0
      });
    }
  });
  
  instanceReset(store.sysinfo.ucmlInfo.userOid).then(()=>{
    // ✅ 修复：根据 store 中的语言设置初始化 i18n locale
    if (store.sysinfo.config.language==="1"){
      locale.value = 'en';
      store.sysinfo.title = store.sysinfo.config.i18Title;
    } else {
      // ✅ 确保默认语言是中文
      locale.value = 'zh';
    }
    ApiRadar.apiUrl = store.sysinfo.serverIp;
    CesiumUtils.CesiumInit().then((viewer)=>{
      // ✅ 设置全局viewer引用
      window.viewer = viewer;
      window.CesiumUtils = CesiumUtils;
      // ✅ 设置Cesium对象（从CesiumUtils获取）
      if (CesiumUtils.Cesium) {
        window.Cesium = CesiumUtils.Cesium;
        console.log('[Cesium初始化] Cesium对象已设置', !!window.Cesium);
      }
      console.log('[Cesium初始化] viewer已设置到window.viewer', !!window.viewer);
      
      loadLayer(store.sysinfo.ucmlInfo.orgOid).then(()=>{
        ApiRadar.getRadarData().then(async res=>{
          // ✅ 初始化项目数据，确保每个项目都有devices数组
          store.projectInfo.projectData = (res.data.data || []).map(p => ({
            ...p,
            devices: p.devices || []  // 初始化devices数组
          }));
          
          // ✅ 为每个项目加载设备信息
          const loadDevicesPromises = store.projectInfo.projectData.map(async (project) => {
            try {
              const devicesRes = await ApiRadar.getDevicesByProjectId(project.projectId);
              if (devicesRes.data && devicesRes.data.code === 200 && devicesRes.data.data) {
                // 映射设备数据
                project.devices = devicesRes.data.data.map(d => {
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
                    dataVersion: d.dataVersion || '0',
                    algorithmParam: d.algorithmParam || {}
                  };
                });
                
                console.log(`项目 ${project.projectName} 加载了 ${project.devices.length} 个设备`);
              }
            } catch (error) {
              console.error(`加载项目 ${project.projectName} 的设备失败:`, error);
              project.devices = [];  // 确保devices是数组
            }
          });
          
          // 等待所有设备加载完成
          await Promise.all(loadDevicesPromises);
          
          // ✅ 设置默认选中第一个项目
          if (store.projectInfo.projectData.length > 0) {
            store.projectInfo.projectSelected = store.projectInfo.projectData[0].projectId;
            const data = store.projectInfo.projectData[0];
            
            // ✅ 安全检查：确保有设备再访问
            if (data.devices && data.devices.length > 0) {
              const device = data.devices[0];
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
            }
            
            staticDataBind();
            // ✅ 优先使用场景配置，如果没有则使用默认相机配置
            if (data.sceneLongitude && data.sceneLatitude) {
              console.log('[首次加载] 使用项目场景配置定位');
              store.loadProjectScene(data);
            } else if (data['defaultCamera'] && data['defaultCamera']['lon'] !== undefined) {
              console.log('[首次加载] 使用默认相机配置定位');
              CesiumUtils.CameraFlyToPostion(data['defaultCamera']['lon'],data['defaultCamera']['lat'],data['defaultCamera']['alt'],data['defaultCamera']['heading'],data['defaultCamera']['pitch'],data['defaultCamera']['roll']);
            } else {
              console.log('[首次加载] 没有场景配置，使用默认位置');
            }
            monitorLoad(data.geoMarks||[]);
            store.startRadarMQTT();
          }
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