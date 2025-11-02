<template>
  <div id="treeview">
    <div>
      <a-space :size="2">
        <a-button class="custom-ant-btn" type="primary"  size="small" @click="showMonitoringPage('point')">
          <template #icon> <AimOutlined /></template>
          {{$t('map.addMonitorTitle')}}
        </a-button>
        <a-button class="custom-ant-btn" type="primary"  size="small" @click="showMonitoringPage('polygon')">
          <template #icon>  <BuildOutlined/></template>
          {{$t('map.addMonitorPolygonTitle')}}
        </a-button>
      </a-space>
    </div>
    <div class="statusbar">
      <a-space :size="2">
        <el-badge :max="999999" :value="store.monitorDevice.treeData[0].children.length" class="item" type="primary">
          <el-button color="rgba(0,0,0,.5)" size="small" text disabled style="cursor:default">{{$t('common.monitoringPoint')}}</el-button>
        </el-badge>
        <el-badge :max="999999" :value="store.monitorDevice.treeData[1].children.length" class="item">
          <el-button color="rgba(0,0,0,.5)" size="small" text disabled style="cursor:default">{{$t('common.monitoringPolygon')}}</el-button>
        </el-badge>
        <el-switch class="ml-4" v-model="selectedAll" :active-text="$t('common.showAll')" :inactive-text="$t('common.hide')" @change="switchOnChange"/>
      </a-space>
    </div>
    <a-input-search v-model:value="searchValue" :placeholder="$t('common.search')" />
    <a-spin :spinning="store.monitorDevice.treeSpinning">
      <a-tree
          v-model:expandedKeys="expandedKeys"
          :auto-expand-parent="autoExpandParent"
          :tree-data="store.dynamicTreeData"
          @select="itemSelect"
          @check="itemCheck"
          class="custom-ant-tree"
      >
        <template #title="{ key: treeKey, title }">
          <a-dropdown :trigger="['contextmenu']">
        <span v-if="title.indexOf(searchValue) > -1">
          {{ title.substr(0, title.indexOf(searchValue)) }}
          <span style="color: #13d7ea">{{ searchValue }}</span>
          {{ title.substr(title.indexOf(searchValue) + searchValue.length) }}
        </span>
            <span v-else>{{ title }}</span>
            <template #overlay>
              <a-menu @click="({ key: menuKey }) => onContextMenuClick(treeKey, menuKey)" theme="dark">
                <a-menu-item key="deleteMonitor">{{$t('common.delete')+$t('common.monitor')}}</a-menu-item>
                <a-menu-item key="copyId">{{$t('common.copy')+$t('common.monitor')}}Id</a-menu-item>
              </a-menu>
            </template>
          </a-dropdown>
        </template>
      </a-tree>
    </a-spin>
  </div>
  <el-dialog v-model="radarConfirm" title="请选择需要绑定的雷达并确认在该雷达覆盖范围内"  width="30%">
    <el-form>
      <el-form-item label="雷达名称">
        <el-select v-model="store.radarInfo.deviceId" style="width: 100%">
          <el-option
              v-for="item in store.projectInfo.deviceData"
              :key="item.id"
              :label="item.name"
              :value="item.id"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="添加方式" v-show="!addPolygon">
        <el-radio-group v-model="form.addMethod">
          <el-radio label="地图选点"></el-radio>
          <el-radio label="手输坐标"></el-radio>
        </el-radio-group>
      </el-form-item>
      <el-form-item label="类型设置" v-show="addPolygon">
        <el-radio-group v-model="form.enableShield">
          <el-radio label="监测区域"></el-radio>
          <el-radio label="屏蔽区域"></el-radio>
        </el-radio-group>
      </el-form-item>
      <el-form-item label="点位名称" v-show="form.addMethod==='手输坐标'&&!addPolygon">
        <el-input v-model="form.title"></el-input>
      </el-form-item>
      <el-form-item label="点位经度" v-show="form.addMethod==='手输坐标'&&!addPolygon">
        <el-input v-model="form.longitude"></el-input>
      </el-form-item>
      <el-form-item label="点位纬度" v-show="form.addMethod==='手输坐标'&&!addPolygon">
        <el-input v-model="form.latitude"></el-input>
      </el-form-item>
      <el-form-item label="点位高度" v-show="form.addMethod==='手输坐标'&&!addPolygon">
        <el-input v-model="form.altitude"></el-input>
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="radarConfirm = false">取消</el-button>
        <el-button type="primary" @click="nextOperate">{{form.addMethod==='地图选点'?'下一步':'完成添加'}}</el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup>
