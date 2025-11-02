<template>
  <div id="toolbar">
    <a-button class="custom-ant-btn" @click="itemClick('basemap')" type="primary">
      <icon>
        <template #component>
          <svg width="1em" height="1em" fill="currentColor" viewBox="0 0 1024 1024">
            <path d="M722.876882 192.283437L355.542958 12.697963 329.240035 0l-41.721877 22.674934L0 192.283437v831.716563l335.589017-192.283437 336.496014 173.236493 41.721878 19.046944 25.395925-16.325952 287.518158-175.957485V0L722.876882 192.283437zM67.117803 226.749336l224.028344-130.607618v687.503986L67.117803 911.532329V226.749336z m301.123118 547.826395l-12.697963-6.348982V83.443756l319.263065 156.91054v691.131975L368.240921 774.575731z m594.990257 21.767936l-224.028344 137.863596V255.773251l19.046945-12.697963 204.981399-131.514615v684.782994z"></path>
          </svg>
        </template>
      </icon>
      {{$t('map.baseMap')}}
    </a-button>
    <a-button class="custom-ant-btn"  @click="itemClick('layer')" type="primary">
      <icon>
        <template #component>
          <svg width="1em" height="1em" fill="currentColor" viewBox="0 0 1024 1024">
            <path d="M993.622303 209.223676l-460.79999-204.799996a51.199999 51.199999 0 0 0-41.59488 0l-460.79999 204.799996a51.199999 51.199999 0 0 0 0 93.593598l460.79999 204.799996a51.384319 51.384319 0 0 0 41.59488 0l460.79999-204.799996a51.199999 51.199999 0 0 0 0-93.593598z" fill="#ffffff" p-id="8806"></path><path d="M512.035113 767.999985a51.199999 51.199999 0 0 1-20.7872-4.4032l-460.79999-204.799996a51.210239 51.210239 0 0 1 41.594879-93.593598L512.035113 660.766707l439.992311-195.543036a51.210239 51.210239 0 0 1 41.594879 93.593598l-460.79999 204.799996A51.199999 51.199999 0 0 1 512.035113 767.999985z" fill="#ffffff" p-id="8807"></path><path d="M512.035113 1023.99998a51.199999 51.199999 0 0 1-20.7872-4.4032l-460.79999-204.799996a51.210239 51.210239 0 0 1 41.594879-93.593598L512.035113 916.766702l439.992311-195.543036a51.210239 51.210239 0 0 1 41.594879 93.593598l-460.79999 204.799996A51.199999 51.199999 0 0 1 512.035113 1023.99998z" fill="#ffffff" p-id="8808"></path>
          </svg>
        </template>
      </icon>
      {{$t('common.layer')}}
    </a-button>
