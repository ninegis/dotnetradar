<template>
  <div class="monitoring-3d-simple">
    <div class="header">
      <h1>🎉 欢迎进入边坡雷达三维监测系统</h1>
      <p>登录成功！系统主界面已加载</p>
      <el-button type="primary" @click="handleLogout">退出登录</el-button>
      <el-button @click="goToDashboard">返回仪表盘</el-button>
    </div>
    
    <div class="content">
      <el-card>
        <template #header>
          <div class="card-header">
            <span>系统状态</span>
            <el-tag type="success">运行中</el-tag>
          </div>
        </template>
        <el-descriptions :column="2" border>
          <el-descriptions-item label="项目名称">新疆天隆希望矿区</el-descriptions-item>
          <el-descriptions-item label="在线状态">
            <el-tag type="success">在线</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="监测点数量">15</el-descriptions-item>
          <el-descriptions-item label="设备数量">8</el-descriptions-item>
          <el-descriptions-item label="当前用户">管理员</el-descriptions-item>
          <el-descriptions-item label="登录时间">{{ loginTime }}</el-descriptions-item>
        </el-descriptions>
      </el-card>

      <el-row :gutter="20" style="margin-top: 20px">
        <el-col :span="8">
          <el-card>
            <el-statistic title="监测点总数" :value="15">
              <template #prefix>
                <el-icon style="vertical-align: middle">
                  <Location />
                </el-icon>
              </template>
            </el-statistic>
          </el-card>
        </el-col>
        <el-col :span="8">
          <el-card>
            <el-statistic title="告警数量" :value="3">
              <template #prefix>
                <el-icon style="vertical-align: middle; color: #f56c6c">
                  <Warning />
                </el-icon>
              </template>
            </el-statistic>
          </el-card>
        </el-col>
        <el-col :span="8">
          <el-card>
            <el-statistic title="设备在线" :value="8">
              <template #prefix>
                <el-icon style="vertical-align: middle; color: #67c23a">
                  <Monitor />
                </el-icon>
              </template>
            </el-statistic>
          </el-card>
        </el-col>
      </el-row>

      <el-alert
        title="登录跳转成功！"
        type="success"
        description="您已成功登录并跳转到三维监测主界面。完整的Cesium 3D功能正在开发中..."
        style="margin-top: 20px"
        :closable="false"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { Location, Warning, Monitor } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';

const router = useRouter();
const loginTime = ref(new Date().toLocaleString());

const handleLogout = () => {
  localStorage.removeItem('token');
  ElMessage.success('已退出登录');
  router.push('/login');
};

const goToDashboard = () => {
  router.push('/dashboard');
};
</script>

<style scoped>
.monitoring-3d-simple {
  width: 100%;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 40px;
}

.header {
  text-align: center;
  color: white;
  margin-bottom: 40px;
}

.header h1 {
  font-size: 32px;
  margin-bottom: 16px;
}

.header p {
  font-size: 18px;
  margin-bottom: 24px;
}

.header .el-button {
  margin: 0 8px;
}

.content {
  max-width: 1200px;
  margin: 0 auto;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
