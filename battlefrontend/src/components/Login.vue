<template>
  <div class="login-wrapper">
    <el-card class="login-box">
      <template #header>
        <h2 style="text-align: center; margin: 0;">欢迎登录</h2>
      </template>

      <el-form :model="loginForm" :rules="rules" ref="formRef" label-position="top">
        <el-form-item label="账号" prop="Account">
          <el-input 
            v-model="loginForm.Account" 
            placeholder="请输入账号" 
            prefix-icon="User" 
          />
        </el-form-item>

        <el-form-item label="密码" prop="Password">
          <el-input 
            v-model="loginForm.Password" 
            type="password" 
            placeholder="请输入密码" 
            show-password 
            prefix-icon="Lock"
            @keyup.enter="handleLogin" 
          />
        </el-form-item>

        <el-button 
          type="primary" 
          :loading="loading" 
          @click="handleLogin" 
          style="width: 100%; margin-top: 10px;"
        >
          {{ loading ? '登录中...' : '立 即 登 录' }}
        </el-button>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { loginApi } from '@/api/user'; // 引入上面定义的接口

const router = useRouter();
const formRef = ref();
const loading = ref(false);

// 1. 数据结构：严格匹配后端 LoginDTO
const loginForm = reactive({
  Account: '',
  Password: ''
});

// 2. 表单校验规则
const rules = {
  Account: [{ required: true, message: '账号必填', trigger: 'blur' }],
  Password: [{ required: true, message: '密码必填', trigger: 'blur' }]
};

// 3. 登录逻辑
const handleLogin = async () => {
  if (!formRef.value) return;

  try {
    // 表单预校验
    await formRef.value.validate();
    
    loading.value = true;
    
    /** * 发起请求 
     * 因为你的 request.ts 拦截器返回了 res.data
     * 所以这里的 res 直接就是 { token, id, name, profession }
     */
    const res = await loginApi(loginForm) as any;
    
    // 存储 Token (对应你拦截器中 401 清理时的 key)
    localStorage.setItem('token', res.token);
    localStorage.setItem('userName', res.name); // 顺便存一下用户名方便显示

    ElMessage.success('登录成功');

    // 4. 根据 profession 字段进行业务跳转逻辑
    if (res.profession === null || res.profession === undefined || res.profession === '') {
      // 职业信息为空 -> 去设置页
      router.push({name:'UserInitialSet'}); 
    } else {
      // 已有职业信息 -> 去首页
      router.push({name:'UserHome'});
    }

  } catch (error: any) {
    // 错误已经在 request.ts 的响应拦截器里 console.error 或弹出提示了
    console.warn('登录流程中断:', error);
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.login-wrapper {
  height: 100vh;
  width: 100vw;
  display: flex;
  justify-content: center;
  align-items: center;
  /* 渐变背景 */
  background: linear-gradient(135deg, #409eff 0%, #1d2b50 100%);
}

.login-box {
  width: 380px;
  border-radius: 8px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
}
</style>