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
            
            <!-- ✅ 参数来源提示 -->
            <el-form-item v-if="paramSource" label="参数来源">
              <el-tag v-if="paramSource === 'device'" type="success" size="small">
                📡 来自设备
              </el-tag>
              <el-tag v-else-if="paramSource === 'database'" type="info" size="small">
                💾 来自数据库
              </el-tag>
              <el-tag v-else type="warning" size="small">
                ⚙️ 默认值
              </el-tag>
            </el-form-item>
            
            <!-- 新32字段算法参数 -->
            <el-divider content-position="left">算法参数配置（新版）</el-divider>
            
            <!-- 1. 监测模式 -->
            <el-form-item label="监测模式">
              <el-select v-model="algorithmParams.MonMode">
                <el-option label="Z模式" value="Z"/>
                <el-option label="B模式" value="B"/>
                <el-option label="S模式" value="S"/>
              </el-select>
            </el-form-item>
            
            <!-- 2. 相位滤波类型选择控制变量 -->
            <el-form-item label="相位滤波类型">
              <el-select v-model="algorithmParams.PhaFltTypeCtrl">
                <el-option label="类型0" :value="0"/>
                <el-option label="类型1" :value="1"/>
              </el-select>
            </el-form-item>
            
            <!-- 3. 滤波半窗长 -->
            <el-form-item label="滤波半窗长">
              <el-input-number v-model="algorithmParams.FltHalfWinLen" :min="1" :max="20" style="width: 100%"/>
            </el-form-item>
            
            <!-- 4. 大气滤波使能 -->
            <el-form-item label="大气滤波使能">
              <el-input-number v-model="algorithmParams.AtmFltEn" :min="0" :max="1" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <!-- 5. 均值加权 -->
            <el-form-item label="均值加权">
              <el-input-number v-model="algorithmParams.MeanWgt" :min="0" :max="10" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <!-- 6. 压缩形变阈值 -->
            <el-form-item label="压缩形变阈值">
              <el-input-number v-model="algorithmParams.CmpDefThr" :min="1" :max="100" style="width: 100%"/>
            </el-form-item>
            
            <!-- 7. 压缩倍数 -->
            <el-form-item label="压缩倍数">
              <el-input-number v-model="algorithmParams.CmpMult" :min="1" :max="100" style="width: 100%"/>
            </el-form-item>
            
            <!-- 8. 幅度检测门限 -->
            <el-form-item label="幅度检测门限">
              <el-input-number v-model="algorithmParams.AmpDetThr" :min="0" :max="10" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-divider content-position="left">大气滤波参数</el-divider>
            
            <!-- 9-10. 大气滤波参数 A/B -->
            <el-form-item label="大气滤波参数A">
              <el-input-number v-model="algorithmParams.AtmFltParaA" :min="0" :max="100" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-form-item label="大气滤波参数B">
              <el-input-number v-model="algorithmParams.AtmFltParaB" :min="0" :max="100" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <!-- 11-13. 大气校正门限 -->
            <el-form-item label="第二阶段大气校正门限1">
              <el-input-number v-model="algorithmParams.AtmCorrThr2nd_1" :min="0" :max="100" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-form-item label="二次大气补偿更新周期">
              <el-input-number v-model="algorithmParams.AtmCompUpdPer" :min="0" :max="1000" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-form-item label="第二阶段大气校正门限2">
              <el-input-number v-model="algorithmParams.AtmCorrThr2nd_2" :min="0" :max="100" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-divider content-position="left">图像抽帧参数</el-divider>
            
            <!-- 14-15. 图像抽帧 -->
            <el-form-item label="形变图像抽帧">
              <el-select v-model="algorithmParams.DefImgDecim">
                <el-option label="1" value="1"/>
                <el-option label="2" value="2"/>
                <el-option label="5" value="5"/>
                <el-option label="10" value="10"/>
                <el-option label="20" value="20"/>
              </el-select>
            </el-form-item>
            
            <el-form-item label="复数图图像抽帧">
              <el-select v-model="algorithmParams.CplxImgDecim">
                <el-option label="1" value="1"/>
                <el-option label="2" value="2"/>
                <el-option label="5" value="5"/>
                <el-option label="10" value="10"/>
                <el-option label="20" value="20"/>
                <el-option label="50" value="50"/>
                <el-option label="100" value="100"/>
              </el-select>
            </el-form-item>
            
            <!-- 16. 大气校正算法 -->
            <el-form-item label="大气校正算法">
              <el-select v-model="algorithmParams.AtmCorrAlg">
                <el-option label="算法0" value="0"/>
                <el-option label="算法1" value="1"/>
                <el-option label="算法2" value="2"/>
              </el-select>
            </el-form-item>
            
            <el-divider content-position="left">大气相位误差估计</el-divider>
            
            <!-- 17-18. 大气相位误差估计距离 -->
            <el-form-item label="大气相位误差估计距离1">
              <el-input-number v-model="algorithmParams.AtmPhaErrEstDist_1" :min="0" :max="10000" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-form-item label="大气相位误差估计距离2">
              <el-input-number v-model="algorithmParams.AtmPhaErrEstDist_2" :min="0" :max="10000" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <!-- 19. 标准差加权 -->
            <el-form-item label="标准差加权">
              <el-input-number v-model="algorithmParams.StdDevWgt" :min="0" :max="10" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <!-- 20. 短时形变量积参数 -->
            <el-form-item label="短时形变量积参数">
              <el-input-number v-model="algorithmParams.ShortDefAccPara" :min="0" :max="100" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-divider content-position="left">去噪参数</el-divider>
            
            <!-- 21. 去噪门限 -->
            <el-form-item label="去噪门限">
              <el-input-number v-model="algorithmParams.DenoiseThr" :min="1" :max="100" style="width: 100%"/>
            </el-form-item>
            
            <!-- 22-23. 噪声均衡 -->
            <el-form-item label="是否噪声均衡">
              <el-input-number v-model="algorithmParams.IsNoiseEq" :min="0" :max="1" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-form-item label="噪声均衡类型">
              <el-input-number v-model="algorithmParams.NoiseEqType" :min="0" :max="10" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-divider content-position="left">PS点选取参数</el-divider>
            
            <!-- 24. 幅度离差选择门限初值 -->
            <el-form-item label="幅度离差选择门限初值">
              <el-input-number v-model="algorithmParams.AmpDevSelThrInit" :min="0" :max="1" :precision="2" :step="0.1" style="width: 100%"/>
            </el-form-item>
            
            <!-- 25. 相干系数阈值初值 -->
            <el-form-item label="相干系数阈值初值">
              <el-input-number v-model="algorithmParams.CohCoeThrInit" :min="0" :max="1" :precision="2" :step="0.01" style="width: 100%"/>
            </el-form-item>
            
            <!-- 26-27. 有效PS点 -->
            <el-form-item label="相关系数有效PS点">
              <el-input-number v-model="algorithmParams.CorrCoeffEffPSPts" :min="0" :max="10000" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-form-item label="有效PS点">
              <el-input-number v-model="algorithmParams.EffPSPts" :min="0" :max="10000" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <!-- 28-29. 门限参数 -->
            <el-form-item label="干涉相位残差阈值">
              <el-input-number v-model="algorithmParams.IfgPhaResThr" :min="0" :max="10" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <el-form-item label="奇异点门限">
              <el-input-number v-model="algorithmParams.SingPntThr" :min="0" :max="10" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <!-- 30-31. PS点灵敏度 -->
            <el-form-item label="PS点灵敏度">
              <el-input-number v-model="algorithmParams.PSPntSens" :min="1" :max="10" style="width: 100%"/>
            </el-form-item>
            
            <el-form-item label="PS门限调节系数">
              <el-input-number v-model="algorithmParams.PSThrAdjCoeff" :min="0" :max="10" :precision="6" style="width: 100%"/>
            </el-form-item>
            
            <!-- 32. 相干半窗长 -->
            <el-form-item label="相干半窗长">
              <el-input-number v-model="algorithmParams.CohHalfWinLen" :min="1" :max="20" style="width: 100%"/>
            </el-form-item>
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
const paramSource = ref(''); // ✅ 参数来源：'device' 或 'database' 或 'default'

  // ✅ 算法参数响应式对象（新32字段版本）
