<template>
  <section id="idspeedtarget" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;速度指标配置</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn custom-btn" type="primary" ghost block @click="commitUpdate">保存配置</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <!-- ✅ 项目选择 -->
            <el-form-item label="项目">
              <el-select v-model="store.projectInfo.projectSelected" @change="onProjectChange" placeholder="选择项目">
                <el-option v-for="item in store.projectInfo.projectData" :key="item.projectId" 
                  :label="item.projectName" :value="item.projectId" />
              </el-select>
            </el-form-item>
            
            <el-divider content-position="left">速度指标配置</el-divider>
            
            <el-alert 
              title="速度指标说明" 
              type="info" 
              :closable="false"
              style="margin-bottom: 15px;">
              <template #default>
                <p style="margin: 0;">形变曲线和图像计算基准单位间隔</p>
                <p style="margin: 5px 0 0 0;">选择的时间单位将用于计算速度和加速度</p>
              </template>
            </el-alert>
            
            <!-- ✅ 多选模式 -->
            <el-form-item label="速度时间单位（多选）">
              <el-select
                v-model="selectedTimeUnits"
                multiple
                placeholder="选择一个或多个时间单位"
                style="width: 100%"
              >
                <el-option label="30分钟" value="00" />
                <el-option label="1小时" value="01" />
                <el-option label="1天" value="02" />
                <el-option label="3天" value="03" />
                <el-option label="1周" value="04" />
                <el-option label="1月" value="05" />
              </el-select>
            </el-form-item>
            
            <!-- ✅ 单选开关（快速选择常用配置） -->
            <el-divider content-position="left">快速选择</el-divider>
            
            <el-form-item label="启用单选">
              <el-switch v-model="useCheckboxes" active-text="使用复选框" inactive-text="使用下拉框" />
            </el-form-item>
            
            <div v-show="useCheckboxes">
              <el-form-item label="时间单位">
                <el-checkbox-group v-model="selectedTimeUnits">
                  <el-checkbox label="00">30分钟</el-checkbox>
                  <el-checkbox label="01">1小时</el-checkbox>
                  <el-checkbox label="02">1天</el-checkbox>
                  <el-checkbox label="03">3天</el-checkbox>
                  <el-checkbox label="04">1周</el-checkbox>
                  <el-checkbox label="05">1月</el-checkbox>
                </el-checkbox-group>
              </el-form-item>
            </div>
            
            <!-- ✅ 已选择的时间单位显示 -->
            <el-form-item label="已选择">
              <el-tag 
                v-for="unit in selectedTimeUnits" 
                :key="unit" 
                closable
                @close="removeTimeUnit(unit)"
                style="margin-right: 8px;"
              >
                {{ getTimeUnitLabel(unit) }}
              </el-tag>
              <el-text v-if="selectedTimeUnits.length === 0" type="info">未选择任何时间单位</el-text>
            </el-form-item>
            
            <!-- ✅ 速度图像自动生成配置 -->
            <el-divider content-position="left">速度图像自动生成</el-divider>
            
            <el-alert 
              title="速度图像生成说明" 
              type="info" 
              :closable="false"
              style="margin-bottom: 15px;">
              <template #default>
                <p style="margin: 0;">启用后系统将自动生成速度图像和加速度图像</p>
                <p style="margin: 5px 0 0 0;">• 速度图：显示位移随时间的变化率</p>
                <p style="margin: 5px 0 0 0;">• 加速度图：显示速度随时间的变化率</p>
              </template>
            </el-alert>
            
            <!-- 速度图像配置 -->
            <el-form-item label="自动生成速度图">
              <el-switch 
                v-model="autoGenSpeedImage"
                active-text="开启"
                inactive-text="关闭"
              />
            </el-form-item>
            
            <div v-show="autoGenSpeedImage">
              <el-form-item label="速度图生成间隔">
                <el-input-number 
                  v-model="speedImageInterval"
                  :min="1"
                  :max="1440"
                  placeholder="分钟"
                  style="width: 100%"
                />
                <el-text type="info" size="small" style="margin-top: 5px; display: block;">
                  单位：分钟（建议：60分钟）
                </el-text>
              </el-form-item>
            </div>
            
            <!-- 加速度图像配置 -->
            <el-form-item label="自动生成加速度图">
              <el-switch 
                v-model="autoGenAccelerationImage"
                active-text="开启"
                inactive-text="关闭"
              />
            </el-form-item>
            
            <div v-show="autoGenAccelerationImage">
              <el-form-item label="加速度图生成间隔">
                <el-input-number 
                  v-model="accelerationImageInterval"
                  :min="1"
                  :max="1440"
                  placeholder="分钟"
                  style="width: 100%"
                />
                <el-text type="info" size="small" style="margin-top: 5px; display: block;">
                  单位：分钟（建议：120分钟）
                </el-text>
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
import {defineComponent, ref, onMounted, watch} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import axios from 'axios';
import {useMapStore} from "@/store/index.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {showMessage} from "@/utils/tools.js";
import {useI18n} from "vue-i18n";

