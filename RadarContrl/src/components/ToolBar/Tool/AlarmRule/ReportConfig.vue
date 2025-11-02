<template>
  <section id="idreportconfig" v-show="visible" class="">
    <DragContainer :dragger-width="276">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;监测报表配置</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn" type="primary" ghost block @click="showMessage('未接入...')">提交更改</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <el-form-item label="报告名称">
              <el-input/>
            </el-form-item>
            <el-form-item label="报告描述">
              <el-input/>
            </el-form-item>
            <el-form-item label="是否启用">
              <el-switch/>
            </el-form-item>
            <el-form-item label="模板路径">
              <el-input/>
            </el-form-item>
            <el-form-item label="时间选择">
              <el-select>
                <el-option label="日报" value="Day"></el-option>
                <el-option label="周报" value="Week"></el-option>
                <el-option label="月报" value="Month"></el-option>
                <el-option label="24小时" value="24Hours"></el-option>
                <el-option label="自定义时间" value="24Hours"></el-option>
              </el-select>
            </el-form-item>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-29 / 11:56:34 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, toRaw} from 'vue';
import {useMapStore} from "@/store/index.js";
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import {showMessage} from "@/utils/tools.js";
/*-- name --*/
defineComponent({
  name: "reportconfig",
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

/*-- events --*/
onMounted(() => {
  //console.log('ReportConfig.onMounted');
});
</script>

<style scoped>
#idreportconfig {
  height: 100%;
  width: 100%;
}
</style>