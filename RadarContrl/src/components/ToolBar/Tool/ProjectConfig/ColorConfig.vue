<template>
  <section id="idcolorconfig" v-show="visible">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em" height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126">
              <path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path>
              <path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path>
            </svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;色条配置</span>
      </template>
      <template v-slot:dragger-content>
        <!-- 保存按钮 -->
        <a-row type="flex" :gutter="8" align="middle" style="margin-bottom: 15px;">
          <a-col :span="24">
            <a-button class="custom-ant-btn custom-btn" type="primary" ghost block @click="saveColorConfig">
              保存配置
            </a-button>
          </a-col>
        </a-row>
        
        <a-row class="custom-row">
          <el-form :model="colorConfig" label-width="140px">
            <!-- 项目选择 -->
            <el-form-item label="项目">
              <el-select v-model="store.projectInfo.projectSelected" @change="onProjectChange" placeholder="选择项目">
                <el-option v-for="item in store.projectInfo.projectData" :key="item.projectId" 
                  :label="item.projectName" :value="item.projectId" />
              </el-select>
            </el-form-item>
            
            <!-- 配色类型选择 -->
            <el-divider content-position="left">色条类型</el-divider>
            <el-form-item label="色条类型">
              <el-radio-group v-model="currentColorType" @change="onColorTypeChange">
                <el-radio value="displacement">位移色条</el-radio>
                <el-radio value="scattering">散射色条</el-radio>
              </el-radio-group>
            </el-form-item>
            
            <!-- 配色方案 -->
            <el-divider content-position="left">配色方案</el-divider>
            <el-form-item label="配色方案类型">
              <el-radio-group v-model="colorConfig.colorSchemeType" @change="updateColorPreview">
                <el-radio :value="0">线性配色</el-radio>
                <el-radio :value="1">分类配色</el-radio>
              </el-radio-group>
            </el-form-item>
            
            <!-- 色条预览 -->
            <el-form-item label="色条预览">
              <div class="colorbar-preview" :id="getColorbarId()"></div>
            </el-form-item>
            
            <!-- 数值范围 -->
            <el-divider content-position="left">数值范围</el-divider>
            
            <el-form-item label="自适应范围">
              <el-switch 
                v-model="colorConfig.autoAdaptRange"
                active-text="开启"
                inactive-text="关闭"
                @change="updateColorPreview"
              />
              <div style="margin-top: 5px;">
                <el-text type="info" size="small">开启后根据实际数据自动调整范围</el-text>
              </div>
            </el-form-item>
            
            <el-form-item :label="getValueLabel('min')">
              <el-input-number 
                v-model="colorConfig.minValue" 
                :min="-10000" 
                :max="10000"
                :precision="2"
                :disabled="colorConfig.autoAdaptRange"
                @change="updateColorPreview"
                style="width: 100%"
              />
            </el-form-item>
            
            <el-form-item :label="getValueLabel('max')">
              <el-input-number 
                v-model="colorConfig.maxValue" 
                :min="-10000" 
                :max="10000"
                :precision="2"
                :disabled="colorConfig.autoAdaptRange"
                @change="updateColorPreview"
                style="width: 100%"
              />
            </el-form-item>
            
            <!-- 线性配色设置 -->
            <div v-show="colorConfig.colorSchemeType === 0">
              <el-divider content-position="left">HSL色相设置</el-divider>
              
              <el-alert title="说明" type="info" :closable="false" style="margin-bottom: 15px;">
                色相范围: 0-360（0=红, 120=绿, 240=蓝）
              </el-alert>
              
              <el-form-item label="起始色相">
                <el-slider 
                  v-model="colorConfig.hslHStart" 
                  :min="0" 
                  :max="360"
                  :marks="{ 0: '红', 120: '绿', 240: '蓝', 360: '红' }"
                  @change="updateColorPreview"
                  style="width: 100%"
                />
                <el-input-number 
                  v-model="colorConfig.hslHStart" 
                  :min="0" 
                  :max="360"
                  @change="updateColorPreview"
                  style="width: 100%; margin-top: 10px;"
                />
              </el-form-item>
              
              <el-form-item label="结束色相">
                <el-slider 
                  v-model="colorConfig.hslHEnd" 
                  :min="0" 
                  :max="360"
                  :marks="{ 0: '红', 120: '绿', 240: '蓝', 360: '红' }"
                  @change="updateColorPreview"
                  style="width: 100%"
                />
                <el-input-number 
                  v-model="colorConfig.hslHEnd" 
                  :min="0" 
                  :max="360"
                  @change="updateColorPreview"
                  style="width: 100%; margin-top: 10px;"
                />
              </el-form-item>
              
              <el-form-item label="饱和度">
                <el-slider 
                  v-model="colorConfig.hslS" 
                  :min="0" 
                  :max="1"
                  :step="0.05"
                  :marks="{ 0: '灰', 0.5: '中等', 1: '鲜艳' }"
                  @change="updateColorPreview"
                  style="width: 100%"
                />
              </el-form-item>
              
              <el-form-item label="亮度">
                <el-slider 
                  v-model="colorConfig.hslL" 
                  :min="0" 
                  :max="1"
                  :step="0.05"
                  :marks="{ 0: '黑', 0.5: '标准', 1: '白' }"
                  @change="updateColorPreview"
                  style="width: 100%"
                />
              </el-form-item>
            </div>
            
            <!-- 分类配色设置 -->
            <div v-show="colorConfig.colorSchemeType === 1">
              <el-divider content-position="left">分类设置</el-divider>
              
              <el-alert title="说明" type="info" :closable="false" style="margin-bottom: 15px;">
                将数值范围分为N个类别，每个类别使用不同颜色
              </el-alert>
              
              <el-form-item label="分类数量">
                <el-input-number 
                  v-model="colorConfig.classCount"
                  :min="2"
                  :max="10"
                  @change="generateClassColors"
                  style="width: 100%"
                />
              </el-form-item>
              
              <el-form-item>
                <a-button type="primary" @click="generateClassColors" block>
                  自动生成分类区间
                </a-button>
              </el-form-item>
            </div>
            
            <!-- 透明通道设置 -->
            <el-divider content-position="left">透明通道</el-divider>
            
            <el-form-item label="启用透明通道">
              <el-switch 
                v-model="colorConfig.filterEnable" 
                @change="updateColorPreview"
                active-text="开启"
                inactive-text="关闭"
              />
              <div style="margin-top: 5px;">
                <el-text type="info" size="small">设置透明度范围，隐藏或半透明显示特定区域</el-text>
              </div>
            </el-form-item>
            
            <div v-show="colorConfig.filterEnable">
              <el-form-item label="透明度系数">
                <el-slider 
                  v-model="colorConfig.filterAlpha" 
                  :min="0" 
                  :max="1"
                  :step="0.05"
                  :marks="{ 0: '透明', 0.5: '半透明', 1: '不透明' }"
                  @change="updateColorPreview"
                  style="width: 100%"
                />
              </el-form-item>
              
              <el-form-item label="透明度最小值">
                <el-input-number 
                  v-model="colorConfig.filterMin" 
                  :min="-10000" 
                  :max="10000"
                  :precision="2"
                  @change="updateColorPreview"
                  style="width: 100%"
                />
              </el-form-item>
              
              <el-form-item label="透明度最大值">
                <el-input-number 
                  v-model="colorConfig.filterMax" 
                  :min="-10000" 
                  :max="10000"
                  :precision="2"
                  @change="updateColorPreview"
                  style="width: 100%"
                />
              </el-form-item>
            </div>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