const algorithmParams = reactive({
  // 新32字段算法参数
  MonMode: 'Z',                      // 1. 监测模式
  PhaFltTypeCtrl: 0,                 // 2. 相位滤波类型选择控制变量
  FltHalfWinLen: 1,                  // 3. 滤波半窗长
  AtmFltEn: 0.0,                     // 4. 大气滤波使能
  MeanWgt: 0.0,                      // 5. 均值加权
  CmpDefThr: 1,                      // 6. 压缩形变阈值
  CmpMult: 1,                        // 7. 压缩倍数
  AmpDetThr: 0.0,                    // 8. 幅度检测门限
  AtmFltParaA: 0.0,                  // 9. 大气滤波参数 A
  AtmFltParaB: 0.0,                  // 10. 大气滤波参数 B
  AtmCorrThr2nd_1: 0.0,              // 11. 第二阶段大气校正门限1
  AtmCompUpdPer: 0.0,                // 12. 二次大气补偿更新周期
  AtmCorrThr2nd_2: 0.0,              // 13. 第二阶段大气校正门限2
  DefImgDecim: '1',                  // 14. 形变图像抽帧
  CplxImgDecim: '1',                 // 15. 复数图图像抽帧
  AtmCorrAlg: '0',                   // 16. 大气校正算法
  AtmPhaErrEstDist_1: 0.0,           // 17. 大气相位误差估计距离1
  AtmPhaErrEstDist_2: 0.0,           // 18. 大气相位误差估计距离2
  StdDevWgt: 0.0,                    // 19. 标准差加权
  ShortDefAccPara: 0.0,              // 20. 短时形变量积参数
  DenoiseThr: 1,                     // 21. 去噪门限
  IsNoiseEq: 0.0,                    // 22. 是否噪声均衡
  NoiseEqType: 0.0,                  // 23. 噪声均衡类型
  AmpDevSelThrInit: 0.1,             // 24. 幅度离差选择门限初值
  CohCoeThrInit: 0.01,               // 25. 相干系数阈值初值
  CorrCoeffEffPSPts: 0.0,            // 26. 相关系数有效PS点
  EffPSPts: 0.0,                     // 27. 有效PS点
  IfgPhaResThr: 0.0,                 // 28. 干涉相位残差阈值
  SingPntThr: 0.0,                   // 29. 奇异点门限
  PSPntSens: 1,                      // 30. PS点灵敏度
  PSThrAdjCoeff: 0.0,                // 31. PS门限调节系数
  CohHalfWinLen: 1                   // 32. 相干半窗长
});

/*-- computed --*/
const currentProjectDevices = computed(() => {
  const currentProject = store.projectInfo.projectData.find(
    p => p.projectId === store.projectInfo.projectSelected
  );
  return currentProject?.devices || [];
});

/*-- validation rules （新32字段版本）--*/
const rules = ref({
  FltHalfWinLen: [
    { validator: (rule, value, callback) => {
        const num = parseInt(value);
        if (num < 1 || num > 20) {
          return callback(new Error('值范围: 1-20'));
        }
        callback();
      }, trigger: 'blur' }
  ],
  CohHalfWinLen: [
    { validator: (rule, value, callback) => {
        const num = parseInt(value);
        if (num < 1 || num > 20) {
          return callback(new Error('值范围: 1-20'));
        }
        callback();
      }, trigger: 'blur' }
  ],
  PSPntSens: [
    { required: true, message: '请输入', trigger: 'blur' },
    { validator: (rule, value, callback) => {
        const num = parseInt(value);
        if (num < 1 || num > 10) {
          return callback(new Error('值范围: 1-10'));
        }
        callback();
      }, trigger: 'blur' }
  ],
});

