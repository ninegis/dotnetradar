using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.WebAPI.Models;
using RadarSystem.WebAPI.Services;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 数据回滚/恢复控制器
    /// </summary>
    [ApiController]
    [Route("api/rollback")]
    [Authorize]
    public class RollbackController : ControllerBase
    {
        private readonly IDataManageService _dataManageService;
        private readonly ILogger<RollbackController> _logger;

        public RollbackController(
            IDataManageService dataManageService,
            ILogger<RollbackController> logger)
        {
            _dataManageService = dataManageService;
            _logger = logger;
        }

        /// <summary>
        /// 验证并恢复地理位置设备数据
        /// POST /api/rollback/validate/geo/device
        /// </summary>
        [HttpPost("validate/geo/device")]
        public async Task<IActionResult> ValidateAndRestoreGeoDevice([FromBody] RollbackRequest request)
        {
            try
            {
                _logger.LogInformation("数据恢复请求: {ProjectId}/{DeviceId}/{GeoMaskId}, 时间范围: {StartTime}-{EndTime}", 
                    request.ProjectId, request.DeviceId, request.GeoMaskId, request.StartTime, request.EndTime);
                
                var restoreRequest = new RadarSystem.WebAPI.Models.DataRestoreRequest
                {
                    ProjectId = request.ProjectId,
                    DeviceId = request.DeviceId,
                    GeoMaskId = request.GeoMaskId,  // 使用GeoMaskId
                    GeoMaskType = request.GeoMaskType,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime
                };
                
                await _dataManageService.RestoreDataAsync(restoreRequest);
                
                return Ok(new { code = 200, message = "数据恢复任务已提交" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据恢复失败");
                return Ok(new { code = 500, message = $"恢复失败: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// 数据回滚请求参数
    /// </summary>
    public class RollbackRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string GeoMaskId { get; set; } = string.Empty;
        public string GeoMaskType { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string RollbackStatus { get; set; } = "unstart";
        public string DataType { get; set; } = "10";
        public string DeleteStatus { get; set; } = "false";
    }
}

