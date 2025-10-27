using Microsoft.Extensions.Logging;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 雷达参数服务 - 临时简化版本
    /// </summary>
    public class RadarParamsService
    {
        private readonly ILogger<RadarParamsService> _logger;

        public RadarParamsService(ILogger<RadarParamsService> logger)
        {
            _logger = logger;
        }

        public async Task<object> GetRadarParamsAsync(string deviceId)
        {
            _logger.LogInformation("获取雷达参数: {DeviceId}", deviceId);
            return new { };
        }
    }
}