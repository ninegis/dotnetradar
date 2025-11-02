<template>
  <section id="idimagegenerate" v-show="visible" class="">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <svg width="1em"  height="1em" fill="currentColor" t="1701093043138" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5126"><path d="M512 69.963c248.05 0 445.217 197.167 445.217 445.217S760.05 960.398 512 960.398 66.783 763.23 66.783 515.18 263.95 69.963 512 69.963m0-63.603C232.15 6.36 3.18 235.33 3.18 515.18 3.18 795.031 232.15 1024 512 1024s508.82-228.969 508.82-508.82C1020.82 235.33 791.85 6.36 512 6.36z" fill="" p-id="5127"></path><path d="M512 432.497c-38.161 0-63.602 25.44-63.602 57.242v273.49c0 31.802 25.44 57.243 63.602 57.243 38.161 0 63.602-25.44 63.602-57.242V489.74c0-31.802-25.44-57.243-63.602-57.243z m0-95.404c38.161 0 63.602-25.44 63.602-63.602 0-38.162-25.44-63.603-63.602-63.603-38.161 0-63.602 25.441-63.602 63.603 0 38.161 25.44 63.602 63.602 63.602z" fill="" p-id="5128"></path></svg>
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;{{$t('legend.radar')+$t('common.image')+$t('common.generate')}}</span>
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
            
            <el-divider content-position="left">雷达图像生成配置</el-divider>
            
            <el-text class="mx-1" type="success">{{$t('decoration.imageDefo')+$t('common.image')+$t('common.generate')}}</el-text>
            <el-form-item :label="$t('common.generate')+$t('common.type')">
              <el-select v-model="store.radarInfo.imageAnalysisConfig['genImageType']" placeholder="选择生成方式">
                <el-option :key="$t('params.frameExtract')" :label="$t('params.frameExtract')" value="01"/>
                <el-option :key="$t('common.timeInterval')" :label="$t('common.timeInterval')" value="02"/>
              </el-select>
            </el-form-item>
            <div v-show="store.radarInfo.imageAnalysisConfig['genImageType']==='02'">
              <el-text class="mx-1" type="warning">{{(store.sysinfo.config.language==='0'?'以下单位为"分钟"':'The following units are "minutes"')}}</el-text>
              <el-form-item :label="$t('decoration.imageDefo')+$t('common.image')+$t('common.generate')">
                <el-input v-model.number="store.radarInfo.imageAnalysisConfig['followDefoInterval']"/>
              </el-form-item>
              <el-form-item :label="$t('decoration.imageScat')+$t('common.image')+$t('common.generate')">
                <el-input v-model.number="store.radarInfo.imageAnalysisConfig['scatInterval']"/>
              </el-form-item>
            </div>
            <div v-show="store.radarInfo.imageAnalysisConfig['genImageType']==='01'">
              <el-text class="mx-1" type="warning">{{(store.sysinfo.config.language==='0'?'以下设置转动几圈生成1次图像':'Rotate the following settings a few times to generate an image once')}}</el-text>
              <el-form-item :label="$t('decoration.imageDefo')+$t('common.image')+$t('common.generate')">
                <el-select v-model="store.radarInfo.imageAnalysisConfig['followDefoNumber']" :placeholder="$t('common.select')">
                  <el-option :key="$t('common.donot')+$t('common.generate')" :label="$t('common.donot')+$t('common.generate')" value="0"/>
                  <el-option key="1" :label="'1'+$t('common.circle')" :value="1"/>
                  <el-option key="2" :label="'2'+$t('common.circle')" :value="2"/>
                  <el-option key="5" :label="'5'+$t('common.circle')" :value="5"/>
                  <el-option key="10" :label="'10'+$t('common.circle')" :value="10"/>
                  <el-option key="20" :label="'20'+$t('common.circle')" :value="20"/>
                </el-select>
              </el-form-item>
              <el-form-item :label="$t('decoration.imageScat')+$t('common.image')+$t('common.generate')">
                <el-select v-model="store.radarInfo.imageAnalysisConfig['scatNumber']" placeholder="选择">
                  <el-option :key="$t('common.donot')+$t('common.generate')" :label="$t('common.donot')+$t('common.generate')" value="0"/>
                  <el-option key="1" :label="'1'+$t('common.circle')" :value="1"/>
                  <el-option key="2" :label="'2'+$t('common.circle')" :value="2"/>
                  <el-option key="5" :label="'5'+$t('common.circle')" :value="5"/>
                  <el-option key="10" :label="'10'+$t('common.circle')" :value="10"/>
                  <el-option key="20" :label="'20'+$t('common.circle')" :value="20"/>
                </el-select>
              </el-form-item>
            </div>
