using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.WebAPI.Models;
using RadarSystem.WebAPI.Services;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 告警记录控制器
    /// </summary>
    [ApiController]
    [Route("api/alarmNotify")]
    [Authorize]
    public class AlarmRecordController : ControllerBase
    {
        private readonly IAlarmRecordService _alarmRecordService;
        private readonly ILogger<AlarmRecordController> _logger;

        public AlarmRecordController(
            IAlarmRecordService alarmRecordService,
            ILogger<AlarmRecordController> logger)
        {
            _alarmRecordService = alarmRecordService;
            _logger = logger;
        }

        /// <summary>
        /// 查询告警记录数量
        /// POST /api/alarmNotify/recordList/count
        /// </summary>
        [HttpPost("recordList/count")]
        public async Task<IActionResult> QueryAlarmRecordCount([FromBody] RadarSystem.WebAPI.Models.AlarmRecordQueryRequest request)
        {
            try
            {
                var count = await _alarmRecordService.GetAlarmRecordCountAsync(request);
                return Ok(new { code = 200, data = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询告警记录数量失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 查询告警记录列表
        /// POST /api/alarmNotify/recordList/list
        /// </summary>
        [HttpPost("recordList/list")]
        public async Task<IActionResult> QueryAlarmRecord([FromBody] RadarSystem.WebAPI.Models.AlarmRecordQueryRequest request)
        {
            try
            {
                var records = await _alarmRecordService.GetAlarmRecordsAsync(request);
                return Ok(new { code = 200, data = records });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询告警记录失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }
    }
}

