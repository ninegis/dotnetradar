using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadarSystem.Core.Models;
using RadarSystem.Data.Context;
using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 设备管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeviceController : ControllerBase
    {
        private readonly RadarDbContext _dbContext;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(RadarDbContext dbContext, ILogger<DeviceController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取设备列表（包含雷达参数和算法参数）
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<Device>>>> GetDevices([FromQuery] string? projectId = null)
        {
            try
            {
                var query = _dbContext.Devices.AsQueryable();
                
                if (!string.IsNullOrEmpty(projectId))
                {
                    query = query.Where(d => d.ProjectId == projectId);
                }
                
                var deviceEntities = await query
                    .OrderByDescending(d => d.CreateTime)
                    .ToListAsync();

                var devices = new List<Device>();
                
                foreach (var d in deviceEntities)
                {
                    // ✅ 查询雷达参数
                    var radarParam = await _dbContext.RadarParams
                        .FirstOrDefaultAsync(rp => rp.ProjectId == d.ProjectId && rp.DeviceId == d.DeviceId);
                    
                    // ✅ 查询算法参数
                    var algoParam = await _dbContext.AlgorithmConfigs
                        .FirstOrDefaultAsync(ap => ap.ProjectId == d.ProjectId && ap.DeviceId == d.DeviceId);
                    
                    var device = new Device
                    {
                        Id = d.Id,
                        DeviceId = d.DeviceId,
                        DeviceName = d.DeviceName,
                        DeviceType = d.DeviceType,
                        DeviceTypeCode = d.DeviceTypeCode,
                        ProjectId = d.ProjectId,
                        Status = d.Status,
                        IpAddress = d.IpAddress,
                        Port = d.Port,
                        Longitude = d.Longitude,
                        Latitude = d.Latitude,
                        Elevation = d.Elevation,
                        SlaveId = d.SlaveId,
                        Orientation = d.Orientation,
                        Description = d.Description,
                        LastUpdateTime = d.LastUpdateTime,
                        CreateTime = d.CreateTime,
                        UpdateTime = d.UpdateTime,
                        // ✅ 填充雷达参数
                        Params = radarParam != null ? new Dictionary<string, object>
                        {
                            { "radarOri", d.Orientation },
                            { "ImgAngleStart", radarParam.ImgAngleStart },
                            { "ImgAngleEnd", radarParam.ImgAngleEnd },
                            { "RngMin", radarParam.RngMin },
                            { "RngMax", radarParam.RngMax },
                            { "FreqBand", radarParam.FreqBand },
                            { "AnteBeam_half", radarParam.AnteBeamHalf },
                            { "dataVersion", radarParam.DataVersion },
                            { "modelSelect", radarParam.ModelSelect ?? "" }
                        } : new Dictionary<string, object>
                        {
                            { "radarOri", d.Orientation },
                            { "ImgAngleStart", 0.0 },
                            { "ImgAngleEnd", 360.0 },
                            { "RngMin", 0.0 },
                            { "RngMax", 1000.0 },
                            { "FreqBand", "0" },
                            { "AnteBeam_half", 60.0 },
                            { "dataVersion", "0" },
                            { "modelSelect", "" }
                        },
                        // ✅ 填充算法参数（如果有）
                        AlgorithmParam = algoParam != null ? new Dictionary<string, object>
                        {
                            // TODO: 根据设备类型填充不同的算法参数
                        } : new Dictionary<string, object>()
                    };
                    
                    devices.Add(device);
                }
                    
                return Ok(ApiResponse<List<Device>>.Ok(devices));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备列表失败");
                return Ok(ApiResponse<List<Device>>.Fail(500, $"获取设备列表失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取设备详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Device>>> GetDevice(string id)
        {
            try
            {
                var entity = await _dbContext.Devices
                    .FirstOrDefaultAsync(d => d.DeviceId == id);
                
                if (entity == null)
                {
                    return Ok(ApiResponse<Device>.Fail(404, "设备不存在"));
                }

                var device = new Device
                {
                    Id = entity.Id,
                    DeviceId = entity.DeviceId,
                    DeviceName = entity.DeviceName,
                    DeviceType = entity.DeviceType,
                    ProjectId = entity.ProjectId,
                    Status = entity.Status,
                    IpAddress = entity.IpAddress,
                    Port = entity.Port,
                    Longitude = entity.Longitude,
                    Latitude = entity.Latitude,
                    Elevation = entity.Elevation,
                    Description = entity.Description,
                    CreateTime = entity.CreateTime,
                    UpdateTime = entity.UpdateTime
                };

                return Ok(ApiResponse<Device>.Ok(device));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备详情失败");
                return Ok(ApiResponse<Device>.Fail(500, $"获取设备详情失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 创建设备
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> CreateDevice([FromBody] CreateDeviceRequest request)
        {
            try
            {
                // ✅ 检查DeviceId是否已存在（唯一性约束）
                var exists = await _dbContext.Devices.AnyAsync(d => d.DeviceId == request.DeviceId);
                if (exists)
                {
                    return Ok(ApiResponse<int>.Fail(400, $"设备ID '{request.DeviceId}' 已存在"));
                }

                // ✅ 检查SlaveId是否已存在（唯一性约束）
                if (!string.IsNullOrEmpty(request.SlaveId))
                {
                    var slaveIdExists = await _dbContext.Devices.AnyAsync(d => d.SlaveId == request.SlaveId);
                    if (slaveIdExists)
                    {
                        return Ok(ApiResponse<int>.Fail(400, $"SlaveId '{request.SlaveId}' 已存在，请使用不同的SlaveId"));
                    }
                }

                // 检查项目是否存在
                var projectExists = await _dbContext.Projects.AnyAsync(p => p.ProjectId == request.ProjectId);
                if (!projectExists)
                {
                    return Ok(ApiResponse<int>.Fail(400, $"项目ID '{request.ProjectId}' 不存在"));
                }

                var entity = new Data.Models.DeviceEntity
                {
                    DeviceId = request.DeviceId,
                    DeviceName = request.DeviceName,
                    DeviceType = request.DeviceType,
                    ProjectId = request.ProjectId,
                    Status = "Active",
                    IpAddress = request.IpAddress,
                    Port = request.Port,
                    Longitude = request.Longitude,
                    Latitude = request.Latitude,
                    Elevation = request.Elevation,
                    SlaveId = request.SlaveId ?? string.Empty,
                    Orientation = request.Orientation,
                    Description = request.Description,
                    LastUpdateTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };

                _dbContext.Devices.Add(entity);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("设备创建成功: {DeviceId}", request.DeviceId);
                return Ok(ApiResponse<int>.Ok(entity.Id, "设备创建成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建设备失败");
                return Ok(ApiResponse<int>.Fail(500, $"创建设备失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新设备
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateDevice(string id, [FromBody] Device device)
        {
            try
            {
                var entity = await _dbContext.Devices.FirstOrDefaultAsync(d => d.DeviceId == id);
                if (entity == null)
                {
                    return Ok(ApiResponse<bool>.Fail(404, "设备不存在"));
                }

                // ✅ 检查SlaveId是否与其他设备冲突（唯一性约束）
                if (!string.IsNullOrEmpty(device.SlaveId) && device.SlaveId != entity.SlaveId)
                {
                    if (await _dbContext.Devices.AnyAsync(d => d.SlaveId == device.SlaveId && d.DeviceId != id))
                    {
                        return Ok(ApiResponse<bool>.Fail(400, $"SlaveId={device.SlaveId}已被其他设备使用，请使用不同的SlaveId"));
                    }
                }

                entity.DeviceName = device.DeviceName;
                entity.DeviceType = device.DeviceType;
                entity.Status = device.Status;
                entity.IpAddress = device.IpAddress;
                entity.Port = device.Port;
                entity.Longitude = device.Longitude;
                entity.Latitude = device.Latitude;
                entity.Elevation = device.Elevation;
                entity.SlaveId = device.SlaveId;
                entity.Orientation = device.Orientation;
                entity.Description = device.Description;
                entity.LastUpdateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("设备更新成功: {DeviceId}", id);
                return Ok(ApiResponse<bool>.Ok(true, "设备更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新设备失败");
                return Ok(ApiResponse<bool>.Fail(500, $"更新设备失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDevice(string id)
        {
            try
            {
                var entity = await _dbContext.Devices.FirstOrDefaultAsync(d => d.DeviceId == id);
                if (entity == null)
                {
                    return Ok(ApiResponse<bool>.Fail(404, "设备不存在"));
                }

                _dbContext.Devices.Remove(entity);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("设备删除成功: {DeviceId}", id);
                return Ok(ApiResponse<bool>.Ok(true, "设备删除成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除设备失败");
                return Ok(ApiResponse<bool>.Fail(500, $"删除设备失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取设备类型列表
        /// </summary>
        [HttpGet("types")]
        public ActionResult<ApiResponse<List<RadarSystem.WebAPI.Models.DeviceTypeInfo>>> GetDeviceTypes()
        {
            try
            {
                var types = new List<object>
                {
                    new { Id = 0, Name = "边坡雷达", Category = "雷达" },
                    new { Id = 1, Name = "视频", Category = "视频" },
                    new { Id = 2, Name = "气象站", Category = "传感器" },
                    new { Id = 3, Name = "GNSS", Category = "定位" },
                    new { Id = 4, Name = "建筑物雷达", Category = "雷达" },
                    new { Id = 5, Name = "边坡雷达Mini", Category = "雷达" },
                    new { Id = 6, Name = "建筑物雷达2D", Category = "雷达" },
                    new { Id = 7, Name = "MIMO雷达", Category = "雷达" },
                    new { Id = 8, Name = "普适雷达", Category = "雷达" },
                    new { Id = 9, Name = "球形摄像机", Category = "视频" },
                    new { Id = 10, Name = "测斜计", Category = "传感器" },
                    new { Id = 11, Name = "振动传感器", Category = "传感器" },
                    new { Id = 12, Name = "电机(外设)", Category = "控制" }
                };

                return Ok(ApiResponse<List<object>>.Ok(types));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备类型列表失败");
                return Ok(ApiResponse<List<object>>.Fail(500, $"获取设备类型列表失败: {ex.Message}"));
            }
        }
    }
}

