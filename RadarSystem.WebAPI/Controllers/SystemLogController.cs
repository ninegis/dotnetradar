using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.Core.Interfaces;
using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 系统日志控制器
    /// </summary>
    [ApiController]
    [Route("api/log")]
    [Authorize]
    public class SystemLogController : ControllerBase
    {
        private readonly ISystemLogService _systemLogService;
        private readonly ILogger<SystemLogController> _logger;

        public SystemLogController(
            ISystemLogService systemLogService,
            ILogger<SystemLogController> logger)
        {
            _systemLogService = systemLogService;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户IP地址信息
        /// GET /api/log/ipaddress
        /// </summary>
        [HttpGet("ipaddress")]
        public async Task<IActionResult> GetUserAddress()
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var addressInfo = await _systemLogService.GetAddressByIpAsync(ipAddress);
                return Ok(new { code = 200, data = addressInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取IP地址信息失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 添加雷达操作日志
        /// POST /api/log/radar
        /// </summary>
        [HttpPost("radar")]
        public async Task<IActionResult> AddRadarLog([FromBody] AddRadarLogRequest request)
        {
            try
            {
                await _systemLogService.AddRadarLogAsync(request);
                return Ok(new { code = 200, message = "日志添加成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加雷达日志失败");
                return Ok(new { code = 500, message = $"添加失败: {ex.Message}" });
            }
        }
    }
}

