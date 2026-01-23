import { createRouter, createWebHistory } from 'vue-router'
// 导入你的组件（请确保路径正确）
import Login from '../components/Login.vue' 
import UserInitialSet from '../components/UserInitialSet.vue'
import UserHome from '../components/UserHome.vue'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: Login
  },
  {
    path: '/user-initial-set',
    name: 'UserInitialSet',
    component: UserInitialSet
  },
  {
    path: '/user-home',
    name: 'UserHome',
    component: UserHome
  },
  {
    path: '/',
    redirect: '/login' // 默认打开登录页
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router