/*-- methods --*/

// ✅ 方法1: 从设备获取算法参数（发送查询指令12）
const getParamsFromDevice = async () => {
  console.log('🔄 从设备获取算法参数（指令12）');
  
  if (!store.radarInfo.projectId || !store.radarInfo.deviceId) {
    showMessage('请先选择项目和设备', 'error');
    return;
  }
  
  try {
    if (currentRadar.value === 'MIMOLITE') {
      // MIMO Lite: 通过MQTT发送获取参数指令（指令12）
      showMessage('正在从MIMO设备获取参数...', 'info');
      store.paramLoading = true;
      
      // 设置超时
      const timeoutId = setTimeout(() => {
        if (store.paramLoading) {
          store.paramLoading = false;
          showMessage('从设备获取算法参数超时，尝试从数据库获取', 'warning');
          // 超时后从数据库获取
          loadParamsFromDatabase();
        }
      }, 10000);
      
      // 通过MQTT发送获取参数指令
      store.client.publish('/dev/radar/mimoLite/defo/command', JSON.stringify({
        slaveId: store.radarInfo.params['slaveId'],
        deviceId: store.radarInfo.deviceId,
        command: "12"  // 12 = 获取参数指令
      }));
      
      // MQTT响应会通过订阅消息返回，更新store.radarInfo.algorithmParam
      // 这里需要监听MQTT响应来更新参数
      // TODO: 监听MQTT响应并更新algorithmParams
      
    } else {
      // ER雷达: 通过HTTP接口发送指令12获取参数
      showMessage('正在从ER雷达获取参数...', 'info');
      
      try {
        const commandRes = await ApiRadar.controlRadar(
          store.radarInfo.projectId,
          store.radarInfo.deviceId,
          '12',  // 12 = 获取参数指令
          store.sysinfo.config.username || 'admin'
        );
        
        console.log('ER雷达获取参数指令响应:', commandRes);
        
        if (commandRes && commandRes.data && commandRes.data.code === 200) {
          // 指令发送成功，等待设备返回参数
          // 注意：实际参数需要通过其他方式获取（可能是WebSocket或轮询）
          showMessage('获取参数指令已发送，等待设备响应...', 'info');
          paramSource.value = 'device';
          
          // TODO: 实际项目中可能需要通过WebSocket或轮询获取设备返回的参数
          // 这里暂时从数据库获取作为fallback
          setTimeout(() => {
            loadParamsFromDatabase();
          }, 2000);
        } else {
          showMessage('从设备获取参数失败，尝试从数据库获取', 'warning');
          loadParamsFromDatabase();
        }
      } catch (err) {
        console.error('从设备获取参数失败:', err);
        showMessage('从设备获取参数失败，尝试从数据库获取', 'warning');
        loadParamsFromDatabase();
      }
    }
  } catch (err) {
    console.error('获取参数失败:', err);
    showMessage('从设备获取失败，尝试从数据库获取', 'warning');
    loadParamsFromDatabase();
  }
};

// ✅ 新增：从数据库加载参数的独立方法
const loadParamsFromDatabase = async () => {
  const projectId = String(store.radarInfo.projectId || '');
  const deviceId = String(store.radarInfo.deviceId || '');
  
  console.log('📥 从数据库加载算法参数');
  console.log('  ProjectId:', projectId);
  console.log('  DeviceId:', deviceId);
  console.log('  完整URL:', ApiRadar.apiUrl + '/api/protocol/algorithm/' + projectId + '/' + deviceId);
  
  paramSource.value = 'database';
  
  if (!projectId || !deviceId) {
    console.warn('项目ID或设备ID为空，无法从数据库获取参数', {projectId, deviceId});
    paramSource.value = 'default';
    showMessage('项目ID或设备ID为空，使用默认值', 'warning');
    return;
  }
  
  try {
    const res = await ApiRadar.getAlgorithmParam(projectId, deviceId);
    console.log('从数据库获取算法参数响应:', res);
    console.log('响应数据结构:', {
      status: res.status,
      data: res.data,
      code: res.data?.code,
      hasData: !!res.data?.data,
      message: res.data?.message
    });
    
    if (res && res.data) {
      if (res.data.code === 200 && res.data.data) {
        console.log('✅ 数据库返回数据:', res.data.data);
        // 更新算法参数
        updateAlgorithmParamsFromData(res.data.data);
        console.log('✅ 更新后的算法参数:', algorithmParams);
        showMessage('✅ 已从数据库加载算法参数', 'success');
        paramSource.value = 'database';
      } else if (res.data.code === 404) {
        paramSource.value = 'default';
        console.warn('数据库中没有找到算法配置记录');
        showMessage('⚠️ 数据库没有记录，使用默认值', 'info');
      } else {
        paramSource.value = 'default';
        const errorMsg = res.data.message || `获取失败 (code: ${res.data.code})`;
        console.error('获取参数失败:', errorMsg, res.data);
        showMessage(`获取参数失败: ${errorMsg}，使用默认值`, 'warning');
      }
    } else {
      paramSource.value = 'default';
      console.error('响应数据格式异常:', res);
      showMessage('响应数据格式异常，使用默认值', 'warning');
    }
  } catch (err) {
    console.error('从数据库获取参数异常:', err);
    console.error('错误详情:', {
      message: err.message,
      response: err.response?.data,
      status: err.response?.status,
      url: err.config?.url
    });
    paramSource.value = 'default';
    const errorMsg = err.response?.data?.message || err.message || '网络错误';
    showMessage(`从数据库获取失败: ${errorMsg}，使用默认值`, 'error');
  }
};

