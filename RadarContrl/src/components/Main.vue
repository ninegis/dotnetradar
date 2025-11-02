<template>
  <section id="idmain" v-show="visible">
    <CesiumContainer/>
  </section>
</template>

<script setup>
// map / 2023-05-31 / 17:10:46 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive} from 'vue';
import CesiumContainer from './CesiumContainer.vue'
import {useDark} from "@vueuse/core";

/*-- name --*/
defineComponent({
  name: "main",
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
/*-- stores --*/

/*-- vars --*/

/*-- methods --*/

/*-- events --*/
onMounted(() => {
  useDark().value = true;
  //console.log('Main.onMounted');
});
</script>

<style scoped>
#idmain{
  height: 100%;
  width: 100%;
}
</style>