<template>
  <section id="iddangerconfig" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{store.sysinfo.config.language==="0"?"隐患区域分析":"Danger Areas"}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn custom-btn" type="primary" ghost block @click="commitUpdate">{{$t('common.commitChange')}}</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <el-form-item :label="(store.sysinfo.config.language==='0'?'隐患点阈值':'Hazard threshold')+'(mm)'">
              <el-input v-model="store.radarInfo.autoAnalysisHiddenAreaConfig['threshold']"/>
            </el-form-item>
            <el-text type="warning">{{store.sysinfo.config.language==='0'?'点的位移值超过阈值将是为隐患点':'If the displacement value of a point exceeds the threshold, it will be considered a hidden danger point'}}</el-text>
            <el-form-item :label="(store.sysinfo.config.language==='0'?'隐患区域面积阈值':'Threshold area of hidden danger zone')+'(m²)'">
              <el-input v-model="store.radarInfo.autoAnalysisHiddenAreaConfig['areaThreshold']"/>
            </el-form-item>
            <el-text type="warning">{{(store.sysinfo.config.language==='0'?'隐患区域面积小于阈值将被过滤':'Hidden danger areas with an area smaller than the threshold will be filtered')}} </el-text>
            <el-form-item :label="(store.sysinfo.config.language==='0'?'隐患生成设置(间隔 帧)':'Hidden danger generation setting (interval frame)')">
              <el-input v-model="store.radarInfo.autoAnalysisHiddenAreaConfig['analysisDec']"/>
            </el-form-item>
            <el-form-item :label="(store.sysinfo.config.language==='0'?'是否开启隐患区域分析':'Whether to activate the analysis of hidden danger areas')">
              <el-switch v-model="store.radarInfo.autoAnalysisHiddenAreaConfig['autoAnalysisFlag']"/>
            </el-form-item>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-29 / 12:34:51 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {showMessage} from "@/utils/tools.js";
import {ApiRadar} from "@/axios/apiRadar.js";
/*-- name --*/
defineComponent({
  name: "dangerconfig",
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
const commitUpdate=()=>{
  const params = store.radarInfo.autoAnalysisHiddenAreaConfig;
  params['projectId'] = store.radarInfo.projectId;
  ApiRadar.updateDangerArea(params).then(res=>{
    ApiRadar.AddRadarLog('修改隐患区域分析',store.sysinfo.config.username,store.sysinfo.address,store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
    showMessage(res.data.data);
  })
}
/*-- events --*/
onMounted(() => {
  //console.log('DangerConfig.onMounted');
});
</script>

<style scoped>
#iddangerconfig {
  height: 100%;
  width: 100%;
}
</style>