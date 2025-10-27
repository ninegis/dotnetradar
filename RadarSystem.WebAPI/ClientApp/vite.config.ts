import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      'cesium': 'cesium'
    }
  },
  define: {
    CESIUM_BASE_URL: JSON.stringify('/cesium')
  },
  server: {
    host: '127.0.0.1', // 强制使用IPv4地址
    port: 8080, // 修改为8080端口
    strictPort: true, // 确保使用指定端口
    proxy: {
      '/api': {
        target: 'http://localhost:8099', // API代理到8099
        changeOrigin: true
      }
    }
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
    rollupOptions: {
      external: [],
      output: {
        manualChunks: {
          'cesium': ['cesium']
        }
      }
    }
  },
  optimizeDeps: {
    include: ['cesium']
  }
})