<!--            <el-form-item label="显示区域">-->
<!--              <el-switch v-model="store.radarInfo.imageAnalysisConfig['enableAlarmArea']" />-->
<!--            </el-form-item>-->
<!--            <el-divider />-->
<!--            <el-text class="mx-1" type="success">速度图像生成配置：</el-text>-->
<!--            <el-form-item label="速度图像生成时间">-->
<!--              <el-select-->
<!--                  v-model="store.radarInfo.imageDiffAnalysisConfig['differenceTimeUnit']"-->
<!--                  multiple-->
<!--                  placeholder="选择一个或多个时间指标"-->
<!--              >-->
<!--                <el-option key="item.value" label="30分钟" value="01"/>-->
<!--                <el-option key="item.value" label="1小时" value="02"/>-->
<!--                <el-option key="item.value" label="2小时" value="03"/>-->
<!--                <el-option key="item.value" label="4小时" value="04"/>-->
<!--                <el-option key="item.value" label="24小时" value="05"/>-->
<!--                <el-option key="item.value" label="3天" value="10"/>-->
<!--                <el-option key="item.value" label="7天" value="06"/>-->
<!--              </el-select>-->
<!--            </el-form-item>-->
<!--            <el-form-item label="是否开启速度图像自动生成">-->
<!--              <el-switch v-model="store.radarInfo.imageDiffAnalysisConfig['genDifference']"/>-->
<!--            </el-form-item>-->
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
// sloperadar-cesium / 2023-11-28 / 18:21:16 / QingQiangJia
/*-- imports --*/
import {defineComponent, ref, onMounted, computed, reactive, watch} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon from '@ant-design/icons-vue';
import axios from 'axios';
import {useMapStore} from "@/store/index.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {showMessage} from "@/utils/tools.js";
import {useI18n} from "vue-i18n";

