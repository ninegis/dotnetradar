<template>
  <div class="dragger-container" ref="draggerContainer" :style="draggerPosition">
    <div class="dragger-header" @mousedown.stop="draggerStart">
      <slot name="dragger-header"></slot>
      <span class="closebtn" @click="stores.componentSlotDestroy()">
        <CloseOutlined/>
      </span>
    </div>
    <div class="dragger-content">
      <slot name="dragger-content"></slot>
    </div>
  </div>
</template>

<script>
import { CloseOutlined } from '@ant-design/icons-vue'
import {useMapStore} from "@/store/index.js";

export default {
  name: "DraggerContaier",
  components:{
    CloseOutlined,
  },
  props: {
    draggerWidth: {
      //容器宽度
      type: Number,
      required: true,
    },
  },
  data() {
    return {
      draggerEvent: null,
      mouseUp: false,
      draggerContainerRight: 0,
      draggerContainerTop: 0,
      stores:null
    };
  },
  computed: {
    draggerPosition() {
      return {
        width: this.draggerWidth + "px",
        top: this.draggerContainerTop + "px",
        right: this.draggerContainerRight + "px",
      };
    },
  },
  methods: {
    draggerStart(e) {
      const disX =
        e.clientX - this.$refs.draggerContainer.offsetLeft;
      const disY = e.clientY - this.$refs.draggerContainer.offsetTop;
      document.onmousemove = (ev) => {
        let right = ev.clientX - disX;
        let top = ev.clientY - disY;

        this.draggerContainerRight = -right;
        this.draggerContainerTop = top;
      };

      document.onmouseup = () => {
        document.onmousemove = null;
        document.onmouseup = null;
      };
    },
  },
  created() {
    this.stores = useMapStore();
  }
};
</script>

<style lang='scss' scoped>
.dragger-container {
  right: 0;
  border-radius: 5px;
  background: rgba(30,36,50,0.6);
  z-index: 99;
  border: 1px solid rgba(32,160,255,.3);;
  position: relative;
}
.dragger-header {
  cursor: move;
  height: 40px;
  line-height: 40px;
  text-align: left;
  padding: 0 80px 0 10px;
  color: white;
  border-bottom: 1px solid rgba(32,160,255,.3);
}
.dragger-content{
  max-height: 550px;
  overflow: auto;
}
</style>