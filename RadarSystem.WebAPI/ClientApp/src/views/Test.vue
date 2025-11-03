<template>
  <div class="test-page">
    <div class="page-header">
      <h2>测试页面</h2>
      <p>系统功能测试与调试</p>
    </div>

    <div class="test-container">
      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <span>API 测试</span>
            </template>
            <div class="test-section">
              <el-form :model="apiTest" label-width="100px">
                <el-form-item label="接口地址">
                  <el-input v-model="apiTest.url" placeholder="/api/..." />
                </el-form-item>

                <el-form-item label="请求方法">
                  <el-select v-model="apiTest.method">
                    <el-option label="GET" value="GET" />
                    <el-option label="POST" value="POST" />
                    <el-option label="PUT" value="PUT" />
                    <el-option label="DELETE" value="DELETE" />
                  </el-select>
                </el-form-item>

                <el-form-item label="请求体">
                  <el-input
                    v-model="apiTest.body"
                    type="textarea"
                    :rows="4"
                    placeholder='{"key": "value"}'
                  />
                </el-form-item>

                <el-form-item>
                  <el-button type="primary" @click="handleApiTest" :loading="apiLoading">
                    发送请求
                  </el-button>
                  <el-button @click="handleClearApi">清空</el-button>
                </el-form-item>
              </el-form>

              <div v-if="apiResponse" class="response-box">
                <div class="response-header">响应结果</div>
                <pre>{{ apiResponse }}</pre>
              </div>
            </div>
          </el-card>
        </el-col>

        <el-col :span="12">
          <el-card>
            <template #header>
              <span>WebSocket 测试</span>
            </template>
            <div class="test-section">
              <el-form label-width="100px">
                <el-form-item label="连接状态">
                  <el-tag :type="wsConnected ? 'success' : 'info'">
                    {{ wsConnected ? '已连接' : '未连接' }}
                  </el-tag>
                </el-form-item>

                <el-form-item>
                  <el-button
                    type="primary"
                    @click="handleWsConnect"
                    v-if="!wsConnected"
                  >
                    连接
                  </el-button>
                  <el-button type="danger" @click="handleWsDisconnect" v-else>
                    断开
                  </el-button>
                </el-form-item>

                <el-form-item label="发送消息">
                  <el-input v-model="wsMessage" placeholder="输入消息" />
                </el-form-item>

                <el-form-item>
                  <el-button @click="handleWsSend" :disabled="!wsConnected">
                    发送
                  </el-button>
                </el-form-item>
              </el-form>

              <div class="message-box">
                <div class="message-header">消息记录</div>
                <div class="messages">
                  <div
                    v-for="(msg, index) in wsMessages"
                    :key="index"
                    class="message-item"
                  >
                    {{ msg }}
                  </div>
                  <div v-if="wsMessages.length === 0" class="no-messages">
                    暂无消息
                  </div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <el-row :gutter="20" style="margin-top: 20px;">
        <el-col :span="24">
          <el-card>
            <template #header>
              <span>日志输出</span>
            </template>
            <div class="log-box">
              <div
                v-for="(log, index) in logs"
                :key="index"
                class="log-item"
                :class="`log-${log.type}`"
              >
                <span class="log-time">{{ log.time }}</span>
                <span class="log-message">{{ log.message }}</span>
              </div>
              <div v-if="logs.length === 0" class="no-logs">暂无日志</div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import axios from 'axios'

const apiTest = reactive({
  url: '',
  method: 'GET',
  body: ''
})

const apiLoading = ref(false)
const apiResponse = ref('')

const wsConnected = ref(false)
const wsMessage = ref('')
const wsMessages = ref<string[]>([])

const logs = ref<Array<{ time: string; type: string; message: string }>>([])

let ws: WebSocket | null = null

const addLog = (type: string, message: string) => {
  const time = new Date().toLocaleTimeString()
  logs.value.unshift({ time, type, message })
  if (logs.value.length > 100) {
    logs.value.pop()
  }
}

