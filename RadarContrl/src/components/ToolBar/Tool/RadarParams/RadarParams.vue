<template>
  <section id="idradarparams" v-show="visible" class="">
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
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{ $t('backend.paramSetting') }}</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="custom-ant-btn custom-btn" type="primary" ghost block @click="saveBasicInfo">
            保存基础信息
          </a-button>
          <a-button class="custom-ant-btn custom-btn" type="primary" ghost block @click="sendRadarParams">
            下发雷达参数
          </a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form :model="store.radarInfo.params">
            <!-- ✅ 新增：项目选择 -->
            <el-form-item :label="$t('common.project')">
              <el-select v-model="store.projectInfo.projectSelected" @change="onProjectChange">
                <el-option v-for="item in store.projectInfo.projectData" :key="item.projectId" 
                  :label="item.projectName" :value="item.projectId" />
              </el-select>
            </el-form-item>
            
            <el-form-item :label="$t('common.device')">
              <el-col :span="14">
                <el-select v-model="store.radarInfo.deviceId" :placeholder="$t('decoration.radarDropdown')"
                  @change="selectOnChange">
                  <el-option v-for="item in currentProjectDevices" :key="item.id" :label="item.name"
                    :value="item.id" />
                </el-select>
              </el-col>
              &nbsp;
              <el-col :span="4">
                <a-tooltip :title="$t('backend.addDevice')">
                  <a-button shape="circle" :icon="h(PlusOutlined)" @click="store.toolbarcontent = 'adddevice'" />
                </a-tooltip>
              </el-col>
              &nbsp;
              <el-col :span="4">
                <a-tooltip :title="$t('backend.delDevice')">
                  <a-button shape="circle" :icon="h(DeleteOutlined)" @click="deleteDevice" />
                </a-tooltip>
              </el-col>
            </el-form-item>
            <el-form-item :label="$t('backend.deviceId')">
              <el-input readonly v-model="store.radarInfo.deviceId" />
            </el-form-item>
            <el-form-item :label="$t('backend.deviceName')">
              <el-input v-model="store.radarInfo.deviceName" />
            </el-form-item>
            <el-form-item :label="$t('backend.factoryId')">
              <el-input v-model="currentDevice.factoryId" placeholder="设备出厂ID"/>
            </el-form-item>
            <el-form-item :label="$t('common.longitude')">
              <el-col :span="20">
                <el-input-number v-model="currentDevice.longitude" :precision="6" :step="0.000001" style="width: 100%" placeholder="120.6"/>
              </el-col>
              <el-col :span="4">
                <a-tooltip :title="$t('backend.pickCoordinateInMap')">
                  <a-button shape="circle" :icon="h(EditOutlined)" @click="getCoordinate" />
                </a-tooltip>
              </el-col>
            </el-form-item>
            <el-form-item :label="$t('common.latitude')">
              <el-input-number v-model="currentDevice.latitude" :precision="6" :step="0.000001" style="width: 100%" placeholder="31.3"/>
            </el-form-item>
            <el-form-item :label="$t('common.altitude')">
              <el-input-number v-model="currentDevice.elevation" :precision="2" :step="0.1" style="width: 100%" placeholder="100"/>
            </el-form-item>
            <el-form-item :label="$t('backend.boundaryShow')">
              <el-switch v-model="boundaryShow" @change="boundaryOnChange"></el-switch>
            </el-form-item>
            <el-form-item :label="$t('backend.radarOriAngle')">
              <el-col :span="19">
                <el-input-number style="width: 100%" v-model="currentDevice.orientation" :min="0" :max="360"
                  :precision="2" @change="inputOnChange" placeholder="零点朝向(度)"/>
              </el-col>
              &nbsp;
              <el-col :span="4">
                <a-tooltip :title="$t('backend.computeOriAngle')">
                  <a-button shape="circle" :icon="h(EditOutlined)"
                    @click="computeOriAngleVisible = !computeOriAngleVisible" />
                </a-tooltip>
              </el-col>
            </el-form-item>
            <el-text class="mx-1" type="warning" v-show="computeOriAngleVisible">{{ $t('backend.computeOriAngleTip')
            }}</el-text>
            <el-form-item :label="$t('backend.computeOriAngleImagePt')" v-show="computeOriAngleVisible">
              <el-select v-model="pointImage" :placeholder="$t('decoration.placeholderMonitorDropdown')"
                @change="computeOriAngle">
                <el-option v-for="item in markData" :key="item.id" :label="item.name" :value="item.id" />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('backend.computeOriAngleModelPt')" v-show="computeOriAngleVisible">
              <el-select v-model="pointModel" :placeholder="$t('decoration.placeholderMonitorDropdown')"
                @change="computeOriAngle">
                <el-option v-for="item in markData" :key="item.id" :label="item.name" :value="item.id" />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('backend.startAngle')">
              <el-input-number v-model="store.radarInfo.params['ImgAngleStart']" :min="0" :max="360" @change="inputOnChange" placeholder="0"/>
            </el-form-item>
            <el-form-item :label="$t('backend.endAngle')">
              <el-input-number v-model="store.radarInfo.params['ImgAngleEnd']" :min="0" :max="360" @change="inputOnChange" placeholder="360"/>
            </el-form-item>
            <el-form-item :label="$t('backend.minDistance')">
              <el-input-number v-model="store.radarInfo.params['RngMin']" :min="0" style="width: 100%" placeholder="0"/>
            </el-form-item>
            <el-form-item :label="$t('backend.maxDistance')">
              <el-input-number v-model="store.radarInfo.params['RngMax']" :min="0" style="width: 100%" placeholder="1000"/>
            </el-form-item>
            <el-form-item :label="$t('backend.freqBandSelect')">
              <el-select v-model="store.radarInfo.params['FreqBand']" :placeholder="$t('backend.freqBandDropSelect')">
                <el-option key="0" label="0" value="0" />
                <el-option key="1" label="1" value="1" />
                <el-option key="2" label="2" value="2" />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('backend.antennaAngle')">
              <el-select v-model="store.radarInfo.params['AnteBeam_half']"
                :placeholder="$t('backend.antennaAngleDrop')">
                <el-option key="30" label="30°" :value="30" />
                <el-option key="60" label="60°" :value="60" />
                <el-option key="90" label="90°" :value="90" />
              </el-select>
            </el-form-item>
            <el-form-item :label="$t('backend.projectConfig')">
              <el-select v-model="dataVersion" :placeholder="$t('backend.projectConfSelect')">
                <el-option key="0" :label="$t('backend.generateImageMode1')" value="0" />
                <el-option key="1" :label="$t('backend.generateImageMode2')" value="1" />
                <el-option key="2" :label="$t('backend.generateImageMode3')" value="2" />
              </el-select>
            </el-form-item>
            <div v-show="currentRadar === 'MIMOLITE'">
              <el-form-item :label="$t('backend.refreshFreq') + '(' + $t('common.second') + ')'">
                <el-input v-model="store.radarInfo.params['UpdateTime']" />
              </el-form-item>
              <el-form-item :label="$t('backend.monitorMode')">
                <el-select v-model="store.radarInfo.params['ModelSelect']" :placeholder="$t('backend.monitorMode')">
                  <el-option key="1" :label="$t('backend.moveMode')" value="0" />
                  <el-option key="1" :label="$t('backend.monitorMode')" value="1" />
                </el-select>
              </el-form-item>
            </div>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-28 / 17:24:27 / QingQiangJia
