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
})
