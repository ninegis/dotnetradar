<template>
  <section id="iddrawing" v-show="visible" class="">
    <DragContainer :dragger-width="276">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg t="1691716222821" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5105" width="16" height="16"><path d="M105.8 218.1l43.8-43.8L399.3 424l-65.7 65.7L105.8 262c-12.1-12.1-12.1-31.8 0-43.9z" p-id="5106" fill="#ffffff"></path><path d="M227.32 96.88l21.92 21.92-87.61 87.61-21.92-21.92zM304.561 174.02l21.92 21.92-87.61 87.61-21.92-21.92zM379.718 249.279l21.92 21.92-87.61 87.61-21.92-21.92zM454.945 324.466l21.92 21.92-87.61 87.61-21.92-21.92z" p-id="5107" fill="#ffffff"></path><path d="M537.4 693.5l65.7-65.7 247.6 247.6-43.8 43.8c-12.1 12.1-31.7 12.1-43.8 0L537.4 693.5z" p-id="5108" fill="#ffffff"></path><path d="M680.627 550.171l21.92 21.92-87.61 87.61-21.92-21.92zM755.854 625.359l21.92 21.92-87.61 87.61-21.92-21.92zM831.152 700.617l21.92 21.92-87.61 87.61-21.92-21.92zM906.379 775.805l21.92 21.92-87.61 87.61-21.92-21.92z" p-id="5109" fill="#ffffff"></path><path d="M630.5 217.8L214.3 634.1c-12.1 12.1-12.1 31.7 0 43.8l131.4 131.4c12.1 12.1 31.7 12.1 43.8 0l416.2-416.2c12.1-12.1 12.1-31.7 0-43.8L674.4 217.8c-12.1-12.1-31.8-12.1-43.9 0zM367.7 743.6L280 656l372.4-372.4 87.6 87.6-372.3 372.4zM740.1 108.3l-43.8 43.8c-12.1 12.1-12.1 31.7 0 43.8l131.4 131.4c12.1 12.1 31.7 12.1 43.8 0l43.8-43.8c12.1-12.1 12.1-31.7 0-43.8L783.9 108.3c-12.1-12.1-31.7-12.1-43.8 0z m98.6 186.2L729.1 185l43.8-43.8 109.5 109.5-43.7 43.8zM169.4 694.7L97.9 906.1c-4.1 12.1 7.5 23.7 19.6 19.6L329 854.3c10.9-3.7 14.1-17.5 6-25.6l-140-140c-8.1-8.2-22-4.9-25.6 6z m29.9 63.9l65.7 65.7L153.6 870l45.7-111.4z" p-id="5110" fill="#ffffff"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;图上标绘</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle" class="custome-row"  v-for="row in 2" :key="row">
          <a-col class="gutter-row" :span="8" v-for="col in 3" :key="col">
            <div class="gutter-box">
              <div  class="tool-thum" v-bind:class="{ToolItemClick:toolbarList[col+(row-1)*3-1].name ===toolItemClick}" :style="toolbarList[col+(row-1)*3-1].style" @click="GotoEvent(toolbarList[col+(row-1)*3-1].name)">
                <img :src='toolbarList[col+(row-1)*3-1].name===toolItemClick?`${toolbarList[col+(row-1)*3-1].image_b}`:`${toolbarList[col+(row-1)*3-1].image}`' :alt="toolbarList[col+(row-1)*3-1].title">
              </div>
              <span v-bind:class="{'selectedTitle':toolbarList[col+(row-1)*3-1].name===toolItemClick}">
                          {{toolbarList[col+(row-1)*3-1].title}}
            </span>
            </div>
          </a-col>
        </a-row>
        <a-row type="flex" :gutter="16" align="middle" class="custome-row">
          <a-button type="primary" ghost block @click="removeDrawing">清空图上标绘</a-button>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// desktop / 2023-08-11 / 08:57:33 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw, onUnmounted} from 'vue';
