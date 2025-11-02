<template>
  <DraggerContainer :dragger-width="draggerWidth">
    <template v-slot:dragger-header>
      <Icon>
        <template #component>
          <svg width="1em" height="1em" fill="currentColor" viewBox="0 0 1024 1024">
            <path d="M787.2 1024l-640 0c-57.6 0-102.4-44.8-102.4-102.4L44.8 102.4C44.8 44.8 89.6 0 147.2 0l729.6 0c57.6 0 102.4 44.8 102.4 102.4l0 742.4c0 19.2-12.8 32-32 32s-32-12.8-32-32L915.2 102.4C915.2 83.2 896 64 876.8 64L147.2 64C128 64 108.8 83.2 108.8 102.4l0 819.2C108.8 940.8 128 960 147.2 960l640 0c19.2 0 32 12.8 32 32S806.4 1024 787.2 1024z" fill="#ffffff" p-id="5061"></path><path d="M800 339.2l-576 0C204.8 339.2 192 326.4 192 307.2l0-128c0-19.2 12.8-32 32-32l576 0c19.2 0 32 12.8 32 32l0 128C832 326.4 819.2 339.2 800 339.2zM256 275.2l512 0 0-64L256 211.2 256 275.2z" fill="#ffffff" p-id="5062"></path><path d="M332.8 524.8 249.6 524.8C230.4 524.8 217.6 512 217.6 492.8L217.6 441.6c0-19.2 12.8-32 32-32l89.6 0c19.2 0 32 12.8 32 32l0 51.2C364.8 512 352 524.8 332.8 524.8z" fill="#ffffff" p-id="5063"></path><path d="M556.8 524.8 467.2 524.8C448 524.8 435.2 512 435.2 492.8L435.2 441.6c0-19.2 12.8-32 32-32l89.6 0c19.2 0 32 12.8 32 32l0 51.2C588.8 512 576 524.8 556.8 524.8z" fill="#ffffff" p-id="5064"></path><path d="M774.4 524.8l-89.6 0c-19.2 0-32-12.8-32-32L652.8 441.6c0-19.2 12.8-32 32-32l89.6 0c19.2 0 32 12.8 32 32l0 51.2C806.4 512 793.6 524.8 774.4 524.8z" fill="#ffffff" p-id="5065"></path><path d="M332.8 697.6 249.6 697.6c-19.2 0-32-12.8-32-32L217.6 614.4c0-19.2 12.8-32 32-32l89.6 0c19.2 0 32 12.8 32 32l0 51.2C364.8 684.8 352 697.6 332.8 697.6z" fill="#ffffff" p-id="5066"></path><path d="M556.8 697.6 467.2 697.6c-19.2 0-32-12.8-32-32L435.2 614.4c0-19.2 12.8-32 32-32l89.6 0c19.2 0 32 12.8 32 32l0 51.2C588.8 684.8 576 697.6 556.8 697.6z" fill="#ffffff" p-id="5067"></path><path d="M774.4 697.6l-89.6 0c-19.2 0-32-12.8-32-32L652.8 614.4c0-19.2 12.8-32 32-32l89.6 0c19.2 0 32 12.8 32 32l0 51.2C806.4 684.8 793.6 697.6 774.4 697.6z" fill="#ffffff" p-id="5068"></path><path d="M332.8 876.8 249.6 876.8c-19.2 0-32-12.8-32-32l0-51.2c0-19.2 12.8-32 32-32l89.6 0c19.2 0 32 12.8 32 32l0 51.2C364.8 857.6 352 876.8 332.8 876.8z" fill="#ffffff" p-id="5069"></path><path d="M556.8 876.8 467.2 876.8c-19.2 0-32-12.8-32-32l0-51.2c0-19.2 12.8-32 32-32l89.6 0c19.2 0 32 12.8 32 32l0 51.2C588.8 857.6 576 876.8 556.8 876.8z" fill="#ffffff" p-id="5070"></path><path d="M774.4 876.8l-89.6 0c-19.2 0-32-12.8-32-32l0-51.2c0-19.2 12.8-32 32-32l89.6 0c19.2 0 32 12.8 32 32l0 51.2C806.4 857.6 793.6 876.8 774.4 876.8z" fill="#ffffff" p-id="5071"></path>          </svg>
        </template>
      </Icon>
      <span class="dragger-header">&nbsp;&nbsp;&nbsp;图上量测</span>

    </template>
    <template v-slot:dragger-content>
      <a-row type="flex" :gutter="16" align="middle" class="custome-row"  v-for="row in 1" :key="row">
        <a-col class="gutter-row" :span="8" v-for="col in 3" :key="col">
          <div class="gutter-box">
            <div  class="tool-thum" v-bind:class="{ToolItemClick:toolbarList[col+(row-1)*3-1].name ===toolItemClick}" :style="toolbarList[col+(row-1)*3-1].style" @click="GotoEvent(toolbarList[col+(row-1)*3-1].name)">
              <img :src='toolbarList[col+(row-1)*3-1].name===toolItemClick?`${toolbarList[col+(row-1)*3-1].imageb}`:`${toolbarList[col+(row-1)*3-1].image}`' :alt="toolbarList[col+(row-1)*3-1].title">
            </div>
            <span v-bind:class="{'selectedTitle':toolbarList[col+(row-1)*3-1].name===toolItemClick}">
                          {{toolbarList[col+(row-1)*3-1].title}}
            </span>
          </div>
        </a-col>
      </a-row>
      <a-row type="flex" :gutter="16" align="middle" class="custome-row">
        <a-button type="primary" ghost block @click="clearEntityCollection(store.viewer,store.drawingentity)">清空测量数据</a-button>
      </a-row>
    </template>
  </DraggerContainer>
