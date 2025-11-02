<template>
  <section id="idprojectedit" v-show="visible">
    <DragContainer :dragger-width="store.dragContainer.width">
      <template v-slot:dragger-header>
        <Icon>
          <template #component>
            <EditOutlined />
          </template>
        </Icon>
        <span class="dragger-header">&nbsp;&nbsp;&nbsp;修改项目</span>
      </template>
      <template v-slot:dragger-content>
        <a-row type="flex" :gutter="16" align="middle">
          <a-button class="executeBtn" type="primary" ghost block @click="commitUpdate">保存修改</a-button>
        </a-row>
        <a-row class="custom-row">
          <el-form>
            <el-form-item label="项目ID">
              <el-input v-model="form.projectId" disabled placeholder="项目ID不可修改"/>
            </el-form-item>
            
            <el-form-item label="项目名称">
              <el-input v-model="form.projectName" placeholder="请输入项目名称"/>
            </el-form-item>
            
            <el-form-item label="项目描述">
              <el-input v-model="form.description" type="textarea" :rows="3" placeholder="请输入项目描述"/>
            </el-form-item>
            
            <el-form-item label="项目状态">
              <el-select v-model="form.status" placeholder="Active">
                <el-option label="活跃" value="Active"/>
                <el-option label="暂停" value="Paused"/>
                <el-option label="已完成" value="Completed"/>
              </el-select>
            </el-form-item>
            
            <el-divider>联系信息</el-divider>
            
            <el-form-item label="联系人">
              <el-input v-model="form.contactPerson" placeholder="请输入联系人姓名"/>
            </el-form-item>
            
            <el-form-item label="联系电话">
              <el-input v-model="form.contactPhone" placeholder="请输入联系电话"/>
            </el-form-item>
            
            <el-form-item label="联系邮箱">
              <el-input v-model="form.contactEmail" placeholder="请输入联系邮箱"/>
            </el-form-item>
            
            <el-divider>地理位置</el-divider>
            
            <el-form-item label="经度">
              <el-input-number v-model="form.longitude" :precision="6" :step="0.000001" style="width: 100%" placeholder="120.6"/>
            </el-form-item>
            
            <el-form-item label="纬度">
              <el-input-number v-model="form.latitude" :precision="6" :step="0.000001" style="width: 100%" placeholder="31.3"/>
            </el-form-item>
            
            <el-form-item label="高度(m)">
              <el-input-number v-model="form.elevation" :precision="2" :step="0.1" style="width: 100%" placeholder="0"/>
            </el-form-item>
            
            <el-form-item label="位置描述">
              <el-input v-model="form.location" placeholder="如：苏州工业园区"/>
            </el-form-item>
          </el-form>
        </a-row>
      </template>
    </DragContainer>
  </section>
</template>

<script setup>
import {defineComponent, ref, onMounted, reactive, watch} from 'vue';
import DragContainer from "@/components/DragContainer/DragContainer.vue";
import Icon, {EditOutlined} from '@ant-design/icons-vue';
import {useMapStore} from "@/store/index.js";
import {ApiRadar} from "@/axios/apiRadar.js";
import {showMessage} from "@/utils/tools.js";
import axios from 'axios';

defineComponent({
  name: "ProjectEdit",
});

const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
  projectId: {
    type: String,
    required: false,
    default: '',
  },
});

const form = reactive({
  projectId: '',
  projectName: '',
  description: '',
  location: '',
  status: 'Active',
  contactPerson: '',
  contactPhone: '',
  contactEmail: '',
  longitude: 0,
  latitude: 0,
  elevation: 0,
});

const store = useMapStore();

// 加载项目详情
const loadProjectDetail = async () => {
  if (!store.projectInfo.projectSelected) return;
  
  try {
    const res = await axios.get(`${ApiRadar.apiUrl}/api/Project/${store.projectInfo.projectSelected}`);
    if (res.data.code === 200 && res.data.data) {
      const project = res.data.data;
      Object.assign(form, {
        projectId: project.projectId,
        projectName: project.projectName,
        description: project.description || '',
        location: project.location || '',
        status: project.status || 'Active',
        contactPerson: project.contactPerson || '',
        contactPhone: project.contactPhone || '',
        contactEmail: project.contactEmail || '',
        longitude: project.longitude || 0,
        latitude: project.latitude || 0,
        elevation: project.elevation || 0,
      });
    }
  } catch (error) {
    console.error('加载项目详情失败:', error);
    showMessage('加载项目详情失败: ' + (error.response?.data?.message || error.message), 'error');
  }
};

const commitUpdate = async () => {
  if (!form.projectName) {
    showMessage('项目名称不能为空', 'warning');
    return;
  }
  
  try {
    const res = await ApiRadar.UpdateProject(form.projectId, {
      projectName: form.projectName,
      projectDescribe: form.description,
      contact: form.contactPerson,
      phone: form.contactPhone,
      email: form.contactEmail,
      lon: form.longitude,
      lat: form.latitude,
      alt: form.elevation
    });
    
    console.log('修改项目响应:', res);
    if (res.data.code === 200) {
      showMessage('项目修改成功！', 'success');
      ApiRadar.AddRadarLog("修改项目" + form.projectName, store.sysinfo.config.username, store.sysinfo.address, store.sysinfo.config.projectCode, store.sysinfo.config.shortName).then();
      // 重新加载项目列表
      ApiRadar.getRadarData().then(res => {
        store.projectInfo.projectData = res.data.data;
        store.toolbarcontent = 'projectConfig';
      });
    } else {
      showMessage(res.data.message || '修改项目失败', 'error');
    }
  } catch (error) {
    console.error('修改项目失败:', error);
    showMessage('修改项目失败: ' + (error.response?.data?.message || error.message || '未知错误'), 'error');
  }
};

onMounted(() => {
  loadProjectDetail();
});

// 监听选中项目变化
watch(() => store.projectInfo.projectSelected, () => {
  loadProjectDetail();
});
</script>

<style scoped>
#idprojectedit {
  height: 100%;
  width: 100%;
}
</style>

