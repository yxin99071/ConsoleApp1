import request from '@/api/request';

// 登录接口
export function loginApi(data: { Account: string; Password: string }) {
  return request({
    url: '/user/login', // 请替换为后端真实的 Controller 路由地址
    method: 'post',
    data
  });
}
export function getProfileApi() {
  return request({
    url: '/user/profile',
    method: 'get'
  });
}

// 修改个人资料
export function updateProfileApi(data: { account: string; name: string }) {
  return request({
    url: '/user/alert',
    method: 'post', // 假设是 post，如果后端是 put 请修改
    data
  });
}
//初始化
export function initProfileApi(data: {
  name: string;
  account?: string;
  profession: string;
  secondProfession: string | null;
  initialSkills: any[]; 
}) {
  return request({
    url: '/user/init',
    method: 'post',
    data
  });
}