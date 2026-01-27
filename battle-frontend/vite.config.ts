import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path' // 引入 path 模块
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [vue(),tailwindcss()],
  resolve: {
    alias: {
      // 这里的 '@' 将指向项目的 'src' 目录
      '@': path.resolve(__dirname, './src'),
    },
  },
})
