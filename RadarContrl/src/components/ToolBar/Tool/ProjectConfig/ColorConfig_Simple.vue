<template>
  <section id="idcolorconfig" v-show="visible">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em" height="1em" fill="currentColor" viewBox="0 0 1024 1024">
              <path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z"></path>
              <path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z"></path>
            </svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;色条配置</span>
      </template>
      <template v-slot:dragger-content>
        <a-row class="custom-row">
          <div style="padding: 20px; color: white;">
            <h2>✅ 色条配置页面已显示</h2>
            <p>如果能看到这个，说明组件可以正常渲染！</p>
            <p>现在正在加载完整版本...</p>
          </div>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
import { defineComponent } from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import { useMapStore } from "@/store/index.js";

defineComponent({
  name: "colorconfig",
});

const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
});

const store = useMapStore();

console.log('✅ ColorConfig组件已加载（简化版）');
</script>

<style scoped>
#idcolorconfig {
  height: 100%;
  width: 100%;
}
</style>

