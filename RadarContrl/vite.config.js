import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from "path";
import { join } from "path";
import { existsSync } from 'fs';
import fsExtra from 'fs-extra';
const { copySync } = fsExtra;

const cesiumSource = './node_modules/cesium'

// Cesium静态资源复制插件
const cesiumCopyPlugin = () => {
  return {
    name: 'cesium-copy-plugin',
    buildStart() {
      // 确保Cesium目录存在
      const cesiumDir = path.resolve(__dirname, cesiumSource);
      if (!existsSync(cesiumDir)) {
        console.warn('Cesium not found in node_modules');
        return;
      }

      // 在构建完成后复制Cesium静态资源
      this.__cesiumCopied = false;
    },
    writeBundle() {
      if (this.__cesiumCopied) return;
      this.__cesiumCopied = true;

      const cesiumDir = path.resolve(__dirname, cesiumSource);
      const distDir = path.resolve(__dirname, 'dist');
      const cesiumBuildDir = path.join(cesiumDir, 'Build', 'Cesium');

      try {
        // 复制Workers目录
        const workersSrc = path.join(cesiumBuildDir, 'Workers');
        const workersDst = path.join(distDir, 'Cesium', 'Workers');
        if (existsSync(workersSrc)) {
          copySync(workersSrc, workersDst, { overwrite: true });
          console.log('✓ Cesium Workers copied');
        }

        // 复制Assets目录
        const assetsSrc = path.join(cesiumBuildDir, 'Assets');
        const assetsDst = path.join(distDir, 'Cesium', 'Assets');
        if (existsSync(assetsSrc)) {
          copySync(assetsSrc, assetsDst, { overwrite: true });
          console.log('✓ Cesium Assets copied');
        }

        // 复制ThirdParty目录
        const thirdPartySrc = path.join(cesiumBuildDir, 'ThirdParty');
        const thirdPartyDst = path.join(distDir, 'Cesium', 'ThirdParty');
        if (existsSync(thirdPartySrc)) {
          copySync(thirdPartySrc, thirdPartyDst, { overwrite: true });
          console.log('✓ Cesium ThirdParty copied');
        }

        // 复制Widgets目录（包含CSS文件）
        const widgetsSrc = path.join(cesiumBuildDir, 'Widgets');
        const widgetsDst = path.join(distDir, 'Cesium', 'Widgets');
        if (existsSync(widgetsSrc)) {
          copySync(widgetsSrc, widgetsDst, { overwrite: true });
          console.log('✓ Cesium Widgets copied');
        }

      } catch (error) {
        console.error('Error copying Cesium resources:', error);
      }
    }
  };
};

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
  plugins: [
    vue(),
    cesiumCopyPlugin()
  ],
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
