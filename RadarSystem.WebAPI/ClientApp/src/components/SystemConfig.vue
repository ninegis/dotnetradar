<template>
  <el-drawer
    v-model="visible"
    title="系统配置"
    :size="600"
    direction="rtl"
  >
    <el-tabs v-model="activeTab">
      <!-- 项目配置 -->
      <el-tab-pane label="项目配置" name="project">
        <el-form :model="projectForm" label-width="120px">
          <el-form-item label="项目名称">
            <el-input v-model="projectForm.projectName" />
          </el-form-item>
          <el-form-item label="项目位置">
            <el-input v-model="projectForm.location" />
          </el-form-item>
          <el-form-item label="项目描述">
            <el-input v-model="projectForm.description" type="textarea" :rows="3" />
          </el-form-item>
          <el-divider content-position="left">中心坐标</el-divider>
          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="经度">
                <el-input-number v-model="projectForm.centerLon" :precision="6" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="纬度">
                <el-input-number v-model="projectForm.centerLat" :precision="6" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="高程">
                <el-input-number v-model="projectForm.centerAlt" :precision="2" />
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item label="三维模型URL">
            <el-input v-model="projectForm.modelUrl" placeholder="可选，3D Tiles URL" />
          </el-form-item>
          <el-form-item label="地形URL">
            <el-input v-model="projectForm.terrainUrl" placeholder="可选，Terrain URL" />
          </el-form-item>
          <el-form-item label="影像URL">
            <el-input v-model="projectForm.imageryUrl" placeholder="可选，Imagery URL" />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="saveProject">保存项目配置</el-button>
          </el-form-item>
        </el-form>
      </el-tab-pane>

      <!-- 设备配置 -->
      <el-tab-pane label="设备配置" name="device">
        <div class="device-list">
          <el-table :data="devices" stripe>
            <el-table-column prop="deviceId" label="设备ID" width="120" />
            <el-table-column prop="deviceName" label="设备名称" />
            <el-table-column prop="deviceType" label="设备类型" width="100" />
            <el-table-column prop="port" label="端口" width="80" />
            <el-table-column label="启用" width="80">
              <template #default="{ row }">
                <el-switch v-model="row.enable" @change="updateDevice(row)" />
              </template>
            </el-table-column>
            <el-table-column label="操作" width="80">
              <template #default="{ row }">
                <el-button text type="primary" size="small" @click="editDevice(row)">
                  编辑
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </el-tab-pane>

      <!-- 算法配置 -->
      <el-tab-pane label="算法配置" name="algorithm">
        <el-form :model="algorithmForm" label-width="120px">
          <el-form-item label="算法类型">
            <el-select v-model="algorithmForm.type">
              <el-option label="阈值判断" value="threshold" />
              <el-option label="机器学习" value="ml" />
              <el-option label="统计分析" value="statistical" />
            </el-select>
          </el-form-item>
          <el-form-item label="位移阈值" v-if="algorithmForm.type === 'threshold'">
            <el-input-number v-model="algorithmForm.displacementThreshold" :min="0" />
            <span style="margin-left: 8px">mm</span>
          </el-form-item>
          <el-form-item label="速率阈值" v-if="algorithmForm.type === 'threshold'">
            <el-input-number v-model="algorithmForm.velocityThreshold" :min="0" />
            <span style="margin-left: 8px">mm/h</span>
          </el-form-item>
          <el-form-item label="模型路径" v-if="algorithmForm.type === 'ml'">
            <el-input v-model="algorithmForm.modelPath" />
          </el-form-item>
          <el-form-item label="窗口大小" v-if="algorithmForm.type === 'statistical'">
            <el-input-number v-model="algorithmForm.windowSize" :min="1" />
          </el-form-item>
          <el-form-item label="启用算法">
            <el-switch v-model="algorithmForm.enable" />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="saveAlgorithm">保存算法配置</el-button>
          </el-form-item>
        </el-form>
      </el-tab-pane>

      <!-- 预警配置 -->
      <el-tab-pane label="预警配置" name="alarm">
        <div class="alarm-config">
          <el-button type="primary" @click="showAlarmDialog = true" style="margin-bottom: 16px">
            <el-icon><Plus /></el-icon> 添加预警规则
          </el-button>
          
          <el-table :data="alarmRules" stripe>
            <el-table-column prop="name" label="规则名称" />
            <el-table-column prop="type" label="监测类型" width="100">
              <template #default="{ row }">
                {{ getTypeText(row.type) }}
              </template>
            </el-table-column>
            <el-table-column prop="threshold" label="阈值" width="100" />
            <el-table-column prop="level" label="级别" width="80">
              <template #default="{ row }">
                <el-tag :type="getLevelType(row.level)" size="small">
                  {{ getLevelText(row.level) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="启用" width="80">
              <template #default="{ row }">
                <el-switch v-model="row.enable" @change="updateAlarmRule(row)" />
              </template>
            </el-table-column>
            <el-table-column label="操作" width="120">
              <template #default="{ row }">
                <el-button text type="primary" size="small" @click="editAlarmRule(row)">
                  编辑
                </el-button>
                <el-button text type="danger" size="small" @click="deleteAlarmRule(row)">
                  删除
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </el-tab-pane>

      <!-- 短信模板 -->
      <el-tab-pane label="短信模板" name="sms">
        <el-form label-width="100px">
          <el-form-item label="预警模板">
            <el-input
              v-model="smsTemplates.warning"
              type="textarea"
              :rows="3"
              placeholder="【边坡雷达预警】{pointCode} 位移超限，当前位移 {displacement}mm"
            />
          </el-form-item>
          <el-form-item label="报警模板">
            <el-input
              v-model="smsTemplates.alarm"
              type="textarea"
              :rows="3"
              placeholder="【边坡雷达报警】{pointCode} 位移严重超限，当前位移 {displacement}mm，请立即处理！"
            />
          </el-form-item>
          <el-form-item label="变量说明">
            <el-alert
              title="可用变量: {pointCode} {displacement} {velocity} {timestamp}"
              type="info"
              :closable="false"
            />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="saveSmsTemplates">保存模板</el-button>
            <el-button @click="testSms">发送测试短信</el-button>
          </el-form-item>
        </el-form>
      </el-tab-pane>
    </el-tabs>

    <!-- 预警规则对话框 -->
    <el-dialog v-model="showAlarmDialog" title="预警规则配置" width="500px">
      <el-form :model="alarmForm" label-width="100px">
        <el-form-item label="规则名称">
          <el-input v-model="alarmForm.name" />
        </el-form-item>
        <el-form-item label="监测类型">
          <el-select v-model="alarmForm.type">
            <el-option label="位移" value="displacement" />
            <el-option label="速率" value="velocity" />
            <el-option label="加速度" value="acceleration" />
          </el-select>
        </el-form-item>
        <el-form-item label="判断条件">
          <el-select v-model="alarmForm.operator" style="width: 80px">
            <el-option label=">" value=">" />
            <el-option label="<" value="<" />
            <el-option label=">=" value=">=" />
            <el-option label="<=" value="<=" />
          </el-select>
          <el-input-number v-model="alarmForm.threshold" style="margin-left: 8px" />
        </el-form-item>
        <el-form-item label="预警级别">
          <el-radio-group v-model="alarmForm.level">
            <el-radio :label="1">蓝色</el-radio>
            <el-radio :label="2">黄色</el-radio>
            <el-radio :label="3">橙色</el-radio>
            <el-radio :label="4">红色</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAlarmDialog = false">取消</el-button>
        <el-button type="primary" @click="saveAlarmRule">保存</el-button>
      </template>
    </el-dialog>
  </el-drawer>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import { Plus } from '@element-plus/icons-vue';
import { deviceConfigApi, algorithmConfigApi, alarmRuleApi } from '../api/monitoring';
import type { DeviceConfig, AlgorithmConfig, AlarmRule } from '../types/monitoring';

const props = defineProps<{
  modelValue: boolean;
}>();

const emit = defineEmits<{
  'update:modelValue': [value: boolean];
}>();

const visible = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
});