<!--    <a-dropdown-button class="custom-ant-btn" type="primary"  @click="dropdownClick">-->
<!--      <icon>-->
<!--        <template #component>-->
<!--          <svg width="1em" height="1em" fill="currentColor" viewBox="0 0 1024 1024">-->
<!--            <path d="M819.45098 912.313725H621.176471c-50.196078 0-90.352941-40.156863-90.352942-90.352941V677.647059c0-10.039216 8.784314-18.823529 18.82353-18.82353s18.823529 8.784314 18.823529 18.82353v144.313725c0 28.862745 23.843137 52.705882 52.705883 52.705883h197.019607c28.862745 0 52.705882-23.843137 52.705883-52.705883V623.686275c0-28.862745-23.843137-52.705882-52.705883-52.705883H675.137255c-10.039216 0-18.823529-8.784314-18.82353-18.823529s8.784314-18.823529 18.82353-18.82353h144.313725c50.196078 0 90.352941 40.156863 90.352942 90.352942v197.019607c0 50.196078-40.156863 91.607843-90.352942 91.607843zM342.588235 490.666667H199.529412c-50.196078 0-90.352941-40.156863-90.352941-90.352942V203.294118c0-50.196078 40.156863-90.352941 90.352941-90.352942h197.019608c50.196078 0 90.352941 40.156863 90.352941 90.352942v144.313725c0 10.039216-8.784314 18.823529-18.82353 18.82353s-18.823529-8.784314-18.823529-18.82353V203.294118c0-28.862745-23.843137-52.705882-52.705882-52.705883H199.529412c-28.862745 0-52.705882 23.843137-52.705883 52.705883v197.019607c0 28.862745 23.843137 52.705882 52.705883 52.705883h144.313725c10.039216 0 18.823529 8.784314 18.82353 18.823529s-8.784314 18.823529-20.078432 18.82353zM819.45098 490.666667H675.137255c-10.039216 0-18.823529-8.784314-18.82353-18.82353s8.784314-18.823529 18.82353-18.823529h144.313725c28.862745 0 52.705882-23.843137 52.705883-52.705883V203.294118c0-28.862745-23.843137-52.705882-52.705883-52.705883H621.176471c-28.862745 0-52.705882 23.843137-52.705883 52.705883v144.313725c0 10.039216-8.784314 18.823529-18.823529 18.82353s-18.823529-8.784314-18.82353-18.82353V203.294118c0-50.196078 40.156863-90.352941 90.352942-90.352942h197.019607c50.196078 0 90.352941 40.156863 90.352942 90.352942v197.019607c1.254902 50.196078-38.901961 90.352941-89.09804 90.352942zM396.54902 912.313725H199.529412c-50.196078 0-90.352941-40.156863-90.352941-90.352941V623.686275c0-50.196078 40.156863-90.352941 90.352941-90.352942h144.313725c10.039216 0 18.823529 8.784314 18.82353 18.82353s-8.784314 18.823529-18.82353 18.823529H199.529412c-28.862745 0-52.705882 23.843137-52.705883 52.705883v197.019607c0 28.862745 23.843137 52.705882 52.705883 52.705883h197.019608c28.862745 0 52.705882-23.843137 52.705882-52.705883V677.647059c0-10.039216 8.784314-18.823529 18.823529-18.82353s18.823529 8.784314 18.82353 18.82353v144.313725c1.254902 48.941176-40.156863 90.352941-90.352941 90.352941zM500.705882 619.921569c-60.235294 0-107.921569-48.941176-107.921568-107.921569 0-60.235294 48.941176-107.921569 107.921568-107.921569s107.921569 48.941176 107.921569 107.921569c0 58.980392-48.941176 107.921569-107.921569 107.921569z m0-179.450981c-38.901961 0-70.27451 31.372549-70.274509 70.27451s31.372549 70.27451 70.274509 70.27451 70.27451-31.372549 70.27451-70.27451-31.372549-70.27451-70.27451-70.27451z" fill="#ffffff" p-id="10796"></path>            </svg>-->
<!--        </template>-->
<!--      </icon>-->
<!--      {{$t('common.tool')}}-->
<!--      <template #overlay>-->
<!--        <a-menu @click="handleCommandMenuClick" theme="dark">-->
<!--          <a-menu-item :key="item.key" v-for="item in toolList">-->
<!--            {{store.sysinfo.config.language==="0"?item.title:item.englishTitle}}-->
<!--          </a-menu-item>-->
<!--        </a-menu>-->
<!--      </template>-->
<!--      <template #icon><DownOutlined @click="dropdownClick"/></template>-->
<!--    </a-dropdown-button>-->
    <a-dropdown>
      <template #overlay>
        <a-menu @click="handleCommandMenuClick" theme="dark">
          <a-menu-item :key="item.key" v-for="item in toolList">
            {{store.sysinfo.config.language==="0"?item.title:item.englishTitle}}
          </a-menu-item>
        </a-menu>
      </template>
      <a-button class="custom-ant-btn" type="primary">
        <icon>
          <template #component>
            <svg width="1em" height="1em" fill="currentColor" viewBox="0 0 1024 1024">
              <path d="M819.45098 912.313725H621.176471c-50.196078 0-90.352941-40.156863-90.352942-90.352941V677.647059c0-10.039216 8.784314-18.823529 18.82353-18.82353s18.823529 8.784314 18.823529 18.82353v144.313725c0 28.862745 23.843137 52.705882 52.705883 52.705883h197.019607c28.862745 0 52.705882-23.843137 52.705883-52.705883V623.686275c0-28.862745-23.843137-52.705882-52.705883-52.705883H675.137255c-10.039216 0-18.823529-8.784314-18.82353-18.823529s8.784314-18.823529 18.82353-18.82353h144.313725c50.196078 0 90.352941 40.156863 90.352942 90.352942v197.019607c0 50.196078-40.156863 91.607843-90.352942 91.607843zM342.588235 490.666667H199.529412c-50.196078 0-90.352941-40.156863-90.352941-90.352942V203.294118c0-50.196078 40.156863-90.352941 90.352941-90.352942h197.019608c50.196078 0 90.352941 40.156863 90.352941 90.352942v144.313725c0 10.039216-8.784314 18.823529-18.82353 18.82353s-18.823529-8.784314-18.823529-18.82353V203.294118c0-28.862745-23.843137-52.705882-52.705882-52.705883H199.529412c-28.862745 0-52.705882 23.843137-52.705883 52.705883v197.019607c0 28.862745 23.843137 52.705882 52.705883 52.705883h144.313725c10.039216 0 18.823529 8.784314 18.82353 18.823529s-8.784314 18.823529-20.078432 18.82353zM819.45098 490.666667H675.137255c-10.039216 0-18.823529-8.784314-18.82353-18.82353s8.784314-18.823529 18.82353-18.823529h144.313725c28.862745 0 52.705882-23.843137 52.705883-52.705883V203.294118c0-28.862745-23.843137-52.705882-52.705883-52.705883H621.176471c-28.862745 0-52.705882 23.843137-52.705883 52.705883v144.313725c0 10.039216-8.784314 18.823529-18.823529 18.82353s-18.823529-8.784314-18.82353-18.82353V203.294118c0-50.196078 40.156863-90.352941 90.352942-90.352942h197.019607c50.196078 0 90.352941 40.156863 90.352942 90.352942v197.019607c1.254902 50.196078-38.901961 90.352941-89.09804 90.352942zM396.54902 912.313725H199.529412c-50.196078 0-90.352941-40.156863-90.352941-90.352941V623.686275c0-50.196078 40.156863-90.352941 90.352941-90.352942h144.313725c10.039216 0 18.823529 8.784314 18.82353 18.82353s-8.784314 18.823529-18.82353 18.823529H199.529412c-28.862745 0-52.705882 23.843137-52.705883 52.705883v197.019607c0 28.862745 23.843137 52.705882 52.705883 52.705883h197.019608c28.862745 0 52.705882-23.843137 52.705882-52.705883V677.647059c0-10.039216 8.784314-18.823529 18.823529-18.82353s18.823529 8.784314 18.82353 18.82353v144.313725c1.254902 48.941176-40.156863 90.352941-90.352941 90.352941zM500.705882 619.921569c-60.235294 0-107.921569-48.941176-107.921568-107.921569 0-60.235294 48.941176-107.921569 107.921568-107.921569s107.921569 48.941176 107.921569 107.921569c0 58.980392-48.941176 107.921569-107.921569 107.921569z m0-179.450981c-38.901961 0-70.27451 31.372549-70.274509 70.27451s31.372549 70.27451 70.274509 70.27451 70.27451-31.372549 70.27451-70.27451-31.372549-70.27451-70.27451-70.27451z" fill="#ffffff" p-id="10796"></path>            </svg>
          </template>
        </icon>
        {{$t('common.tool')}}
        <DownOutlined />
      </a-button>
    </a-dropdown>
    <div class="toolbar-content" ref="toolBarContent">
      <component :is="currentComponent" :visible="'show'"/>
    </div>
    <div class="rightbottom-toolbar">
      <el-tooltip :content="store.sysinfo.config.language==='0'?item.content:item.englishName" v-for="item in props.toolbar">
        <div class="toolbaricon" @click="iconClick(item.name)">
          <img :src="item.icon" :alt="item.content">
        </div>
      </el-tooltip>
    </div>
  </div>

