<template>
  <section id="idalgorithmparams" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;算法参数配置</span>
      </template>
      <template v-slot:dragger-content>
        <!-- ✅ 三个按钮 -->
        <a-row type="flex" :gutter="8" align="middle">
          <a-col :span="8">
            <a-button class="custom-ant-btn custom-btn" type="primary" ghost block @click="getParamsFromDevice">
              获取参数
            </a-button>
          </a-col>
          <a-col :span="8">
            <a-button class="custom-ant-btn custom-btn" type="default" ghost block @click="saveToDatabase">
              保存数据
            </a-button>
          </a-col>
          <a-col :span="8">
            <a-button class="custom-ant-btn custom-btn" type="primary" ghost block @click="sendToDevice">
              下发指令
            </a-button>
          </a-col>
        </a-row>
        
        <a-row class="custom-row">
          <el-form :model="algorithmParams" :rules="rules">
            <!-- 项目选择 -->
            <el-form-item label="项目">
              <el-select v-model="store.projectInfo.projectSelected" @change="onProjectChange">
                <el-option v-for="item in store.projectInfo.projectData" :key="item.projectId" 
                  :label="item.projectName" :value="item.projectId" />
              </el-select>
            </el-form-item>
            
            <!-- 设备选择 -->
            <el-form-item label="设备">
              <el-select v-model="store.radarInfo.deviceId" placeholder="请选择设备" @change="selectOnChange">
                <el-option v-for="item in currentProjectDevices" :key="item.id" 
                  :label="item.name" :value="item.id" />
              </el-select>
            </el-form-item>
            
            <!-- 通用算法参数 -->
            <el-divider content-position="left">通用参数</el-divider>
            
            <el-form-item label="形变图像抽取" prop="DefoImageDec">
              <el-select v-model="algorithmParams.DefoImageDec">
                <el-option label="1" value="0"/>
                <el-option label="2" value="1"/>
                <el-option label="5" value="4"/>
                <el-option label="10" value="9"/>
                <el-option label="20" value="19"/>
              </el-select>
            </el-form-item>
            
            <el-form-item label="散射图像抽取" prop="ScatImageDec">
              <el-select v-model="algorithmParams.ScatImageDec">
                <el-option label="1" value="0"/>
                <el-option label="2" value="1"/>
                <el-option label="5" value="4"/>
                <el-option label="10" value="9"/>
                <el-option label="20" value="19"/>
                <el-option label="50" value="49"/>
                <el-option label="100" value="99"/>
              </el-select>
            </el-form-item>
            
            <el-form-item label="大气相位误差估计">
              <el-select v-model="algorithmParams.AtmPhaErrEstFuncSwitch">
                <el-option label="距离模式" value="0"/>
                <el-option label="相近模式" value="1"/>
                <el-option label="高程模式" value="2"/>
              </el-select>
            </el-form-item>
            
            <el-text type="warning">PS点选取灵敏度系数：值越大，选取点数越多，建议值5</el-text>
            <el-form-item label="PS灵敏度系数" prop="SensCoef">
              <el-input-number v-model="algorithmParams.SensCoef" :min="1" :max="9" style="width: 100%"/>
            </el-form-item>
            
            <!-- MIMO Lite专用参数 -->
            <div v-show="currentRadar === 'MIMOLITE'">
              <el-divider content-position="left">MIMO Lite专用参数</el-divider>
              
              <el-form-item label="Beta滤波参数" prop="BetaFilter">
                <el-input-number v-model="algorithmParams.BetaFilter" :min="2" :max="10" style="width: 100%"/>
              </el-form-item>
              
              <el-form-item label="窗口相干" prop="WinCoheren">
                <el-input-number v-model="algorithmParams.WinCoheren" :min="1" :max="3" style="width: 100%"/>
              </el-form-item>
              
              <el-form-item label="滤波类型">
                <el-select v-model="algorithmParams.FilterType">
                  <el-option label="启用" value="0"/>
                  <el-option label="关闭" value="1"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="监测模式">
                <el-select v-model="algorithmParams.MonitorMode">
                  <el-option label="Z模式" value="0"/>
                  <el-option label="B模式" value="1"/>
                  <el-option label="S模式" value="2"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="Alpha滤波参数" prop="AlphaFilter">
                <el-input-number v-model="algorithmParams.AlphaFilter" :min="1" :max="10" style="width: 100%"/>
              </el-form-item>
              
              <el-form-item label="滤波宽度" prop="FilterWidth">
                <el-input-number v-model="algorithmParams.FilterWidth" :min="1" :max="20" style="width: 100%"/>
              </el-form-item>
              
              <el-form-item label="去噪阈值" prop="DeNoiseThread">
                <el-input-number v-model="algorithmParams.DeNoiseThread" :min="0" :max="100" style="width: 100%"/>
              </el-form-item>
            </div>
            
            <!-- ER雷达专用参数（扩展） -->
            <div v-show="currentRadar === 'ER'">
              <el-divider content-position="left">ER雷达专用参数</el-divider>
              
              <el-form-item label="距离徙动补偿">
                <el-select v-model="algorithmParams.RngCellMigrationCompensation">
                  <el-option label="关闭" :value="0"/>
                  <el-option label="启用" :value="1"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="运动补偿">
                <el-select v-model="algorithmParams.MotionCompensation">
                  <el-option label="关闭" :value="0"/>
                  <el-option label="启用" :value="1"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="自动聚焦">
                <el-select v-model="algorithmParams.AutoFocusing">
                  <el-option label="关闭" :value="0"/>
                  <el-option label="启用" :value="1"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="SAR成像">
                <el-select v-model="algorithmParams.SARImaging">
                  <el-option label="关闭" :value="0"/>
                  <el-option label="启用" :value="1"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="加窗处理">
                <el-select v-model="algorithmParams.Windowing">
                  <el-option label="关闭" :value="0"/>
                  <el-option label="启用" :value="1"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="相位校正方法">
                <el-select v-model="algorithmParams.PhaseCorrectionMethod">
                  <el-option label="方法1" :value="0"/>
                  <el-option label="方法2" :value="1"/>
                  <el-option label="方法3" :value="2"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="相位校正参数">
                <el-input-number v-model="algorithmParams.PhaseCorrectionParam" :min="0" :max="10" :precision="2" style="width: 100%"/>
              </el-form-item>
              
              <el-form-item label="插值方法">
                <el-select v-model="algorithmParams.Interpolation">
                  <el-option label="最近邻" :value="0"/>
                  <el-option label="双线性" :value="1"/>
                  <el-option label="三次样条" :value="2"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="图像增强">
                <el-select v-model="algorithmParams.ImageEnhancement">
                  <el-option label="关闭" :value="0"/>
                  <el-option label="启用" :value="1"/>
                </el-select>
              </el-form-item>
              
              <el-form-item label="图像增强参数">
                <el-input-number v-model="algorithmParams.ImageEnhancementParam" :min="0" :max="10" :precision="2" style="width: 100%"/>
              </el-form-item>
            </div>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
