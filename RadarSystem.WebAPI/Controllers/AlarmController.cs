using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;
using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 告警管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlarmController : ControllerBase
    {
        private readonly IAlarmService _alarmService;
        private readonly ILogger<AlarmController> _logger;

        public AlarmController(IAlarmService alarmService, ILogger<AlarmController> logger)
        {
            _alarmService = alarmService;
            _logger = logger;
        }

        /// <summary>
        /// 查询报警记录
        /// </summary>
        [HttpGet("records")]
        public async Task<ActionResult<ApiResponse<List<AlarmRecord>>>> GetAlarmRecords([FromQuery] AlarmQueryRequest request)
        {
            try
            {
                var records = await _alarmService.QueryAlarmRecordsAsync(request);
                return Ok(ApiResponse<List<AlarmRecord>>.Ok(records));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询报警记录失败");
                return Ok(ApiResponse<List<AlarmRecord>>.Fail(500, $"查询报警记录失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 创建报警记录
        /// </summary>
        [HttpPost("records")]
        public async Task<ActionResult<ApiResponse<bool>>> CreateAlarmRecord([FromBody] AlarmRecord record)
        {
            try
            {
                var result = await _alarmService.CreateAlarmRecordAsync(record);
                return Ok(ApiResponse<bool>.Ok(result, "报警记录创建成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建报警记录失败");
                return Ok(ApiResponse<bool>.Fail(500, $"创建报警记录失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 报警统计（按等级）
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<Dictionary<AlarmLevel, int>>>> GetAlarmStatistics(
            [FromQuery] string projectId,
            [FromQuery] string[] ruleIds,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            try
            {
                var stats = await _alarmService.GetAlarmCountByLevelAsync(projectId, ruleIds, startTime, endTime);
                return Ok(ApiResponse<Dictionary<AlarmLevel, int>>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取报警统计失败");
                return Ok(ApiResponse<Dictionary<AlarmLevel, int>>.Fail(500, $"获取报警统计失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新报警处理状态
        /// </summary>
        [HttpPut("records/{handleId}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateAlarmStatus(string handleId, [FromBody] string status)
        {
            try
            {
                var result = await _alarmService.UpdateAlarmHandleStatusAsync(handleId, status);
                return Ok(ApiResponse<bool>.Ok(result, "报警状态更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新报警状态失败");
                return Ok(ApiResponse<bool>.Fail(500, $"更新报警状态失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 批量更新扫描状态
        /// </summary>
        [HttpPut("records/scan-status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateScanStatus([FromBody] UpdateScanStatusRequest request)
        {
            try
            {
                var result = await _alarmService.UpdateScanStatusAsync(request.HandleIds, request.ScanStatus);
                return Ok(ApiResponse<bool>.Ok(result, "扫描状态更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新扫描状态失败");
                return Ok(ApiResponse<bool>.Fail(500, $"更新扫描状态失败: {ex.Message}"));
            }
        }
    }

    public class UpdateScanStatusRequest
    {
        public string[] HandleIds { get; set; } = Array.Empty<string>();
        public string ScanStatus { get; set; } = string.Empty;
    }
}