</template>

<script setup>
// map / 2023-05-16 / 17:02:22 / QingQiangJia
/*-- imports --*/
import {BaseMap, MeasureDialog, Layer, Command, Drawing, RadarImage, DeviceConfig, DeviceList, DeviceEdit, ProjectAdd, ProjectEdit} from './load'
import Icon,{DownOutlined} from '@ant-design/icons-vue'
import {computed, defineComponent, ref, onMounted, reactive, h} from 'vue';
import {useMapStore} from "@/store/index.js";
import {AlarmRule,RadarParams,AlgorithmParams,ProjectConfig,ImageGenerate,PitchControl,CommandIcon,SpeedTarget,ColorConfig,DangerConfig,MessageTemplate,DiskConfig,AlarmRecord,AlarmRuleInfo,AlarmContactInfo,AlarmContactList,ReportConfig} from '@/components/ToolBar/load.js';
import {
  ClearIcon,
  CoordinateIcon,
  MeasureLength,
  RadarImageIcon,
  measurestickarea
} from "@/assets/load.js";
import LayerPreview from "@/components/ToolBar/Tool/ProjectConfig/LayerPreview.vue";
import LayerConfig from "@/components/ToolBar/Tool/ProjectConfig/LayerConfig.vue";
import {CesiumUtils} from "@/utils/CesiumUtils.js";
import DataRestore from "@/components/ToolBar/Tool/DataManage/DataRestore.vue";
import DataGenerate from "@/components/ToolBar/Tool/DataManage/DataGenerate.vue";
import AllowPeopleList from "@/components/ToolBar/Tool/AlarmRule/AllowPeopleList.vue";
import AllowPeopleInfo from "@/components/ToolBar/Tool/AlarmRule/AllowPeopleInfo.vue";
import {useI18n} from "vue-i18n";

