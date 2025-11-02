using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.WebAPI.Models;
using System.Diagnostics;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 配置管理控制器
    /// </summary>
    [ApiController]
    [Route("api/config")]
    [Authorize]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(
            IConfiguration configuration,
            ILogger<ConfigController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 获取配置信息
        /// GET /api/config/info
        /// </summary>
        [HttpGet("info")]
        public ApiResponse<ConfigInfo> GetConfigInfo()
        {
            try
            {
                var configInfo = new ConfigInfo
                {
                    // 系统配置
                    SystemName = "边坡雷达监测系统",
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    
                    // 数据库配置
                    DatabaseType = "SQLite + TDengine",
                    DatabasePath = _configuration.GetConnectionString("DefaultConnection"),
                    
                    // 磁盘阈值配置（从配置文件读取，如果没有则使用默认值）
                    DiskSpaceThreshold = _configuration.GetValue<int>("DiskSpaceThreshold", 10), // 默认10%
                    DiskSpaceThresholdGB = _configuration.GetValue<int>("DiskSpaceThresholdGB", 50), // 默认50GB
                    
                    // MQTT配置
                    MqttBrokerHost = _configuration.GetValue<string>("Mqtt:BrokerHost", "localhost"),
                    MqttBrokerPort = _configuration.GetValue<int>("Mqtt:BrokerPort", 1883),
                    
                    // TDengine配置
                    TDengineHost = _configuration.GetValue<string>("TDengine:Host", "localhost"),
                    TDenginePort = _configuration.GetValue<int>("TDengine:Port", 6030),
                    
                    // 系统设置
                    MaxUploadFileSize = _configuration.GetValue<long>("MaxUploadFileSize", 100 * 1024 * 1024), // 默认100MB
                    DataRetentionDays = _configuration.GetValue<int>("DataRetentionDays", 365), // 默认保留365天
                    
                    // API设置
                    ApiBaseUrl = "http://localhost:8099",
                    SwaggerEnabled = true,
                    
                    // 其他配置
                    ServerTime = DateTime.Now,
                    ServerTimeZone = TimeZoneInfo.Local.DisplayName
                };

                _logger.LogInformation("获取配置信息成功");

                return ApiResponse<ConfigInfo>.Ok(configInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取配置信息失败");
                return ApiResponse<ConfigInfo>.Fail(500, ex.Message);
            }
        }

        /// <summary>
        /// 更新磁盘阈值配置
        /// POST /api/config/diskThreshold
        /// </summary>
        [HttpPost("diskThreshold")]
        public ApiResponse<object> UpdateDiskThreshold([FromBody] DiskThresholdConfig config)
        {
            try
            {
                // TODO: 实现配置更新逻辑（保存到配置文件或数据库）
                _logger.LogInformation("更新磁盘阈值配置: {ThresholdPercent}%, {ThresholdGB}GB", 
                    config.ThresholdPercent, config.ThresholdGB);

                return ApiResponse<object>.Ok(new
                {
                    success = true,
                    message = "磁盘阈值配置更新成功",
                    config
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新磁盘阈值配置失败");
                return ApiResponse<object>.Fail(500, ex.Message);
            }
        }

        /// <summary>
        /// 获取系统状态
        /// GET /api/config/status
        /// </summary>
        [HttpGet("status")]
        public ApiResponse<SystemStatus> GetSystemStatus()
        {
            try
            {
                var status = new SystemStatus
                {
                    IsOnline = true,
                    ServerTime = DateTime.Now,
                    Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime,
                    CpuUsage = GetCpuUsage(),
                    MemoryUsage = GetMemoryUsage(),
                    ActiveConnections = 0 // TODO: 实现实际的连接数统计
                };

                return ApiResponse<SystemStatus>.Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统状态失败");
                return ApiResponse<SystemStatus>.Fail(500, ex.Message);
            }
        }

        private double GetCpuUsage()
        {
            // TODO: 实现实际的CPU使用率获取
            return 0.0;
        }

        private double GetMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / 1024.0 / 1024.0;
            return memoryMB;
        }
    }

    /// <summary>
    /// 配置信息
    /// </summary>
    public class ConfigInfo
    {
        public string SystemName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = string.Empty;
        public string DatabasePath { get; set; } = string.Empty;
        public int DiskSpaceThreshold { get; set; }
        public int DiskSpaceThresholdGB { get; set; }
        public string MqttBrokerHost { get; set; } = string.Empty;
        public int MqttBrokerPort { get; set; }
        public string TDengineHost { get; set; } = string.Empty;
        public int TDenginePort { get; set; }
        public long MaxUploadFileSize { get; set; }
        public int DataRetentionDays { get; set; }
        public string ApiBaseUrl { get; set; } = string.Empty;
        public bool SwaggerEnabled { get; set; }
        public DateTime ServerTime { get; set; }
        public string ServerTimeZone { get; set; } = string.Empty;
    }

    /// <summary>
    /// 磁盘阈值配置
    /// </summary>
    public class DiskThresholdConfig
    {
        public int ThresholdPercent { get; set; }
        public int ThresholdGB { get; set; }
    }

    /// <summary>
    /// 系统状态
    /// </summary>
    public class SystemStatus
    {
        public bool IsOnline { get; set; }
        public DateTime ServerTime { get; set; }
        public TimeSpan Uptime { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public int ActiveConnections { get; set; }
    }
}