import {useMapStore} from "@/store/index.js";
import DragContainer from '@/components/DragContainer/DragContainer.vue'
import Icon from '@ant-design/icons-vue'
import {
  drawing_point,drawing_point_b,drawing_circle,drawing_circle_b,drawing_polyline,drawing_polyline_b,
  drawing_polygon,drawing_polygon_b,drawing_rectangle,drawing_rectangle_b
} from "@/assets/load.js";
import {drawPoint, clearEntityCollection, drawMultiPoint, showMessage,
  drawPolyline,drawPolygon,drawRectangle,drawCircle} from "@/utils/tools.js";
import {CesiumUtils} from "@/utils/CesiumUtils.js";
/*-- name --*/
defineComponent({
  name: "drawing",
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
const toolItemClick = ref('');
const toolbarList = reactive([
  {
    name:'point',
    title:'点位绘制',
    image:drawing_point,
    image_b:drawing_point_b,
    style:'background:#dd751b',
  },{
    name:'multipoint',
    title:'多点绘制',
    image:drawing_point,
    image_b:drawing_point_b,
    style:'background:#00F5FF',
  },{
    name:'polyline',
    title:'线段绘制',
    image:drawing_polyline,
    image_b:drawing_polyline_b,
    style: 'background:#88b8ff'
  },{
    name:'polygon',
    title:'多边形绘制',
    image:drawing_polygon,
    image_b:drawing_polygon_b,
    style: 'background:#8B4C39'
  },{
    name:'rectangle',
    title:'矩形绘制',
    image:drawing_rectangle,
    image_b:drawing_rectangle_b,
    style: 'background:#8B8989'
  },{
    name:'circle',
    title:'圆圈绘制',
    image:drawing_circle,
    image_b:drawing_circle_b,
    style: 'background:#7CCD7C'
  },{
    name:'trackplayback',
    title:'角度差',
    image:null,
    image_b:null,
    style: 'background:#4169E1'
  },{
    name:'rtpoint',
    title:'三角测量',
    image:null,
    image_b:null,
    style: 'background:#FFC1C1'
  },{
    name:'none',
    title:'坐标量测',
    image:null,
    image_b:null,
    style: 'background:#F4A460'
  }]);
const entityIds = [];
/*-- methods --*/
const removeDrawing = ()=>{
  for (let i = 0; i < entityIds.length; i++) {
    CesiumUtils.EntityRemoveById(entityIds[i]);
  }
}
const GotoEvent=(title)=>{
  if (toolItemClick.value === title) {
    toolItemClick.value = '';
    return;
  }
  switch (title){
    case 'point':
      CesiumUtils.DrawPoint('请在地图上单击一个点',drawing_point).then(result=>{
        entityIds.push(result[1].id);
      })
      break;
    case 'multipoint':
      showMessage('结束，请按鼠标右击结束','success');
      drawMultiPoint(store.viewer).then(entityIdArray=>{
        for (let i = 0; i < entityIdArray.length; i++) {
          store.drawingentity.push(entityIdArray[i]);
        }
      })
      break;
    case 'polyline':
      showMessage('结束，请按鼠标右击结束','success');
      drawPolyline(store.viewer).then(entity=>{
        store.drawingentity.push(entity.id);
      })
      break;
    case 'polygon':
      showMessage('结束，请按鼠标右击结束','success');
      drawPolygon(store.viewer).then(data=>{
        store.drawingentity.push(data[1].id);
      })
      break;
    case 'circle':
      drawCircle(store.viewer).then(id=>{
        store.drawingentity.push(id);
      });
      break;
    case 'rectangle':
      drawRectangle(store.viewer).then(data=>{
        store.drawingentity.push(data[1].id);
      })
      break;
  }
}
/*-- events --*/
onMounted(() => {
  //console.log('Drawing.onMounted');
});
onUnmounted(()=>{
  clearEntityCollection(store.viewer,store.drawingentity);
})
</script>

<style scoped>
#iddrawing {
  height: 100%;
  width: 100%;
}
:deep(.custome-row){
  margin-top: 10px;
}
</style>