/*-- name --*/
defineComponent({
  name: "imagegenerate",
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

/*-- methods --*/

// 项目切换
const onProjectChange = async () => {
  console.log('ImageGenerate: 项目切换:', store.projectInfo.projectSelected);
  store.radarInfo.projectId = String(store.projectInfo.projectSelected || '');
  
  // 加载该项目的图像生成配置
  await loadImageConfig();
};

// 加载图像配置
const loadImageConfig = async () => {
  if (!store.radarInfo.projectId) {
    console.warn('项目ID为空，跳过加载图像配置');
    return;
  }
  
  try {
    // ✅ 修复：正确的接口路径
    const res = await axios.get(ApiRadar.apiUrl + '/api/protocol/project/imageAnalysisConfig/' + store.radarInfo.projectId);
    console.log('加载图像配置响应:', res);
    
    if (res.data && res.data.code === 200 && res.data.data) {
      // ✅ 更新store（找到配置）
      const data = res.data.data;
      
      // ✅ 使用Object.assign强制更新响应式对象
      Object.assign(store.radarInfo.imageAnalysisConfig, {
        genImageType: String(data.genImageType || '01'),
        followDefoInterval: Number(data.defoInterval || 60),
        scatInterval: Number(data.scatInterval || 60),
        followDefoNumber: Number(data.defoNumber || 10),
        scatNumber: Number(data.scatNumber || 10)
      });
      
      console.log('图像配置已加载:', {
        genImageType: store.radarInfo.imageAnalysisConfig.genImageType,
        followDefoInterval: store.radarInfo.imageAnalysisConfig.followDefoInterval,
        scatInterval: store.radarInfo.imageAnalysisConfig.scatInterval,
        followDefoNumber: store.radarInfo.imageAnalysisConfig.followDefoNumber,
        scatNumber: store.radarInfo.imageAnalysisConfig.scatNumber
      });
      
      // ✅ 额外验证：确保页面上的绑定值
      console.log('验证v-model绑定值:');
      console.log('  genImageType:', store.radarInfo.imageAnalysisConfig['genImageType']);
      console.log('  followDefoInterval:', store.radarInfo.imageAnalysisConfig['followDefoInterval']);
      console.log('  scatInterval:', store.radarInfo.imageAnalysisConfig['scatInterval']);
      console.log('  followDefoNumber:', store.radarInfo.imageAnalysisConfig['followDefoNumber']);
      console.log('  scatNumber:', store.radarInfo.imageAnalysisConfig['scatNumber']);
    } else if (res.data && res.data.code === 404) {
      // ✅ 未找到配置，使用默认值
      console.log('该项目没有图像配置，使用默认值');
      Object.assign(store.radarInfo.imageAnalysisConfig, {
        genImageType: '01',
        followDefoInterval: 60,
        scatInterval: 60,
        followDefoNumber: 10,
        scatNumber: 10
      });
    }
  } catch (err) {
    console.error('加载图像配置失败:', err);
    // 使用默认值
    store.radarInfo.imageAnalysisConfig = {
      genImageType: '01',
      followDefoInterval: 60,
      scatInterval: 60,
      followDefoNumber: 10,
      scatNumber: 10
    };
  }
};

// 保存配置
const commitUpdate = async () => {
  console.log('保存图像生成配置');
  
  if (!store.radarInfo.projectId) {
    showMessage('请先选择项目', 'error');
    return;
  }
  
  console.log('当前配置:', store.radarInfo.imageAnalysisConfig);
  
  try {
    const res = await ApiRadar.updateImageAnalysisConfig(
      store.radarInfo.projectId,
      store.radarInfo.imageDiffAnalysisConfig,
      store.radarInfo.imageAnalysisConfig
    );
    
    console.log('保存图像配置响应:', res);
    
    if (res.data && res.data.code === 200) {
      showMessage('图像生成配置保存成功');
      
      // 记录日志
      await ApiRadar.AddRadarLog(
        "修改雷达图像生成配置",
        store.sysinfo.config.username,
        store.sysinfo.address,
        store.sysinfo.config.projectCode,
        store.sysinfo.config.shortName
      );
    } else {
      showMessage(res.data?.message || '保存失败', 'error');
    }
  } catch (err) {
    console.error('保存图像配置失败:', err);
    showMessage('保存失败: ' + err.message, 'error');
  }
};

/*-- events --*/
onMounted(async () => {
  console.log('ImageGenerate.onMounted');
  console.log('当前projectSelected:', store.projectInfo.projectSelected);
  console.log('当前imageAnalysisConfig:', store.radarInfo.imageAnalysisConfig);
  
  // ✅ 先初始化默认配置（确保对象存在）
  if (!store.radarInfo.imageAnalysisConfig) {
    store.radarInfo.imageAnalysisConfig = {
      genImageType: '01',
      followDefoInterval: 60,
      scatInterval: 60,
      followDefoNumber: 10,
      scatNumber: 10
    };
    console.log('初始化默认配置');
  }
  
  // ✅ 设置projectId
  if (store.projectInfo.projectSelected) {
    store.radarInfo.projectId = String(store.projectInfo.projectSelected);
    console.log('开始加载图像配置...');
    // ✅ 加载配置（会覆盖默认值）
    await loadImageConfig();
    console.log('加载完成，当前配置:', store.radarInfo.imageAnalysisConfig);
  } else if (store.projectInfo.projectData && store.projectInfo.projectData.length > 0) {
    // ✅ 如果没有选中项目，默认选择第一个
    console.log('没有选中项目，默认选择第一个');
    store.projectInfo.projectSelected = store.projectInfo.projectData[0].projectId;
    store.radarInfo.projectId = String(store.projectInfo.projectSelected);
    await loadImageConfig();
  }
});

// ✅ 监听项目切换
watch(() => store.projectInfo.projectSelected, async (newVal, oldVal) => {
  if (newVal && newVal !== oldVal) {
    console.log('ImageGenerate: 监听到项目切换', oldVal, '→', newVal);
    store.radarInfo.projectId = String(newVal);
    await loadImageConfig();
  }
});
</script>

<style scoped>
#idimagegenerate {
  height: 100%;
  width: 100%;
}
</style>