/*-- name --*/
defineComponent({
  name: "speedtarget",
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
const selectedTimeUnits = ref([]);  // 选中的时间单位数组
const useCheckboxes = ref(false);   // 是否使用复选框模式

// ✅ 速度图像自动生成配置
const autoGenSpeedImage = ref(false);
const speedImageInterval = ref(60);
const autoGenAccelerationImage = ref(false);
const accelerationImageInterval = ref(120);

/*-- methods --*/

// 获取时间单位标签
const getTimeUnitLabel = (value) => {
  const labels = {
    '00': '30分钟',
    '01': '1小时',
    '02': '1天',
    '03': '3天',
    '04': '1周',
    '05': '1月'
  };
  return labels[value] || value;
};

// 移除时间单位
const removeTimeUnit = (unit) => {
  const index = selectedTimeUnits.value.indexOf(unit);
  if (index > -1) {
    selectedTimeUnits.value.splice(index, 1);
  }
};

// 项目切换
const onProjectChange = async () => {
  console.log('SpeedTarget: 项目切换:', store.projectInfo.projectSelected);
  store.radarInfo.projectId = String(store.projectInfo.projectSelected || '');
  
  // 加载该项目的速度配置
  await loadSpeedConfig();
};

// 加载速度配置
const loadSpeedConfig = async () => {
  if (!store.radarInfo.projectId) {
    console.warn('项目ID为空，跳过加载速度配置');
    return;
  }
  
  try {
    const res = await axios.get(ApiRadar.apiUrl + '/api/protocol/query/speed/target/' + store.radarInfo.projectId);
    console.log('加载速度配置响应:', res);
    
    if (res.data && res.data.code === 200 && res.data.data) {
      // ✅ 解析timeUnits（逗号分隔的字符串 → 数组）
      if (res.data.data.timeUnits) {
        selectedTimeUnits.value = res.data.data.timeUnits.split(',').filter(u => u);
      } else {
        selectedTimeUnits.value = [];
      }
      
      // ✅ 加载速度图像自动生成配置
      autoGenSpeedImage.value = res.data.data.autoGenSpeedImage || false;
      speedImageInterval.value = res.data.data.speedImageInterval || 60;
      autoGenAccelerationImage.value = res.data.data.autoGenAccelerationImage || false;
      accelerationImageInterval.value = res.data.data.accelerationImageInterval || 120;
      
      // 同步到store
      store.radarInfo.imageDiffAnalysisConfig = {
        ...store.radarInfo.imageDiffAnalysisConfig,
        timeUnit: selectedTimeUnits.value,
        autoGenSpeedImage: autoGenSpeedImage.value,
        speedImageInterval: speedImageInterval.value,
        autoGenAccelerationImage: autoGenAccelerationImage.value,
        accelerationImageInterval: accelerationImageInterval.value
      };
      
      console.log('速度配置已加载:', {
        timeUnits: selectedTimeUnits.value,
        autoGenSpeed: autoGenSpeedImage.value,
        speedInterval: speedImageInterval.value,
        autoGenAccel: autoGenAccelerationImage.value,
        accelInterval: accelerationImageInterval.value
      });
    } else if (res.data && res.data.code === 404) {
      // ✅ 未找到配置，使用默认值
      console.log('该项目没有速度配置，使用默认值');
      selectedTimeUnits.value = [];
      autoGenSpeedImage.value = false;
      speedImageInterval.value = 60;
      autoGenAccelerationImage.value = false;
      accelerationImageInterval.value = 120;
    }
  } catch (err) {
    console.error('加载速度配置失败:', err);
    selectedTimeUnits.value = [];
    autoGenSpeedImage.value = false;
    speedImageInterval.value = 60;
    autoGenAccelerationImage.value = false;
    accelerationImageInterval.value = 120;
  }
};

// 保存配置
const commitUpdate = async () => {
  console.log('保存速度指标配置');
  
  if (!store.radarInfo.projectId) {
    showMessage('请先选择项目', 'error');
    return;
  }
  
  if (selectedTimeUnits.value.length === 0) {
    showMessage('请至少选择一个时间单位', 'warning');
    return;
  }
  
  // ✅ 准备保存参数（包含速度图像自动生成配置）
  const params = {
    projectId: store.radarInfo.projectId,
    timeUnit: selectedTimeUnits.value,
    autoGenSpeedImage: autoGenSpeedImage.value,
    speedImageInterval: speedImageInterval.value,
    autoGenAccelerationImage: autoGenAccelerationImage.value,
    accelerationImageInterval: accelerationImageInterval.value
  };
  
  console.log('准备保存的速度配置:', params);
  
  try {
    // ✅ 使用axios直接调用，传递完整参数
    const res = await axios.post(ApiRadar.apiUrl + '/api/protocol/update/speed/target', params);
    console.log('保存速度配置响应:', res);
    
    if (res.data && res.data.code === 200) {
      showMessage('速度指标配置保存成功');
      
      // 同步到store
      store.radarInfo.imageDiffAnalysisConfig = {
        ...store.radarInfo.imageDiffAnalysisConfig,
        timeUnit: selectedTimeUnits.value
      };
      
      // 记录日志
      await ApiRadar.AddRadarLog(
        '修改速度指标配置',
        store.sysinfo.config.username,
        store.sysinfo.address,
        store.sysinfo.config.projectCode,
        store.sysinfo.config.shortName
      );
    } else {
      showMessage(res.data?.message || '保存失败', 'error');
    }
  } catch (err) {
    console.error('保存速度配置失败:', err);
    showMessage('保存失败: ' + err.message, 'error');
  }
};

/*-- events --*/
onMounted(async () => {
  console.log('SpeedTarget.onMounted');
  
  // 初始化默认值
  if (!store.radarInfo.imageDiffAnalysisConfig) {
    store.radarInfo.imageDiffAnalysisConfig = {
      timeUnit: []
    };
  }
  
  // 设置projectId
  if (store.projectInfo.projectSelected) {
    store.radarInfo.projectId = String(store.projectInfo.projectSelected);
    // 加载配置
    await loadSpeedConfig();
  } else if (store.projectInfo.projectData.length > 0) {
    // 默认选择第一个项目
    store.projectInfo.projectSelected = store.projectInfo.projectData[0].projectId;
    store.radarInfo.projectId = String(store.projectInfo.projectSelected);
    await loadSpeedConfig();
  }
});

// ✅ 监听项目切换
watch(() => store.projectInfo.projectSelected, async (newVal, oldVal) => {
  if (newVal && newVal !== oldVal) {
    console.log('SpeedTarget: 监听到项目切换', oldVal, '→', newVal);
    store.radarInfo.projectId = String(newVal);
    await loadSpeedConfig();
  }
});
</script>

<style scoped>
#idspeedtarget {
  height: 100%;
  width: 100%;
}

:deep(.el-checkbox) {
  margin-right: 20px;
  margin-bottom: 10px;
}
</style>
