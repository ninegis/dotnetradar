using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.Core.Interfaces;
using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// Kotiot接口控制器 - 对应前端 kotiotApiUrl 接口
    /// </summary>
    [ApiController]
    [Route("api/kotiot")]
    [Authorize]
    public class KotiotController : ControllerBase
    {
        private readonly IRadarImageService _radarImageService;
        private readonly IDataManageService _dataManageService;
        private readonly IDeviceService _deviceService;
        private readonly ILogger<KotiotController> _logger;

        public KotiotController(
            IRadarImageService radarImageService,
            IDataManageService dataManageService,
            IDeviceService deviceService,
            ILogger<KotiotController> logger)
        {
            _radarImageService = radarImageService;
            _dataManageService = dataManageService;
            _deviceService = deviceService;
            _logger = logger;
        }

        #region 雷达图像

        /// <summary>
        /// 查询图像数量
        /// GET /api/kotiot/radar/image/count
        /// </summary>
        [HttpGet("radar/image/count")]
        public async Task<IActionResult> QueryImageCount(
            [FromQuery] string projectId,
            [FromQuery] string deviceId,
            [FromQuery] string startDateTime,
            [FromQuery] string endDateTime,
            [FromQuery] string? type,
            [FromQuery] string? status)
        {
            try
            {
                var count = await _radarImageService.GetImageCountAsync(
                    projectId, deviceId, startDateTime, endDateTime, type, status);
                return Ok(new { code = 200, data = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询图像数量失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 查询图像列表
        /// GET /api/kotiot/radar/image/list
        /// </summary>
        [HttpGet("radar/image/list")]
        public async Task<IActionResult> QueryImageList(
            [FromQuery] string projectId,
            [FromQuery] string deviceId,
            [FromQuery] string startDateTime,
            [FromQuery] string endDateTime,
            [FromQuery] string? type,
            [FromQuery] string? status,
            [FromQuery] int count = 100)
        {
            try
            {
                var images = await _radarImageService.GetImageListAsync(
                    projectId, deviceId, startDateTime, endDateTime, type, status, count);
                return Ok(new { code = 200, data = images });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询图像列表失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取图像资源
        /// GET /api/kotiot/image/getResource
        /// </summary>
        [HttpGet("image/getResource")]
        public async Task<IActionResult> GetImageResource(
            [FromQuery] string url,
            [FromQuery] string filename)
        {
            try
            {
                var imageData = await _radarImageService.GetImageResourceAsync(url, filename);
                return File(imageData, "image/png", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取图像资源失败");
                return NotFound(new { code = 404, message = $"图像不存在: {ex.Message}" });
            }
        }

        /// <summary>
        /// 生成雷达图像
        /// POST /api/kotiot/radar/image/generate
        /// </summary>
        [HttpPost("radar/image/generate")]
        public async Task<IActionResult> GenerateImage([FromBody] GenerateImageRequest request)
        {
            try
            {
                var result = await _radarImageService.GenerateImageAsync(request);
                return Ok(new { code = 200, data = result, message = "图像生成任务已提交" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成雷达图像失败");
                return Ok(new { code = 500, message = $"生成失败: {ex.Message}" });
            }
        }

        #endregion

        #region 数据管理

        /// <summary>
        /// 数据恢复
        /// POST /api/kotiot/data/restore
        /// </summary>
        [HttpPost("data/restore")]
        public async Task<IActionResult> DataRestore([FromBody] DataRestoreRequest request)
        {
            try
            {
                await _dataManageService.RestoreDataAsync(request);
                return Ok(new { code = 200, message = "数据恢复任务已提交" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据恢复失败");
                return Ok(new { code = 500, message = $"恢复失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 数据生成
        /// POST /api/kotiot/data/generate
        /// </summary>
        [HttpPost("data/generate")]
        public async Task<IActionResult> DataGenerate([FromBody] DataGenerateRequest request)
        {
            try
            {
                await _dataManageService.GenerateDataAsync(request);
                return Ok(new { code = 200, message = "数据生成任务已提交" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据生成失败");
                return Ok(new { code = 500, message = $"生成失败: {ex.Message}" });
            }
        }

        #endregion

        #region 设备状态

        /// <summary>
        /// 获取雷达在线状态（按时间）
        /// GET /api/kotiot/radar/onlineStatus
        /// </summary>
        [HttpGet("radar/onlineStatus")]
        public async Task<IActionResult> GetRadarOnlineStatus(
            [FromQuery] string deviceId,
            [FromQuery] string datetime)
        {
            try
            {
                var status = await _deviceService.GetRadarOnlineStatusByTimeAsync(deviceId, datetime);
                return Ok(new { code = 200, data = status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取雷达在线状态失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取雷达最后心跳时间
        /// GET /api/kotiot/radar/heartbeat
        /// </summary>
        [HttpGet("radar/heartbeat")]
        public async Task<IActionResult> GetRadarHeartbeat([FromQuery] string deviceId)
        {
            try
            {
                var heartbeat = await _deviceService.GetRadarLastHeartbeatAsync(deviceId);
                return Ok(new { code = 200, data = heartbeat });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取雷达心跳时间失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        #endregion
    }
}

