<template>
  <div style="padding: 40px; background: #f0f0f0; min-height: 100vh;">
    <h1 style="color: green;">✅ Vue组件正常工作！</h1>
    <p style="font-size: 18px; margin: 20px 0;">如果你能看到这个页面，说明Vue和Router正常。</p>
    
    <div style="background: white; padding: 20px; border-radius: 8px; margin: 20px 0;">
      <h2>系统信息：</h2>
      <ul style="line-height: 2;">
        <li>✅ Vue 3 已加载</li>
        <li>✅ Vue Router 工作正常</li>
        <li>✅ 组件渲染成功</li>
        <li>✅ Vite开发服务器运行中</li>
      </ul>
    </div>

    <div style="background: #fff3cd; padding: 20px; border-radius: 8px; border-left: 4px solid #ffc107;">
      <h3>🔧 下一步测试：</h3>
      <button @click="testRouter" style="margin: 10px; padding: 10px 20px; font-size: 16px; cursor: pointer;">
        测试路由 - 跳转到Dashboard
      </button>
      <button @click="testAPI" style="margin: 10px; padding: 10px 20px; font-size: 16px; cursor: pointer;">
        测试API - 登录接口
      </button>
    </div>

    <div v-if="message" style="margin-top: 20px; padding: 15px; background: #d4edda; border: 1px solid #c3e6cb; border-radius: 4px;">
      <strong>测试结果：</strong>
      <pre style="margin: 10px 0; background: #f8f9fa; padding: 10px; overflow-x: auto;">{{ message }}</pre>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const message = ref('')

const testRouter = () => {
  message.value = '正在跳转到Dashboard...'
  setTimeout(() => {
    router.push('/dashboard')
  }, 1000)
}

const testAPI = async () => {
  try {
    message.value = '正在测试API...'
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: 'admin', password: 'admin123' })
    })
    const data = await response.json()
    message.value = JSON.stringify(data, null, 2)
  } catch (error) {
    message.value = '错误: ' + error
  }
}
</script>