const handleApiTest = async () => {
  if (!apiTest.url) {
    ElMessage.warning('请输入接口地址')
    return
  }

  apiLoading.value = true
  addLog('info', `发送 ${apiTest.method} 请求: ${apiTest.url}`)

  try {
    const config: any = {
      method: apiTest.method,
      url: apiTest.url
    }

    if (apiTest.body && (apiTest.method === 'POST' || apiTest.method === 'PUT')) {
      config.data = JSON.parse(apiTest.body)
    }

    const response = await axios(config)
    apiResponse.value = JSON.stringify(response.data, null, 2)
    addLog('success', 'API 请求成功')
    ElMessage.success('请求成功')
  } catch (error: any) {
    apiResponse.value = JSON.stringify(
      {
        error: error.message,
        response: error.response?.data
      },
      null,
      2
    )
    addLog('error', `API 请求失败: ${error.message}`)
    ElMessage.error('请求失败')
  } finally {
    apiLoading.value = false
  }
}

const handleClearApi = () => {
  apiTest.url = ''
  apiTest.method = 'GET'
  apiTest.body = ''
  apiResponse.value = ''
}

const handleWsConnect = () => {
  try {
    ws = new WebSocket(`ws://${window.location.host}/ws`)

    ws.onopen = () => {
      wsConnected.value = true
      addLog('success', 'WebSocket 连接成功')
      ElMessage.success('WebSocket 已连接')
    }

    ws.onmessage = (event) => {
      wsMessages.value.unshift(`收到: ${event.data}`)
      addLog('info', `收到 WebSocket 消息: ${event.data}`)
    }

    ws.onerror = () => {
      addLog('error', 'WebSocket 错误')
      ElMessage.error('WebSocket 错误')
    }

    ws.onclose = () => {
      wsConnected.value = false
      addLog('info', 'WebSocket 连接关闭')
      ElMessage.info('WebSocket 已断开')
    }
  } catch (error: any) {
    addLog('error', `WebSocket 连接失败: ${error.message}`)
    ElMessage.error('连接失败')
  }
}

const handleWsDisconnect = () => {
  if (ws) {
    ws.close()
    ws = null
  }
}

const handleWsSend = () => {
  if (!wsMessage.value) {
    ElMessage.warning('请输入消息')
    return
  }

  if (ws && wsConnected.value) {
    ws.send(wsMessage.value)
    wsMessages.value.unshift(`发送: ${wsMessage.value}`)
    addLog('info', `发送 WebSocket 消息: ${wsMessage.value}`)
    wsMessage.value = ''
  }
}
</script>

<style scoped>
.test-page {
  padding: 20px;
}

.page-header {
  margin-bottom: 20px;
}

.page-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 500;
}

.page-header p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.test-section {
  min-height: 400px;
}

.response-box,
.message-box,
.log-box {
  margin-top: 20px;
}

.response-header,
.message-header {
  font-weight: 500;
  margin-bottom: 10px;
  padding-bottom: 10px;
  border-bottom: 1px solid #e8e8e8;
}

pre {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
  max-height: 300px;
}

.messages {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  max-height: 300px;
  overflow-y: auto;
}

.message-item {
  padding: 8px 0;
  border-bottom: 1px solid #e0e0e0;
  font-size: 13px;
}

.message-item:last-child {
  border-bottom: none;
}

.no-messages,
.no-logs {
  text-align: center;
  color: #999;
  padding: 20px;
}

.log-box {
  background: #1e1e1e;
  padding: 15px;
  border-radius: 4px;
  max-height: 400px;
  overflow-y: auto;
}

