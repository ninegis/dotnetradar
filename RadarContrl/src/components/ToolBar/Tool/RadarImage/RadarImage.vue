<template>
  <section id="idalarmrule" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em" height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024"
              version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126">
              <path
                d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z"
                fill="" p-id="5127"></path>
              <path
                d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z"
                fill="" p-id="5128"></path>
            </svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp; {{ $t('backend.radarImageRecord') + resultCount }}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle" class="custome-row">
          <el-form class="custom-form">
            <el-form-item :label="$t('decoration.labelSelectRadar')">
              <el-checkbox-group v-model="store.radarSelected">
                <el-checkbox :label="item.name" :value="item.id" v-for="item in store.projectInfo.deviceData" />
              </el-checkbox-group>
            </el-form-item>
            <el-form-item :label="$t('decoration.labelSelectTime')">
              <el-col :span="11">
                <el-date-picker v-model="startTime" type="datetime"
                  :placeholder="$t('common.placeholderSelectStartTime')" />
              </el-col>
              <el-col :span="2" class="text-center">
                <span class="text-gray-500">{{ $t('common.to') }}</span>
              </el-col>
              <el-col :span="11">
                <el-date-picker v-model="endTime" type="datetime"
                  :placeholder="$t('common.placeholderSelectEndTime')" />
              </el-col>
            </el-form-item>
            <el-form-item :label="$t('decoration.labelImageType')">
              <el-checkbox-group v-model="form.reflectcategory">
                <el-checkbox :label="$t('decoration.imageDefo')" value="连续形变" />
                <el-checkbox :label="$t('decoration.imageScat')" value="复散射" />
                <el-checkbox :label="$t('decoration.imageDefoSpeed')" value="连续形变速度" />
                <el-checkbox :label="$t('decoration.imageDefoRegion')" value="区间形变差值" />
              </el-checkbox-group>
            </el-form-item>
            <el-form-item :label="$t('decoration.labelImageStatus')">
              <el-checkbox-group v-model="form.statuscategory">
                <el-checkbox :label="$t('decoration.statusSuccess')" value="成功" />
                <el-checkbox :label="$t('decoration.statusFailed')" value="失败" />
                <el-checkbox :label="$t('decoration.statusSkip')" value="跳过" />
                <el-checkbox :label="$t('decoration.statusUnstart')" value="未开始" />
              </el-checkbox-group>
            </el-form-item>
            <el-form-item :label="$t('common.rotation')">
              <el-col :span="18">
                <el-slider v-model="rotationDegrees" :min="-180" :max="180" :step="1" @input="updateRotation" />
              </el-col>
              <el-col :span="6">
                <el-input-number v-model="rotationDegrees" :min="-180" :max="180" :step="1" @change="updateRotation"
                  size="small" style="margin-left: 10px;" />
              </el-col>
            </el-form-item>
          </el-form>
          <el-form-item style="width: 100%">
            <a-button class="custom-ant-btn" type="primary" ghost block @click="searchData">{{ $t('common.image') +
              $t('common.record') + $t('common.search') }}</a-button>
            <a-button class="custom-ant-btn" type="primary" ghost danger @click="clearImage">{{ $t('common.image') +
              $t('common.clear') }}</a-button>
          </el-form-item>
        </a-row>
        <a-row>
          <el-table @cell-dblclick="tableOnDbClick" :data="store.imageData" style="width: 100%;color:white"
            class="custom-table" height="400">
            <el-table-column prop="name" :label="$t('common.name')" />
            <el-table-column prop="type" :label="$t('common.type')" width="90" />
            <el-table-column prop="sequence" :label="$t('common.index')" />
            <el-table-column prop="time" :label="$t('common.time')" width="155" />
            <el-table-column prop="status" :label="$t('common.time')" />
            <el-table-column :label="$t('common.operate')">
              <template #default="scope">
                <el-button link type="primary" size="small" @click="execute(scope.row)">{{ $t('common.generate')
                }}</el-button>
              </template>
            </el-table-column>
          </el-table>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// kot-security-cloud-radar / 2023-11-27 / 21:52:50 / 71901
