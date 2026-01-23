import axios from 'axios';
// 将类型导入和普通导入分开，或者在类型前加 type 关键字
import type { AxiosInstance, AxiosResponse, InternalAxiosRequestConfig } from 'axios';

const service: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
  withCredentials: true, // 必须开启！允许跨域携带 Cookie
});

service.interceptors.request.use(
  (config) => {
    // 1. 从本地把刚才存的 token 拿出来
    const token = localStorage.getItem('token');
    
    // 2. 塞进请求头 (注意 Bearer 后面有个空格)
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);


// src/utils/request.ts 响应拦截器部分
service.interceptors.response.use(
  (response: AxiosResponse) => {
    const res = response.data;
    
    // 如果后端直接返回的是数据对象（没有包装 code），则直接返回 res
    // 或者根据状态码判断：
    if (response.status === 200 || response.status === 204) {
      return res; 
    }
    
    // 如果你确实定义了统一结构，保留原逻辑，但现在显然对不上
    return Promise.reject(new Error('请求失败'));
  },
  (error) => {
    // 处理 401 或网络错误
    return Promise.reject(error);
  }
);

export default service;