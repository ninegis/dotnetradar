<template>
  <section id="idalarmcontactinfo" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon class="custom-header-icon" @click="store.toolbarcontent='allowPeople'">
          <template #component>
            <svg fill="currentColor" t="1701236917704" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="4208" width="1em" height="1em"><path d="M256 460.8h665.6v102.4H256z" p-id="4209"></path><path d="M409.6 801.792l72.192-72.704L264.704 512l217.088-217.088L409.6 222.208 119.808 512 409.6 801.792z" p-id="4210"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('backend.rebackAllowPeople')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row>
          <a-button style="margin-bottom: 5px" type="primary" ghost block @click="commitUpdate">{{store.alarmContactInfo?'提交修改':'提交新增'}}</a-button>
          <el-form
              style="width: 100%"
              :model="form"
              label-position="left">
            <el-form-item :label="$t('common.fullName')">
              <el-input v-model="form.name"/>
            </el-form-item>
            <el-form-item :label="$t('common.phone')">
              <el-input v-model="form.phone" />
            </el-form-item>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-29 / 14:46:41 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {getAlarmLevel, staticDataBind} from "@/utils/radartool.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {showMessage} from "@/utils/tools.js";
import {getAlarmLevelIndex} from "@/utils/entityObjects.js";
/*-- name --*/
defineComponent({
  name: "alarmcontactinfo",
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
  if (store.alarmContactInfo){
    ApiRadar.updateAllowPeople(form.name, form.phone,store.sysinfo.config.projectCode)
        .then(res=>{
          showMessage(res.data.data);
          store.toolbarcontent = 'allowPeople';
        })
  }else{
    ApiRadar.addAllowPeople(form.name, form.phone,store.sysinfo.config.projectCode)
        .then(res=>{
          showMessage(res.data.data);
          store.toolbarcontent = 'allowPeople';
        })
  }
}
/*-- events --*/
onMounted(() => {
  if (!store.alarmContactInfo)return;
  form.name = store.alarmContactInfo.name;
  form.phone = store.alarmContactInfo.phone;
  form.id = store.alarmContactInfo.id;
  // console.log('AlarmContactInfo.onMounted');
});
</script>

<style scoped>
#idalarmcontactinfo {
  height: 100%;
  width: 100%;
}
</style>