.log-item {
  padding: 4px 0;
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

.log-time {
  color: #888;
  margin-right: 10px;
}

.log-message {
  color: #fff;
}

.log-success .log-message {
  color: #67c23a;
}

.log-error .log-message {
  color: #f56c6c;
}

.log-info .log-message {
  color: #409eff;
}
</style>

  <div class="test-page">
    <div class="page-header">
      <h2>测试页面</h2>
      <p>系统功能测试与调试</p>
    </div>

    <div class="test-container">
      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <span>API 测试</span>
            </template>
            <div class="test-section">
              <el-form :model="apiTest" label-width="100px">
                <el-form-item label="接口地址">
                  <el-input v-model="apiTest.url" placeholder="/api/..." />
                </el-form-item>

                <el-form-item label="请求方法">
                  <el-select v-model="apiTest.method">
                    <el-option label="GET" value="GET" />
                    <el-option label="POST" value="POST" />
                    <el-option label="PUT" value="PUT" />
                    <el-option label="DELETE" value="DELETE" />
                  </el-select>
                </el-form-item>

                <el-form-item label="请求体">
                  <el-input
                    v-model="apiTest.body"
                    type="textarea"
                    :rows="4"
                    placeholder='{"key": "value"}'
                  />
                </el-form-item>

                <el-form-item>
                  <el-button type="primary" @click="handleApiTest" :loading="apiLoading">
                    发送请求
                  </el-button>
                  <el-button @click="handleClearApi">清空</el-button>
                </el-form-item>
              </el-form>

              <div v-if="apiResponse" class="response-box">
                <div class="response-header">响应结果</div>
                <pre>{{ apiResponse }}</pre>
              </div>
            </div>
          </el-card>
        </el-col>

        <el-col :span="12">
          <el-card>
            <template #header>
              <span>WebSocket 测试</span>
            </template>
            <div class="test-section">
              <el-form label-width="100px">
                <el-form-item label="连接状态">
                  <el-tag :type="wsConnected ? 'success' : 'info'">
                    {{ wsConnected ? '已连接' : '未连接' }}
                  </el-tag>
                </el-form-item>

                <el-form-item>
                  <el-button
                    type="primary"
                    @click="handleWsConnect"
                    v-if="!wsConnected"
                  >
                    连接
                  </el-button>
                  <el-button type="danger" @click="handleWsDisconnect" v-else>
                    断开
                  </el-button>
                </el-form-item>

                <el-form-item label="发送消息">
                  <el-input v-model="wsMessage" placeholder="输入消息" />
                </el-form-item>

                <el-form-item>
                  <el-button @click="handleWsSend" :disabled="!wsConnected">
                    发送
                  </el-button>
                </el-form-item>
              </el-form>

              <div class="message-box">
                <div class="message-header">消息记录</div>
                <div class="messages">
                  <div
                    v-for="(msg, index) in wsMessages"
                    :key="index"
                    class="message-item"
                  >
                    {{ msg }}
                  </div>
                  <div v-if="wsMessages.length === 0" class="no-messages">
                    暂无消息
                  </div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <el-row :gutter="20" style="margin-top: 20px;">
        <el-col :span="24">
          <el-card>
            <template #header>
              <span>日志输出</span>
            </template>
            <div class="log-box">
              <div
                v-for="(log, index) in logs"
                :key="index"
                class="log-item"
                :class="`log-${log.type}`"
              >
                <span class="log-time">{{ log.time }}</span>
                <span class="log-message">{{ log.message }}</span>
              </div>
              <div v-if="logs.length === 0" class="no-logs">暂无日志</div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import axios from 'axios'

const apiTest = reactive({
  url: '',
  method: 'GET',
  body: ''
})

const apiLoading = ref(false)
const apiResponse = ref('')

const wsConnected = ref(false)
const wsMessage = ref('')
const wsMessages = ref<string[]>([])

const logs = ref<Array<{ time: string; type: string; message: string }>>([])

let ws: WebSocket | null = null

const addLog = (type: string, message: string) => {
  const time = new Date().toLocaleTimeString()
  logs.value.unshift({ time, type, message })
  if (logs.value.length > 100) {
    logs.value.pop()
  }
}

const handleApiTest = async () => {
  if (!apiTest.url) {
    ElMessage.warning('请输入接口地址')
    return
  }

  apiLoading.value = true
  addLog('info', `发送 ${apiTest.method} 请求: ${apiTest.url}`)

  try {
    const config: any = {
      method: apiTest.method,
      url: apiTest.url
    }

    if (apiTest.body && (apiTest.method === 'POST' || apiTest.method === 'PUT')) {
      config.data = JSON.parse(apiTest.body)
    }

    const response = await axios(config)
    apiResponse.value = JSON.stringify(response.data, null, 2)
    addLog('success', 'API 请求成功')
    ElMessage.success('请求成功')
  } catch (error: any) {
    apiResponse.value = JSON.stringify(
      {
        error: error.message,
        response: error.response?.data
      },
      null,
      2
    )
    addLog('error', `API 请求失败: ${error.message}`)
    ElMessage.error('请求失败')
  } finally {
    apiLoading.value = false
  }
}

const handleClearApi = () => {
  apiTest.url = ''
  apiTest.method = 'GET'
  apiTest.body = ''
  apiResponse.value = ''
}

const handleWsConnect = () => {
  try {
    ws = new WebSocket(`ws://${window.location.host}/ws`)

    ws.onopen = () => {
      wsConnected.value = true
      addLog('success', 'WebSocket 连接成功')
      ElMessage.success('WebSocket 已连接')
    }

    ws.onmessage = (event) => {
      wsMessages.value.unshift(`收到: ${event.data}`)
      addLog('info', `收到 WebSocket 消息: ${event.data}`)
    }

    ws.onerror = () => {
      addLog('error', 'WebSocket 错误')
      ElMessage.error('WebSocket 错误')
    }

    ws.onclose = () => {
      wsConnected.value = false
      addLog('info', 'WebSocket 连接关闭')
      ElMessage.info('WebSocket 已断开')
    }
  } catch (error: any) {
    addLog('error', `WebSocket 连接失败: ${error.message}`)
    ElMessage.error('连接失败')
  }
}

const handleWsDisconnect = () => {
  if (ws) {
    ws.close()
    ws = null
  }
}

const handleWsSend = () => {
  if (!wsMessage.value) {
    ElMessage.warning('请输入消息')
    return
  }

  if (ws && wsConnected.value) {
    ws.send(wsMessage.value)
    wsMessages.value.unshift(`发送: ${wsMessage.value}`)
    addLog('info', `发送 WebSocket 消息: ${wsMessage.value}`)
    wsMessage.value = ''
  }
}
</script>

<style scoped>
.test-page {
  padding: 20px;
}

.page-header {
  margin-bottom: 20px;
}

.page-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 500;
}

