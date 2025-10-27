using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 雷达命令控制器 - 对应前端雷达控制接口
    /// </summary>
    [ApiController]
    [Authorize]
    public class RadarCommandController : ControllerBase
    {
        private readonly ILogger<RadarCommandController> _logger;

        public RadarCommandController(ILogger<RadarCommandController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 发送ArcSAR雷达控制命令
        /// GET /api/arcsar/command/{projectId}/{deviceId}/{command}/{userName}
        /// </summary>
        [HttpGet("/api/arcsar/command/{projectId}/{deviceId}/{command}/{userName}")]
        public async Task<ApiResponse<object>> SendArcsarCommand(
            string projectId, 
            string deviceId, 
            string command, 
            string userName)
        {
            try
            {
                _logger.LogInformation("发送ArcSAR命令: Project={ProjectId}, Device={DeviceId}, Command={Command}, User={UserName}", 
                    projectId, deviceId, command, userName);

                // TODO: 实现实际的雷达控制逻辑
                // 1. 验证设备存在且在线
                // 2. 通过Netty或MQTT发送命令到设备
                // 3. 等待设备响应

                return ApiResponse<object>.Ok(new
                {
                    success = true,
                    message = $"命令 {command} 已发送到设备 {deviceId}",
                    projectId,
                    deviceId,
                    command,
                    userName,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送ArcSAR命令失败");
                return ApiResponse<object>.Fail(500, ex.Message);
            }
        }

        /// <summary>
        /// 发送MimoLite雷达控制命令
        /// GET /api/mimoLite/command/{projectId}/{deviceId}/{command}/{userName}
        /// </summary>
        [HttpGet("/api/mimoLite/command/{projectId}/{deviceId}/{command}/{userName}")]
        public async Task<ApiResponse<object>> SendMimoLiteCommand(
            string projectId, 
            string deviceId, 
            string command, 
            string userName)
        {
            try
            {
                _logger.LogInformation("发送MimoLite命令: Project={ProjectId}, Device={DeviceId}, Command={Command}, User={UserName}", 
                    projectId, deviceId, command, userName);

                // TODO: 实现实际的雷达控制逻辑
                // 1. 验证设备存在且在线
                // 2. 通过Netty或MQTT发送命令到设备
                // 3. 等待设备响应

                return ApiResponse<object>.Ok(new
                {
                    success = true,
                    message = $"命令 {command} 已发送到设备 {deviceId}",
                    projectId,
                    deviceId,
                    command,
                    userName,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送MimoLite命令失败");
                return ApiResponse<object>.Fail(500, ex.Message);
            }
        }
    }
}

