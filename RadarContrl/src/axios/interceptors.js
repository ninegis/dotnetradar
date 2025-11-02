import axios from 'axios';

// 请求拦截器 - 自动添加Token
axios.interceptors.request.use(
    config => {
        // 从sessionStorage获取token
        const token = sessionStorage.getItem('token');
        if (token) {
            config.headers['Authorization'] = `Bearer ${token}`;
        }
        return config;
    },
    error => {
        console.error('请求错误:', error);
        return Promise.reject(error);
    }
);

// 响应拦截器 - 处理Token过期
axios.interceptors.response.use(
    response => {
        // 如果响应中包含新token，更新sessionStorage
        const newToken = response.headers['x-new-token'];
        if (newToken) {
            sessionStorage.setItem('token', newToken);
        }
        return response;
    },
    error => {
        if (error.response) {
            switch (error.response.status) {
                case 401:
                    // Token过期或未授权，清除token并跳转登录
                    console.warn('Token过期，跳转到登录页');
                    sessionStorage.removeItem('token');
                    sessionStorage.setItem('isauthorized', 'false');
                    if (window.location.pathname !== '/') {
                        window.location.href = '/';
                    }
                    break;
                case 403:
                    console.error('无权限访问');
                    break;
                case 500:
                    console.error('服务器错误');
                    break;
                default:
                    console.error(`请求错误: ${error.response.status}`);
            }
        } else {
            console.error('网络错误:', error.message);
        }
        return Promise.reject(error);
    }
);

export default axios;