/*-- imports --*/
import { defineComponent, ref, onMounted, computed, reactive } from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import axios from 'axios';  // ✅ 添加axios导入
import { useMapStore } from "@/store/index.js";
import { ApiRadar } from "@/axios/apiRadar.js";
import { showMessage } from "@/utils/tools.js";
import { staticDataBind } from "@/utils/radartool.js";
import { useI18n } from "vue-i18n";

/*-- name --*/
defineComponent({
  name: "algorithmparams",
});

/*-- props  --*/
const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
});

/*-- store --*/
const store = useMapStore();
const { t } = useI18n();

/*-- vars --*/
const currentRadar = ref('');

// ✅ 算法参数响应式对象（完整版）
const algorithmParams = reactive({
  // 通用参数
  DefoImageDec: '0',
  ScatImageDec: '0',
  AtmPhaErrEstFuncSwitch: '0',
  SensCoef: 5,
  
  // MIMO Lite专用参数
  BetaFilter: 5,
  WinCoheren: 2,
  FilterType: '0',
  MonitorMode: '0',
  AlphaFilter: 5,
  FilterWidth: 5,
  DeNoiseThread: 10,
  
  // ER雷达专用参数
  RngCellMigrationCompensation: 0,
  MotionCompensation: 0,
  AutoFocusing: 0,
  SARImaging: 0,
  Windowing: 0,
  PhaseCorrectionMethod: 0,
  PhaseCorrectionParam: 0,
  Interpolation: 0,
  ImageEnhancement: 0,
  ImageEnhancementParam: 0
});

/*-- computed --*/
const currentProjectDevices = computed(() => {
  const currentProject = store.projectInfo.projectData.find(
    p => p.projectId === store.projectInfo.projectSelected
  );
  return currentProject?.devices || [];
});

