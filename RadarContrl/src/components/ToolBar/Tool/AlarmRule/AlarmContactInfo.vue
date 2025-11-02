<template>
  <section id="idalarmcontactinfo" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon class="custom-header-icon" @click="store.toolbarcontent='alarmPeople'">
          <template #component>
            <svg fill="currentColor" t="1701236917704" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="4208" width="1em" height="1em"><path d="M256 460.8h665.6v102.4H256z" p-id="4209"></path><path d="M409.6 801.792l72.192-72.704L264.704 512l217.088-217.088L409.6 222.208 119.808 512 409.6 801.792z" p-id="4210"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('backend.rebackAlarmList')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row>
          <a-button style="margin-bottom: 5px" type="primary" ghost block @click="commitUpdate">{{store.alarmContactInfo?$t('common.commitChange'):$t('common.commitAppend')}}</a-button>
          <el-form
              :model="form"
              label-width="100px"
              label-position="left">
            <el-form-item :label="$t('backend.contactName')">
              <el-input v-model="form.name" />
            </el-form-item>
            <el-form-item :label="$t('backend.contactEmail')">
              <el-input v-model="form.email" />
            </el-form-item>
            <el-form-item :label="$t('common.phone')">
              <el-input v-model="form.phone" />
            </el-form-item>
            <el-form-item :label="$t('decoration.alarmLevel')">
              <el-checkbox-group v-model="form.level">
                <el-checkbox :label="$t('decoration.alarmNormal')"/>
                <el-checkbox :label="$t('decoration.alarmBlue')"/>
                <el-checkbox :label="$t('decoration.alarmYellow')"/>
                <el-checkbox :label="$t('decoration.alarmOrange')"/>
                <el-checkbox :label="$t('decoration.alarmRed')"/>
              </el-checkbox-group>
            </el-form-item>
            <el-form-item :label="$t('common.isEnabled')">
              <el-radio-group v-model="form.enabled">
                <el-radio :label="$t('common.enable')" />
                <el-radio :label="$t('common.disable')" />
              </el-radio-group>
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
import {useI18n} from "vue-i18n";
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
const {t} = useI18n();
/*-- vars --*/

/*-- methods --*/
const commitUpdate=()=>{
  const status = form.enabled === t('common.enable');
  const level = toRaw(form.level).map(item => getAlarmLevelIndex(item));
  if (store.alarmContactInfo){
    ApiRadar.updateAlarmContact(form.id,form.name,form.email, form.phone, level, status, store.radarInfo.projectId)
        .then(res=>{
          showMessage(res.data.data);
          store.toolbarcontent = 'alarmPeople';
        })
  }else{
    ApiRadar.addAlarmContact(form.name, form.email, form.phone, level, status, store.radarInfo.projectId)
        .then(res=>{
          showMessage(res.data.data);
          store.toolbarcontent = 'alarmPeople';
        })
  }
}
/*-- events --*/
onMounted(() => {
  if (!store.alarmContactInfo)return;
  form.name = store.alarmContactInfo.name;
  form.email = store.alarmContactInfo.email;
  form.phone = store.alarmContactInfo.phone;
  form.enabled = store.alarmContactInfo.enable?t('common.enable'):t('common.disable');
  form.level = toRaw(store.alarmContactInfo.alarmLevel).map(item=>getAlarmLevel(item));
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