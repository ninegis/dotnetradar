<template>
  <section id="idheader" v-show="visible" class="">
    <el-text size="large" class="headertitile"><b>{{store.sysinfo.title}}</b></el-text>
    <div class="projectContainer">
      <div class="projectContainerBox">
        <el-text class="projectContainerBoxTitle">{{$t('backend.currentProject')}}:</el-text>
        <el-select size="small" v-model="store.projectInfo.projectSelected" style="width: 100px"  @change="projectOnChange">
          <el-option v-for="item in store.projectInfo.projectData" :label="item.projectName" :value="item.projectId" :key="item.projectId"/>
        </el-select>
        <a-space :size="50">
          <el-badge @click="badgeOnClick(item)" :value="item.online?$t('common.online')+(item['deviceInfo']['processorStatus']/10)+'°':$t('common.offline')" v-for="item in store.projectInfo.deviceData" class="item" :type="item.online?'success':'danger'">
            <el-button color="rgba(0,0,0,.5)" size="small" text disabled style="cursor:default">{{item.deviceName}}</el-button>
          </el-badge>
        </a-space>
      </div>
    </div>
    <div class="float-right headerrightbtn">

      <el-tooltip :content="$t('other.translation')" placement="top">
        <el-dropdown @command="localeOnChange" style="top: 10px;margin-right: 10px;">
          <el-icon :size="24">
            <svg x="1740539374907" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="16185" width="24" height="24"><path d="M938.666667 981.333333c-17.066667 0-29.866667-8.533333-38.4-25.6l-59.733334-119.466666h-277.333333l-59.733333 119.466666c-8.533333 21.333333-34.133333 29.866667-55.466667 17.066667-25.6-8.533333-34.133333-34.133333-21.333333-51.2l72.533333-140.8 145.066667-290.133333c12.8-21.333333 34.133333-38.4 59.733333-38.4s46.933333 12.8 59.733333 38.4l145.066667 290.133333 72.533333 140.8c8.533333 21.333333 0 46.933333-17.066666 55.466667-12.8 4.266667-17.066667 4.266667-25.6 4.266666z m-332.8-226.133333h192l-98.133334-192-93.866666 192zM85.333333 844.8c-17.066667 0-29.866667-8.533333-38.4-25.6-8.533333-21.333333 0-46.933333 21.333334-55.466667 93.866667-46.933333 179.2-110.933333 247.466666-187.733333-46.933333-64-85.333333-128-110.933333-192-8.533333-21.333333 4.266667-46.933333 25.6-55.466667 21.333333-8.533333 46.933333 4.266667 55.466667 25.6 21.333333 51.2 46.933333 102.4 81.066666 149.333334 59.733333-85.333333 102.4-179.2 128-281.6H85.333333c-25.6 0-42.666667-17.066667-42.666666-42.666667s17.066667-42.666667 42.666666-42.666667h243.2V85.333333c0-25.6 17.066667-42.666667 42.666667-42.666666s42.666667 17.066667 42.666667 42.666666v51.2h238.933333c25.6 0 42.666667 17.066667 42.666667 42.666667s-17.066667 42.666667-42.666667 42.666667h-68.266667c-25.6 128-85.333333 247.466667-162.133333 349.866666l25.6 25.6c17.066667 17.066667 17.066667 42.666667 0 59.733334-17.066667 17.066667-42.666667 17.066667-59.733333 0l-17.066667-17.066667c-72.533333 81.066667-162.133333 149.333333-264.533333 200.533333-8.533333 0-17.066667 4.266667-21.333334 4.266667z" fill="currentColor" p-id="16186"></path></svg>
          </el-icon>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item :command="0">{{$t('common.chinese')}}</el-dropdown-item>
              <el-dropdown-item :command="1">{{$t('common.english')}}</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </el-tooltip>
      <a-tooltip :title="$t('common.exit')">
        <a-button size="middle" class="mr-2" shape="circle" @click="exitSystem()">
          <template #icon>
            <svg t="1719042486093" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="4867" width="24" height="24"><path d="M533.333333 64a21.333333 21.333333 0 0 1 21.333334 21.333333v42.666667a21.333333 21.333333 0 0 1-21.333334 21.333333H170.666667v725.333334h362.666666a21.333333 21.333333 0 0 1 21.333334 21.333333v42.666667a21.333333 21.333333 0 0 1-21.333334 21.333333H170.666667a85.333333 85.333333 0 0 1-85.226667-81.066667L85.333333 874.666667V149.333333a85.333333 85.333333 0 0 1 81.066667-85.226666L170.666667 64h362.666666z m194.581334 219.584l183.168 183.168a64 64 0 0 1 2.88 87.424l-2.88 3.072-183.168 183.168a21.333333 21.333333 0 0 1-30.165334 0l-30.165333-30.165333a21.333333 21.333333 0 0 1 0-30.165334L792.96 554.666667H362.666667a21.333333 21.333333 0 0 1-21.333334-21.333334v-42.666666a21.333333 21.333333 0 0 1 21.333334-21.333334h430.293333l-125.376-125.418666a21.333333 21.333333 0 0 1 0-30.165334l30.165333-30.165333a21.333333 21.333333 0 0 1 30.165334 0z" fill="#222429" p-id="4868"></path></svg>
          </template>
        </a-button>
      </a-tooltip>
    </div>
  </section>
</template>

<script setup>
// Sloperadar Backstage Cesium / 2024-06-22 / 15:17:05 / 71901
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw, h} from 'vue';
import {useMapStore} from "@/store/index.js";
import {CloseCircleOutlined, CloseOutlined, PlusOutlined} from "@ant-design/icons-vue";
import {useRouter} from "vue-router";
import {staticDataBind} from "@/utils/radartool.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {showMessage} from "@/utils/tools.js";
import {useI18n} from "vue-i18n";

/*-- name --*/
defineComponent({
  name: "header",
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
const { t,locale } = useI18n();
const router = useRouter();
/*-- vars --*/

/*-- methods --*/
const localeOnChange=(value)=>{
  locale.value = value===0?'zh':'en';
  store.sysinfo.config.language = value===0?'0':'1';
}
const badgeOnClick=(item)=>{
  ApiRadar.GetRadarLastHeartbeatTime(store.sysinfo.serverIp,item.id).then(res=>{
    showMessage(res.data.code===500?res.data.data:('最后上线时间为:'+res.data.data.substring(0,19)),'success',6000);
  })
}
const exitSystem=()=>{
  sessionStorage.setItem('isauthorized', 'false');
  router.push('/login');
  window.location.reload();
}
const projectOnChange=()=>{
  staticDataBind();
}
/*-- events --*/
onMounted(() => {
  //console.log('Header.onMounted');
});
</script>

<style scoped>
#idheader {
  width: 100%;
  height: 50px;
  position: absolute;
  z-index: 999;
  background: rgba(0,0,0,.5);
}
.headertitile{
  font-size: 24px;
  height: 50px;
  line-height: 50px;
  margin-left: 15px;
  position: absolute;
}
.headerrightbtn{
  height: 50px;
  line-height: 50px;
  margin-top: 2px;
}
.projectContainer{
  height: 100%;
  position: absolute;
  left: 440px;
  padding: 9px;
}
.projectContainerBox{
  height: 100%;
  width: 100%;
  border: 1px solid rgba(255,255,255,1);
  padding: 0 10px 0 10px;
  border-radius: 5px;
}
.projectContainerBoxTitle{
  line-height: 30px;
  height: 30px;
}
</style>