import { defineComponent, ref, reactive, onMounted, watch, nextTick } from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import axios from 'axios';
import { useMapStore } from "@/store/index.js";
import { renderColorBar } from "@/utils/radartool.js";
import { ApiRadar } from "@/axios/apiRadar.js";
import { showMessage } from "@/utils/tools.js";
import { useI18n } from "vue-i18n";

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
const { t } = useI18n();

// 当前色条类型：displacement（位移）或 scattering（散射）
const currentColorType = ref('displacement');

// 色条配置
const colorConfig = reactive({
  colorSchemeType: 0,      // 0:线性, 1:分类
  minValue: -100,
  maxValue: 100,
  hslHStart: 240,          // 蓝色
  hslHEnd: 0,              // 红色
  hslDirection: 0,         // 色相方向
  hslS: 1.0,               // 饱和度
  hslL: 0.5,               // 亮度
  filterEnable: false,     // 透明通道开关
  filterAlpha: 0.5,
  filterMin: -50,
  filterMax: 50,
  classCount: 5,           // 分类数量
  autoAdaptRange: false,   // 自适应范围
  adaptBufferRatio: 0.1    // 缓冲比例
});

// 获取色条元素ID
const getColorbarId = () => {
  return currentColorType.value === 'displacement' ? 'defoColorbar' : 'scatColorbar';
};

