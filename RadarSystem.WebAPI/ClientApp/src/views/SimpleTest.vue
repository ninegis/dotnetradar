<template>
  <div class="simple-test">
    <div class="page-header">
      <h2>简单测试</h2>
      <p>用于快速测试和调试的页面</p>
    </div>

    <div class="test-content">
      <el-card>
        <template #header>
          <span>测试工具</span>
        </template>

        <el-form :model="testForm" label-width="120px">
          <el-form-item label="测试输入">
            <el-input v-model="testForm.input" placeholder="请输入测试内容" />
          </el-form-item>

          <el-form-item label="测试选项">
            <el-select v-model="testForm.option" placeholder="请选择">
              <el-option label="选项1" value="option1" />
              <el-option label="选项2" value="option2" />
              <el-option label="选项3" value="option3" />
            </el-select>
          </el-form-item>

          <el-form-item>
            <el-button type="primary" @click="handleTest">执行测试</el-button>
            <el-button @click="handleReset">重置</el-button>
          </el-form-item>
        </el-form>
      </el-card>

      <el-card style="margin-top: 20px;" v-if="testResult">
        <template #header>
          <span>测试结果</span>
        </template>
        <pre>{{ testResult }}</pre>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'

const testForm = reactive({
  input: '',
  option: ''
})

const testResult = ref('')

const handleTest = () => {
  if (!testForm.input) {
    ElMessage.warning('请输入测试内容')
    return
  }

  testResult.value = JSON.stringify({
    timestamp: new Date().toISOString(),
    input: testForm.input,
    option: testForm.option,
    status: 'success'
  }, null, 2)

  ElMessage.success('测试执行成功')
}

const handleReset = () => {
  testForm.input = ''
  testForm.option = ''
  testResult.value = ''
  ElMessage.info('已重置')
}
</script>

<style scoped>
.simple-test {
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

.test-content {
  max-width: 800px;
}

pre {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
}
</style>

  <div class="simple-test">
    <div class="page-header">
      <h2>简单测试</h2>
      <p>用于快速测试和调试的页面</p>
    </div>

    <div class="test-content">
      <el-card>
        <template #header>
          <span>测试工具</span>
        </template>

        <el-form :model="testForm" label-width="120px">
          <el-form-item label="测试输入">
            <el-input v-model="testForm.input" placeholder="请输入测试内容" />
          </el-form-item>

          <el-form-item label="测试选项">
            <el-select v-model="testForm.option" placeholder="请选择">
              <el-option label="选项1" value="option1" />
              <el-option label="选项2" value="option2" />
              <el-option label="选项3" value="option3" />
            </el-select>
          </el-form-item>

          <el-form-item>
            <el-button type="primary" @click="handleTest">执行测试</el-button>
            <el-button @click="handleReset">重置</el-button>
          </el-form-item>
        </el-form>
      </el-card>

      <el-card style="margin-top: 20px;" v-if="testResult">
        <template #header>
          <span>测试结果</span>
        </template>
        <pre>{{ testResult }}</pre>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'

const testForm = reactive({
  input: '',
  option: ''
})

const testResult = ref('')

const handleTest = () => {
  if (!testForm.input) {
    ElMessage.warning('请输入测试内容')
    return
  }

  testResult.value = JSON.stringify({
    timestamp: new Date().toISOString(),
    input: testForm.input,
    option: testForm.option,
    status: 'success'
  }, null, 2)

  ElMessage.success('测试执行成功')
}

const handleReset = () => {
  testForm.input = ''
  testForm.option = ''
  testResult.value = ''
  ElMessage.info('已重置')
}
</script>

<style scoped>
.simple-test {
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

.test-content {
  max-width: 800px;
}

pre {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
}
</style>

  <div class="simple-test">
    <div class="page-header">
      <h2>简单测试</h2>
      <p>用于快速测试和调试的页面</p>
    </div>

    <div class="test-content">
      <el-card>
        <template #header>
          <span>测试工具</span>
        </template>

        <el-form :model="testForm" label-width="120px">
          <el-form-item label="测试输入">
            <el-input v-model="testForm.input" placeholder="请输入测试内容" />
          </el-form-item>

          <el-form-item label="测试选项">
            <el-select v-model="testForm.option" placeholder="请选择">
              <el-option label="选项1" value="option1" />
              <el-option label="选项2" value="option2" />
              <el-option label="选项3" value="option3" />
            </el-select>
          </el-form-item>

          <el-form-item>
            <el-button type="primary" @click="handleTest">执行测试</el-button>
            <el-button @click="handleReset">重置</el-button>
          </el-form-item>
        </el-form>
      </el-card>

      <el-card style="margin-top: 20px;" v-if="testResult">
        <template #header>
          <span>测试结果</span>
        </template>
        <pre>{{ testResult }}</pre>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'

const testForm = reactive({
  input: '',
  option: ''
})

const testResult = ref('')

const handleTest = () => {
  if (!testForm.input) {
    ElMessage.warning('请输入测试内容')
    return
  }

  testResult.value = JSON.stringify({
    timestamp: new Date().toISOString(),
    input: testForm.input,
    option: testForm.option,
    status: 'success'
  }, null, 2)

  ElMessage.success('测试执行成功')
}

const handleReset = () => {
  testForm.input = ''
  testForm.option = ''
  testResult.value = ''
  ElMessage.info('已重置')
}
</script>

<style scoped>
.simple-test {
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

.test-content {
  max-width: 800px;
}

pre {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
}
</style>

  <div class="simple-test">
    <div class="page-header">
      <h2>简单测试</h2>
      <p>用于快速测试和调试的页面</p>
    </div>

    <div class="test-content">
      <el-card>
        <template #header>
          <span>测试工具</span>
        </template>

        <el-form :model="testForm" label-width="120px">
          <el-form-item label="测试输入">
            <el-input v-model="testForm.input" placeholder="请输入测试内容" />
          </el-form-item>

          <el-form-item label="测试选项">
            <el-select v-model="testForm.option" placeholder="请选择">
              <el-option label="选项1" value="option1" />
              <el-option label="选项2" value="option2" />
              <el-option label="选项3" value="option3" />
            </el-select>
          </el-form-item>

          <el-form-item>
            <el-button type="primary" @click="handleTest">执行测试</el-button>
            <el-button @click="handleReset">重置</el-button>
          </el-form-item>
        </el-form>
      </el-card>

      <el-card style="margin-top: 20px;" v-if="testResult">
        <template #header>
          <span>测试结果</span>
        </template>
        <pre>{{ testResult }}</pre>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'

const testForm = reactive({
  input: '',
  option: ''
})

const testResult = ref('')

const handleTest = () => {
  if (!testForm.input) {
    ElMessage.warning('请输入测试内容')
    return
  }

  testResult.value = JSON.stringify({
    timestamp: new Date().toISOString(),
    input: testForm.input,
    option: testForm.option,
    status: 'success'
  }, null, 2)

  ElMessage.success('测试执行成功')
}

const handleReset = () => {
  testForm.input = ''
  testForm.option = ''
  testResult.value = ''
  ElMessage.info('已重置')
}
</script>

<style scoped>
.simple-test {
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

.test-content {
  max-width: 800px;
}

pre {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
}
</style>