/*-- validation rules --*/
const rules = ref({
  WinCoheren: [
    { required: true, message: '请输入', trigger: 'blur' },
    { validator: (rule, value, callback) => {
        const num = parseInt(value);
        if (num < 1 || num > 3) {
          return callback(new Error('值范围: 1-3'));
        }
        callback();
      }, trigger: 'blur' }
  ],
  BetaFilter: [
    { required: true, message: '请输入', trigger: 'blur' },
    { validator: (rule, value, callback) => {
        const num = parseInt(value);
        if (num <= 1 || num > 10) {
          return callback(new Error('值范围: 2-10'));
        }
        callback();
      }, trigger: 'blur' }
  ],
  SensCoef: [
    { required: true, message: '请输入', trigger: 'blur' },
    { validator: (rule, value, callback) => {
        const num = parseInt(value);
        if (num < 1 || num > 9) {
          return callback(new Error('值范围: 1-9'));
        }
        callback();
      }, trigger: 'blur' }
  ],
});

/*-- methods --*/

// ✅ 方法1: 从设备获取算法参数（发送查询指令）
const getParamsFromDevice = async () => {
  console.log('从设备获取算法参数');
  
  if (!store.radarInfo.projectId || !store.radarInfo.deviceId) {
    showMessage('请先选择项目和设备', 'error');
    return;
  }
  
  try {
    if (currentRadar.value === 'MIMOLITE') {
      // MIMO Lite: 发送获取参数指令（指令12）
      showMessage('正在从MIMO设备获取参数...', 'info');
      store.paramLoading = true;
      
      // 设置超时
      setTimeout(() => {
        if (store.paramLoading) {
          store.paramLoading = false;
          showMessage('获取算法参数超时', 'error');
        }
      }, 10000);
      
      // 通过MQTT发送获取参数指令
      store.client.publish('/dev/radar/mimoLite/defo/command', JSON.stringify({
        slaveId: store.radarInfo.params['slaveId'],
        deviceId: store.radarInfo.deviceId,
        command: "12"  // 12 = 获取参数指令
      }));
      
      // MQTT响应会通过订阅消息返回，更新store.radarInfo.algorithmParam
      // 这里等待MQTT响应...
      
    } else {
      // ER雷达: 调用HTTP接口获取参数
      showMessage('正在从ER雷达获取参数...', 'info');
      
      const res = await ApiRadar.getAlgorithmParam(store.radarInfo.projectId, store.radarInfo.deviceId);
      console.log('获取算法参数响应:', res);
      
      if (res.data && res.data.code === 200 && res.data.data) {
        // 更新算法参数
        Object.assign(algorithmParams, res.data.data);
        showMessage('参数获取成功');
      } else {
        showMessage('获取参数失败', 'error');
      }
    }
  } catch (err) {
    console.error('获取参数失败:', err);
    showMessage('获取失败: ' + err.message, 'error');
  }
};