// 获取数值标签
const getValueLabel = (type) => {
  const labels = {
    displacement: { min: '位移最小值(mm)', max: '位移最大值(mm)' },
    scattering: { min: '散射最小值(dB)', max: '散射最大值(dB)' }
  };
  return labels[currentColorType.value][type];
};

// 项目切换
const onProjectChange = async () => {
  console.log('ColorConfig: 项目切换:', store.projectInfo.projectSelected);
  store.radarInfo.projectId = String(store.projectInfo.projectSelected || '');
  await loadColorConfig();
};

// 色条类型切换
const onColorTypeChange = async () => {
  console.log('色条类型切换:', currentColorType.value);
  await loadColorConfig();
};

// 加载色条配置
const loadColorConfig = async () => {
  if (!store.radarInfo.projectId) {
    console.warn('项目ID为空，跳过加载色条配置');
    return;
  }
  
  console.log('加载色条配置:', {
    projectId: store.radarInfo.projectId,
    mode: currentColorType.value
  });
  
  try {
    // 转换mode：displacement → defo, scattering → scat
    const mode = currentColorType.value === 'displacement' ? 'defo' : 'scat';
    const res = await axios.get(
      `${ApiRadar.apiUrl}/api/protocol/colorBar/${store.radarInfo.projectId}/${mode}`
    );
    
    console.log('加载色条配置响应:', res);
    
    if (res.data && res.data.code === 200 && res.data.data) {
      // 更新配置
      const data = res.data.data;
      Object.assign(colorConfig, {
        colorSchemeType: data.colorSchemeType ?? 0,
        minValue: data.minValue ?? -100,
        maxValue: data.maxValue ?? 100,
        hslHStart: data.hslHStart ?? 240,
        hslHEnd: data.hslHEnd ?? 0,
        hslDirection: data.hslDirection ?? 0,
        hslS: data.hslS ?? 1.0,
        hslL: data.hslL ?? 0.5,
        filterEnable: data.filterEnable === 1,
        filterAlpha: data.filterAlpha ?? 0.5,
        filterMin: data.filterMin ?? -50,
        filterMax: data.filterMax ?? 50,
        classCount: data.classCount ?? 5,
        autoAdaptRange: data.autoAdaptRange ?? false,
        adaptBufferRatio: data.adaptBufferRatio ?? 0.1
      });
      
      console.log('色条配置已加载:', colorConfig);
      await nextTick();
      updateColorPreview();
    } else if (res.data && res.data.code === 404) {
      // 未找到配置，使用默认值
      console.log('未找到色条配置，使用默认值');
      setDefaultConfig();
    }
  } catch (err) {
    console.error('加载色条配置失败:', err);
    setDefaultConfig();
  }
};

// 设置默认配置
const setDefaultConfig = () => {
  try {
    console.log('设置默认色条配置，类型:', currentColorType.value);
    
    if (currentColorType.value === 'displacement') {
      Object.assign(colorConfig, {
        colorSchemeType: 0,
        minValue: -100,
        maxValue: 100,
        hslHStart: 240,
        hslHEnd: 0,
        hslDirection: 0,
        hslS: 1.0,
        hslL: 0.5,
        filterEnable: false,
        filterAlpha: 0.5,
        filterMin: -50,
        filterMax: 50,
        classCount: 5,
        autoAdaptRange: false,
        adaptBufferRatio: 0.1
      });
    } else {
      Object.assign(colorConfig, {
        colorSchemeType: 0,
        minValue: -50,
        maxValue: 10,
        hslHStart: 240,
        hslHEnd: 120,
        hslDirection: 0,
        hslS: 1.0,
        hslL: 0.5,
        filterEnable: false,
        filterAlpha: 0.5,
        filterMin: -30,
        filterMax: 5,
        classCount: 5,
        autoAdaptRange: false,
        adaptBufferRatio: 0.1
      });
    }
    
    nextTick(() => {
      try {
        updateColorPreview();
      } catch (err) {
        console.error('更新预览失败:', err);
      }
    });
  } catch (err) {
    console.error('设置默认配置失败:', err);
  }
};

