using Microsoft.Extensions.Logging;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 系统日志服务 - 临时简化版本
    /// </summary>
    public class SystemLogService
    {
        private readonly ILogger<SystemLogService> _logger;

        public SystemLogService(ILogger<SystemLogService> logger)
        {
            _logger = logger;
        }

        public Task<object> GetSystemLogsAsync()
        {
            _logger.LogInformation("获取系统日志");
            return Task.FromResult<object>(new List<object>());
        }
    }
}