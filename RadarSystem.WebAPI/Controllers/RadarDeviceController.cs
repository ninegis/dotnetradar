using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.Core.Interfaces;
using RadarSystem.WebAPI.Models;
using RadarSystem.WebAPI.Services;
using IDeviceService = RadarSystem.Core.Interfaces.IDeviceService;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 雷达设备状态和数据管理控制器
    /// </summary>
    [ApiController]
    [Route("api/radar")]
    [Authorize]
    public class RadarDeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly RadarSystem.Core.Interfaces.IDataManageService _dataManageService;
        private readonly ILogger<RadarDeviceController> _logger;

        public RadarDeviceController(
            IDeviceService deviceService,
            RadarSystem.Core.Interfaces.IDataManageService dataManageService,
            ILogger<RadarDeviceController> logger)
        {
            _deviceService = deviceService;
            _dataManageService = dataManageService;
            _logger = logger;
        }

        /// <summary>
        /// 获取雷达最后心跳时间
        /// </summary>
        /// <remarks>GET /api/radar/lastheartbeat</remarks>
        [HttpGet("lastheartbeat")]
        public async Task<IActionResult> GetLastHeartbeat([FromQuery] string url, [FromQuery] string deviceId)
        {
            try
            {
                // TODO: 实现实际的心跳时间查询逻辑
                var heartbeat = new { lastHeartbeat = DateTime.Now, deviceId, status = "online" };
                return Ok(new { code = 200, data = heartbeat });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取雷达心跳时间失败: {DeviceId}", deviceId);
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取雷达在线状态（按时间）
        /// </summary>
        /// <remarks>GET /api/radar/lastonline</remarks>
        [HttpGet("lastonline")]
        public async Task<IActionResult> GetLastOnlineStatus(
            [FromQuery] string url, 
            [FromQuery] string deviceId, 
            [FromQuery] string datetime)
        {
            try
            {
                // TODO: 实现实际的在线状态查询逻辑
                var status = new { deviceId, datetime, status = "online", isOnline = true };
                return Ok(new { code = 200, data = status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取雷达在线状态失败: {DeviceId}, {DateTime}", deviceId, datetime);
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 生成雷达数据（按时间间隔）
        /// POST /api/radar/generatedatabyinterval
        /// </summary>
        [HttpPost("generatedatabyinterval")]
        public async Task<IActionResult> GenerateDataByInterval([FromBody] GenerateDataRequest request)
        {
            try
            {
                _logger.LogInformation("生成雷达数据: {ProjectId}/{DeviceId}, 时间范围: {StartTime}-{EndTime}", 
                    request.ProjectId, request.DeviceId, request.StartTime, request.EndTime);
                
                var dataRequest = new RadarSystem.WebAPI.Models.DataGenerateRequest
                {
                    ProjectId = request.ProjectId,
                    DeviceId = request.DeviceId,
                    GeoMarkId = request.MarkId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Interval = request.Interval,
                    MaxValue = request.MaxValue,
                    MinValue = request.MinValue,
                    Target = request.Target,
                    CurrentValue = request.CurrentValue
                };
                
                await _dataManageService.GenerateDataAsync(dataRequest);
                
                return Ok(new { code = 200, message = "数据生成任务已提交" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成雷达数据失败");
                return Ok(new { code = 500, message = $"生成失败: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// 生成数据请求参数
    /// </summary>
    public class GenerateDataRequest
    {
        public string Url { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int Interval { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public string MarkId { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
    }
}

