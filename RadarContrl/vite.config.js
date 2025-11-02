import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from "path";
import { join } from "path";
const cesiumSource = './node_modules/cesium'
export default defineConfig({
  base: './',
  publicPath: './',
  server: {
    host: '0.0.0.0',
    open: true
  },
  resolve: {
    alias: {
      "cesium": path.resolve(__dirname, cesiumSource),
      '@': join(__dirname, "src"),
    }
  },
  plugins: [vue()],
  build: {
    chunkSizeWarningLimit: 1000, // 提高警告阈值到1000KB
    rollupOptions: {
      output: {
        manualChunks: {
          // 将Cesium单独打包
          'cesium': ['cesium'],
          // Vue核心库
          'vue-vendor': ['vue', 'vue-router'],
          // 其他第三方库
          'vendor': ['axios'],
        },
      },
    },
  },
})
