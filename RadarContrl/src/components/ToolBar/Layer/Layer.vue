<template>
  <div id="layertree">
    <a-tree
        v-model:expandedKeys="expandedKeys"
        v-model:checkedKeys="store.layerCheckedKeys"
        checkable
        :tree-data="treeData"
        @check="itemCheck"
        class="custom-ant-tree"
        :auto-expand-parent="autoExpandParent"
        @select="itemSelect"
    ></a-tree>
  </div>
</template>

<script setup>
// map / 2023-05-15 / 08:31:13 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, toRaw, watch} from 'vue';
import {ucmlPost} from "@/axios/axios.js";
import {
  FormatJsonToLayerTreeData,
  FormatTreeToJsonArray,
  showMessage,getParentKey
} from "@/utils/tools.js";
import {useMapStore} from "@/store/index.js";
import * as Cesium from 'cesium';
import {CesiumUtils} from "@/utils/CesiumUtils.js";
import {Cesium3DTileset, GeoJsonDataSource, ImageryLayer, KmlDataSource} from "cesium";

/*-- name --*/
defineComponent({
  name: "layer",
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
const store = new useMapStore();
/*-- vars --*/
const searchValue = ref('');
const treeData = ref([]);
const expandedKeys = ref([]);
const clickValue = ref('');
const autoExpandParent = ref(true);
const originData = ref([]);
/*-- methods --*/
function itemSelect(selectedKeys,e){
  if (!e.selected){
    if (e.node['children']===undefined){
      store.layerCheckedKeys.splice(store.layerCheckedKeys.indexOf(e.node.key),1);
      itemCheck(store.layerCheckedKeys,e);
      return;
    }
    if (e.node['parent']!==undefined){
      clickValue.value = e.node['parent'].node.key;
    }else{
      clickValue.value = '&&&&&&';
      autoExpandParent.value = false;
    }
    return;
  }
  if (e.node['children']!==undefined){
    if (e.node['children'][0].key===clickValue.value){
      if (e.node['parent']===undefined){
        clickValue.value = '&&&&&&';
        autoExpandParent.value = false;
      }else{
        clickValue.value = e.node.key;
        autoExpandParent.value = false;
      }
      e.selected = true;
    }else{
      clickValue.value = e.node['children'][0].key;
    }
  }else{
    //子节点
    store.layerCheckedKeys.push(e.node.key);
    itemCheck(store.layerCheckedKeys,e);
  }
}
function itemCheck(key,item){
  let node = item.node;
  const nodechecked = node.checked;
  node = FormatTreeToJsonArray([node]);
  for (let i = 0; i < node.length; i++) {
    if (nodechecked){
      for (let i = 0; i < node.length; i++) {
        if (toRaw(store.layerList)[node[i].key] instanceof GeoJsonDataSource||toRaw(store.layerList)[node[i].key] instanceof KmlDataSource){
          CesiumUtils.DataSourceRemove(toRaw(store.layerList)[node[i].key]);
        }else if (toRaw(store.layerList)[node[i].key] instanceof ImageryLayer){
          CesiumUtils.LayerImageryRemove(toRaw(store.layerList)[node[i].key]);
        }else if (toRaw(store.layerList)[node[i].key] instanceof Cesium3DTileset){
          CesiumUtils.LayerPrimitiveRemove(toRaw(store.layerList[node[i].key]));
        }
        delete store.layerList[node[i].key];
      }
      return;
    }
    switch (node[i]['ServiceType']){
      case '3dtile':
        CesiumUtils.LayerPrimitive3dtileAdd(node[i]['ServiceAddress']).then(tileset=>{
          store.layerList[node[i].key] = tileset;
          CesiumUtils.ZoomToEntity(tileset);
        })
        break;
      case 'geojson':
        CesiumUtils.LayerGeoJsonAdd(node[i]['ServiceAddress']).then(ds=>{
          store.layerList[node[i].key] = ds;
          CesiumUtils.ZoomToEntity(ds);
        })
        break;
      case 'wms':
        CesiumUtils.LayerImageryWMSAdd(node[i]['ServiceAddress'],node[i]['ServiceCode']).then(layer=>{
          store.layerList[node[i].key] = layer;
        })
        break;
      case 'wmts':
        CesiumUtils.LayerImageryWMTSAdd(node[i]['ServiceAddress'],node[i]['ServiceCode']).then(layer=>{
          store.layerList[node[i].key] = layer;
        })
        break;
      case 'wfs':
        CesiumUtils.LayerGeoJsonAdd(node[i]['ServiceAddress']+'?service=WFS&request=GetFeature&typeName='+node[i]['ServiceCode']+'&outputFormat=application/json').then(layer=>{
          store.layerList[node[i].key] = layer;
        })
        break;
      case 'kml':
        CesiumUtils.LayerKMLAdd(node[i]['ServiceAddress']).then(layer=>{
          store.layerList[node[i].key] = layer;
          CesiumUtils.ZoomToEntity(layer);
        })
        break;
      case 'tms':
        CesiumUtils.LayerImageryTMSAdd(node[i]['ServiceAddress']).then(layer=>{
          store.layerList[node[i].key] = layer;
        })
        break;
      default:
        showMessage('暂不支持')
        break;
    }
  }
}
function loadTree(){
  ucmlPost('BPO_M2023001','GetCurrentUserMapTree',{
    "prjOID":'',"orgOID":store.sysinfo.ucmlInfo.orgOid
  }).then(res=>{
    if (res.status !== 200){return;}
    originData.value = res.data.Entity[Object.keys(res.data.Entity)[0]];
    treeData.value = FormatJsonToLayerTreeData(res.data.Entity,'oid','pfk','name');
  })
}
/*-- events --*/
watch(clickValue, value => {
  if (value===''){
    autoExpandParent.value = false;
    return;
  }
  expandedKeys.value = originData.value
      .map((item) => {
        if (item.key.indexOf(value) > -1) {
          return getParentKey(item.key, toRaw(treeData.value));
        }
        return null;
      })
      .filter((item, i, self) => item && self.indexOf(item) === i);
  clickValue.value = value;
  autoExpandParent.value = true;
});
onMounted(() => {
  if (window.localrelease!==undefined)return
  loadTree();
});
</script>

<style scoped>
:deep(.ant-tree-node-content-wrapper:focus){
  background: rgba(0, 0, 0, 0.5) !important;
}
#layertree{
  width: 276px;
}
</style>