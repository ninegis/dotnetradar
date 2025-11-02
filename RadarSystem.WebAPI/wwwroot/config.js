// 动态获取当前访问地址
(function() {
    const hostname = window.location.hostname;
    const protocol = window.location.protocol;
    
    window.localrelease = {
        // C# 后端地址（自动匹配当前访问地址）
        url: protocol + '//' + hostname + ':8099',
        websocketUrl: 'ws://' + hostname + ':8099/wss',
        
        // 登录凭证（开发测试用）
        username: 'admin',
        password: 'admin123',
        
        // 系统配置
        title: '边坡雷达监测系统',
        shortName: '雷达监测',
        
        // 报表配置
        reportRadarUrl: protocol + '//' + hostname + ':8099/report',
        reportSign: 'radar_report_sign_key'
    };
    
    // 输出当前配置（调试用）
    console.log('[Config] 当前访问地址:', window.location.href);
    console.log('[Config] API地址:', window.localrelease.url);
    console.log('[Config] WebSocket地址:', window.localrelease.websocketUrl);
})();