// 生成分类颜色
const generateClassColors = () => {
  console.log('自动生成分类颜色，数量:', colorConfig.classCount);
  updateColorPreview();
  showMessage(`已生成 ${colorConfig.classCount} 个分类`, 'success');
};

// 更新色条预览
const updateColorPreview = () => {
  try {
    const elementId = getColorbarId();
    const label = currentColorType.value === 'displacement' ? '位移(mm)' : '散射强度(dB)';
    
    setTimeout(() => {
      try {
        const element = document.getElementById(elementId);
        if (!element) {
          console.warn('色条元素未找到:', elementId);
          return;
        }
        
        // 生成颜色数组
        const colorArray = generateColorArray();
        
        const renderData = {
          minValue: colorConfig.minValue,
          maxValue: colorConfig.maxValue,
          hslHStart: colorConfig.hslHStart,
          hslHEnd: colorConfig.hslHEnd,
          colorArray: colorArray
        };
        
        renderColorBar(renderData, elementId, label);
      } catch (err) {
        console.error('渲染色条失败:', err);
      }
    }, 100);
  } catch (err) {
    console.error('updateColorPreview错误:', err);
  }
};

// 生成颜色数组
const generateColorArray = () => {
  try {
    const colors = [];
    const steps = 10;
    
    if (colorConfig.colorSchemeType === 0) {
      // 线性配色
      for (let i = 0; i <= steps; i++) {
        const ratio = i / steps;
        const hue = colorConfig.hslHStart + (colorConfig.hslHEnd - colorConfig.hslHStart) * ratio;
        colors.push(hslToHex(hue, colorConfig.hslS * 100, colorConfig.hslL * 100));
      }
    } else {
      // 分类配色：生成均匀分布的颜色
      const count = colorConfig.classCount || 5;
      for (let i = 0; i < count; i++) {
        const ratio = count > 1 ? i / (count - 1) : 0;
        const hue = colorConfig.hslHStart + (colorConfig.hslHEnd - colorConfig.hslHStart) * ratio;
        colors.push(hslToHex(hue, 100, 50));
      }
    }
    
    return colors.length > 0 ? colors : ['#0000FF', '#00FFFF', '#00FF00', '#FFFF00', '#FF0000'];
  } catch (err) {
    console.error('生成颜色数组失败:', err);
    return ['#0000FF', '#00FFFF', '#00FF00', '#FFFF00', '#FF0000'];
  }
};

// HSL转HEX
const hslToHex = (h, s, l) => {
  try {
    // 规范化输入值
    h = h % 360;
    if (h < 0) h += 360;
    s = Math.max(0, Math.min(100, s));
    l = Math.max(0, Math.min(100, l));
    
    s /= 100;
    l /= 100;
    const c = (1 - Math.abs(2 * l - 1)) * s;
    const x = c * (1 - Math.abs((h / 60) % 2 - 1));
    const m = l - c / 2;
    let r = 0, g = 0, b = 0;

    if (0 <= h && h < 60) {
      r = c; g = x; b = 0;
    } else if (60 <= h && h < 120) {
      r = x; g = c; b = 0;
    } else if (120 <= h && h < 180) {
      r = 0; g = c; b = x;
    } else if (180 <= h && h < 240) {
      r = 0; g = x; b = c;
    } else if (240 <= h && h < 300) {
      r = x; g = 0; b = c;
    } else if (300 <= h && h < 360) {
      r = c; g = 0; b = x;
    }

    const toHex = (val) => {
      const hex = Math.round((val + m) * 255).toString(16);
      return hex.length === 1 ? '0' + hex : hex;
    };

    return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
  } catch (err) {
    console.error('HSL转HEX失败:', err, { h, s, l });
    return '#808080'; // 返回灰色作为后备
  }
};

