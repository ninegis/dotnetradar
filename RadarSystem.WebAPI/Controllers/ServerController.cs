using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// Server接口控制器 - 对应前端kotiotApiUrl的/api/server/*接口
    /// </summary>
    [ApiController]
    [Route("api/server")]
    [Authorize]
    public class ServerController : ControllerBase
    {
        private readonly ILogger<ServerController> _logger;

        public ServerController(ILogger<ServerController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 添加允许人员
        /// GET /api/server/addAllowPeople?name={name}&amp;phone={phone}&amp;project_code={project_code}
        /// </summary>
        [HttpGet("addAllowPeople")]
        public ApiResponse<object> AddAllowPeople(
            [FromQuery] string name,
            [FromQuery] string phone,
            [FromQuery] string project_code)
        {
            try
            {
                _logger.LogInformation("添加允许人员: {Name}, {Phone}, {ProjectCode}", name, phone, project_code);
                
                // TODO: 实现实际的人员管理逻辑
                // 1. 验证项目代码
                // 2. 添加到允许人员列表
                // 3. 保存到数据库
                
                return ApiResponse<object>.Ok(new
                {
                    success = true,
                    message = "添加允许人员成功",
                    id = Guid.NewGuid().ToString(),
                    name,
                    phone,
                    projectCode = project_code
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加允许人员失败");
                return ApiResponse<object>.Fail(500, ex.Message);
            }
        }

        /// <summary>
        /// 获取允许人员列表
        /// GET /api/server/getAllowPeople?project_code={project_code}
        /// </summary>
        [HttpGet("getAllowPeople")]
        public ApiResponse<List<AllowPerson>> GetAllowPeople([FromQuery] string project_code)
        {
            try
            {
                _logger.LogInformation("查询允许人员列表: {ProjectCode}", project_code);
                
                // TODO: 从数据库查询允许人员列表
                var allowPeople = new List<AllowPerson>
                {
                    new AllowPerson
                    {
                        Id = "1",
                        Name = "示例人员",
                        Phone = "13800138000",
                        ProjectCode = project_code
                    }
                };
                
                return ApiResponse<List<AllowPerson>>.Ok(allowPeople);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询允许人员列表失败");
                return ApiResponse<List<AllowPerson>>.Fail(500, ex.Message);
            }
        }

        /// <summary>
        /// 删除允许人员
        /// GET /api/server/delAllowPeople?id={id}
        /// </summary>
        [HttpGet("delAllowPeople")]
        public ApiResponse<object> DeleteAllowPeople([FromQuery] string id)
        {
            try
            {
                _logger.LogInformation("删除允许人员: {Id}", id);
                
                // TODO: 实现实际的删除逻辑
                
                return ApiResponse<object>.Ok(new
                {
                    success = true,
                    message = "删除允许人员成功",
                    id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除允许人员失败");
                return ApiResponse<object>.Fail(500, ex.Message);
            }
        }

        /// <summary>
        /// 获取用户地址（通过IP）
        /// POST /api/server/getuseraddress
        /// </summary>
        [HttpPost("getuseraddress")]
        public ApiResponse<UserAddress> GetUserAddress()
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                _logger.LogInformation("获取用户地址: {IpAddress}", ipAddress);
                
                // TODO: 通过IP地址获取实际地理位置（使用IP地理位置API）
                
                return ApiResponse<UserAddress>.Ok(new UserAddress
                {
                    IpAddress = ipAddress,
                    Country = "中国",
                    Province = "未知",
                    City = "未知",
                    Address = "基于IP地址的地理位置"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户地址失败");
                return ApiResponse<UserAddress>.Fail(500, ex.Message);
            }
        }

        /// <summary>
        /// 添加雷达操作日志
        /// POST /api/server/addradaroperatelog
        /// </summary>
        [HttpPost("addradaroperatelog")]
        public ApiResponse<object> AddRadarOperateLog(
            [FromQuery] string operate_content,
            [FromQuery] string operate_username,
            [FromQuery] string address,
            [FromQuery] string project_code,
            [FromQuery] string project_name)
        {
            try
            {
                _logger.LogInformation("添加操作日志: {User} - {Content}", operate_username, operate_content);
                
                // TODO: 保存到系统日志表
                
                return ApiResponse<object>.Ok(new
                {
                    success = true,
                    message = "操作日志已记录",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加操作日志失败");
                return ApiResponse<object>.Fail(500, ex.Message);
            }
        }
    }

    /// <summary>
    /// 允许人员信息
    /// </summary>
    public class AllowPerson
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户地址信息
    /// </summary>
    public class UserAddress
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}