</template>

<script setup>
// map / 2023-05-16 / 11:14:08 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, onUnmounted} from 'vue';
import DraggerContainer from '@/components/DragContainer/DragContainer.vue'
import Icon,{ DownOutlined, CloseOutlined } from '@ant-design/icons-vue'
import {useMapStore} from "@/store/index.js";
import {measureCoordinate, showMessage,clearEntityCollection,
  measureDistance,measureArea} from '@/utils/tools';
import {measurelocation,measurestickground,measureprofile,
  measurelocationb,measurestickgroundb, measureprofileb} from '@/assets/load';


/*-- name --*/
defineComponent({
  name: "measuredialog",
});
/*-- props  --*/
const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
  title:{
    type:String,
    required:false,
    default:'图上量测'
  }
});
/*-- stores --*/
const store = useMapStore();
/*-- vars --*/
const toolItemClick = ref('');
const draggerWidth = ref(276);
const toolbarList = reactive([
  {
    name:'coordinate',
    title:'坐标量测',
    image:measurelocation,
    imageb:measurelocationb,
    style:'background:#dd751b',
  },{
    name:'distance',
    title:'距离量测',
    image:measurestickground,
    imageb:measurestickgroundb,
    style:'background:#00F5FF',
  },{
    name:'area',
    title:'面积量测',
    image:measureprofile,
    imageb:measureprofileb,
    style: 'background:#88b8ff'
  },{
    name:'topview',
    title:'水平面积',
    image:'src/assets/measureicon/measure-level',
    style: 'background:#8B4C39'
  },{
    name:'sideview',
    title:'贴地面积',
    image:'src/assets/measureicon/measure-stickarea',
    style: 'background:#8B8989'
  },{
    name:'viewtracking',
    title:'角度',
    image:'src/assets/measureicon/measure-angle',
    style: 'background:#7CCD7C'
  },{
    name:'trackplayback',
    title:'角度差',
    image:'src/assets/measureicon/measure-angledifference',
    style: 'background:#4169E1'
  },{
    name:'rtpoint',
    title:'三角测量',
    image:'src/assets/measureicon/measure-triangulation',
    style: 'background:#FFC1C1'
  },{
    name:'none',
    title:'坐标量测',
    image:'src/assets/measureicon/measure-location',
    style: 'background:#F4A460'
  }])
/*-- methods --*/
function GotoEvent(title) {
  if (toolItemClick.value === title) {
    toolItemClick.value = '';
    return;
  }
  toolItemClick.value = title;
  switch (title){
    case 'coordinate':
      showMessage('结束，请按鼠标右击结束','success',6000);
      measureCoordinate(store.viewer).then((entityIdArray)=>{
        GotoEvent(title);
        for (let i = 0; i < entityIdArray.length; i++) {
          store.drawingentity.push(entityIdArray[i]);
        }
      })
      break;
    case 'distance':
      showMessage('结束，请按鼠标右击结束','success',6000);
      measureDistance(store.viewer).then(entityIdArray=>{
        GotoEvent(title);
        for (let i = 0; i < entityIdArray.length; i++) {
          store.drawingentity.push(entityIdArray[i]);
        }
      })
      break;
    case 'area':
      showMessage('结束，请按鼠标右击结束','success',6000);
      measureArea(viewer).then(entityIdArray=>{
        GotoEvent(title);
        for (let i = 0; i < entityIdArray.length; i++) {
          store.drawingentity.push(entityIdArray[i]);
        }
      })
      break;
  }
}
/*-- events --*/
onMounted(() => {
  //console.log('MeasureDialog.onMounted');
});
onUnmounted(()=>{
  clearEntityCollection(store.viewer,store.drawingentity);
})
</script>

<style scoped>
:deep(.custome-row){
  margin-top: 10px;
}
</style>