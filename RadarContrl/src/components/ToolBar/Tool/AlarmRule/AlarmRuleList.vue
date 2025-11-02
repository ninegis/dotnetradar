<template>
  <section id="idalarmrule" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('backend.alarmRule')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle" class="custome-row">
          <a-button class="custom-ant-btn" type="primary" ghost block @click="store.alarmRuleInfo=null;store.toolbarcontent='alarmRuleInfo'">新增规则</a-button>
        </a-row>
        <a-row>
          <el-table :data="tableData" style="width: 100%;color:white" class="custom-table" height="300">
            <el-table-column type="expand">
              <template #default="props">
                  <p class="expanded-p">{{$t('backend.operator')+':'+ $t('decoration.alarmBlue')+ props.row.bluerule +','+$t('decoration.alarmYellow')+props.row.yellowrule+','+$t('decoration.alarmOrange')+props.row.orangerule+','+$t('decoration.alarmRed')+props.row.redrule}}</p>
              </template>
            </el-table-column>
            <el-table-column prop="name" :label="$t('common.name')" width="88"/>
            <el-table-column prop="describe" :label="$t('common.describe')" width="130"/>
            <el-table-column prop="operator" :label="$t('backend.operator')" width="60"/>
            <el-table-column prop="absvalue" :label="$t('common.absolute')" width="70"/>
            <el-table-column prop="enabled" :label="$t('common.isEnabled')" width="80"/>
            <el-table-column label="操作" width="99">
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
// kot-security-cloud-radar / 2023-11-27 / 21:52:50 / 71901
/*-- imports --*/
import {defineComponent, ref, onMounted, computed} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {getColorRule, getTarget, getTargetStatus, getTimeUnit,getValue} from "@/utils/radartool.js";
import {showMessage} from "@/utils/tools.js";
import {useI18n} from "vue-i18n";
/*-- name --*/
defineComponent({
  name: "alarmrule",
});
/*-- props  --*/
const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
});
/*-- stores --*/
const store = useMapStore();
const {t} = useI18n();
/*-- vars --*/
const tableData = ref([]);
/*-- methods --*/
const itemWatch = (row)=>{
  store.alarmRuleInfo = row;
  store.toolbarcontent = 'alarmRuleInfo';
}
const itemDelete = (row)=>{
  ApiRadar.deleteAlarmRule(row.id,store.radarInfo.projectId).then(res=>{
    showMessage(res.data.data);
    dataInit();
  })
}
const dataInit = ()=>{
  ApiRadar.getAlarmRule(store.radarInfo.projectId).then(res=>{
    tableData.value = [];
    const data = res.data.data;
    for (let i = 0; i <data.length ; i++) {
      tableData.value.push({
        id:data[i].id,
        name:data[i].ruleName,
        describe:data[i].ruleDescription,
        operator:data[i].alarmRule,
        bluerule:getColorRule(data[i].alarmTargetThresholds[0].targetCheckbox),
        yellowrule:getColorRule(data[i].alarmTargetThresholds[1].targetCheckbox),
        orangerule:getColorRule(data[i].alarmTargetThresholds[2].targetCheckbox),
        redrule:getColorRule(data[i].alarmTargetThresholds[3].targetCheckbox),
        targetStr:getTarget(data[i].alarmTargetThresholds[0].targetCheckbox),
        absvalue:data[i].targetFlag?t('common.yes'):t('common.no'),
        enabled:data[i].enable?t('common.enable'):t('common.disable'),
        content:data[i].alarmContent,
        geoMarkArray:data[i].geoMarkArray,
        target:getTargetStatus(data[i].alarmTargetThresholds[0].targetCheckbox),
        displacementvalue:getValue(data[i].alarmTargetThresholds,0),
        speedvalue:getValue(data[i].alarmTargetThresholds,1),
        accelerationvalue:getValue(data[i].alarmTargetThresholds,2),
        speedtimeunit:getTimeUnit(data[i].alarmTargetThresholds,1),
        acceleratetimeunit:getTimeUnit(data[i].alarmTargetThresholds,2),
      })
    }
  })
}
/*-- events --*/
onMounted(() => {
  dataInit();
  //console.log('AlarmRule.onMounted');
});
</script>

<style scoped>
.expanded-p{
  margin-left: 25px;
}
</style>