// ✅ 方法2: 保存到数据库（不下发指令）
const saveToDatabase = async () => {
  console.log('保存算法参数到数据库');
  
  if (!store.radarInfo.projectId || !store.radarInfo.deviceId) {
    showMessage('请先选择项目和设备', 'error');
    return;
  }
  
  // 准备参数
  const params = {
    projectId: store.radarInfo.projectId,
    deviceId: store.radarInfo.deviceId,
    // 通用参数
    DefoImageDec: algorithmParams.DefoImageDec,
    ScatImageDec: algorithmParams.ScatImageDec,
    AtmPhaErrEstFuncSwitch: algorithmParams.AtmPhaErrEstFuncSwitch,
    SensCoef: algorithmParams.SensCoef
  };
  
  // 根据设备类型添加特定参数
  if (currentRadar.value === 'MIMOLITE') {
    params.BetaFilter = algorithmParams.BetaFilter;
    params.WinCoheren = algorithmParams.WinCoheren;
    params.FilterType = algorithmParams.FilterType;
    params.MonitorMode = algorithmParams.MonitorMode;
    params.AlphaFilter = algorithmParams.AlphaFilter;
    params.FilterWidth = algorithmParams.FilterWidth;
    params.DeNoiseThread = algorithmParams.DeNoiseThread;
  } else {
    params.RngCellMigrationCompensation = algorithmParams.RngCellMigrationCompensation;
    params.MotionCompensation = algorithmParams.MotionCompensation;
    params.AutoFocusing = algorithmParams.AutoFocusing;
    params.SARImaging = algorithmParams.SARImaging;
    params.Windowing = algorithmParams.Windowing;
    params.PhaseCorrectionMethod = algorithmParams.PhaseCorrectionMethod;
    params.PhaseCorrectionParam = algorithmParams.PhaseCorrectionParam;
    params.Interpolation = algorithmParams.Interpolation;
    params.ImageEnhancement = algorithmParams.ImageEnhancement;
    params.ImageEnhancementParam = algorithmParams.ImageEnhancementParam;
  }
  
  console.log('准备保存的算法参数:', params);
  
  try {
    const saveUrl = currentRadar.value === 'MIMOLITE' 
      ? '/api/protocol/update/radar/mimolite/algoparam'
      : '/api/protocol/update/radar/algoparam';
    
    const res = await axios.post(ApiRadar.apiUrl + saveUrl, params);
    console.log('保存算法参数响应:', res);
    
    if (res.data && res.data.code === 200) {
      showMessage('算法参数保存成功');
      
      // 重新加载设备信息
      await refreshDeviceData();
    } else {
      showMessage(res.data?.message || '保存失败', 'error');
    }
  } catch (err) {
    console.error('保存算法参数失败:', err);
    showMessage('保存失败: ' + err.message, 'error');
  }
};