.page-header p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.test-section {
  min-height: 400px;
}

.response-box,
.message-box,
.log-box {
  margin-top: 20px;
}

.response-header,
.message-header {
  font-weight: 500;
  margin-bottom: 10px;
  padding-bottom: 10px;
  border-bottom: 1px solid #e8e8e8;
}

pre {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
  max-height: 300px;
}

.messages {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  max-height: 300px;
  overflow-y: auto;
}

.message-item {
  padding: 8px 0;
  border-bottom: 1px solid #e0e0e0;
  font-size: 13px;
}

.message-item:last-child {
  border-bottom: none;
}

.no-messages,
.no-logs {
  text-align: center;
  color: #999;
  padding: 20px;
}

.log-box {
  background: #1e1e1e;
  padding: 15px;
  border-radius: 4px;
  max-height: 400px;
  overflow-y: auto;
}

.log-item {
  padding: 4px 0;
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

.log-time {
  color: #888;
  margin-right: 10px;
}

.log-message {
  color: #fff;
}

.log-success .log-message {
  color: #67c23a;
}

.log-error .log-message {
  color: #f56c6c;
}

.log-info .log-message {
  color: #409eff;
}
</style>

  <div class="test-page">
    <div class="page-header">
      <h2>测试页面</h2>
      <p>系统功能测试与调试</p>
    </div>

    <div class="test-container">
      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <span>API 测试</span>
            </template>
            <div class="test-section">
              <el-form :model="apiTest" label-width="100px">
                <el-form-item label="接口地址">
                  <el-input v-model="apiTest.url" placeholder="/api/..." />
                </el-form-item>

                <el-form-item label="请求方法">
                  <el-select v-model="apiTest.method">
                    <el-option label="GET" value="GET" />
                    <el-option label="POST" value="POST" />
                    <el-option label="PUT" value="PUT" />
                    <el-option label="DELETE" value="DELETE" />
                  </el-select>
                </el-form-item>

                <el-form-item label="请求体">
                  <el-input
                    v-model="apiTest.body"
                    type="textarea"
                    :rows="4"
                    placeholder='{"key": "value"}'
                  />
                </el-form-item>

                <el-form-item>
                  <el-button type="primary" @click="handleApiTest" :loading="apiLoading">
                    发送请求
                  </el-button>
                  <el-button @click="handleClearApi">清空</el-button>
                </el-form-item>
              </el-form>

              <div v-if="apiResponse" class="response-box">
                <div class="response-header">响应结果</div>
                <pre>{{ apiResponse }}</pre>
              </div>
            </div>
          </el-card>
        </el-col>

        <el-col :span="12">
          <el-card>
            <template #header>
              <span>WebSocket 测试</span>
            </template>
            <div class="test-section">
              <el-form label-width="100px">
                <el-form-item label="连接状态">
                  <el-tag :type="wsConnected ? 'success' : 'info'">
                    {{ wsConnected ? '已连接' : '未连接' }}
                  </el-tag>
                </el-form-item>

                <el-form-item>
                  <el-button
                    type="primary"
                    @click="handleWsConnect"
                    v-if="!wsConnected"
                  >
                    连接
                  </el-button>
                  <el-button type="danger" @click="handleWsDisconnect" v-else>
                    断开
                  </el-button>
                </el-form-item>

                <el-form-item label="发送消息">
                  <el-input v-model="wsMessage" placeholder="输入消息" />
                </el-form-item>

                <el-form-item>
                  <el-button @click="handleWsSend" :disabled="!wsConnected">
                    发送
                  </el-button>
                </el-form-item>
              </el-form>

              <div class="message-box">
                <div class="message-header">消息记录</div>
                <div class="messages">
                  <div
                    v-for="(msg, index) in wsMessages"
                    :key="index"
                    class="message-item"
                  >
                    {{ msg }}
                  </div>
                  <div v-if="wsMessages.length === 0" class="no-messages">
                    暂无消息
                  </div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <el-row :gutter="20" style="margin-top: 20px;">
        <el-col :span="24">
          <el-card>
            <template #header>
              <span>日志输出</span>
            </template>
            <div class="log-box">
              <div
                v-for="(log, index) in logs"
                :key="index"
                class="log-item"
                :class="`log-${log.type}`"
              >
                <span class="log-time">{{ log.time }}</span>
                <span class="log-message">{{ log.message }}</span>
              </div>
              <div v-if="logs.length === 0" class="no-logs">暂无日志</div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import axios from 'axios'

const apiTest = reactive({
  url: '',
  method: 'GET',
  body: ''
})

const apiLoading = ref(false)
const apiResponse = ref('')

const wsConnected = ref(false)
const wsMessage = ref('')
const wsMessages = ref<string[]>([])

const logs = ref<Array<{ time: string; type: string; message: string }>>([])

let ws: WebSocket | null = null

const addLog = (type: string, message: string) => {
  const time = new Date().toLocaleTimeString()
  logs.value.unshift({ time, type, message })
  if (logs.value.length > 100) {
    logs.value.pop()
  }
}

const handleApiTest = async () => {
  if (!apiTest.url) {
    ElMessage.warning('请输入接口地址')
    return
  }

  apiLoading.value = true
  addLog('info', `发送 ${apiTest.method} 请求: ${apiTest.url}`)

  try {
    const config: any = {
      method: apiTest.method,
      url: apiTest.url
    }

    if (apiTest.body && (apiTest.method === 'POST' || apiTest.method === 'PUT')) {
      config.data = JSON.parse(apiTest.body)
    }

    const response = await axios(config)
    apiResponse.value = JSON.stringify(response.data, null, 2)
    addLog('success', 'API 请求成功')
    ElMessage.success('请求成功')
  } catch (error: any) {
    apiResponse.value = JSON.stringify(
      {
        error: error.message,
        response: error.response?.data
      },
      null,
      2
    )
    addLog('error', `API 请求失败: ${error.message}`)
    ElMessage.error('请求失败')
  } finally {
    apiLoading.value = false
  }
}

const handleClearApi = () => {
  apiTest.url = ''
  apiTest.method = 'GET'
  apiTest.body = ''
  apiResponse.value = ''
}

const handleWsConnect = () => {
  try {
    ws = new WebSocket(`ws://${window.location.host}/ws`)

    ws.onopen = () => {
      wsConnected.value = true
      addLog('success', 'WebSocket 连接成功')
      ElMessage.success('WebSocket 已连接')
    }

    ws.onmessage = (event) => {
      wsMessages.value.unshift(`收到: ${event.data}`)
      addLog('info', `收到 WebSocket 消息: ${event.data}`)
    }

    ws.onerror = () => {
      addLog('error', 'WebSocket 错误')
      ElMessage.error('WebSocket 错误')
    }

    ws.onclose = () => {
      wsConnected.value = false
      addLog('info', 'WebSocket 连接关闭')
      ElMessage.info('WebSocket 已断开')
    }
  } catch (error: any) {
    addLog('error', `WebSocket 连接失败: ${error.message}`)
    ElMessage.error('连接失败')
  }
}

const handleWsDisconnect = () => {
  if (ws) {
    ws.close()
    ws = null
  }
}

const handleWsSend = () => {
  if (!wsMessage.value) {
    ElMessage.warning('请输入消息')
    return
  }

  if (ws && wsConnected.value) {
    ws.send(wsMessage.value)
    wsMessages.value.unshift(`发送: ${wsMessage.value}`)
    addLog('info', `发送 WebSocket 消息: ${wsMessage.value}`)
    wsMessage.value = ''
  }
}
</script>

<style scoped>
.test-page {
  padding: 20px;
}

.page-header {
  margin-bottom: 20px;
}

.page-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 500;
}

