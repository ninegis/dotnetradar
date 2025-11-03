using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadarSystem.Core.Models;
using RadarSystem.Data.Context;
using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 项目管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly RadarDbContext _dbContext;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(RadarDbContext dbContext, ILogger<ProjectController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取项目列表
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<Project>>>> GetProjects()
        {
            try
            {
                var projectEntities = await _dbContext.Projects
                    .Where(p => p.Status == "Active")
                    .OrderByDescending(p => p.CreateTime)
                    .ToListAsync();

                var projects = new List<Project>();
                
                foreach (var e in projectEntities)
                {
                    // 获取该项目下的所有设备
                    var deviceEntities = await _dbContext.Devices
                        .Where(d => d.ProjectId == e.ProjectId)
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

                    var project = new Project
                    {
                        Id = e.Id,
                        ProjectId = e.ProjectId,
                        ProjectName = e.ProjectName,
                        Description = e.Description,
                        Location = e.Location,
                        Status = e.Status,
                        CreatedBy = e.CreatedBy,
                        StoragePath = e.StoragePath,
                        ContactPerson = e.ContactPerson,
                        ContactPhone = e.ContactPhone,
                        ContactEmail = e.ContactEmail,
                        Longitude = e.Longitude,
                        Latitude = e.Latitude,
                        Elevation = e.Elevation,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        CreateTime = e.CreateTime,
                        UpdateTime = e.UpdateTime,
                        // ✅ 场景字段
                        SceneLongitude = e.SceneLongitude,
                        SceneLatitude = e.SceneLatitude,
                        SceneHeight = e.SceneHeight,
                        SceneHeading = e.SceneHeading,
                        ScenePitch = e.ScenePitch,
                        SceneRoll = e.SceneRoll,
                        Devices = devices
                    };
                    
                    projects.Add(project);
                }

                return Ok(ApiResponse<List<Project>>.Ok(projects));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取项目列表失败");
                return Ok(ApiResponse<List<Project>>.Fail(500, $"获取项目列表失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取项目详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Project>>> GetProject(string id)
        {
            try
            {
                var entity = await _dbContext.Projects
                    .FirstOrDefaultAsync(p => p.ProjectId == id);
                
                if (entity == null)
                {
                    return Ok(ApiResponse<Project>.Fail(404, "项目不存在"));
                }

                // 获取项目下的设备
                var deviceEntities = await _dbContext.Devices
                    .Where(d => d.ProjectId == id)
                    .ToListAsync();

                var project = new Project
                {
                    Id = entity.Id,
                    ProjectId = entity.ProjectId,
                    ProjectName = entity.ProjectName,
                    Description = entity.Description,
                    Location = entity.Location,
                    Status = entity.Status,
                    CreatedBy = entity.CreatedBy,
                    StoragePath = entity.StoragePath,
                    ContactPerson = entity.ContactPerson,
                    ContactPhone = entity.ContactPhone,
                    ContactEmail = entity.ContactEmail,
                    Longitude = entity.Longitude,
                    Latitude = entity.Latitude,
                    Elevation = entity.Elevation,
                    StartDate = entity.StartDate,
                    EndDate = entity.EndDate,
                    CreateTime = entity.CreateTime,
                    UpdateTime = entity.UpdateTime,
                    // ✅ 场景字段
                    SceneLongitude = entity.SceneLongitude,
                    SceneLatitude = entity.SceneLatitude,
                    SceneHeight = entity.SceneHeight,
                    SceneHeading = entity.SceneHeading,
                    ScenePitch = entity.ScenePitch,
                    SceneRoll = entity.SceneRoll,
                    Devices = deviceEntities.Select(d => new Device
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
                        CreateTime = d.CreateTime,
                        UpdateTime = d.UpdateTime
                    }).ToList()
                };

                return Ok(ApiResponse<Project>.Ok(project));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取项目详情失败");
                return Ok(ApiResponse<Project>.Fail(500, $"获取项目详情失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 创建项目
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                // 检查项目ID是否已存在
                var exists = await _dbContext.Projects.AnyAsync(p => p.ProjectId == request.ProjectId);
                if (exists)
                {
                    return Ok(ApiResponse<int>.Fail(400, $"项目ID '{request.ProjectId}' 已存在"));
                }

                var entity = new Data.Models.ProjectEntity
                {
                    ProjectId = request.ProjectId,
                    ProjectName = request.ProjectName,
                    Description = request.Description,
                    Location = request.Location,
                    Status = "Active",
                    CreatedBy = request.CreatedBy,
                    StoragePath = request.StoragePath,
                    ContactPerson = request.ContactPerson,
                    ContactPhone = request.ContactPhone,
                    ContactEmail = request.ContactEmail,
                    Longitude = request.Longitude,
                    Latitude = request.Latitude,
                    Elevation = request.Elevation,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };

                _dbContext.Projects.Add(entity);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("项目创建成功: {ProjectId}", request.ProjectId);
                return Ok(ApiResponse<int>.Ok(entity.Id, "项目创建成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建项目失败");
                return Ok(ApiResponse<int>.Fail(500, $"创建项目失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新项目
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateProject(string id, [FromBody] Project project)
        {
            try
            {
                var entity = await _dbContext.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
                if (entity == null)
                {
                    return Ok(ApiResponse<bool>.Fail(404, "项目不存在"));
                }

                entity.ProjectName = project.ProjectName;
                entity.Description = project.Description;
                entity.Location = project.Location;
                entity.Status = project.Status;
                entity.ContactPerson = project.ContactPerson;
                entity.ContactPhone = project.ContactPhone;
                entity.ContactEmail = project.ContactEmail;
                entity.Longitude = project.Longitude;
                entity.Latitude = project.Latitude;
                entity.Elevation = project.Elevation;
                entity.StartDate = project.StartDate;
                entity.EndDate = project.EndDate;
                entity.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("项目更新成功: {ProjectId}", id);
                return Ok(ApiResponse<bool>.Ok(true, "项目更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新项目失败");
                return Ok(ApiResponse<bool>.Fail(500, $"更新项目失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 删除项目
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(string id)
        {
            try
            {
                // 检查项目下是否有设备
                var hasDevices = await _dbContext.Devices.AnyAsync(d => d.ProjectId == id);
                if (hasDevices)
                {
                    return Ok(ApiResponse<bool>.Fail(400, "项目下还有设备，请先删除设备"));
                }

                var entity = await _dbContext.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
                if (entity == null)
                {
                    return Ok(ApiResponse<bool>.Fail(404, "项目不存在"));
                }

                _dbContext.Projects.Remove(entity);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("项目删除成功: {ProjectId}", id);
                return Ok(ApiResponse<bool>.Ok(true, "项目删除成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除项目失败");
                return Ok(ApiResponse<bool>.Fail(500, $"删除项目失败: {ex.Message}"));
            }
        }
    }
}

