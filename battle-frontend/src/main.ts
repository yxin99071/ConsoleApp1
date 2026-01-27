import { createApp } from 'vue'
import App from './App.vue'
import router from './router' // 引入路由
import './style.css' // 确保你的 Tailwind 在这里

const app = createApp(App)
app.use(router) // 使用插件
app.mount('#app')