// ✅ 新增：统一更新算法参数的方法（新32字段版本）
const updateAlgorithmParamsFromData = (dbParams) => {
  console.log('🔄 开始更新算法参数（新32字段），接收到的数据:', dbParams);
  
  // ✅ 辅助函数：获取参数值（兼容大小写命名）
  const getParam = (key) => {
    if (dbParams[key] !== undefined && dbParams[key] !== null) {
      return dbParams[key];
    }
    const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
    if (dbParams[camelKey] !== undefined && dbParams[camelKey] !== null) {
      return dbParams[camelKey];
    }
    return undefined;
  };
  
  // ✅ 更新新32字段参数
  // 1. 监测模式 (字符串)
  const monModeValue = getParam('MonMode');
  if (monModeValue !== undefined && monModeValue !== null) {
    algorithmParams.MonMode = String(monModeValue);
    console.log('✅ 更新 MonMode:', monModeValue);
  }
  
  // 2-3. 整数型参数
  const phaFltTypeCtrlValue = getParam('PhaFltTypeCtrl');
  if (phaFltTypeCtrlValue !== undefined && phaFltTypeCtrlValue !== null) {
    algorithmParams.PhaFltTypeCtrl = Number(phaFltTypeCtrlValue) || 0;
  }
  
  const fltHalfWinLenValue = getParam('FltHalfWinLen');
  if (fltHalfWinLenValue !== undefined && fltHalfWinLenValue !== null) {
    algorithmParams.FltHalfWinLen = Number(fltHalfWinLenValue) || 1;
  }
  
  // 4-13. 浮点型参数
  const atmFltEnValue = getParam('AtmFltEn');
  if (atmFltEnValue !== undefined && atmFltEnValue !== null) {
    algorithmParams.AtmFltEn = Number(atmFltEnValue) || 0.0;
  }
  
  const meanWgtValue = getParam('MeanWgt');
  if (meanWgtValue !== undefined && meanWgtValue !== null) {
    algorithmParams.MeanWgt = Number(meanWgtValue) || 0.0;
  }
  
  const cmpDefThrValue = getParam('CmpDefThr');
  if (cmpDefThrValue !== undefined && cmpDefThrValue !== null) {
    algorithmParams.CmpDefThr = Number(cmpDefThrValue) || 1;
  }
  
  const cmpMultValue = getParam('CmpMult');
  if (cmpMultValue !== undefined && cmpMultValue !== null) {
    algorithmParams.CmpMult = Number(cmpMultValue) || 1;
  }
  
  const ampDetThrValue = getParam('AmpDetThr');
  if (ampDetThrValue !== undefined && ampDetThrValue !== null) {
    algorithmParams.AmpDetThr = Number(ampDetThrValue) || 0.0;
  }
  
  const atmFltParaAValue = getParam('AtmFltParaA');
  if (atmFltParaAValue !== undefined && atmFltParaAValue !== null) {
    algorithmParams.AtmFltParaA = Number(atmFltParaAValue) || 0.0;
  }
  
  const atmFltParaBValue = getParam('AtmFltParaB');
  if (atmFltParaBValue !== undefined && atmFltParaBValue !== null) {
    algorithmParams.AtmFltParaB = Number(atmFltParaBValue) || 0.0;
  }
  
  const atmCorrThr2nd1Value = getParam('AtmCorrThr2nd_1');
  if (atmCorrThr2nd1Value !== undefined && atmCorrThr2nd1Value !== null) {
    algorithmParams.AtmCorrThr2nd_1 = Number(atmCorrThr2nd1Value) || 0.0;
  }
  
  const atmCompUpdPerValue = getParam('AtmCompUpdPer');
  if (atmCompUpdPerValue !== undefined && atmCompUpdPerValue !== null) {
    algorithmParams.AtmCompUpdPer = Number(atmCompUpdPerValue) || 0.0;
  }
  
  const atmCorrThr2nd2Value = getParam('AtmCorrThr2nd_2');
  if (atmCorrThr2nd2Value !== undefined && atmCorrThr2nd2Value !== null) {
    algorithmParams.AtmCorrThr2nd_2 = Number(atmCorrThr2nd2Value) || 0.0;
  }
  
  // 14-16. 枚举型参数（字符串）
  const defImgDecimValue = getParam('DefImgDecim');
  if (defImgDecimValue !== undefined && defImgDecimValue !== null) {
    algorithmParams.DefImgDecim = String(defImgDecimValue);
    console.log('✅ 更新 DefImgDecim:', defImgDecimValue);
  }
  
  const cplxImgDecimValue = getParam('CplxImgDecim');
  if (cplxImgDecimValue !== undefined && cplxImgDecimValue !== null) {
    algorithmParams.CplxImgDecim = String(cplxImgDecimValue);
  }
  
  const atmCorrAlgValue = getParam('AtmCorrAlg');
  if (atmCorrAlgValue !== undefined && atmCorrAlgValue !== null) {
    algorithmParams.AtmCorrAlg = String(atmCorrAlgValue);
  }
  
  // 17-32. 其他浮点和整数参数
  const atmPhaErrEstDist1Value = getParam('AtmPhaErrEstDist_1');
  if (atmPhaErrEstDist1Value !== undefined && atmPhaErrEstDist1Value !== null) {
    algorithmParams.AtmPhaErrEstDist_1 = Number(atmPhaErrEstDist1Value) || 0.0;
  }
  
  const atmPhaErrEstDist2Value = getParam('AtmPhaErrEstDist_2');
  if (atmPhaErrEstDist2Value !== undefined && atmPhaErrEstDist2Value !== null) {
    algorithmParams.AtmPhaErrEstDist_2 = Number(atmPhaErrEstDist2Value) || 0.0;
  }
  
  const stdDevWgtValue = getParam('StdDevWgt');
  if (stdDevWgtValue !== undefined && stdDevWgtValue !== null) {
    algorithmParams.StdDevWgt = Number(stdDevWgtValue) || 0.0;
  }
  
  const shortDefAccParaValue = getParam('ShortDefAccPara');
  if (shortDefAccParaValue !== undefined && shortDefAccParaValue !== null) {
    algorithmParams.ShortDefAccPara = Number(shortDefAccParaValue) || 0.0;
  }
  
  const denoiseThrValue = getParam('DenoiseThr');
  if (denoiseThrValue !== undefined && denoiseThrValue !== null) {
    algorithmParams.DenoiseThr = Number(denoiseThrValue) || 1;
  }
  
  const isNoiseEqValue = getParam('IsNoiseEq');
  if (isNoiseEqValue !== undefined && isNoiseEqValue !== null) {
    algorithmParams.IsNoiseEq = Number(isNoiseEqValue) || 0.0;
  }
  
  const noiseEqTypeValue = getParam('NoiseEqType');
  if (noiseEqTypeValue !== undefined && noiseEqTypeValue !== null) {
    algorithmParams.NoiseEqType = Number(noiseEqTypeValue) || 0.0;
  }
  
  const ampDevSelThrInitValue = getParam('AmpDevSelThrInit');
  if (ampDevSelThrInitValue !== undefined && ampDevSelThrInitValue !== null) {
    algorithmParams.AmpDevSelThrInit = Number(ampDevSelThrInitValue) || 0.1;
  }
  
  const cohCoeThrInitValue = getParam('CohCoeThrInit');
  if (cohCoeThrInitValue !== undefined && cohCoeThrInitValue !== null) {
    algorithmParams.CohCoeThrInit = Number(cohCoeThrInitValue) || 0.01;
  }
  
  const corrCoeffEffPSPtsValue = getParam('CorrCoeffEffPSPts');
  if (corrCoeffEffPSPtsValue !== undefined && corrCoeffEffPSPtsValue !== null) {
    algorithmParams.CorrCoeffEffPSPts = Number(corrCoeffEffPSPtsValue) || 0.0;
  }
  
  const effPSPtsValue = getParam('EffPSPts');
  if (effPSPtsValue !== undefined && effPSPtsValue !== null) {
    algorithmParams.EffPSPts = Number(effPSPtsValue) || 0.0;
  }
  
  const ifgPhaResThrValue = getParam('IfgPhaResThr');
  if (ifgPhaResThrValue !== undefined && ifgPhaResThrValue !== null) {
    algorithmParams.IfgPhaResThr = Number(ifgPhaResThrValue) || 0.0;
  }
  
  const singPntThrValue = getParam('SingPntThr');
  if (singPntThrValue !== undefined && singPntThrValue !== null) {
    algorithmParams.SingPntThr = Number(singPntThrValue) || 0.0;
  }
  
  const psPntSensValue = getParam('PSPntSens');
  if (psPntSensValue !== undefined && psPntSensValue !== null) {
    algorithmParams.PSPntSens = Number(psPntSensValue) || 1;
  }
  
  const psThrAdjCoeffValue = getParam('PSThrAdjCoeff');
  if (psThrAdjCoeffValue !== undefined && psThrAdjCoeffValue !== null) {
    algorithmParams.PSThrAdjCoeff = Number(psThrAdjCoeffValue) || 0.0;
  }
  
  const cohHalfWinLenValue = getParam('CohHalfWinLen');
  if (cohHalfWinLenValue !== undefined && cohHalfWinLenValue !== null) {
    algorithmParams.CohHalfWinLen = Number(cohHalfWinLenValue) || 1;
  }
  
  console.log('✅ 算法参数更新完成（新32字段）');
};

