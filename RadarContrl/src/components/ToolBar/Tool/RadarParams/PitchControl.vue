<template>
  <section id="idpitchcontrol" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;俯仰电机控制</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn custom-btn" type="primary" ghost block @click="commitUpdate">提交更改</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <el-form-item label="在线状态">
              <el-switch disabled v-model="store.radarInfo.tiltMotor['online']" />
            </el-form-item>
            <el-form-item label="俯仰角度">
              <el-input v-model="store.radarInfo.tiltMotor['params']['pitch']"/>
            </el-form-item>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-28 / 18:00:25 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {showMessage} from "@/utils/tools.js";
import {ApiRadar} from "@/axios/apiRadar.js";
/*-- name --*/
defineComponent({
  name: "pitchcontrol",
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
  ApiRadar.updateTiltMotorPitch(store.radarInfo.projectId,store.radarInfo.tiltMotor['id'],store.radarInfo.tiltMotor['params']['pitch'])
      .then(res=>{
        showMessage(res.data.data);
      }).catch(()=>{
        showMessage('操作失败','error');
  })
}
/*-- events --*/
onMounted(() => {
  //console.log('PitchControl.onMounted');
});
</script>

<style scoped>
#idpitchcontrol {
  height: 100%;
  width: 100%;
}
</style>