// ✅ 方法3: 下发到设备（先下发指令，成功后再保存）
const sendToDevice = async () => {
  console.log('下发算法参数到设备');
  console.log('currentRadar.value:', currentRadar.value);
  
  if (!store.radarInfo.projectId || !store.radarInfo.deviceId) {
    showMessage('请先选择项目和设备', 'error');
    return;
  }
  
  // 准备参数
  const params = {
    projectId: store.radarInfo.projectId,
    deviceId: store.radarInfo.deviceId,
    SensCoef: algorithmParams.SensCoef,
    AtmPhaErrEstFuncSwitch: algorithmParams.AtmPhaErrEstFuncSwitch,
    DefoImageDec: algorithmParams.DefoImageDec,
    ScatImageDec: algorithmParams.ScatImageDec
  };
  
  try {
    // ✅ 步骤1: 发送指令到设备
    let commandResult;
    
    if (currentRadar.value === 'MIMOLITE') {
      // MIMO Lite: 通过MQTT发送参数
      params.command = '13';  // 13 = 设置参数指令
      params.slaveId = store.radarInfo.params['slaveId'];
      params.BetaFilter = algorithmParams.BetaFilter;
      params.WinCoheren = algorithmParams.WinCoheren;
      params.FilterType = algorithmParams.FilterType;
      params.MonitorMode = algorithmParams.MonitorMode;
      params.AlphaFilter = algorithmParams.AlphaFilter;
      params.FilterWidth = algorithmParams.FilterWidth;
      params.DeNoiseThread = algorithmParams.DeNoiseThread;
      
      console.log('通过MQTT发送MIMO Lite算法参数:', params);
      store.client.publish('/dev/radar/mimoLite/defo/command', JSON.stringify(params));
      
      // MQTT没有同步响应，假设成功
      commandResult = { status: 200 };
      
    } else {
      // ER雷达: 通过HTTP发送指令（指令13）
      console.log('发送ER雷达算法控制指令...');
      params.RngCellMigrationCompensation = algorithmParams.RngCellMigrationCompensation;
      params.MotionCompensation = algorithmParams.MotionCompensation;
      params.AutoFocusing = algorithmParams.AutoFocusing;
      params.SARImaging = algorithmParams.SARImaging;
      params.Windowing = algorithmParams.Windowing;
      params.PhaseCorrectionMethod = algorithmParams.PhaseCorrectionMethod;
      params.PhaseCorrectionParam = algorithmParams.PhaseCorrectionParam;
      params.Interpolation = algorithmParams.Interpolation;
      params.ImageEnhancement = algorithmParams.ImageEnhancement;
      params.ImageEnhancementParam = algorithmParams.ImageEnhancementParam;
      
      commandResult = await ApiRadar.controlRadar(
        store.radarInfo.projectId,
        store.radarInfo.deviceId,
        '13',  // 13 = 算法参数控制指令
        'qingqiangjia'
      );
    }
    
    console.log('指令响应:', commandResult);
    
    // ✅ 步骤2: 指令成功后，保存到数据库
    if (commandResult && commandResult.status === 200) {
      console.log('指令发送成功，开始保存参数到数据库...');
      
      const saveUrl = currentRadar.value === 'MIMOLITE' 
        ? '/api/protocol/update/radar/mimolite/algoparam'
        : '/api/protocol/update/radar/algoparam';
      
      const saveParams = {
        projectId: store.radarInfo.projectId,
        deviceId: store.radarInfo.deviceId,
        DefoImageDec: algorithmParams.DefoImageDec,
        ScatImageDec: algorithmParams.ScatImageDec,
        AtmPhaErrEstFuncSwitch: algorithmParams.AtmPhaErrEstFuncSwitch,
        SensCoef: algorithmParams.SensCoef
      };
      
      if (currentRadar.value === 'MIMOLITE') {
        saveParams.BetaFilter = algorithmParams.BetaFilter;
        saveParams.WinCoheren = algorithmParams.WinCoheren;
        saveParams.FilterType = algorithmParams.FilterType;
        saveParams.MonitorMode = algorithmParams.MonitorMode;
        saveParams.AlphaFilter = algorithmParams.AlphaFilter;
        saveParams.FilterWidth = algorithmParams.FilterWidth;
        saveParams.DeNoiseThread = algorithmParams.DeNoiseThread;
      } else {
        saveParams.RngCellMigrationCompensation = algorithmParams.RngCellMigrationCompensation;
        saveParams.MotionCompensation = algorithmParams.MotionCompensation;
        saveParams.AutoFocusing = algorithmParams.AutoFocusing;
        saveParams.SARImaging = algorithmParams.SARImaging;
        saveParams.Windowing = algorithmParams.Windowing;
        saveParams.PhaseCorrectionMethod = algorithmParams.PhaseCorrectionMethod;
        saveParams.PhaseCorrectionParam = algorithmParams.PhaseCorrectionParam;
        saveParams.Interpolation = algorithmParams.Interpolation;
        saveParams.ImageEnhancement = algorithmParams.ImageEnhancement;
        saveParams.ImageEnhancementParam = algorithmParams.ImageEnhancementParam;
      }
      
      const saveRes = await axios.post(ApiRadar.apiUrl + saveUrl, saveParams);
      console.log('保存算法参数到数据库响应:', saveRes);
      
      if (saveRes.data && saveRes.data.code === 200) {
        showMessage('算法参数下发并保存成功');
        
        // 记录日志
        await ApiRadar.AddRadarLog(
          currentRadar.value === 'MIMOLITE' ? "下发普适雷达算法参数" : "下发雷达算法参数",
          store.sysinfo.config.username,
          store.sysinfo.address,
          store.sysinfo.config.projectCode,
          store.sysinfo.config.shortName
        );
        
        // 重新加载设备信息
        await refreshDeviceData();
      } else {
        showMessage('参数保存到数据库失败', 'error');
      }
    } else {
      showMessage('指令发送失败，参数未保存到数据库', 'warning');
      console.warn('指令发送失败，保持原有参数值');
    }
  } catch (err) {
    console.error('下发算法参数失败:', err);
    showMessage('下发失败: ' + err.message, 'error');
  }
};

