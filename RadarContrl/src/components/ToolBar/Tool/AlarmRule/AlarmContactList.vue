<template>
  <section id="idalarmcontactlist" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;预警联系人列表</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle" class="custome-row">
          <a-button class="custom-ant-btn" type="primary" ghost block @click="store.alarmContactInfo=null;store.toolbarcontent='alarmContactInfo'">{{$t('alarmInfo.addContact')}}</a-button>
        </a-row>
        <a-row>
          <el-table :data="contactData" style="width: 100%">
            <el-table-column prop="name" :label="$t('common.fullName')" width="70"/>
            <el-table-column prop="phone" :label="$t('common.phone')" width="110"/>
            <el-table-column prop="alarmLevel" :formatter="formatLevel" :label="$t('decoration.alarmLevel')" width="260"/>
            <el-table-column prop="enable" :formatter="formatStatus" :label="$t('common.status')" width="60"/>
            <el-table-column prop="email" :label="$t('common.email')" width="220"/>
            <el-table-column :label="$t('common.operate')" width="99" fixed="right">
              <template #default="scope">
                <el-button link type="primary" size="small" @click="itemWatch(scope.row)">{{$t('common.modify')}}</el-button>
                <el-button link type="primary" size="small" @click="itemDelete(scope.row)">{{$t('common.delete')}}</el-button>
              </template>
            </el-table-column>
          </el-table>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-29 / 14:46:29 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {showMessage} from "@/utils/tools.js";
import {getAlarmLevel} from "@/utils/radartool.js";
import {useI18n} from "vue-i18n";
/*-- name --*/
defineComponent({
  name: "alarmcontactlist",
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
const {t} = useI18n()
/*-- vars --*/
const contactData = ref([]);
/*-- methods --*/
const formatStatus=(row, column, cellValue)=>{
  return cellValue?t('common.enable'):t('common.disable');
}
const formatLevel=(row,column,cellValue)=>{
  let alarmlevel = '';
  for (let i = 0; i < cellValue.length; i++) {
    alarmlevel+=getAlarmLevel(cellValue[i])+',';
  }
  return alarmlevel.substring(0,alarmlevel.length-1);
}
const itemWatch = (row)=>{
  store.alarmContactInfo = row;
  store.toolbarcontent = 'alarmContactInfo';
}
const itemDelete = (row)=>{
  ApiRadar.deleteAlarmContact(row.id,store.radarInfo.projectId).then(res=>{
    showMessage(res.data.data);
    dataInit();
  })
}
const dataInit=()=>{
  ApiRadar.getAlarmContact(store.radarInfo.projectId).then(res=>{
    contactData.value = res.data.code===0?res.data.data:[];
  })
}
/*-- events --*/
onMounted(() => {
  dataInit();
  //console.log('AlarmContactList.onMounted');
});
</script>

<style scoped>
#idalarmcontactlist {
  height: 100%;
  width: 100%;
}
</style>