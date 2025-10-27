using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.Core.Interfaces;
using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 自定义接口控制器 - 对应前端 /api/custom/* 接口
    /// </summary>
    [ApiController]
    [Route("api/custom")]
    [Authorize]
    public class CustomController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ISystemConfigService _systemConfigService;
        private readonly IRadarControlService _radarControlService;
        private readonly IAlarmRecordService _alarmRecordService;
        private readonly ILogger<CustomController> _logger;

        public CustomController(
            IProjectService projectService,
            ISystemConfigService systemConfigService,
            IRadarControlService radarControlService,
            IAlarmRecordService alarmRecordService,
            ILogger<CustomController> logger)
        {
            _projectService = projectService;
            _systemConfigService = systemConfigService;
            _radarControlService = radarControlService;
            _alarmRecordService = alarmRecordService;
            _logger = logger;
        }

        #region 项目管理

        /// <summary>
        /// 更新项目信息
        /// POST /api/custom/updateProjectInfo
        /// </summary>
        [HttpPost("updateProjectInfo")]
        public async Task<IActionResult> UpdateProjectInfo([FromBody] UpdateProjectInfoRequest request)
        {
            try
            {
                await _projectService.UpdateProjectInfoAsync(request);
                return Ok(new { code = 200, message = "项目信息更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新项目信息失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        #endregion

        #region 雷达控制

        /// <summary>
        /// 控制雷达
        /// POST /api/custom/controlRadar
        /// </summary>
        [HttpPost("controlRadar")]
        public async Task<IActionResult> ControlRadar([FromBody] RadarControlRequest request)
        {
            try
            {
                await _radarControlService.ControlRadarAsync(request);
                return Ok(new { code = 200, message = "雷达控制指令已发送" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "雷达控制失败");
                return Ok(new { code = 500, message = $"控制失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 设置参数控制
        /// POST /api/custom/set/param/control
        /// </summary>
        [HttpPost("set/param/control")]
        public async Task<IActionResult> SetParamControl([FromBody] SetParamControlRequest request)
        {
            try
            {
                await _radarControlService.SetParamControlAsync(request);
                return Ok(new { code = 200, message = "参数控制设置成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置参数控制失败");
                return Ok(new { code = 500, message = $"设置失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 设置MIMO Lite参数控制
        /// POST /api/custom/set/mimolite/param/control
        /// </summary>
        [HttpPost("set/mimolite/param/control")]
        public async Task<IActionResult> SetMimoLiteParamControl([FromBody] SetParamControlRequest request)
        {
            try
            {
                await _radarControlService.SetMimoLiteParamControlAsync(request);
                return Ok(new { code = 200, message = "MIMO Lite参数控制设置成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置MIMO Lite参数控制失败");
                return Ok(new { code = 500, message = $"设置失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新俯仰电机角度
        /// POST /api/custom/update/tiltMotor/pitch
        /// </summary>
        [HttpPost("update/tiltMotor/pitch")]
        public async Task<IActionResult> UpdateTiltMotorPitch([FromBody] UpdateTiltMotorRequest request)
        {
            try
            {
                await _radarControlService.UpdateTiltMotorPitchAsync(request);
                return Ok(new { code = 200, message = "俯仰电机角度更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新俯仰电机角度失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        #endregion

        #region 系统配置

        /// <summary>
        /// 获取磁盘存储配置
        /// GET /api/custom/getDiskStorage
        /// </summary>
        [HttpGet("getDiskStorage")]
        public async Task<IActionResult> GetDiskStorage()
        {
            try
            {
                var config = await _systemConfigService.GetDiskStorageAsync();
                return Ok(new { code = 200, data = config });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取磁盘存储配置失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新磁盘存储配置
        /// POST /api/custom/updateDiskStorage
        /// </summary>
        [HttpPost("updateDiskStorage")]
        public async Task<IActionResult> UpdateDiskStorage([FromBody] UpdateDiskStorageRequest request)
        {
            try
            {
                await _systemConfigService.UpdateDiskStorageAsync(request);
                return Ok(new { code = 200, message = "磁盘存储配置更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新磁盘存储配置失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取磁盘阈值
        /// GET /api/custom/getDiskThreshold
        /// </summary>
        [HttpGet("getDiskThreshold")]
        public async Task<IActionResult> GetDiskThreshold()
        {
            try
            {
                var threshold = await _systemConfigService.GetDiskThresholdAsync();
                return Ok(new { code = 200, data = threshold });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取磁盘阈值失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 添加告警消息
        /// POST /api/custom/addAlarmMessage
        /// </summary>
        [HttpPost("addAlarmMessage")]
        public async Task<IActionResult> AddAlarmMessage([FromBody] AddAlarmMessageRequest request)
        {
            try
            {
                await _alarmRecordService.AddAlarmMessageAsync(request);
                return Ok(new { code = 200, message = "告警消息添加成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加告警消息失败");
                return Ok(new { code = 500, message = $"添加失败: {ex.Message}" });
            }
        }

        #endregion
    }
}