// 刷新设备数据
const refreshDeviceData = async () => {
  try {
    const devRes = await ApiRadar.getDevicesByProjectId(store.radarInfo.projectId);
    if (devRes.data && devRes.data.code === 200 && devRes.data.data) {
      const projectIndex = store.projectInfo.projectData.findIndex(
        p => p.projectId === store.radarInfo.projectId
      );
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
  } catch (err) {
    console.error('刷新设备数据失败:', err);
  }
};

// 项目切换
const onProjectChange = async () => {
  console.log('AlgorithmParams: 项目切换:', store.projectInfo.projectSelected);
  
  store.radarInfo.projectId = String(store.projectInfo.projectSelected || '');
  
  // ✅ 调用设备查询接口
  try {
    const res = await ApiRadar.getDevicesByProjectId(store.projectInfo.projectSelected);
    if (res.data && res.data.code === 200 && res.data.data) {
      const projectIndex = store.projectInfo.projectData.findIndex(
        p => p.projectId === store.projectInfo.projectSelected
      );
      if (projectIndex !== -1) {
        store.projectInfo.projectData[projectIndex].devices = res.data.data.map(d => ({
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
    }
  } catch (err) {
    console.error('加载设备失败:', err);
  }
  
  staticDataBind();
  
  // 选择第一个设备
  if (currentProjectDevices.value.length > 0) {
    const firstDevice = currentProjectDevices.value[0];
    store.radarInfo.deviceId = String(firstDevice.id);
    store.radarInfo.deviceName = firstDevice.name;
    currentRadar.value = firstDevice.type || 'ER';
    selectOnChange();
  } else {
    console.warn('当前项目没有设备');
    store.radarInfo.deviceId = null;
    store.radarInfo.deviceName = '';
  }
};

// 设备切换
const selectOnChange = async () => {
  console.log('AlgorithmParams: 设备切换:', store.radarInfo.deviceId);
  
  const device = currentProjectDevices.value.find(d => d.id === store.radarInfo.deviceId);
  
  if (!device) {
    console.error('找不到设备:', store.radarInfo.deviceId);
    return;
  }
  
  console.log('找到设备:', device.name, device);
  
  // 设置设备类型
  store.radarInfo.projectId = String(store.projectInfo.projectSelected || '');
  store.radarInfo.deviceId = String(device.id || '');
  store.radarInfo.deviceName = device.name;
  currentRadar.value = device.type || 'ER';
  
  console.log('设备类型:', currentRadar.value);
  
  // ✅ 先尝试从数据库加载算法参数
  try {
    const res = await ApiRadar.getAlgorithmParam(store.radarInfo.projectId, store.radarInfo.deviceId);
    console.log('从数据库获取算法参数响应:', res);
    
    if (res.data && res.data.code === 200 && res.data.data) {
      console.log('从数据库加载算法参数:', res.data.data);
      Object.assign(algorithmParams, res.data.data);
    } else if (res.data && res.data.code === 404) {
      // ✅ 数据库没有记录，尝试从设备对象加载
      console.log('数据库没有记录，从设备对象加载');
      if (device.algorithmParam && Object.keys(device.algorithmParam).length > 0) {
        console.log('从设备对象加载算法参数:', device.algorithmParam);
        Object.assign(algorithmParams, device.algorithmParam);
      } else {
        console.log('设备对象也没有算法参数，使用默认值');
      }
    }
  } catch (err) {
    console.error('获取算法参数失败:', err);
    // 尝试从设备对象加载
    if (device.algorithmParam && Object.keys(device.algorithmParam).length > 0) {
      console.log('从设备对象加载算法参数（fallback）:', device.algorithmParam);
      Object.assign(algorithmParams, device.algorithmParam);
    }
  }
  
  console.log('当前算法参数:', algorithmParams);
};

/*-- events --*/
onMounted(async () => {
  console.log('AlgorithmParams.onMounted - 开始初始化');
  
  // 如果没有选中项目，默认选择第一个
  if (!store.projectInfo.projectSelected && store.projectInfo.projectData.length > 0) {
    store.projectInfo.projectSelected = store.projectInfo.projectData[0].projectId;
    staticDataBind();
  }
  
  // 设置projectId
  if (store.projectInfo.projectSelected) {
    store.radarInfo.projectId = String(store.projectInfo.projectSelected);
    
    // ✅ 加载设备信息
    try {
      const res = await ApiRadar.getDevicesByProjectId(store.projectInfo.projectSelected);
      if (res.data && res.data.code === 200 && res.data.data) {
        const projectIndex = store.projectInfo.projectData.findIndex(
          p => p.projectId === store.projectInfo.projectSelected
        );
        if (projectIndex !== -1) {
          store.projectInfo.projectData[projectIndex].devices = res.data.data.map(d => ({
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
      }
    } catch (err) {
      console.error('加载设备失败:', err);
    }
  }
  
  // 如果没有选中设备，默认选择第一个
  if (!store.radarInfo.deviceId && currentProjectDevices.value.length > 0) {
    store.radarInfo.deviceId = String(currentProjectDevices.value[0].id);
    store.radarInfo.deviceName = currentProjectDevices.value[0].name;
  }
  
  // 加载设备算法参数
  if (store.radarInfo.deviceId && currentProjectDevices.value.length > 0) {
    selectOnChange();
  }
  
  console.log('AlgorithmParams.onMounted - 初始化完成');
});
</script>

<style scoped>
#idalgorithmparams {
  height: 100%;
  width: 100%;
}
</style>

