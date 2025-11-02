<template>
  <section id="idalarmruleinfo" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon class="custom-header-icon" @click="store.toolbarcontent='alarmRule'">
          <template #component>
            <svg fill="currentColor" t="1701236917704" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="4208" width="1em" height="1em"><path d="M256 460.8h665.6v102.4H256z" p-id="4209"></path><path d="M409.6 801.792l72.192-72.704L264.704 512l217.088-217.088L409.6 222.208 119.808 512 409.6 801.792z" p-id="4210"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('backend.rebackAlarmRule')}}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row>
          <a-button style="margin-bottom: 5px" type="primary" ghost block @click="commitUpdate">{{store.alarmRuleInfo?$t('common.commitChange'):$t('common.commitAppend')}}</a-button>
          <el-form
              :model="form"
              style="width: 100%"
              label-position="left">
            <el-form-item :label="$t('alarmInfo.alarmRuleName')">
              <el-input v-model="form.name" />
            </el-form-item>
            <el-form-item :label="$t('alarmInfo.alarmRuleDescribe')">
              <el-input v-model="form.describe" />
            </el-form-item>
            <el-form-item :label="$t('backend.radarDevice')">
              <el-select
                  v-model="store.radarInfo.deviceId"
                  :placeholder="$t('decoration.radarDropdown')"
              >
                <el-option
                    v-for="item in store.projectInfo.deviceData"
                    :key="item.id"
                    :label="item.name"
                    :value="item.id"
                />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('alarmInfo.alarmRule')">
              <el-radio-group v-model="form.operator">
                <el-radio label=">" />
                <el-radio label="<" />
                <el-radio label=">=" />
                <el-radio label="<=" />
              </el-radio-group>
            </el-form-item>
            <el-form-item :label="$t('alarmInfo.isAbs')">
              <el-radio-group v-model="form.absvalue">
                <el-radio :label="$t('common.yes')" />
                <el-radio :label="$t('common.no')" />
              </el-radio-group>
            </el-form-item>
            <el-form-item :label="$t('backend.monitorSelectAll')">
              <el-switch
                  v-model="form.selectall"
                  inline-prompt
                  :active-text="$t('common.selectAll')"
                  :inactive-text="$t('common.selectNone')"
                  @change="monitorSelected"
              />
            </el-form-item>
            <el-form-item :label="$t('common.monitor')">
              <el-select
                  v-model="form.monitoringposition"
                  multiple
                  :placeholder="$t('decoration.placeholderMonitorDropdown')"
                  style="width: 100%"
              >
                <el-option
                    v-for="item in monitorPoints.filter(item=>item.radarId===store.radarInfo.deviceId)"
                    :key="item.id"
                    :label="item.name"
                    :value="item.id"
                />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('alarmInfo.ruleSelect')">
              <el-checkbox-group v-model="form.target">
                <el-checkbox :label="$t('common.deform')"/>
                <el-checkbox :label="$t('common.speed')"/>
                <el-checkbox :label="$t('common.accelerate')"/>
              </el-checkbox-group>
            </el-form-item>
            <el-form-item :label="$t('common.deform')" v-show="form.target.indexOf($t('common.deform'))!==-1">
              <el-col :span="6">
                <el-input v-model="form.displaceblue" :placeholder="$t('common.blue')"/>
              </el-col>
              <el-col :span="6">
                <el-input v-model="form.displaceyellow" :placeholder="$t('common.yellow')"/>
              </el-col>
              <el-col :span="6">
                <el-input v-model="form.displaceorange" :placeholder="$t('common.orange')"/>
              </el-col>
              <el-col :span="6">
                <el-input v-model="form.displacered" :placeholder="$t('common.red')"/>
              </el-col>
            </el-form-item>
            <el-form-item :label="$t('common.speed')" v-show="form.target.indexOf($t('common.speed'))!==-1">
              <el-col :span="6">
                <el-input v-model="form.speedblue" :placeholder="$t('common.blue')"/>
              </el-col>
              <el-col :span="6">
                <el-input v-model="form.speedyellow" :placeholder="$t('common.yellow')"/>
              </el-col>
              <el-col :span="6">
                <el-input v-model="form.speedorange" :placeholder="$t('common.orange')"/>
              </el-col>
              <el-col :span="6">
                <el-input v-model="form.speedred" :placeholder="$t('common.red')"/>
              </el-col>
            </el-form-item>
            <el-form-item :label="$t('alarmInfo.speedUnit')" v-show="form.target.indexOf($t('common.speed'))!==-1">
              <el-select
                  v-model="form.speedtimeunit"
                  :placeholder="$t('alarmInfo.speedUnitDropdown')"
                  style="width: 100%"
              >
                <el-option :label="'30'+$t('common.minute')" value="02"/>
                <el-option :label="'1'+$t('common.hour')" value="03"/>
                <el-option :label="'1'+$t('common.day')" value="04"/>
                <el-option :label="'1'+$t('common.week')" value="05"/>
                <el-option :label="'1'+$t('common.month')" value="06"/>
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('common.accelerate')" v-show="form.target.indexOf($t('common.accelerate'))!==-1">
              <el-col :span="6">
                <el-input v-model="form.accelerateblue" :placeholder="$t('common.blue')"/>
              </el-col>
              <el-col :span="6">
                <el-input v-model="form.accelerateyellow" :placeholder="$t('common.yellow')"/>
              </el-col>
              <el-col :span="6">
                <el-input v-model="form.accelerateorange" :placeholder="$t('common.orange')"/>
              </el-col>
              <el-col :span="6">
                <el-input v-model="form.acceleratered" :placeholder="$t('common.red')"/>
              </el-col>
            </el-form-item>
            <el-form-item :label="$t('alarmInfo.accelerateUnit')" v-show="form.target.indexOf($t('common.accelerate'))!==-1">
              <el-select
                  v-model="form.acceleratetimeunit"
                  :placeholder="$t('alarmInfo.accelerateUnitDropdown')"
                  style="width: 100%"
              >
                <el-option :label="'30'+$t('common.minute')" value="02"/>
                <el-option :label="'1'+$t('common.hour')" value="03"/>
                <el-option :label="'1'+$t('common.day')" value="04"/>
                <el-option :label="'1'+$t('common.week')" value="05"/>
                <el-option :label="'1'+$t('common.month')" value="06"/>
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('common.isEnabled')">
              <el-radio-group v-model="form.enabled">
                <el-radio :label="$t('common.enable')" />
                <el-radio :label="$t('common.disable')" />
              </el-radio-group>
            </el-form-item>
            <!--            <a-button block ghost @click="addrule">{{commitbtntitle}}</a-button>-->
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-29 / 13:37:09 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {showMessage} from "@/utils/tools.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {useI18n} from "vue-i18n";
/*-- name --*/
defineComponent({
  name: "alarmruleinfo",
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
const form = reactive({
  name:'',
  describe:'',
  operator:'',
  absvalue:'',
  monitoringposition:[],
  displaceblue:'',
  displaceyellow:'',
  displaceorange:'',
  displacered:'',
  target:[],
  speedtimeunit:'',
  speedblue:'',
  speedyellow:'',
  speedorange:'',
  speedred:'',
  acceleratetimeunit:'',
  accelerateblue:'',
  accelerateyellow:'',
  accelerateorange:'',
  acceleratered:'',
  enabled:'',
  selectall:false
})
/*-- store --*/
const store = useMapStore();
/*-- vars --*/
const {t} = useI18n();
const monitorPoints = ref([]);
/*-- methods --*/
const monitorSelected=(value)=>{
  if (value){
    form.monitoringposition = monitorPoints.value.map(item=>item.id);
  }else{
    form.monitoringposition = [];
  }
}
const commitUpdate=()=>{
  form.projectId = store.radarInfo.projectId;
  form.deviceId = store.radarInfo.deviceId;
  form.enable = form.enabled===t('common.enable');
  form.targetFlag = form.absvalue===t('common.yes');
  form.displacement = form.target.indexOf(t('common.deform'))!==-1;
  form.speed = form.target.indexOf(t('common.speed'))!==-1;
  form.acceleration = form.target.indexOf(t('common.accelerate'))!==-1;
  form.geoMarkArray = toRaw(form.monitoringposition);
  if (store.alarmRuleInfo){
    ApiRadar.updateAlarmRule(form).then(res=>{
      showMessage(res.data.data);
      store.toolbarcontent = 'alarmRule';
    })
  }else{
    ApiRadar.addAlarmRule(form).then(res=>{
      showMessage(res.data.data);
      store.toolbarcontent = 'alarmRule';
    })
  }
}
/*-- events --*/
onMounted(() => {
  //实体绑定
  // console.log(store.monitorDevice.treeData);
  for (let i = 0; i < 2; i++) {
    for (let j = 0; j < store.monitorDevice.treeData[i].children.length; j++) {
      monitorPoints.value.push({name:store.monitorDevice.treeData[i].children[j].title,id:store.monitorDevice.monitorEntityMap[store.monitorDevice.treeData[i].children[j].key],radarId:store.monitorDevice.treeData[i].children[j].radarId});
    }
  }
  //静态数据初始化
  if (!store.alarmRuleInfo)return;
  form.id = store.alarmRuleInfo.id;
  form.name = store.alarmRuleInfo.name;
  form.describe = store.alarmRuleInfo.describe;
  form.operator = store.alarmRuleInfo.operator;
  form.absvalue = store.alarmRuleInfo.absvalue;
  form.enabled = store.alarmRuleInfo.enabled;
  form.selectall = store.alarmRuleInfo.geoMarkArray.length===Object.keys(store.monitorDevice.monitorEntityMap).length;
  form.monitoringposition = store.alarmRuleInfo.geoMarkArray;
  form.target = store.alarmRuleInfo.target;
  form.displaceblue = store.alarmRuleInfo.displacementvalue[0];
  form.displaceyellow = store.alarmRuleInfo.displacementvalue[1];
  form.displaceorange = store.alarmRuleInfo.displacementvalue[2];
  form.displacered = store.alarmRuleInfo.displacementvalue[3];
  form.speedblue = store.alarmRuleInfo.speedvalue[0];
  form.speedyellow = store.alarmRuleInfo.speedvalue[1];
  form.speedorange = store.alarmRuleInfo.speedvalue[2];
  form.speedred = store.alarmRuleInfo.speedvalue[3];
  form.accelerateblue = store.alarmRuleInfo.accelerationvalue[0];
  form.accelerateyellow = store.alarmRuleInfo.accelerationvalue[1];
  form.accelerateorange = store.alarmRuleInfo.accelerationvalue[2];
  form.acceleratered = store.alarmRuleInfo.accelerationvalue[3];
  form.speedtimeunit = store.alarmRuleInfo.speedtimeunit;
  form.acceleratetimeunit = store.alarmRuleInfo.acceleratetimeunit;
  //console.log('AlarmRuleInfo.onMounted');
});
</script>

<style scoped>
#idalarmruleinfo {
  height: 100%;
  width: 100%;
}
</style>