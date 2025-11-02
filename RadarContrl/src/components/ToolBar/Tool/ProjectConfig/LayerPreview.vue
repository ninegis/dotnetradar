<template>
  <section id="idmessagetemplate" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('common.layer')+$t('common.list')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn custom-btn" type="primary" ghost block @click="store.toolbarcontent='layerconfig'">{{$t('backend.addLayer')}}</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-table :data="tableData" style="width: 100%;color:white" class="custom-table" height="300">
            <el-table-column prop="ServiceName" :label="$t('common.name')"/>
            <el-table-column prop="ServiceType" :label="$t('common.type')" :formatter="formatType"/>
            <el-table-column prop="Deactivateornot" :label="$t('common.enable')" :formatter="formatEnable"/>
            <el-table-column prop="InitializeDisplay" :label="$t('common.disable')" :formatter="formatVisible"/>
            <el-table-column :label="$t('common.operate')" width="200">
              <template #default="scope">
                <el-button link type="primary" size="small" @click="itemOperate(0,scope.row)">{{$t('common.enable')}}/{{$t('common.disable')}}</el-button>
                <el-button link type="primary" size="small" @click="itemOperate(1,scope.row)">({{$t('common.no')}}){{$t('common.visible')}}</el-button>
                <el-button link type="primary" size="small" @click="itemOperate(2,scope.row)">{{$t('common.delete')}}</el-button>
              </template>
            </el-table-column>
          </el-table>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-29 / 12:42:00 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {FormatJsonToLayerTreeData, showMessage} from "@/utils/tools.js";
import {getUUID} from "@/utils/radartool.js";
import Layer from "@/components/ToolBar/Layer/Layer.vue";
import {getGPSLayerTree} from "@/axios/apiucml.js";
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
const form = reactive({})
/*-- store --*/
const store = useMapStore();
const {t} = useI18n();
/*-- vars --*/
const tableData = ref([]);
/*-- methods --*/
const formatVisible=(row)=>{
  return row['InitializeDisplay']==='True'?t('common.visible'):t('common.hide');
}
const formatEnable=(row)=>{
  return row['Deactivateornot']==='1'?t('common.enable'):t('common.disable');
}
const itemOperate=(index,row)=>{
  if (index===2){
    ApiRadar.deleteLayer(row['kot_mapservicemgtOID']).then(res=>{
      showMessage(t('common.deleteSuccess'));
      ApiRadar.getLayer(store.sysinfo.ucmlInfo.orgOid).then(res=>{
        tableData.value = res.data.data;
      })
    })
  }else if (index===0){
    ApiRadar.enableLayer(row['kot_mapservicemgtOID'],row['Deactivateornot']==='1'?'0':'1').then(result=>{
      showMessage(t('map.operateSuccess'));
      ApiRadar.getLayer(store.sysinfo.ucmlInfo.orgOid).then(res=>{
        tableData.value = res.data.data;
      })
    })
  }else if (index===1){
    ApiRadar.showLayer(row['kot_mapservicemgtOID'],row['InitializeDisplay']==='True'?'0':'1').then(result=>{
      showMessage(t('map.operateSuccess'));
      ApiRadar.getLayer(store.sysinfo.ucmlInfo.orgOid).then(res=>{
        tableData.value = res.data.data;
      })
    })
  }
}
const formatType=(row)=>{
  let result = '';
  switch (row['ServiceType']){
    case '3dtile':
      result = t('backend.osgb');
      break;
    case 'terrain':
      result = t('backend.terrain');
      break;
    case 'tms':
      result = t('backend.tms');
      break;
    case 'las':
      result = t('backend.las');
      break;
    case 'bim':
      result = t('backend.bim');
      break;
    case 'shp':
      result = t('backend.shp');
      break;
    case 'geojson':
      result = t('backend.geojson');
      break;
  }
  return result;
}
/*-- events --*/
onMounted(() => {
  if (window.localrelease!==undefined)return
  ApiRadar.getLayer(store.sysinfo.ucmlInfo.orgOid).then(res=>{
    tableData.value = res.data.data;
  })
  //console.log('MessageTemplate.onMounted');
});
</script>

<style scoped>
#idmessagetemplate {
  height: 100%;
  width: 100%;
}
</style>