.page-header p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.test-section {
  min-height: 400px;
}

.response-box,
.message-box,
.log-box {
  margin-top: 20px;
}

.response-header,
.message-header {
  font-weight: 500;
  margin-bottom: 10px;
  padding-bottom: 10px;
  border-bottom: 1px solid #e8e8e8;
}

pre {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
  max-height: 300px;
}

.messages {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  max-height: 300px;
  overflow-y: auto;
}

.message-item {
  padding: 8px 0;
  border-bottom: 1px solid #e0e0e0;
  font-size: 13px;
}

.message-item:last-child {
  border-bottom: none;
}

.no-messages,
.no-logs {
  text-align: center;
  color: #999;
  padding: 20px;
}

.log-box {
  background: #1e1e1e;
  padding: 15px;
  border-radius: 4px;
  max-height: 400px;
  overflow-y: auto;
}

.log-item {
  padding: 4px 0;
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

.log-time {
  color: #888;
  margin-right: 10px;
}

.log-message {
  color: #fff;
}

.log-success .log-message {
  color: #67c23a;
}

.log-error .log-message {
  color: #f56c6c;
}

.log-info .log-message {
  color: #409eff;
}
</style>

  <div class="test-page">
    <div class="page-header">
      <h2>测试页面</h2>
      <p>系统功能测试与调试</p>
    </div>

    <div class="test-container">
      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <span>API 测试</span>
            </template>
            <div class="test-section">
              <el-form :model="apiTest" label-width="100px">
                <el-form-item label="接口地址">
                  <el-input v-model="apiTest.url" placeholder="/api/..." />
                </el-form-item>

                <el-form-item label="请求方法">
                  <el-select v-model="apiTest.method">
                    <el-option label="GET" value="GET" />
                    <el-option label="POST" value="POST" />
                    <el-option label="PUT" value="PUT" />
                    <el-option label="DELETE" value="DELETE" />
                  </el-select>
                </el-form-item>

                <el-form-item label="请求体">
                  <el-input
                    v-model="apiTest.body"
                    type="textarea"
                    :rows="4"
                    placeholder='{"key": "value"}'
                  />
                </el-form-item>

                <el-form-item>
                  <el-button type="primary" @click="handleApiTest" :loading="apiLoading">
                    发送请求
                  </el-button>
                  <el-button @click="handleClearApi">清空</el-button>
                </el-form-item>
              </el-form>

              <div v-if="apiResponse" class="response-box">
                <div class="response-header">响应结果</div>
                <pre>{{ apiResponse }}</pre>
              </div>
            </div>
          </el-card>
        </el-col>

        <el-col :span="12">
          <el-card>
            <template #header>
              <span>WebSocket 测试</span>
            </template>
            <div class="test-section">
              <el-form label-width="100px">
                <el-form-item label="连接状态">
                  <el-tag :type="wsConnected ? 'success' : 'info'">
                    {{ wsConnected ? '已连接' : '未连接' }}
                  </el-tag>
                </el-form-item>

                <el-form-item>
                  <el-button
                    type="primary"
                    @click="handleWsConnect"
                    v-if="!wsConnected"
                  >
                    连接
                  </el-button>
                  <el-button type="danger" @click="handleWsDisconnect" v-else>
                    断开
                  </el-button>
                </el-form-item>

                <el-form-item label="发送消息">
                  <el-input v-model="wsMessage" placeholder="输入消息" />
                </el-form-item>

                <el-form-item>
                  <el-button @click="handleWsSend" :disabled="!wsConnected">
                    发送
                  </el-button>
                </el-form-item>
              </el-form>

              <div class="message-box">
                <div class="message-header">消息记录</div>
                <div class="messages">
                  <div
                    v-for="(msg, index) in wsMessages"
                    :key="index"
                    class="message-item"
                  >
                    {{ msg }}
                  </div>
                  <div v-if="wsMessages.length === 0" class="no-messages">
                    暂无消息
                  </div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <el-row :gutter="20" style="margin-top: 20px;">
        <el-col :span="24">
          <el-card>
            <template #header>
              <span>日志输出</span>
            </template>
            <div class="log-box">
              <div
                v-for="(log, index) in logs"
                :key="index"
                class="log-item"
                :class="`log-${log.type}`"
              >
                <span class="log-time">{{ log.time }}</span>
                <span class="log-message">{{ log.message }}</span>
              </div>
              <div v-if="logs.length === 0" class="no-logs">暂无日志</div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import axios from 'axios'

const apiTest = reactive({
  url: '',
  method: 'GET',
  body: ''
})

const apiLoading = ref(false)
const apiResponse = ref('')

const wsConnected = ref(false)
const wsMessage = ref('')
const wsMessages = ref<string[]>([])

const logs = ref<Array<{ time: string; type: string; message: string }>>([])

let ws: WebSocket | null = null

const addLog = (type: string, message: string) => {
  const time = new Date().toLocaleTimeString()
  logs.value.unshift({ time, type, message })
  if (logs.value.length > 100) {
    logs.value.pop()
  }
}

const handleApiTest = async () => {
  if (!apiTest.url) {
    ElMessage.warning('请输入接口地址')
    return
  }

  apiLoading.value = true
  addLog('info', `发送 ${apiTest.method} 请求: ${apiTest.url}`)

  try {
    const config: any = {
      method: apiTest.method,
      url: apiTest.url
    }

    if (apiTest.body && (apiTest.method === 'POST' || apiTest.method === 'PUT')) {
      config.data = JSON.parse(apiTest.body)
    }

    const response = await axios(config)
    apiResponse.value = JSON.stringify(response.data, null, 2)
    addLog('success', 'API 请求成功')
    ElMessage.success('请求成功')
  } catch (error: any) {
    apiResponse.value = JSON.stringify(
      {
        error: error.message,
        response: error.response?.data
      },
      null,
      2
    )
    addLog('error', `API 请求失败: ${error.message}`)
    ElMessage.error('请求失败')
  } finally {
    apiLoading.value = false
  }
}

const handleClearApi = () => {
  apiTest.url = ''
  apiTest.method = 'GET'
  apiTest.body = ''
  apiResponse.value = ''
}

const handleWsConnect = () => {
  try {
    ws = new WebSocket(`ws://${window.location.host}/ws`)

    ws.onopen = () => {
      wsConnected.value = true
      addLog('success', 'WebSocket 连接成功')
      ElMessage.success('WebSocket 已连接')
    }

    ws.onmessage = (event) => {
      wsMessages.value.unshift(`收到: ${event.data}`)
      addLog('info', `收到 WebSocket 消息: ${event.data}`)
    }

    ws.onerror = () => {
      addLog('error', 'WebSocket 错误')
      ElMessage.error('WebSocket 错误')
    }

    ws.onclose = () => {
      wsConnected.value = false
      addLog('info', 'WebSocket 连接关闭')
      ElMessage.info('WebSocket 已断开')
    }
  } catch (error: any) {
    addLog('error', `WebSocket 连接失败: ${error.message}`)
    ElMessage.error('连接失败')
  }
}

const handleWsDisconnect = () => {
  if (ws) {
    ws.close()
    ws = null
  }
}

const handleWsSend = () => {
  if (!wsMessage.value) {
    ElMessage.warning('请输入消息')
    return
  }

  if (ws && wsConnected.value) {
    ws.send(wsMessage.value)
    wsMessages.value.unshift(`发送: ${wsMessage.value}`)
    addLog('info', `发送 WebSocket 消息: ${wsMessage.value}`)
    wsMessage.value = ''
  }
}
</script>

<style scoped>
.test-page {
  padding: 20px;
}

.page-header {
  margin-bottom: 20px;
}

.page-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 500;
}

.page-header p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.test-section {
  min-height: 400px;
}

.response-box,
.message-box,
.log-box {
  margin-top: 20px;
}

.response-header,
.message-header {
  font-weight: 500;
  margin-bottom: 10px;
  padding-bottom: 10px;
  border-bottom: 1px solid #e8e8e8;
}

pre {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
  max-height: 300px;
}

.messages {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  max-height: 300px;
  overflow-y: auto;
}

.message-item {
  padding: 8px 0;
  border-bottom: 1px solid #e0e0e0;
  font-size: 13px;
}

.message-item:last-child {
  border-bottom: none;
}

.no-messages,
.no-logs {
  text-align: center;
  color: #999;
  padding: 20px;
}

.log-box {
  background: #1e1e1e;
  padding: 15px;
  border-radius: 4px;
  max-height: 400px;
  overflow-y: auto;
}

.log-item {
  padding: 4px 0;
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

.log-time {
  color: #888;
  margin-right: 10px;
}

.log-message {
  color: #fff;
}

.log-success .log-message {
  color: #67c23a;
}

.log-error .log-message {
  color: #f56c6c;
}

.log-info .log-message {
  color: #409eff;
}
</style>





