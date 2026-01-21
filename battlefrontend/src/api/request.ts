import axios from 'axios';
// 将类型导入和普通导入分开，或者在类型前加 type 关键字
import type { AxiosInstance, AxiosResponse, InternalAxiosRequestConfig } from 'axios';

const service: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
});

// 请求拦截器
service.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    return config;
  },
  (error) => Promise.reject(error)
);

// 响应拦截器
service.interceptors.response.use(
  (response: AxiosResponse) => {
    // 这里直接返回数据部分
    return response.data;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export default service;