/*-- name --*/
defineComponent({
  name: "ToolBar"
})
/*-- props  --*/
const props = defineProps({
  toolList: {
    type: Array,
    required: false,
    default: [{
      title:'项目配置',
      englishTitle:'Project Config',
      key:'projectConfig',
      icon:CommandIcon
    },{
      title:'雷达配置',
      englishTitle:'Radar Config',
      key:'radarParams',
      icon:CommandIcon
    },{
      title:'图像配置',
      englishTitle:'Image Config',
      key:'imageConfig',
      icon:CommandIcon
    },{
      title:'算法配置',
      englishTitle:'Algorithm Config',
      key:'algorithmParams',
      icon:CommandIcon
    },{
      //   title:'俯仰控制',
      //   key:'pitchControl',
      //   icon:CommandIcon
      // },{
      title:'指令下发',
      englishTitle:'Command Send',
      key:'sendcommand',
      icon:CommandIcon
    },{
      title:'速度指标',
      englishTitle:'Speed Target',
      key:'speedTarget',
      icon:CommandIcon
    },{
      title:'色条配置',
      englishTitle:'Colorbar Config',
      key:'colorConfig',
      icon:CommandIcon
      // },{
      //   title:'报表配置',
      //   key:'reportConfig',
      //   icon:CommandIcon
    },{
      title:'隐患配置',
      englishTitle:'Hazard config',
      key:'dangerConfig',
      icon:CommandIcon
    },{
      title:'预警规则',
      englishTitle:'Alarm Rule',
      key:'alarmRule',
      icon:CommandIcon
    },{
      title:'预警人员',
      englishTitle:'Alarm People',
      key:'alarmPeople',
      icon:CommandIcon
    },{
      title:'授权人员',
      englishTitle:'Authorizing People',
      key:'allowPeople',
      icon:CommandIcon
    },{
      title:'短信模板',
      englishTitle: "SMS Template",
      key:'messageTemplate',
      icon:CommandIcon
      // },{
      //   title:'硬盘配置',
      //   key:'diskConfig',
      //   icon:CommandIcon
    },{
      title: '图层配置',
      englishTitle:'Layer Config',
      key:'layerpreview',
      icon:CommandIcon
    },{
      title: "数据恢复",
      englishTitle: "Data Restore",
      key: "datarestore",
      icon: CommandIcon
    },{
      title: "数据生成",
      englishTitle: "Data Generate",
      key: "datagenerate",
      icon:CommandIcon
    }
    ],
  },
  toolbar:{
    type: Array,
    required: false,
    default:[{
      content: '雷达图像',
      englishName:'Radar Image',
      name: 'radarimage',
      icon: RadarImageIcon
    },{
      content: '坐标量测',
      englishName: "Coodinate Measure",
      name: 'measurecoordinate',
      icon:CoordinateIcon
    },{
      content: '距离量测',
      englishName:'Distance Measure',
      name: 'measurelinedistance',
      icon:MeasureLength
    },{
      content: '面积量测',
      englishName:'Area Measure',
      name: 'measurearea',
      icon:measurestickarea
    },{
      content: '清除',
      englishName:'Clear',
      name: 'clear',
      icon:ClearIcon
    }]
  }
});
/*-- stores --*/
const store = useMapStore();
/*-- vars --*/
const dropdownvisible = ref(false);
const tempEntityIds = ref([]);

