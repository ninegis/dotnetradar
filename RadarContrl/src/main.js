import { createApp } from 'vue';
import 'ant-design-vue/dist/antd.css';
import 'element-plus/dist/index.css';
import 'element-plus/theme-chalk/dark/css-vars.css'
import './styles/index.css';
import App from './App.vue';
import Antd from 'ant-design-vue';
import ElementPlus from 'element-plus';
import locale from 'element-plus/dist/locale/zh-cn.mjs'
import {createPinia} from "pinia";
import router from '@/router/index';
import {i18n} from '@/locales/index.js'

// ✅ 导入axios拦截器配置（自动添加Token，处理401等）
import './axios/interceptors.js';

const app = createApp(App);
app
    .use(i18n)
    .use(Antd)
    .use(ElementPlus,{locale})
    .use(createPinia())
    .use(router)
    .mount('#app');