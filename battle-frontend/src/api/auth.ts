import axios from 'axios';
import type { LoginDTO, LoginResponse } from '../types/auth';

// 创建 axios 实例
const api = axios.create({
  baseURL: 'https://localhost:7024', // 根据你的后端地址配置
  timeout: 5000
});

// 登录方法
export const login = async (data: LoginDTO): Promise<LoginResponse> => {
  const response = await api.post<LoginResponse>('/user/login', data);
  return response.data;
};
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      // 必须按照 JWT 标准格式：Bearer {token}
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// --- 可选：响应拦截器 (处理 Token 过期) ---
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // 如果收到 401，说明 Token 坏了或过期了，直接踢回登录页
      localStorage.clear();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
// 导出实例供其他模块使用（如设置拦截器）
export default api;