// 保存色条配置
const saveColorConfig = async () => {
  console.log('保存色条配置');
  
  if (!store.radarInfo.projectId) {
    showMessage('请先选择项目', 'error');
    return;
  }
  
  // 转换mode
  const mode = currentColorType.value === 'displacement' ? 'defo' : 'scat';
  
  const params = {
    projectId: store.radarInfo.projectId,
    mode: mode,
    colorSchemeType: colorConfig.colorSchemeType,
    minValue: colorConfig.minValue,
    maxValue: colorConfig.maxValue,
    hslHStart: String(colorConfig.hslHStart),
    hslHEnd: String(colorConfig.hslHEnd),
    hslDirection: colorConfig.hslDirection,
    hslS: colorConfig.hslS,
    hslL: colorConfig.hslL,
    filterEnable: colorConfig.filterEnable ? 1 : 0,
    filterAlpha: String(colorConfig.filterAlpha),
    filterMin: String(colorConfig.filterMin),
    filterMax: String(colorConfig.filterMax),
    classCount: colorConfig.classCount,
    autoAdaptRange: colorConfig.autoAdaptRange,
    adaptBufferRatio: colorConfig.adaptBufferRatio
  };
  
  console.log('准备保存的色条配置:', params);
  
  try {
    const res = await axios.post(
      `${ApiRadar.apiUrl}/api/protocol/update/colorBar`,
      params
    );
    
    console.log('保存色条配置响应:', res);
    
    if (res.data && res.data.code === 200) {
      showMessage('色条配置保存成功');
      
      // 记录日志
      await ApiRadar.AddRadarLog(
        `修改${currentColorType.value === 'displacement' ? '位移' : '散射'}色条配置`,
        store.sysinfo.config.username,
        store.sysinfo.config.address,
        store.sysinfo.config.projectCode,
        store.sysinfo.config.shortName
      );
    } else {
      showMessage(res.data?.message || '保存失败', 'error');
    }
  } catch (err) {
    console.error('保存色条配置失败:', err);
    showMessage('保存失败: ' + err.message, 'error');
  }
};

// 页面挂载
onMounted(async () => {
  console.log('=== ColorConfig组件已挂载 ===');
  console.log('visible prop:', props.visible);
  console.log('store.toolbarcontent:', store.toolbarcontent);
  console.log('store.projectInfo.projectSelected:', store.projectInfo.projectSelected);
  console.log('store.projectInfo.projectData:', store.projectInfo.projectData);
  
  // 先设置默认配置
  setDefaultConfig();
  
  // 设置projectId
  if (store.projectInfo.projectSelected) {
    store.radarInfo.projectId = String(store.projectInfo.projectSelected);
    // 异步加载配置
    await nextTick();
    await loadColorConfig();
  } else if (store.projectInfo.projectData && store.projectInfo.projectData.length > 0) {
    // 默认选择第一个项目
    store.projectInfo.projectSelected = store.projectInfo.projectData[0].projectId;
    store.radarInfo.projectId = String(store.projectInfo.projectSelected);
    await loadColorConfig();
  } else {
    console.warn('没有可用的项目数据');
  }
  
  console.log('=== ColorConfig组件初始化完成 ===');
});

// 监听项目切换
watch(() => store.projectInfo.projectSelected, async (newVal, oldVal) => {
  if (newVal && newVal !== oldVal) {
    console.log('ColorConfig: 监听到项目切换', oldVal, '→', newVal);
    store.radarInfo.projectId = String(newVal);
    await loadColorConfig();
  }
});
</script>

<style scoped>
#idcolorconfig {
  height: 100%;
  width: 100%;
  overflow-y: auto;
}

.colorbar-preview {
  height: 50px;
  width: 100%;
  margin-bottom: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

:deep(.el-slider__marks-text) {
  font-size: 10px;
}
</style>