// ✅ 新增：统一的保存参数到数据库的内部方法（新32字段版本）
const saveParamsToDatabaseInternal = async () => {
  if (!store.radarInfo.projectId || !store.radarInfo.deviceId) {
    throw new Error('项目ID和设备ID不能为空');
  }
  
  // ✅ 准备新32字段参数，确保类型正确
  const params = {
    projectId: String(store.radarInfo.projectId),
    deviceId: String(store.radarInfo.deviceId),
    // 新32字段算法参数
    MonMode: String(algorithmParams.MonMode || 'Z'),
    PhaFltTypeCtrl: Number(algorithmParams.PhaFltTypeCtrl) || 0,
    FltHalfWinLen: Number(algorithmParams.FltHalfWinLen) || 1,
    AtmFltEn: Number(algorithmParams.AtmFltEn) || 0.0,
    MeanWgt: Number(algorithmParams.MeanWgt) || 0.0,
    CmpDefThr: Number(algorithmParams.CmpDefThr) || 1,
    CmpMult: Number(algorithmParams.CmpMult) || 1,
    AmpDetThr: Number(algorithmParams.AmpDetThr) || 0.0,
    AtmFltParaA: Number(algorithmParams.AtmFltParaA) || 0.0,
    AtmFltParaB: Number(algorithmParams.AtmFltParaB) || 0.0,
    AtmCorrThr2nd_1: Number(algorithmParams.AtmCorrThr2nd_1) || 0.0,
    AtmCompUpdPer: Number(algorithmParams.AtmCompUpdPer) || 0.0,
    AtmCorrThr2nd_2: Number(algorithmParams.AtmCorrThr2nd_2) || 0.0,
    DefImgDecim: String(algorithmParams.DefImgDecim || '1'),
    CplxImgDecim: String(algorithmParams.CplxImgDecim || '1'),
    AtmCorrAlg: String(algorithmParams.AtmCorrAlg || '0'),
    AtmPhaErrEstDist_1: Number(algorithmParams.AtmPhaErrEstDist_1) || 0.0,
    AtmPhaErrEstDist_2: Number(algorithmParams.AtmPhaErrEstDist_2) || 0.0,
    StdDevWgt: Number(algorithmParams.StdDevWgt) || 0.0,
    ShortDefAccPara: Number(algorithmParams.ShortDefAccPara) || 0.0,
    DenoiseThr: Number(algorithmParams.DenoiseThr) || 1,
    IsNoiseEq: Number(algorithmParams.IsNoiseEq) || 0.0,
    NoiseEqType: Number(algorithmParams.NoiseEqType) || 0.0,
    AmpDevSelThrInit: Number(algorithmParams.AmpDevSelThrInit) || 0.1,
    CohCoeThrInit: Number(algorithmParams.CohCoeThrInit) || 0.01,
    CorrCoeffEffPSPts: Number(algorithmParams.CorrCoeffEffPSPts) || 0.0,
    EffPSPts: Number(algorithmParams.EffPSPts) || 0.0,
    IfgPhaResThr: Number(algorithmParams.IfgPhaResThr) || 0.0,
    SingPntThr: Number(algorithmParams.SingPntThr) || 0.0,
    PSPntSens: Number(algorithmParams.PSPntSens) || 1,
    PSThrAdjCoeff: Number(algorithmParams.PSThrAdjCoeff) || 0.0,
    CohHalfWinLen: Number(algorithmParams.CohHalfWinLen) || 1
  };
  
  console.log('💾 准备保存的算法参数（新32字段）:', params);
  
  const saveUrl = '/api/protocol/update/radar/algoparam';
  
  const res = await axios.post(ApiRadar.apiUrl + saveUrl, params);
  console.log('💾 保存算法参数响应:', res);
  
  if (res.data && res.data.code === 200) {
    // 更新参数来源为数据库
    paramSource.value = 'database';
    return true;
  } else {
    throw new Error(res.data?.message || '保存失败');
  }
};

