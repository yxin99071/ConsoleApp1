import { createApp } from 'vue'
import App from './App.vue'
import router from './router' // 确保你已经创建了 router/index.ts
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

const app = createApp(App)

// 注册所有图标（解决你登录页用到的 User, Lock 图标）
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.use(router)
app.use(ElementPlus)
app.mount('#app')