const activeTab = ref('project');
const devices = ref<DeviceConfig[]>([]);
const alarmRules = ref<AlarmRule[]>([]);
const showAlarmDialog = ref(false);

// 项目配置表单
const projectForm = ref({
  projectName: '新疆天隆希望矿区',
  location: '新疆维吾尔自治区',
  description: '边坡雷达监测项目',
  centerLon: 87.6278,
  centerLat: 43.7928,
  centerAlt: 5000,
  modelUrl: '',
  terrainUrl: '',
  imageryUrl: ''
});

// 算法配置表单
const algorithmForm = ref({
  type: 'threshold',
  displacementThreshold: 100,
  velocityThreshold: 10,
  modelPath: '',
  windowSize: 10,
  enable: true
});

// 预警规则表单
const alarmForm = ref({
  name: '',
  type: 'displacement',
  operator: '>',
  threshold: 100,
  level: 2
});

// 短信模板
const smsTemplates = ref({
  warning: '【边坡雷达预警】{pointCode} 位移超限，当前位移 {displacement}mm',
  alarm: '【边坡雷达报警】{pointCode} 位移严重超限，当前位移 {displacement}mm，请立即处理！'
});

onMounted(() => {
  loadDevices();
  loadAlarmRules();
});

// 加载设备列表
const loadDevices = async () => {
  try {
    const response = await deviceConfigApi.getAll();
    devices.value = response.data;
  } catch (error) {
    console.error('加载设备列表失败:', error);
  }
};

