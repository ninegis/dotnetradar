<template>
  <section id="idleftbottompanel" v-show="visible" class="">
    <slot name="custom-component"></slot>
  </section>
</template>

<script setup>
// map / 2023-05-22 / 18:14:05 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed} from 'vue';

/*-- name --*/
defineComponent({
  name: "leftbottompanel",
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

/*-- vars --*/

/*-- methods --*/

/*-- events --*/
onMounted(() => {
  //console.log('LeftbottomPanel.onMounted');
});
</script>

<style scoped>
#idleftbottompanel{
  position: relative;
  z-index: 888;
  padding: 10px;
  border-bottom-left-radius: 5px;
  border-bottom-right-radius: 5px;
  border-bottom: 1px solid rgba(32,160,255,.3);
  border-left: 1px solid rgba(32,160,255,.3);
  border-right: 1px solid rgba(32,160,255,.3);
}
</style>