/*-- imports --*/
import { defineComponent, ref, onMounted, computed, toRaw, reactive } from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import { useMapStore } from "@/store/index.js";
import { ApiRadar } from "@/axios/apiRadar.js";
import {
  addRadarLayer,
  getColorRule,
  getRadarImgTimeUnit, getRadarStatus, getRadarType,
  getTarget,
  getTargetStatus,
  getTimeUnit,
  getValue,
} from "@/utils/radartool.js";
import { showMessage } from "@/utils/tools.js";
import { CommonUtils } from "@/utils/CommonUtils.js";
import { Color } from "cesium";
import dayjs from "dayjs";
import { CesiumUtils } from "@/utils/CesiumUtils.js";
import { useI18n } from "vue-i18n";
/*-- name --*/
defineComponent({
  name: "alarmrule",
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
const store = useMapStore();
const form = reactive({
  reflectcategory: ['连续形变', '复散射'],
  statuscategory: ['成功'],
})
/*-- vars --*/
const endTime = ref(new Date());
const startTime = ref(new Date(new Date().setDate(new Date().getDate() - 1)));
const resultCount = ref('');
const rotationDegrees = ref(0);
const { t } = useI18n();
/*-- methods --*/
const updateRotation = () => {
  // 将角度转换为弧度（Cesium使用弧度）
  store.radarImageRotation = rotationDegrees.value * (Math.PI / 180);
}
const clearImage = () => {
  for (let i = store.radarImageEntityIds.length - 1; i >= 0; i--) {
    CesiumUtils.EntityRemoveById(store.radarImageEntityIds[i]);
    store.radarImageEntityIds.splice(i, 1);
  }
  showMessage(t('backend.imageHasbeenCleared'), 'success');
}
const searchData = () => {
  const radarType = getRadarType(form.reflectcategory);
  const radarStatus = getRadarStatus(form.statuscategory);
  ApiRadar.queryImageCount(store.radarInfo.projectId, store.radarInfo.deviceId,
    CommonUtils.DateTimeToStr(startTime.value),
    CommonUtils.DateTimeToStr(endTime.value), radarType,
    radarStatus).then(count => {
      store.imageData = [];
      if (count.data.data.count === 1) {
        ApiRadar.queryImageList(store.radarInfo.projectId, store.radarInfo.deviceId,
          CommonUtils.DateTimeToStr(startTime.value),
          CommonUtils.DateTimeToStr(endTime.value), radarType,
          radarStatus, count.data.data.dataset[0][0]).then(list => {
            const result = list.data.data.dataset;
            resultCount.value = t('decoration.imageQueryLeftTitle') + result.length + t("decoration.imageQueryRightTitle");
            for (let i = 0; i < result.length; i++) {
              const type = result[i][3];
              if (type === '00' || type === '02') continue;
              const radarName = store.projectInfo.deviceData[CommonUtils.FindIndexOfArray('id', result[i][2], store.projectInfo.deviceData)]['name'];
              if (store.radarSelected.indexOf(result[i][2]) >= 0) {
                store.imageData.push({
                  id: result[i][2],
                  type: parseRaraType(type),
                  type_origin: type,
                  sequence: result[i][4],
                  timeUnit: getRadarImgTimeUnit(result[i][8]),
                  timeUnit_origin: result[i][8],
                  duration: result[i][6],
                  time: result[i][0],
                  status: parseRaraStatus(result[i][7]),
                  status_origin: result[i][7],
                  filedir: result[i][5],
                  name: radarName
                })
              }
            }
          })
      }
    })
}
const parseRaraStatus = (value) => {
  switch (value) {
    case 'success':
      return t('decoration.statusSuccess');
    case 'fail':
      return t('decoration.statusFailed');
    case 'skip':
      return t('decoration.statusSkip');
    case 'unstart':
      return t('decoration.statusUnstart');
  }
}
const parseRaraType = (value) => {
  switch (value) {
    case '01':
    case '61':
      return t('decoration.imageScat');
    case '10':
      return t('decoration.imageDefo');
    case '04':
      return t('decoration.imageDefoSpeed');
    case '05':
      return t('decoration.imageDefoRegion');
    case '00':
      return t('decoration.imageDefoOrigin');
    case '02':
      return t('decoration.imageConfidence');
    default:
      return value;
  }
}
const tableOnDbClick = (row) => {
  const filedir = row.filedir;
  if (filedir === undefined) {
    return;
  }
  const date = dayjs(row.time).format('YYYYMMDD');
  const url = '/data/project/' + store.radarInfo.projectId +
    '/images/' + row.id + '/' + date + '/' + filedir + '/';
  ApiRadar.getImageResource(url, 'imageTiles.json')
    .then((res) => {
      store.scatcolorbar = (row.type === '复散射' || row.type === 'Scattering');
      store.defocolorbar = row.type === "区间形变差值" || row.type === "连续形变" || row.type === "连续形变速度" || row.type === 'Deformed' || row.type === 'Speed' || row.type === 'Interval Speed Image';

      // 根据图像所属的雷达ID，找到对应雷达的坐标
      const radarIndex = CommonUtils.FindIndexOfArray('id', row.id, store.projectInfo.deviceData);
      const radarCoordinates = radarIndex >= 0 ? store.projectInfo.deviceData[radarIndex]['coordinates'] : null;

      // 传递该雷达的中心坐标，使图像围绕其所属雷达位置旋转
      addRadarLayer(CesiumUtils.viewer, store.sysinfo.serverIp + url, res.data.matrixTileTotal, 1, 0, radarCoordinates).then(result => {
        showMessage((store.sysinfo.config.language === '0' ? '加载雷达图像成功' : 'Load radar images successfully.'));
      });
    })
}
const execute = (row) => {
  if (row.status === t('decoration.statusSuccess')) {
    showMessage((store.sysinfo.config.language === '0' ? '当前雷达图像已生成' : 'Current images has been generated.'), 'warning');
    return;
  }
  ApiRadar.generateRadarImage(row.id, row.duration, row.filedir,
    store.radarInfo.projectId, row.sequence, row.status_origin, row.timeUnit_origin,
    row.time, row.type_origin).then(res => {
      showMessage((store.sysinfo.config.language === '0' ? '操作成功,请等待...' : 'The operator has been successful,please wait...'), 'success');
    })
}
/*-- events --*/
onMounted(() => {
  searchData();
  //console.log('AlarmRule.onMounted');
});
</script>

<style scoped>
:deep(.dragger-content) {
  overflow: hidden;
}

.expanded-p {
  margin-left: 25px;
}

.ant-btn.ant-btn-block {
  width: 69%;
}
</style>