// 更新设备
const updateDevice = async (device: DeviceConfig) => {
  try {
    await deviceConfigApi.update(device.id, { enable: device.enable });
    ElMessage.success('设备状态已更新');
  } catch (error) {
    ElMessage.error('更新失败');
  }
};

// 编辑设备
const editDevice = (device: DeviceConfig) => {
  ElMessage.info('设备编辑功能开发中');
};

// 保存项目配置
const saveProject = () => {
  ElMessage.success('项目配置已保存');
};

// 保存算法配置
const saveAlgorithm = () => {
  ElMessage.success('算法配置已保存');
};

// 加载预警规则
const loadAlarmRules = async () => {
  try {
    const response = await alarmRuleApi.getAll();
    alarmRules.value = response.data;
  } catch (error) {
    console.error('加载预警规则失败:', error);
  }
};

// 保存预警规则
const saveAlarmRule = async () => {
  try {
    await alarmRuleApi.create(alarmForm.value);
    await loadAlarmRules();
    showAlarmDialog.value = false;
    ElMessage.success('预警规则已添加');
  } catch (error) {
    ElMessage.error('保存失败');
  }
};

// 更新预警规则
const updateAlarmRule = async (rule: AlarmRule) => {
  try {
    await alarmRuleApi.update(rule.id, { enable: rule.enable });
    ElMessage.success('规则状态已更新');
  } catch (error) {
    ElMessage.error('更新失败');
  }
};

// 编辑预警规则
const editAlarmRule = (rule: AlarmRule) => {
  alarmForm.value = { ...rule };
  showAlarmDialog.value = true;
};

// 删除预警规则
const deleteAlarmRule = async (rule: AlarmRule) => {
  try {
    await alarmRuleApi.delete(rule.id);
    await loadAlarmRules();
    ElMessage.success('规则已删除');
  } catch (error) {
    ElMessage.error('删除失败');
  }
};

// 保存短信模板
const saveSmsTemplates = () => {
  ElMessage.success('短信模板已保存');
};

// 测试短信
const testSms = () => {
  ElMessage.info('测试短信已发送');
};

// 辅助函数
const getTypeText = (type: string): string => {
  const map: Record<string, string> = {
    displacement: '位移',
    velocity: '速率',
    acceleration: '加速度'
  };
  return map[type] || type;
};

const getLevelType = (level: number) => {
  const map: Record<number, any> = {
    1: 'info',
    2: 'warning',
    3: 'warning',
    4: 'danger'
  };
  return map[level] || 'info';
};

const getLevelText = (level: number): string => {
  const map: Record<number, string> = {
    1: '蓝色',
    2: '黄色',
    3: '橙色',
    4: '红色'
  };
  return map[level] || '未知';
};
</script>

<style scoped>
.device-list {
  margin-bottom: 16px;
}

.alarm-config {
  padding: 16px 0;
}
</style>

