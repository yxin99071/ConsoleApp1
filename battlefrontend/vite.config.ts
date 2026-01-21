import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path' // 如果报错，请运行 npm install @types/node -D

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      // 设置 @ 指向 src 目录
      '@': path.resolve(__dirname, './src')
    }
  }
})