/*-- computed --*/
const currentComponent = computed(() => {
  console.log('=== currentComponent 计算属性被调用 ===');
  console.log('当前toolbarcontent:', store.toolbarcontent);
  
  let componentslot = null;
  store.dragContainer.width = 276;
  switch (store.toolbarcontent) {
    case 'basemap':
      componentslot = BaseMap;
      break;
    case 'layer':
      componentslot = Layer;
      break;
    case 'mapmeasure':
      componentslot = MeasureDialog;
      break;
    case 'sendcommand':
      store.dragContainer.width = 276;
      componentslot = Command;
      break;
    case 'drawing':
      componentslot = Drawing;
      break;
    case 'alarmRule':
      store.dragContainer.width = 600;
      componentslot = AlarmRule;
      break;
    case 'radarParams':
      componentslot = RadarParams;
      break;
    case 'algorithmParams':
      componentslot = AlgorithmParams;
      break;
    case 'projectConfig':
      componentslot = ProjectConfig;
      break;
    case 'imageConfig':
      componentslot = ImageGenerate;
      break;
    case 'pitchControl':
      componentslot = PitchControl;
      break;
    case 'speedTarget':
      componentslot = SpeedTarget;
      break;
    case 'colorConfig':
      console.log('=== 选择colorConfig组件 ===');
      store.dragContainer.width = 600;
      componentslot = ColorConfig;
      console.log('ColorConfig组件已设置:', componentslot);
      break;
    case 'dangerConfig':
      componentslot = DangerConfig;
      break;
    case 'messageTemplate':
      componentslot = MessageTemplate;
      break;
    case 'diskConfig':
      componentslot = DiskConfig;
      break;
    case 'alarmRuleInfo':
      store.dragContainer.width = 600;
      componentslot = AlarmRuleInfo;
      break;
    case 'alarmPeople':
      store.dragContainer.width = 600;
      componentslot = AlarmContactList;
      break;
    case 'allowPeople':
      store.dragContainer.width = 600;
      componentslot = AllowPeopleList;
      break;
    case 'alarmContactInfo':
      store.dragContainer.width = 600;
      componentslot = AlarmContactInfo;
      break;
    case 'allowPeopleInfo':
      store.dragContainer.width = 600;
      componentslot = AllowPeopleInfo;
      break;
    case 'reportConfig':
      componentslot = ReportConfig;
      break;
    case 'alarmrecord':
      componentslot = AlarmRecord;
      break;
    case 'radarimage':
      store.dragContainer.width = 600;
      componentslot = RadarImage;
      break;
    case 'layerpreview':
      store.dragContainer.width = 600;
      componentslot = LayerPreview;
      break;
    case 'layerconfig':
      componentslot = LayerConfig;
      break;
    case 'adddevice':
      componentslot = DeviceConfig;
      break;
    case 'deviceList':
      componentslot = DeviceList;
      break;
    case 'deviceEdit':
      componentslot = DeviceEdit;
      break;
    case 'addproject':
      componentslot = ProjectAdd;
      break;
    case 'projectEdit':
      componentslot = ProjectEdit;
      break;
    case 'datarestore':
      componentslot = DataRestore;
      break;
    case 'datagenerate':
      componentslot = DataGenerate;
      break;
  }
  console.log('返回的组件:', componentslot);
  console.log('=== currentComponent 计算属性结束 ===');
  return componentslot;
});
/*-- methods --*/
const iconClick=(category)=>{
  switch (category) {
    case 'measurecoordinate':
      CesiumUtils.MeasureCoordinate().then(res=>{
        tempEntityIds.value = tempEntityIds.value.concat(res);
      })
      break;
    case 'measurelinedistance':
      CesiumUtils.MeasureLineDistance().then(res=>{
        tempEntityIds.value = tempEntityIds.value.concat(res);
      })
      break;
    case 'measurearea':
      CesiumUtils.MeasureArea().then(res=>{
        tempEntityIds.value = tempEntityIds.value.concat(res);
      })
      break;
    case 'clear':
      for (let i = tempEntityIds.value.length-1; i >=0; i--) {
        CesiumUtils.EntityRemoveById(tempEntityIds.value[i]);
        tempEntityIds.value.splice(i,1);
      }
      break;
    case 'radarimage':
      store.toolbarcontent = category;
      break;
  }
}
const itemClick = (title) => {
  if (store.toolbarcontent === title) {
    store.toolbarcontent = '';
    return;
  }
  switch (title) {
    case 'basemap':
      store.toolbarcontent = 'basemap';
      break;
    case 'layer':
      store.toolbarcontent = 'layer';
      break;
    case 'mapmeasure':
      store.toolbarcontent = 'mapmeasure';
      break;
  }
}
function handleCommandMenuClick(e){
  console.log('=== 菜单点击事件 ===');
  console.log('点击的菜单项:', e.key);
  console.log('当前toolbarcontent:', store.toolbarcontent);
  
  dropdownvisible.value = false;
  switch (e.key){
    case 'maptrack':
      itemClick('maptrack');
      break;
    case 'mapmeasure':
      itemClick('mapmeasure');
      break;
    case 'mapdrawing':
      store.toolbarcontent = 'drawing';
      break;
    case 'alarmrule':
      store.toolbarcontent = 'alarmrule';
      break;
    case 'sendcommand':
      store.toolbarcontent = 'sendcommand';
      break;
    default:
      store.toolbarcontent = e.key;
      break;
  }
  
  console.log('设置后的toolbarcontent:', store.toolbarcontent);
  console.log('=== 菜单点击事件结束 ===');
}
function dropdownClick(e){
  dropdownvisible.value = !dropdownvisible.value;
}
/*-- events --*/
onMounted(()=>{

})

</script>

<style scoped>
#toolbar{
  position: absolute;
  margin-top: 55px;
  z-index: 888;
  right: 10px;
  float: right;
}
.toolbar-content {
  position: absolute;
  top: 48px;
  right: 0;
  background: rgba(0,0,0,0.5);
  border-radius: 5px;
  z-index: 111;
  min-height: 0;
}
:deep(.ant-dropdown-menu-item, .ant-dropdown-menu-submenu-title){
  font-size: 16px;
}
.ant-btn-group{
  background: transparent;
}
:deep(.el-input__wrapper .el-input__inner){
  background: transparent;
}
:deep(.executeBtn){
  margin-bottom: 5px;
  margin-left: 5px;
  margin-right: 5px;
}
:deep(.el-form-item){
  margin-bottom: 8px;
}
:deep(.el-divider--horizontal){
  margin:12px 0;
}
:deep(.custom-header-icon){
  cursor:pointer;
}
:deep(.el-select){
  width: 100%;
}
.rightbottom-toolbar{
  position: fixed;
  right: 20px;
  bottom: 10px;
  height: 200px;
  width: 36px;
  cursor: pointer;
}
.toolbaricon{
  margin: 10px 0 0 0;
}
</style>