/*-- imports --*/
import { defineComponent, ref, onMounted, computed, reactive, toRaw, h } from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon, { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons-vue';
import { useMapStore } from "@/store/index.js";
import { ApiRadar } from "@/axios/apiRadar.js";
import axios from 'axios';  // ✅ 添加axios导入
import { showMessage } from "@/utils/tools.js";
import { CesiumUtils } from "@/utils/CesiumUtils.js";
import { MyLocation } from "@/assets/load.js";
import { CommonUtils } from "@/utils/CommonUtils.js";
import {
  CallbackProperty,
  Color,
  Entity,
  ScreenSpaceEventHandler,
  ScreenSpaceEventType
} from "cesium";
import { TurfUtils } from "@/utils/TurfUtils.js";
import { projectDataInit, staticDataBind } from "@/utils/radartool.js";
import { ElMessage, ElMessageBox } from "element-plus";
import { useI18n } from "vue-i18n";
/*-- name --*/
defineComponent({
  name: "radarparams",
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
const { t } = useI18n();
/*-- vars --*/
const pointImage = ref(null);
const pointModel = ref(null);
const markData = ref([]);
const selectedEntityId = ref(null);
const drawEntities = ref([]);
const angleValue = ref(0);
const boundaryShow = ref(false);
const currentRadar = ref('');
const dataVersion = ref("0");
const computeOriAngleVisible = ref(false);

// ✅ 当前设备信息（用于表单绑定）
const currentDevice = reactive({
  factoryId: '',
  longitude: 0,
  latitude: 0,
  elevation: 0,
  orientation: 0
});

/*-- computed --*/
// 当前项目的设备列表
const currentProjectDevices = computed(() => {
  const currentProject = store.projectInfo.projectData.find(
    p => p.projectId === store.projectInfo.projectSelected
  );
  const devices = currentProject?.devices || [];
  
  // ✅ 调试日志：查看设备数据
  console.log('currentProjectDevices computed:', {
    projectId: store.projectInfo.projectSelected,
    devicesCount: devices.length,
    firstDevice: devices[0] ? {
      id: devices[0].id,
      deviceId: devices[0].deviceId,
      name: devices[0].name,
      deviceName: devices[0].deviceName,
      factoryId: devices[0].factoryId,
      longitude: devices[0].longitude,
      params: devices[0].params
    } : null
  });
  
  return devices;
});
/*-- methods --*/
const computeOriAngle = () => {
  if (!pointModel.value || !pointImage.value) return;
  const pImage = CommonUtils.FindObjectOfArray('id', pointImage.value, markData.value);
  const pModel = CommonUtils.FindObjectOfArray('id', pointModel.value, markData.value);
  const radarCoords = [currentDevice.longitude, currentDevice.latitude, currentDevice.elevation];
  const angleBefore = TurfUtils.ComputeAngleByTwoPoint(radarCoords, pImage.coordinates[0]);
  const angleAfter = TurfUtils.ComputeAngleByTwoPoint(radarCoords, pModel.coordinates[0]);
  const angleDiff = angleAfter - angleBefore;
  const angleResult = parseFloat((parseFloat(currentDevice.orientation) + angleDiff).toFixed(2));
  ElMessageBox.confirm(
    '当前计算出的新的零点朝向为' + angleResult + '°，旧的零点朝向为' + currentDevice.orientation + '°，是否需要替换旧值',
    t('backend.computeOriAngleReplaceHint'),
    {
      confirmButtonText: t('common.replace'),
      cancelButtonText: t('common.cancel'),
      type: 'warning',
    }
  )
    .then(() => {
      currentDevice.orientation = angleResult;
      CommonUtils.ShowMessage(t('backend.computeOriAngleReplaceSuccess'));
    })
    .catch(() => {
      CommonUtils.ShowMessage(t('common.operateCancel'));
    })
}
const deleteDevice = () => {
  ApiRadar.DeleteDevice(store.radarInfo.deviceId).then(res => {
    showMessage('删除设备成功');
    ApiRadar.AddRadarLog("删除设备" + store.radarInfo.deviceId, store.sysinfo.config.username, store.sysinfo.address, store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
    projectDataInit();
    store.toolbarcontent = 'radarParams';
  })
}
const computeRealZero = () => {
  const params = store.radarInfo.params;
  console.log((currentDevice.orientation + params['ImgAngleStart'] +
    params['AnteBeam_half'] / 2));
}
const computeAngle = () => {
  let value = 0;
  if (angleValue.value <= 180 && angleValue.value >= 45) {
    value = angleValue.value - 45;
  } else if (angleValue.value >= 0 && angleValue.value < 45) {
    value = 360 - (45 - angleValue.value);
  } else if (angleValue.value > 0 && angleValue.value < 45) {
    value = angleValue.value - 45 + 180;
  } else {
    value = angleValue.value - 45 + 360;
  }
  store.radarInfo.params['ImgAngleStart'] = 0;
  currentDevice.orientation = parseInt(value);
  inputOnChange();
}
const drawLine = () => {
  const c3 = CesiumUtils.FindEntityById(store.radarInfo.entityId)._position._value.clone();
  let positions = [c3], _polygonEntity = new Entity(), polyObj = null;
  const handler = new ScreenSpaceEventHandler(CesiumUtils.viewer.scene.canvas);
  handler.setInputAction(function (e) {
    CesiumUtils.Cartesian2ToCartesian3(e.position).then(cartesian3 => {
      positions.pop();
      positions.push(cartesian3.clone());
      handler.removeInputAction(ScreenSpaceEventType.MOUSE_MOVE);
      handler.removeInputAction(ScreenSpaceEventType.LEFT_CLICK);
      const angle = TurfUtils.ComputeAngleByTwoPoint(CesiumUtils.Cartesian3ToLonlatalt(positions[0]), CesiumUtils.Cartesian3ToLonlatalt(positions[1]));
      CommonUtils.ShowMessage('当前正北角度为' + angle + '度');
      angleValue.value = angle;
      console.log('当前正北：' + angleValue.value);
      console.log('当前零点朝向:' + currentDevice.orientation);
    })
  }, ScreenSpaceEventType.LEFT_CLICK);
  handler.setInputAction(function (e) {
    if (positions.length >= 2) {
      CesiumUtils.Cartesian2ToCartesian3(e.endPosition).then(cartesian3 => {
        if (cartesian3 && cartesian3.x) {
          positions.pop()
          positions.push(cartesian3);
        }
      });

    }
  }, ScreenSpaceEventType.MOUSE_MOVE);
  handler.setInputAction(function (e) {
    handler.removeInputAction(ScreenSpaceEventType.MOUSE_MOVE);
    handler.removeInputAction(ScreenSpaceEventType.LEFT_CLICK);
  }, ScreenSpaceEventType.RIGHT_CLICK);
  _polygonEntity.polyline = {
    width: 3
    , material: Color.AQUA
    , clampToGround: true
  }
  _polygonEntity.polyline.positions = new CallbackProperty(function () {
    return positions
  }, false);
  positions.push(c3);
  drawEntities.value.push(CesiumUtils.viewer.entities.add(_polygonEntity));
}
const inputOnChange = () => {
  const entity = CesiumUtils.FindEntityById(selectedEntityId.value);
  if (!entity) return;
  
  const radarOri = currentDevice.orientation || 0;
  if (currentRadar.value === 'MIMOLITE') {
    entity.polygon.hierarchy = CesiumUtils.GenerateHierarchy(currentDevice.longitude,
      currentDevice.latitude, currentDevice.elevation,
      radarOri + store.radarInfo['params']['ImgAngleStart'] >= 30 ? radarOri + store.radarInfo['params']['ImgAngleStart'] : 30,
      radarOri + store.radarInfo['params']['ImgAngleEnd'],
      parseFloat(store.radarInfo.params['RngMax']) * 1.3);
  } else {
    entity.polygon.hierarchy = CesiumUtils.GenerateHierarchy(currentDevice.longitude, currentDevice.latitude, currentDevice.elevation,
      radarOri + store.radarInfo.params['ImgAngleStart'] + store.radarInfo.params['AnteBeam_half'] / 2,
      radarOri + store.radarInfo.params['AnteBeam_half'] / 2 +
      store.radarInfo.params['ImgAngleEnd'] - store.radarInfo.params['AnteBeam_half'],
      parseFloat(store.radarInfo.params['RngMax']) * 1.3);
  }
}
const boundaryOnChange = () => {
  const index = CommonUtils.FindIndexOfArray('id', store.radarInfo.deviceId, store.projectInfo.deviceData);
  selectedEntityId.value = store.boundaryEntityIds[index]['entityId'];
  CesiumUtils.FindEntityById(store.boundaryEntityIds[index]['entityId']).show = boundaryShow.value;
  inputOnChange();
}
// ✅ 项目切换事件
const onProjectChange = () => {
  console.log('项目切换:', store.projectInfo.projectSelected);
  
  // ✅ 设置projectId
  store.radarInfo.projectId = String(store.projectInfo.projectSelected || '');
  
  // ✅ 调用设备查询接口，获取该项目下的所有设备（包含雷达参数）
  ApiRadar.getDevicesByProjectId(store.projectInfo.projectSelected).then(res => {
    console.log('设备查询结果:', res);
    
    if (res.data && res.data.code === 200 && res.data.data) {
      // ✅ 更新当前项目的设备列表
      const projectIndex = store.projectInfo.projectData.findIndex(p => p.projectId === store.projectInfo.projectSelected);
      if (projectIndex !== -1) {
        // ✅ 映射设备数据（与projectDataInit保持一致）
        store.projectInfo.projectData[projectIndex].devices = res.data.data.map(d => {
          // 设备类型映射
          let deviceTypeStr = 'ER';
          
          // ✅ 调试：查看每个设备的类型代码
          console.log('设备映射:', {
            deviceId: d.deviceId,
            deviceName: d.deviceName,
            deviceTypeCode: d.deviceTypeCode,
            deviceType: d.deviceType
          });
          
          if (d.deviceTypeCode) {
            switch (d.deviceTypeCode) {
              case 1: deviceTypeStr = 'ER'; break;
              case 2: deviceTypeStr = 'MIMOLITE'; break;
              case 5: deviceTypeStr = 'ER'; break;
              case 6: deviceTypeStr = 'ER'; break;
              case 7: deviceTypeStr = 'MIMOLITE'; break;
              case 8: deviceTypeStr = 'MIMOLITE'; break;
              default: deviceTypeStr = 'ER'; break;
            }
          }
          
          console.log('  → 映射后的类型:', deviceTypeStr);
          
          return {
            deviceId: d.deviceId,
            deviceName: d.deviceName,
            id: d.deviceId,
            name: d.deviceName,
            type: deviceTypeStr,
            status: d.status,
            coordinates: [d.longitude || 0, d.latitude || 0, d.elevation || 0],
            longitude: d.longitude || 0,
            latitude: d.latitude || 0,
            elevation: d.elevation || 0,
            factoryId: d.factoryId || '',
            orientation: d.orientation || 0,
            ipAddress: d.ipAddress,
            port: d.port,
            params: d.params || {},
            dataVersion: d.dataVersion || '0',
            algorithmParam: d.algorithmParam || {}
          };
        });
        
        console.log('更新后的设备列表:', store.projectInfo.projectData[projectIndex].devices);
      }
      
      // ✅ 重新绑定数据
      staticDataBind();
      
      console.log('切换后设备数量:', currentProjectDevices.value.length);
      
      // ✅ 选择第一个设备
      if (currentProjectDevices.value.length > 0) {
        const firstDevice = currentProjectDevices.value[0];
        console.log('自动选择第一个设备:', firstDevice.id, firstDevice.name);
        
    const deviceIdStr = String(firstDevice.id);
    store.radarInfo.deviceId = deviceIdStr;
    store.radarInfo.deviceName = firstDevice.name;
    currentRadar.value = firstDevice.type || 'ER';  // ✅ 使用设备类型
    
    // ✅ 立即加载设备详细信息
    selectOnChange(deviceIdStr);
      } else {
        // 没有设备，清空表单
        console.warn('当前项目没有设备');
        store.radarInfo.deviceId = null;
        store.radarInfo.deviceName = '';
        currentDevice.factoryId = '';
        currentDevice.longitude = 0;
        currentDevice.latitude = 0;
        currentDevice.elevation = 0;
        currentDevice.orientation = 0;
        store.radarInfo.params = {};
      }
    }
  });
};

const selectOnChange = (e) => {
  console.log('selectOnChange 被调用, deviceId:', e);
  console.log('currentProjectDevices数量:', currentProjectDevices.value.length);
  
  const device = currentProjectDevices.value.find(d => d.id === e);
  
  if (!device) {
    console.error('找不到设备:', e);
    return;
  }
  
  console.log('找到设备:', device.name, device);
  
  // ✅ 确保projectId和deviceId都是字符串
  store.radarInfo.projectId = String(store.projectInfo.projectSelected || '');
  store.radarInfo.deviceId = String(device.id || '');
  store.radarInfo.deviceName = device.name;
  store.radarInfo.coordinates = device.coordinates || [0, 0, 0];
  store.radarInfo.params = device.params || {};
  
  // ✅ 根据设备类型设置currentRadar（不再用deviceId.substring）
  currentRadar.value = device.type || 'ER';
  console.log('设备类型:', device.type, 'currentRadar.value:', currentRadar.value);
  
  // ✅ 加载设备的独立字段到表单
  currentDevice.factoryId = device.factoryId || '';
  currentDevice.longitude = device.longitude || device.coordinates?.[0] || 0;
  currentDevice.latitude = device.latitude || device.coordinates?.[1] || 0;
  currentDevice.elevation = device.elevation || device.coordinates?.[2] || 0;
  currentDevice.orientation = device.orientation || store.radarInfo.params['radarOri'] || 0;
  
  console.log('加载的设备信息:', {
    factoryId: currentDevice.factoryId,
    longitude: currentDevice.longitude,
    latitude: currentDevice.latitude,
    elevation: currentDevice.elevation,
    orientation: currentDevice.orientation
  });
  
  // ✅ 设置默认值（如果字段为空或0）
  if (store.radarInfo.params['ImgAngleStart'] === undefined || store.radarInfo.params['ImgAngleStart'] === null) {
    store.radarInfo.params['ImgAngleStart'] = 0;
  }
  if (store.radarInfo.params['ImgAngleEnd'] === undefined || store.radarInfo.params['ImgAngleEnd'] === null) {
    store.radarInfo.params['ImgAngleEnd'] = 360;
  }
  if (store.radarInfo.params['RngMin'] === undefined || store.radarInfo.params['RngMin'] === null) {
    store.radarInfo.params['RngMin'] = 0;
  }
  if (store.radarInfo.params['RngMax'] === undefined || store.radarInfo.params['RngMax'] === null) {
    store.radarInfo.params['RngMax'] = 1000;
  }
  if (!store.radarInfo.params['FreqBand']) {
    store.radarInfo.params['FreqBand'] = '0';
  }
  if (store.radarInfo.params['AnteBeam_half'] === undefined || store.radarInfo.params['AnteBeam_half'] === null) {
    store.radarInfo.params['AnteBeam_half'] = 60;
  }
  
  dataVersion.value = device.dataVersion || '0';
  
  const index = CommonUtils.FindIndexOfArray('id', store.radarInfo.deviceId, store.projectInfo.deviceData);
  if (index !== -1) {
    selectedEntityId.value = CommonUtils.FindObjectOfArray('deviceId', store.radarInfo.deviceId, store.boundaryEntityIds)?.['entityId'];
    if (selectedEntityId.value && store.boundaryEntityIds[index]) {
      boundaryShow.value = CesiumUtils.FindEntityById(store.boundaryEntityIds[index]['entityId'])?.show || false;
    }
  }
  
  // ✅ currentRadar已在selectOnChange开头设置，这里不需要重复设置
  // currentRadar.value = device.type已在上面设置
  
  // ✅ 获取当前项目的geoMarks
  const currentProject = store.projectInfo.projectData.find(p => p.projectId === store.projectInfo.projectSelected);
  let geoMarks = currentProject?.geoMarks || [];
  markData.value = geoMarks.length > 0 ? geoMarks.filter(item => item['devices'] && item['devices'][0] === e) : [];
  
  console.log('设备信息加载完成');
}
const getCoordinate = () => {
  CesiumUtils.DrawPoint('请在地图上选择一个点并鼠标左击确认').then((result) => {
    // ✅ 更新独立坐标字段
    currentDevice.longitude = result[0][0];
    currentDevice.latitude = result[0][1];
    currentDevice.elevation = result[0][2];
    store.radarInfo.coordinates = result[0];
    
    const index = CommonUtils.FindIndexOfArray('id', store.radarInfo.deviceId, store.projectInfo.deviceData);
    if (index !== -1) {
      CesiumUtils.EntityRemoveById(store.projectInfo.deviceData[index]['entityId']);
      CesiumUtils.EntityRemove(result[1]);
      CesiumUtils.EntityPointAdd(result[0][0], result[0][1], result[0][2], MyLocation, store.projectInfo.deviceData[index]['name']).then(entity => store.projectInfo.deviceData[index]['entityId'] = entity.id);
    }
  })
}
// ✅ 新方法1: 保存基础信息（直接入库，不下发指令）
const saveBasicInfo = async () => {
  console.log('saveBasicInfo - 保存基础信息');
  
  // 验证必填字段
  if (!store.radarInfo.projectId || !store.radarInfo.deviceId) {
    showMessage('项目ID和设备ID不能为空', 'error');
    return;
  }
  
  if (!store.radarInfo.deviceName) {
    showMessage('设备名称不能为空', 'error');
    return;
  }
  
  // 准备基础信息参数（匹配后端Device DTO）
  const basicInfo = {
    projectId: store.radarInfo.projectId,
    deviceId: store.radarInfo.deviceId,
    deviceName: store.radarInfo.deviceName,  // ✅ 使用deviceName
    factoryId: currentDevice.factoryId || '',
    longitude: currentDevice.longitude,
    latitude: currentDevice.latitude,
    elevation: currentDevice.elevation,  // ✅ 使用elevation
    orientation: currentDevice.orientation,
    // 保留其他必要字段
    deviceType: currentRadar.value === 'MIMOLITE' ? 'MimoLite' : 'ArcSAR',
    status: 'Active'
  };
  
  console.log('准备保存的基础信息:', basicInfo);
  
  try {
    // ✅ 调用DeviceController.UpdateDevice接口（只更新基础信息）
    const res = await axios.put(ApiRadar.apiUrl + '/api/Device/' + store.radarInfo.deviceId, basicInfo);
    console.log('保存基础信息响应:', res);
    
    if (res.data && res.data.code === 200) {
      showMessage('基础信息保存成功');
      
      // ✅ 重新加载设备信息
      const devRes = await ApiRadar.getDevicesByProjectId(store.radarInfo.projectId);
      if (devRes.data && devRes.data.code === 200 && devRes.data.data) {
        const projectIndex = store.projectInfo.projectData.findIndex(p => p.projectId === store.radarInfo.projectId);
        if (projectIndex !== -1) {
          store.projectInfo.projectData[projectIndex].devices = devRes.data.data.map(d => ({
            deviceId: d.deviceId,
            deviceName: d.deviceName,
            id: d.deviceId,
            name: d.deviceName,
            type: d.deviceTypeCode === 2 || d.deviceTypeCode === 7 || d.deviceTypeCode === 8 ? 'MIMOLITE' : 'ER',
            status: d.status,
            coordinates: [d.longitude || 0, d.latitude || 0, d.elevation || 0],
            longitude: d.longitude || 0,
            latitude: d.latitude || 0,
            elevation: d.elevation || 0,
            factoryId: d.factoryId || '',
            orientation: d.orientation || 0,
            ipAddress: d.ipAddress,
            port: d.port,
            params: d.params || {},
            dataVersion: d.dataVersion || '0',
            algorithmParam: d.algorithmParam || {}
          }));
        }
        staticDataBind();
      }
    } else {
      showMessage(res.data?.message || '保存失败', 'error');
    }
  } catch (err) {
    console.error('保存基础信息失败:', err);
    showMessage('保存失败: ' + err.message, 'error');
  }
};

// ✅ 新方法2: 下发雷达参数（先下发指令，成功后再入库）
const sendRadarParams = async () => {
  console.log('sendRadarParams - 下发雷达参数');
  console.log('currentRadar.value:', currentRadar.value);
  
  const params = store.radarInfo.params;
  
  // 验证参数
  if (currentDevice.orientation < 0 || params['RngMax'] < 0 || params['RngMin'] < 0 || params['ImgAngleStart'] < 0 || params['ImgAngleEnd'] < 0) {
    showMessage('探测角度和距离不可小于0', 'error');
    return;
  }
  
  // 准备雷达参数
  const radarParams = {
    projectId: store.radarInfo.projectId,
    deviceId: store.radarInfo.deviceId,
    ImgAngleStart: params['ImgAngleStart'],
    ImgAngleEnd: params['ImgAngleEnd'],
    RngMin: params['RngMin'],
    RngMax: params['RngMax'],
    FreqBand: params['FreqBand'],
    AnteBeam_half: params['AnteBeam_half'],
    dataVersion: dataVersion.value
  };
  
  console.log('准备下发的雷达参数:', radarParams);
  
  try {
    // ✅ 步骤1: 先发送指令到雷达设备
    let commandResult;
    if (currentRadar.value === 'MIMOLITE') {
      radarParams['modelSelect'] = params['ModelSelect'];
      radarParams['updateTime'] = params['UpdateTime'];
      console.log('发送MIMO Lite参数控制指令...');
      commandResult = await ApiRadar.setPushiRadarParamControl(store.radarInfo.projectId, store.radarInfo.deviceId);
    } else {
      console.log('发送ArcSAR参数控制指令...');
      commandResult = await ApiRadar.setParamControl(store.radarInfo.projectId, store.radarInfo.deviceId);
    }
    
    console.log('参数控制指令响应:', commandResult);
    
    // ✅ 步骤2: 指令发送成功后，保存参数到数据库
    if (commandResult && commandResult.status === 200) {
      console.log('指令发送成功，开始保存参数到数据库...');
      
      const saveUrl = currentRadar.value === 'MIMOLITE' 
        ? '/api/protocol/update/radar/mimolite/param'
        : '/api/protocol/update/radar/param';
      
      const saveRes = await axios.post(ApiRadar.apiUrl + saveUrl, radarParams);
      console.log('保存参数到数据库响应:', saveRes);
      
      if (saveRes.data && saveRes.data.code === 200) {
        showMessage('雷达参数下发并保存成功');
        
        // 记录日志
        await ApiRadar.AddRadarLog(
          currentRadar.value === 'MIMOLITE' ? "下发普适雷达参数" : "下发雷达参数",
          store.sysinfo.config.username,
          store.sysinfo.address,
          store.sysinfo.config.projectCode,
          store.sysinfo.config.shortName
        );
        
        // 重新加载设备信息
        const devRes = await ApiRadar.getDevicesByProjectId(store.radarInfo.projectId);
        if (devRes.data && devRes.data.code === 200 && devRes.data.data) {
          const projectIndex = store.projectInfo.projectData.findIndex(p => p.projectId === store.radarInfo.projectId);
          if (projectIndex !== -1) {
            store.projectInfo.projectData[projectIndex].devices = devRes.data.data.map(d => ({
              deviceId: d.deviceId,
              deviceName: d.deviceName,
              id: d.deviceId,
              name: d.deviceName,
              type: d.deviceTypeCode === 2 || d.deviceTypeCode === 7 || d.deviceTypeCode === 8 ? 'MIMOLITE' : 'ER',
              status: d.status,
              coordinates: [d.longitude || 0, d.latitude || 0, d.elevation || 0],
              longitude: d.longitude || 0,
              latitude: d.latitude || 0,
              elevation: d.elevation || 0,
              factoryId: d.factoryId || '',
              orientation: d.orientation || 0,
              ipAddress: d.ipAddress,
              port: d.port,
              params: d.params || {},
              dataVersion: d.dataVersion || '0',
              algorithmParam: d.algorithmParam || {}
            }));
          }
          staticDataBind();
        }
      } else {
        showMessage('参数保存到数据库失败: ' + (saveRes.data?.message || ''), 'error');
      }
    } else {
      showMessage('指令发送失败，参数未保存到数据库', 'warning');
      console.warn('指令发送失败，保持原有参数值');
    }
  } catch (err) {
    console.error('下发雷达参数失败:', err);
    showMessage('下发失败: ' + err.message, 'error');
  }
};

/*-- events --*/
onMounted(async () => {
  console.log('RadarParams.onMounted - 开始初始化');
  console.log('当前projectSelected:', store.projectInfo.projectSelected);
  console.log('projectData数量:', store.projectInfo.projectData.length);
  
  // ✅ 如果当前没有选中项目，默认选择第一个项目
  if (!store.projectInfo.projectSelected && store.projectInfo.projectData.length > 0) {
    console.log('没有选中项目，默认选择第一个');
    store.projectInfo.projectSelected = store.projectInfo.projectData[0].projectId;
    staticDataBind();
  }
  
  // ✅ 设置projectId
  if (store.projectInfo.projectSelected) {
    store.radarInfo.projectId = String(store.projectInfo.projectSelected);
    
    // ✅ 调用设备查询接口，获取最新的设备信息
    try {
      const res = await ApiRadar.getDevicesByProjectId(store.projectInfo.projectSelected);
      console.log('页面加载时设备查询结果:', res);
      
      if (res.data && res.data.code === 200 && res.data.data) {
        // 更新当前项目的设备列表
        const projectIndex = store.projectInfo.projectData.findIndex(p => p.projectId === store.projectInfo.projectSelected);
        if (projectIndex !== -1) {
          // 映射设备数据
          store.projectInfo.projectData[projectIndex].devices = res.data.data.map(d => {
            let deviceTypeStr = 'ER';
            
            // ✅ 调试：查看每个设备的类型代码
            console.log('页面加载-设备映射:', {
              deviceId: d.deviceId,
              deviceName: d.deviceName,
              deviceTypeCode: d.deviceTypeCode,
              deviceType: d.deviceType
            });
            
            if (d.deviceTypeCode) {
              switch (d.deviceTypeCode) {
                case 1: deviceTypeStr = 'ER'; break;
                case 2: deviceTypeStr = 'MIMOLITE'; break;
                case 5: deviceTypeStr = 'ER'; break;
                case 6: deviceTypeStr = 'ER'; break;
                case 7: deviceTypeStr = 'MIMOLITE'; break;
                case 8: deviceTypeStr = 'MIMOLITE'; break;
                default: deviceTypeStr = 'ER'; break;
              }
            }
            
            console.log('  → 页面加载-映射后的类型:', deviceTypeStr);
            
            return {
              deviceId: d.deviceId,
              deviceName: d.deviceName,
              id: d.deviceId,
              name: d.deviceName,
              type: deviceTypeStr,
              status: d.status,
              coordinates: [d.longitude || 0, d.latitude || 0, d.elevation || 0],
              longitude: d.longitude || 0,
              latitude: d.latitude || 0,
              elevation: d.elevation || 0,
              factoryId: d.factoryId || '',
              orientation: d.orientation || 0,
              ipAddress: d.ipAddress,
              port: d.port,
              params: d.params || {},
              dataVersion: d.dataVersion || '0',
              algorithmParam: d.algorithmParam || {}
            };
          });
          
          console.log('页面加载后的设备列表:', store.projectInfo.projectData[projectIndex].devices);
        }
      }
    } catch (error) {
      console.error('加载设备信息失败:', error);
    }
  }
  
  console.log('当前项目设备数量:', currentProjectDevices.value.length);
  
  // ✅ 如果当前没有选中设备，默认选择当前项目的第一个设备
  if (!store.radarInfo.deviceId && currentProjectDevices.value.length > 0) {
    console.log('没有选中设备，默认选择第一个设备');
    store.radarInfo.deviceId = String(currentProjectDevices.value[0].id);
    store.radarInfo.deviceName = currentProjectDevices.value[0].name;
  }
  
  // ✅ 如果有选中的设备，加载设备信息
  if (store.radarInfo.deviceId && currentProjectDevices.value.length > 0) {
    console.log('加载设备信息:', store.radarInfo.deviceId);
    const deviceIdStr = String(store.radarInfo.deviceId);
    // ✅ selectOnChange会自动设置currentRadar.value，这里不需要设置
    selectOnChange(deviceIdStr);
  } else {
    console.warn('没有可用的设备');
  }
});
</script>

<style scoped>
#idradarparams {
  height: 100%;
  width: 100%;
}

.el-button+.el-button {
  margin-left: 0;
}
</style>