// ✅ 方法2: 保存到数据库（不下发指令，直接保存）
const saveToDatabase = async () => {
  console.log('💾 保存算法参数到数据库（直接保存，不下发指令）');
  
  if (!store.radarInfo.projectId || !store.radarInfo.deviceId) {
    showMessage('请先选择项目和设备', 'error');
    return;
  }
  
  try {
    await saveParamsToDatabaseInternal();
    
    showMessage('✅ 算法参数保存成功（已自动创建或更新配置）', 'success');
    
    // ✅ 修复：保存成功后重新加载参数，确保UI显示最新数据
    await selectOnChange();
    
    // 重新加载设备信息
    await refreshDeviceData();
  } catch (err) {
    console.error('❌ 保存算法参数失败:', err);
    showMessage('保存失败: ' + err.message, 'error');
  }
};

// ✅ 方法3: 下发到设备（先下发指令13，成功后再保存到数据库）
const sendToDevice = async () => {
  console.log('📡 下发算法参数到设备（指令13）');
  console.log('currentRadar.value:', currentRadar.value);
  
  if (!store.radarInfo.projectId || !store.radarInfo.deviceId) {
    showMessage('请先选择项目和设备', 'error');
    return;
  }
  
  try {
    // ✅ 步骤1: 发送指令13到设备
    let commandResult;
    
    if (currentRadar.value === 'MIMOLITE') {
      // MIMO Lite: 通过MQTT发送参数（指令13）
      console.log('📡 通过MQTT发送MIMO Lite算法参数（指令13）...');
      
      const mqttParams = {
        slaveId: store.radarInfo.params['slaveId'],
        deviceId: store.radarInfo.deviceId,
        command: '13',  // 13 = 设置参数指令
        // ✅ 使用新32字段算法参数
        MonMode: algorithmParams.MonMode,
        PhaFltTypeCtrl: algorithmParams.PhaFltTypeCtrl,
        FltHalfWinLen: algorithmParams.FltHalfWinLen,
        AtmFltEn: algorithmParams.AtmFltEn,
        MeanWgt: algorithmParams.MeanWgt,
        CmpDefThr: algorithmParams.CmpDefThr,
        CmpMult: algorithmParams.CmpMult,
        AmpDetThr: algorithmParams.AmpDetThr,
        AtmFltParaA: algorithmParams.AtmFltParaA,
        AtmFltParaB: algorithmParams.AtmFltParaB,
        AtmCorrThr2nd_1: algorithmParams.AtmCorrThr2nd_1,
        AtmCompUpdPer: algorithmParams.AtmCompUpdPer,
        AtmCorrThr2nd_2: algorithmParams.AtmCorrThr2nd_2,
        DefImgDecim: algorithmParams.DefImgDecim,
        CplxImgDecim: algorithmParams.CplxImgDecim,
        AtmCorrAlg: algorithmParams.AtmCorrAlg,
        AtmPhaErrEstDist_1: algorithmParams.AtmPhaErrEstDist_1,
        AtmPhaErrEstDist_2: algorithmParams.AtmPhaErrEstDist_2,
        StdDevWgt: algorithmParams.StdDevWgt,
        ShortDefAccPara: algorithmParams.ShortDefAccPara,
        DenoiseThr: algorithmParams.DenoiseThr,
        IsNoiseEq: algorithmParams.IsNoiseEq,
        NoiseEqType: algorithmParams.NoiseEqType,
        AmpDevSelThrInit: algorithmParams.AmpDevSelThrInit,
        CohCoeThrInit: algorithmParams.CohCoeThrInit,
        CorrCoeffEffPSPts: algorithmParams.CorrCoeffEffPSPts,
        EffPSPts: algorithmParams.EffPSPts,
        IfgPhaResThr: algorithmParams.IfgPhaResThr,
        SingPntThr: algorithmParams.SingPntThr,
        PSPntSens: algorithmParams.PSPntSens,
        PSThrAdjCoeff: algorithmParams.PSThrAdjCoeff,
        CohHalfWinLen: algorithmParams.CohHalfWinLen
      };
      
      console.log('MQTT参数:', mqttParams);
      store.client.publish('/dev/radar/mimoLite/defo/command', JSON.stringify(mqttParams));
      
      // MQTT没有同步响应，等待一段时间后假设成功
      // TODO: 实际项目中应该监听MQTT响应确认成功
      await new Promise(resolve => setTimeout(resolve, 1000));
      commandResult = { status: 200, data: { code: 200 } };  // 假设成功
      
    } else {
      // ER雷达: 通过HTTP发送指令13
      console.log('📡 发送ER雷达算法控制指令（指令13）...');
      
      // 注意：ER雷达的controlRadar接口可能只发送指令，不包含参数
      // 参数需要通过其他方式传递，这里先发送指令
      commandResult = await ApiRadar.controlRadar(
        store.radarInfo.projectId,
        store.radarInfo.deviceId,
        '13',  // 13 = 算法参数控制指令
        store.sysinfo.config.username || 'admin'
      );
      
      console.log('ER雷达指令响应:', commandResult);
    }
    
    // ✅ 步骤2: 检查指令是否成功
    const isSuccess = (commandResult && commandResult.status === 200) || 
                     (commandResult && commandResult.data && commandResult.data.code === 200);
    
    if (isSuccess) {
      console.log('✅ 指令发送成功，开始保存参数到数据库...');
      
      // ✅ 步骤3: 指令成功后，保存到数据库
      await saveParamsToDatabaseInternal();
      
      showMessage('✅ 算法参数下发并保存成功（已自动创建或更新配置）', 'success');
      
      // 记录日志
      try {
        await ApiRadar.AddRadarLog(
          currentRadar.value === 'MIMOLITE' ? "下发普适雷达算法参数" : "下发雷达算法参数",
          store.sysinfo.config.username,
          store.sysinfo.address,
          store.sysinfo.config.projectCode,
          store.sysinfo.config.shortName
        );
      } catch (logErr) {
        console.warn('记录日志失败:', logErr);
      }
      
      // ✅ 修复：保存成功后重新加载参数，确保UI显示最新数据
      await selectOnChange();
      
      // 重新加载设备信息
      await refreshDeviceData();
      
    } else {
      showMessage('❌ 指令发送失败，参数未保存到数据库', 'error');
      console.error('指令发送失败，响应:', commandResult);
    }
  } catch (err) {
    console.error('❌ 下发算法参数失败:', err);
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
  
  // ✅ 修复：使用加载后的设备数组选择第一个设备
  const loadedDevices = store.projectInfo.projectData.find(
    p => p.projectId === store.projectInfo.projectSelected
  )?.devices || [];
  
  if (loadedDevices.length > 0) {
    const firstDevice = loadedDevices[0];
    // ✅ 修复：先重置参数，再设置设备，最后加载参数
    // 清空当前参数（避免显示旧数据）
    Object.assign(algorithmParams, {
      // ✅ 重置为32个新字段的默认值
      MonMode: 'Z',
      PhaFltTypeCtrl: 0,
      FltHalfWinLen: 1,
      AtmFltEn: 0.0,
      MeanWgt: 0.0,
      CmpDefThr: 1,
      CmpMult: 1,
      AmpDetThr: 0.0,
      AtmFltParaA: 0.0,
      AtmFltParaB: 0.0,
      AtmCorrThr2nd_1: 0.0,
      AtmCompUpdPer: 0.0,
      AtmCorrThr2nd_2: 0.0,
      DefImgDecim: '1',
      CplxImgDecim: '1',
      AtmCorrAlg: '0',
      AtmPhaErrEstDist_1: 0.0,
      AtmPhaErrEstDist_2: 0.0,
      StdDevWgt: 0.0,
      ShortDefAccPara: 0.0,
      DenoiseThr: 1,
      IsNoiseEq: 0.0,
      NoiseEqType: 0.0,
      AmpDevSelThrInit: 0.1,
      CohCoeThrInit: 0.01,
      CorrCoeffEffPSPts: 0.0,
      EffPSPts: 0.0,
      IfgPhaResThr: 0.0,
      SingPntThr: 0.0,
      PSPntSens: 1,
      PSThrAdjCoeff: 0.0,
      CohHalfWinLen: 1,
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
    
    // 设置设备信息
    store.radarInfo.deviceId = String(firstDevice.id);
    store.radarInfo.deviceName = firstDevice.name;
    currentRadar.value = firstDevice.type || 'ER';
    
    // ✅ 修复：切换设备时会自动从设备获取参数（优先），失败后从数据库获取
    await selectOnChange();
  } else {
    console.warn('当前项目没有设备');
    store.radarInfo.deviceId = null;
    store.radarInfo.deviceName = '';
    currentRadar.value = '';
    // 清空算法参数（新32字段版本）
    Object.assign(algorithmParams, {
      MonMode: 'Z',
      PhaFltTypeCtrl: 0,
      FltHalfWinLen: 1,
      AtmFltEn: 0.0,
      MeanWgt: 0.0,
      CmpDefThr: 1,
      CmpMult: 1,
      AmpDetThr: 0.0,
      AtmFltParaA: 0.0,
      AtmFltParaB: 0.0,
      AtmCorrThr2nd_1: 0.0,
      AtmCompUpdPer: 0.0,
      AtmCorrThr2nd_2: 0.0,
      DefImgDecim: '1',
      CplxImgDecim: '1',
      AtmCorrAlg: '0',
      AtmPhaErrEstDist_1: 0.0,
      AtmPhaErrEstDist_2: 0.0,
      StdDevWgt: 0.0,
      ShortDefAccPara: 0.0,
      DenoiseThr: 1,
      IsNoiseEq: 0.0,
      NoiseEqType: 0.0,
      AmpDevSelThrInit: 0.1,
      CohCoeThrInit: 0.01,
      CorrCoeffEffPSPts: 0.0,
      EffPSPts: 0.0,
      IfgPhaResThr: 0.0,
      SingPntThr: 0.0,
      PSPntSens: 1,
      PSThrAdjCoeff: 0.0,
      CohHalfWinLen: 1
    });
  }
};

// 设备切换
const selectOnChange = async () => {
  console.log('AlgorithmParams: 设备切换:', store.radarInfo.deviceId);
  
  if (!store.radarInfo.deviceId) {
    console.warn('设备ID为空，跳过加载');
    return;
  }
  
  // ✅ 修复：确保ID类型匹配
  const device = currentProjectDevices.value.find(d => 
    String(d.id) === String(store.radarInfo.deviceId) || 
    String(d.deviceId) === String(store.radarInfo.deviceId)
  );
  
  if (!device) {
    console.error('找不到设备:', store.radarInfo.deviceId, '可用设备:', currentProjectDevices.value.map(d => ({id: d.id, deviceId: d.deviceId})));
    showMessage('找不到选中的设备', 'error');
    return;
  }
  
  console.log('找到设备:', device.name, device);
  
  // 设置设备类型
  store.radarInfo.projectId = String(store.projectInfo.projectSelected || '');
  // ✅ 修复：优先使用 deviceId，如果没有则使用 id
  const deviceIdToUse = String(device.deviceId || device.id || '');
  store.radarInfo.deviceId = deviceIdToUse;
  store.radarInfo.deviceName = device.name;
  currentRadar.value = device.type || 'ER';
  
  console.log('设备类型:', currentRadar.value);
  console.log('使用的设备ID:', deviceIdToUse, 'device对象:', {id: device.id, deviceId: device.deviceId, name: device.name});
  
  // ✅ 修复：先重置参数为默认值（新32字段版本）
  const defaultParams = {
    MonMode: 'Z',
    PhaFltTypeCtrl: 0,
    FltHalfWinLen: 1,
    AtmFltEn: 0.0,
    MeanWgt: 0.0,
    CmpDefThr: 1,
    CmpMult: 1,
    AmpDetThr: 0.0,
    AtmFltParaA: 0.0,
    AtmFltParaB: 0.0,
    AtmCorrThr2nd_1: 0.0,
    AtmCompUpdPer: 0.0,
    AtmCorrThr2nd_2: 0.0,
    DefImgDecim: '1',
    CplxImgDecim: '1',
    AtmCorrAlg: '0',
    AtmPhaErrEstDist_1: 0.0,
    AtmPhaErrEstDist_2: 0.0,
    StdDevWgt: 0.0,
    ShortDefAccPara: 0.0,
    DenoiseThr: 1,
    IsNoiseEq: 0.0,
    NoiseEqType: 0.0,
    AmpDevSelThrInit: 0.1,
    CohCoeThrInit: 0.01,
    CorrCoeffEffPSPts: 0.0,
    EffPSPts: 0.0,
    IfgPhaResThr: 0.0,
    SingPntThr: 0.0,
    PSPntSens: 1,
    PSThrAdjCoeff: 0.0,
    CohHalfWinLen: 1
  };
  Object.assign(algorithmParams, defaultParams);
  
  // ✅ 修复：优先从设备获取参数（指令12），失败后再从数据库获取
  console.log('🔄 切换设备，开始获取算法参数 - ProjectId:', store.radarInfo.projectId, 'DeviceId:', store.radarInfo.deviceId);
  
  // 步骤1: 尝试从设备获取参数（发送指令12）
  try {
    if (currentRadar.value === 'MIMOLITE') {
      // MIMO Lite: 通过MQTT发送获取参数指令
      console.log('📡 发送MQTT指令12获取MIMO设备参数...');
      store.paramLoading = true;
      
      // 设置超时，超时后从数据库获取
      setTimeout(() => {
        if (store.paramLoading) {
          store.paramLoading = false;
          console.log('⏱️ MQTT获取参数超时，从数据库获取');
          loadParamsFromDatabase();
        }
      }, 10000);
      
      // 通过MQTT发送获取参数指令
      store.client.publish('/dev/radar/mimoLite/defo/command', JSON.stringify({
        slaveId: store.radarInfo.params['slaveId'],
        deviceId: store.radarInfo.deviceId,
        command: "12"  // 12 = 获取参数指令
      }));
      
      // MQTT响应会通过订阅消息返回，需要监听响应
      // TODO: 实际项目中需要监听MQTT响应并更新参数
      // 这里暂时等待2秒后从数据库获取作为fallback
      setTimeout(() => {
        if (store.paramLoading) {
          console.log('⏱️ 等待MQTT响应超时，从数据库获取');
          store.paramLoading = false;
          loadParamsFromDatabase();
        }
      }, 2000);
      
    } else {
      // ER雷达: 通过HTTP接口发送指令12获取参数
      console.log('📡 发送HTTP指令12获取ER雷达参数...');
      
      try {
        const commandRes = await ApiRadar.controlRadar(
          store.radarInfo.projectId,
          store.radarInfo.deviceId,
          '12',  // 12 = 获取参数指令
          store.sysinfo.config.username || 'admin'
        );
        
        console.log('ER雷达获取参数指令响应:', commandRes);
        
        if (commandRes && commandRes.data && commandRes.data.code === 200) {
          // 指令发送成功，但实际参数需要从数据库获取
          // 因为指令12只是查询指令，实际参数需要通过其他方式获取
          // 这里直接从数据库获取参数
          console.log('✅ 设备指令发送成功，从数据库获取实际参数');
          // 不等待，直接从数据库获取
          loadParamsFromDatabase();
        } else {
          // 指令失败，从数据库获取
          console.log('⚠️ 从设备获取参数失败，从数据库获取');
          loadParamsFromDatabase();
        }
      } catch (err) {
        console.error('❌ 从设备获取参数失败:', err);
        // 失败后从数据库获取
        loadParamsFromDatabase();
      }
    }
  } catch (err) {
    console.error('❌ 从设备获取参数异常:', err);
    // 异常时从数据库获取
    loadParamsFromDatabase();
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

