using Microsoft.Extensions.Logging;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 系统配置服务 - 临时简化版本
    /// </summary>
    public class SystemConfigService
    {
        private readonly ILogger<SystemConfigService> _logger;

        public SystemConfigService(ILogger<SystemConfigService> logger)
        {
            _logger = logger;
        }

        public async Task<object> GetSystemConfigAsync()
        {
            _logger.LogInformation("获取系统配置");
            return new { };
        }
    }
}