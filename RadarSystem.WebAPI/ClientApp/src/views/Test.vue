<template>
  <div style="width: 100vw; height: 100vh; display: flex; flex-direction: column; justify-content: center; align-items: center; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white;">
    <h1 style="font-size: 48px;">✅ 路由跳转成功！</h1>
    <p style="font-size: 24px; margin: 20px 0;">这是测试页面</p>
    <div style="margin-top: 40px;">
      <button @click="goBack" style="padding: 12px 24px; font-size: 16px; cursor: pointer; margin: 0 10px;">返回登录</button>
      <button @click="testApi" style="padding: 12px 24px; font-size: 16px; cursor: pointer; margin: 0 10px;">测试API</button>
    </div>
    <div v-if="apiResult" style="margin-top: 20px; background: white; color: black; padding: 20px; border-radius: 8px; max-width: 600px;">
      <pre>{{ apiResult }}</pre>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const apiResult = ref('');

const goBack = () => {
  localStorage.clear();
  router.push('/login');
};

const testApi = async () => {
  try {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: 'admin', password: 'admin123' })
    });
    const data = await response.json();
    apiResult.value = JSON.stringify(data, null, 2);
  } catch (error) {
    apiResult.value = 'Error: ' + error;
  }
};
</script>