import {AimOutlined,BuildOutlined,VideoCameraOutlined,DownOutlined,UserOutlined} from '@ant-design/icons-vue';
import {defineComponent, watch, ref, onMounted, toRaw, h, reactive} from 'vue';
import {
  foreachTree, getParentKey, showMessage
} from "@/utils/tools.js";
import {useMapStore} from "@/store/index.js";
import {CesiumUtils} from "@/utils/CesiumUtils.js";
import {addMonitorPoint, addMonitorPolygon, getUUID} from "@/utils/radartool.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {CommonUtils} from "@/utils/CommonUtils.js";
import {MonitorPoint} from "@/assets/load.js";


defineComponent({
  name: "TreeView"
});
const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  }
});
const store = useMapStore();
const addPolygon = ref(false);
const searchValue = ref('');
const expandedKeys = ref([]);
const autoExpandParent = ref(true);
const clickValue = ref('');
const form = reactive({
  addMethod:'地图选点',
  enableShield:'监测区域'
})
const selectedAll = ref(true);
const radarConfirm = ref(false);
const nextOperate = ()=>{
  radarConfirm.value = false;
  if (store.radarInfo.deviceId===undefined||store.radarInfo.deviceId===''){
    CommonUtils.ShowMessage('请选择设备，操作失败','warning');
    return;
  }
  if (form.addMethod==='手输坐标'){
    const uuid = getUUID();
    ApiRadar.addMonitoringLocation({
      id:uuid,
      projectId:store.radarInfo.projectId,
      name:form.title,
      type:'GEO-POINT',
      deviceId:store.radarInfo.deviceId,
      coordinate:[[form.longitude,form.latitude,form.altitude]]
    }).then(data=>{
      if (data.data.msg==='操作成功'){
        CommonUtils.ShowMessage('新增成功','success');
        CesiumUtils.EntityPointAdd(form.longitude,form.latitude,form.altitude,MonitorPoint,form.title).then(entity=>{
          store.monitorDevice.treeData[0].children.push({title:form.title,key:entity.id});
          store.monitorDevice.monitorEntityMap[entity.id] = uuid;
        })
      }
    })
  }else{
    addPolygon.value?addMonitorPolygon(form.enableShield==='屏蔽区域'):addMonitorPoint();
  }
}
const switchOnChange=()=>{
  const objects = Object.keys(store.monitorDevice.monitorEntityMap);
  objects.map(item=>{
    CesiumUtils.FindEntityById(item).show = selectedAll.value;
  })
}
const showMonitoringPage=(type)=>{
  addPolygon.value = type==='polygon';
  form.addMethod = '地图选点';
  form.title = '';
  form.longitude = '';
  form.latitude = '';
  form.altitude = '';
  radarConfirm.value = true;
}
watch(searchValue, value => {
  if (value===''){
    expandedKeys.value = [];
    autoExpandParent.value = false;
    return;
  }
  searchValue.value = value;
  for (let i = 0; i < store.monitorDevice.treeData[0].children.length; i++) {
    if (store.monitorDevice.treeData[0].children[i].title.indexOf(value)>-1){
      expandedKeys.value = [store.monitorDevice.treeData[0].key];
      autoExpandParent.value = true;
      return;
    }
  }
  for (let i = 0; i < store.monitorDevice.treeData[1].children.length; i++) {
    if (store.monitorDevice.treeData[1].children[i].title.indexOf(value)>-1){
      expandedKeys.value = [store.monitorDevice.treeData[1].key];
      autoExpandParent.value = true;
      break;
    }
  }
});
watch(clickValue, value => {
  if (value===''){
    autoExpandParent.value = false;
    return;
  }
  expandedKeys.value = store.monitorDevice.treeData
      .map((item) => {
        if (item.key.indexOf(value) > -1) {
          return getParentKey(item.key, toRaw(store.monitorDevice.treeData));
        }
        return null;
      })
      .filter((item, i, self) => item && self.indexOf(item) === i);
  clickValue.value = value;
  autoExpandParent.value = true;
});
function itemSelect(selectedKeys,e){
  if (selectedKeys.indexOf('0-0')>-1||selectedKeys.indexOf('0-1')>-1)expandedKeys.value = selectedKeys;
  if (selectedKeys.indexOf('0-0')===-1&&expandedKeys.value.indexOf('0-0')===0||selectedKeys.indexOf('0-1')===-1&&expandedKeys.value.indexOf('0-1')===0)expandedKeys.value = selectedKeys;
  if (expandedKeys.value.indexOf('0-0')===-1&&expandedKeys.value.indexOf('0-1')===-1){
    CesiumUtils.ZoomToEntity(CesiumUtils.FindEntityById(selectedKeys[0]));
  }
}
function itemCheck(checkedKeys, e){
  if (checkedKeys.length===0){
    //长度为0 清空所有点标记
    store.trackEntityCollection.map(entity=>{
      CesiumUtils.EntityRemove(entity);
    })
    store.trackEntityCollection = [];
    return;
  }
  if (!e.checked){
    //取消选择 清空该树下的所有点标记
    foreachTree(CesiumUtils.viewer,[e.node],false)
    return;
  }

  //选中后 加载该树下的所有点标记
  store.treeEntityNum = 0;
  foreachTree(CesiumUtils.viewer,[e.node]);
  setTimeout(function (){
    toRaw(CesiumUtils.viewer).zoomTo(toRaw(store.trackEntityCollection));
  },500);
}
function handleCommandMenuClick(e) {
  store.toolbarcontent = store.toolbarcontent==='command'?'':'command';
}
function onContextMenuClick(treeKey, menuKey){
  switch (menuKey){
    case 'deleteMonitor':
      ApiRadar.deleteMonitor(store.monitorDevice.monitorEntityMap[treeKey],store.radarInfo.projectId).then(()=>{
        showMessage('删除成功');
        CesiumUtils.EntityRemoveById(treeKey);
        let index = CommonUtils.FindIndexOfArray('key',treeKey,store.monitorDevice.treeData[0].children);
        if (index!==-1){
          store.monitorDevice.treeData[0].children.splice(index,1)
        }else{
          index = CommonUtils.FindIndexOfArray('key',treeKey,store.monitorDevice.treeData[1].children);
          store.monitorDevice.treeData[1].children.splice(index,1)
        }
      })
      break;
    case 'copyId':{
      navigator.clipboard.writeText(store.monitorDevice.monitorEntityMap[treeKey]);
      showMessage("已复制到剪切板"+store.monitorDevice.monitorEntityMap[treeKey]);
    }
  }

}
onMounted(()=>{

})
</script>

<style scoped>
#treeview{

  position: absolute;
  z-index: 888;
  margin: 60px 0 0 10px;
}
:deep(.ant-input){
  background: rgba(0,0,0,0.5);
  color:white;
}
:deep(.ant-input-group-addon){
  background: transparent;
}
:deep(.ant-input-search-button){
  background: rgba(0,0,0,0.5);
}
:deep(.anticon-search){
  color:white;
}
.ant-input-search{
  margin: 8px 0 8px 0;
}
.statusbar{
  margin-top: 15px;
  text-align: left;
}
.ant-btn-group{
  background: transparent;
}
</style>