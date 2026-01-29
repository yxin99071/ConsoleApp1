import { createRouter, createWebHistory } from 'vue-router';
import LoginView from '../views/LoginView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { requiresAuth: false }
    },
    {
      path: '/lobby',
      name: 'lobby',
      component: () => import('../views/LobbyView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/create-character',
      name: 'create-character',
      component: () => import('../views/CreateCharacterView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/',
      redirect: '/login'
    },
    {
      path: '/fight/:battleId?',
      name: 'FightCenter',
      component: () => import('../views/FightCenterView.vue'),
      meta: { requiresAuth: true },
      // 仅在有路径参数时读取 params，否则读取 state
      props: (route) => ({
        battleId: route.params.battleId,
        battleInitData: window.history.state?.battleInitData
      })
    }
  ]
});

// 路由守卫：处理登录拦截与职业分流
router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('token');
  // 显式转为布尔值，防止 "null" 字符串干扰
  const hasToken = !!token && token !== 'undefined' && token !== 'null';
  const profession = localStorage.getItem('userProfession');
  const hasProfession = !!profession && profession !== 'undefined' && profession !== 'null';

  // 1. 目的地是登录页
  if (to.path === '/login') {
    if (hasToken) {
      // 已登录就别待在登录页了
      return next(hasProfession ? '/lobby' : '/create-character');
    }
    return next(); // 没登录，允许进登录页
  }

  // 2. 目的地是需要授权的页面
  if (to.meta.requiresAuth) {
    if (!hasToken) {
      // 没登录，强制去登录
      return next('/login');
    }

    // 已登录，但没职业，且目的地不是创建页
    if (!hasProfession && to.path !== '/create-character') {
      return next('/create-character');
    }
    if (to.name === 'FightCenter') {
      const hasInitData = !!window.history.state?.battleInitData;
      const hasBattleId = !!to.params.battleId;

      // 如果是从 PK 按钮过来的，确保数据存在；如果是直接访问，则交给组件内部处理“留空”状态
      if (to.path === '/fight' && !hasInitData && !hasBattleId) {
        // 允许进入，但在组件内提示“请选择对局”
      }
    }
    // ----------------------------

    if (!hasProfession && to.path !== '/create-character') {
      return next('/create-character');
    }
  }

  // 3. 其他默认放行
  next();
});


export default router;