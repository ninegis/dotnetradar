using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;
using RadarSystem.WebAPI.Models;
using System.Text.Json;
using BCrypt.Net;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 协议接口控制器 - 对应前端 /api/protocol/* 接口
    /// </summary>
    [ApiController]
    [Route("api/protocol")]
    [Authorize]
    public class ProtocolController : ControllerBase
    {
        private readonly RadarDbContext _dbContext;
        private readonly ILogger<ProtocolController> _logger;

        public ProtocolController(
            RadarDbContext dbContext,
            ILogger<ProtocolController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 添加项目（支持自动生成项目ID）
        /// POST /api/protocol/add/project
        /// </summary>
        [HttpPost("add/project")]
        public async Task<IActionResult> AddProject([FromBody] AddProjectRequest request)
        {
            try
            {
                // ✅ 如果ProjectId为空或null，自动生成：KOT_日期_随机5位数
                if (string.IsNullOrWhiteSpace(request.ProjectId))
                {
                    var dateStr = DateTime.Now.ToString("yyyyMMdd");
                    var random5Digits = new Random().Next(10000, 99999);
                    request.ProjectId = $"KOT_{dateStr}_{random5Digits}";
                    _logger.LogInformation("自动生成项目ID: {ProjectId}", request.ProjectId);
                }
                
                // 检查项目ID是否已存在
                var existingProject = await _dbContext.Projects
                    .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);
                
                if (existingProject != null)
                {
                    return Ok(new { code = 500, message = $"项目ID '{request.ProjectId}' 已存在" });
                }
                
                // 创建项目实体
                var projectEntity = new ProjectEntity
                {
                    ProjectId = request.ProjectId,
                    ProjectName = request.ProjectName,
                    Description = request.ProjectDescribe,
                    ContactPerson = request.Contact,
                    ContactPhone = request.Phone,
                    ContactEmail = request.Email,
                    Longitude = request.Lon ?? 0,
                    Latitude = request.Lat ?? 0,
                    Elevation = 0,
                    StoragePath = $"./Data/Projects/{request.ProjectId}",
                    Status = "Active",
                    CreatedBy = User.Identity?.Name ?? "system",
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };
                
                _dbContext.Projects.Add(projectEntity);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("项目创建成功: {ProjectId} - {ProjectName}", request.ProjectId, request.ProjectName);
                return Ok(new { code = 200, data = new { projectId = request.ProjectId }, message = "项目添加成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加项目失败");
                return Ok(new { code = 500, message = $"添加项目失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新项目图像分析配置
        /// POST /api/protocol/update/project/imageAnalysisConfig
        /// </summary>
        [HttpPost("update/project/imageAnalysisConfig")]
        public async Task<IActionResult> UpdateImageAnalysisConfig([FromBody] UpdateImageAnalysisConfigRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProjectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                _logger.LogInformation("更新图像分析配置: ProjectId={ProjectId}, GenImageType={GenImageType}", 
                    request.ProjectId, request.GenImageType);

                // 获取现有配置（如果存在）
                var existingConfig = await _dbContext.ImageAnalysisConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == request.ProjectId);

                // ✅ 更新或创建配置（使用独立字段，不再使用ConfigJson）
                if (existingConfig != null)
                {
                    // 更新现有配置
                    existingConfig.GenImageType = request.GenImageType ?? existingConfig.GenImageType;
                    existingConfig.DefoInterval = request.DefoInterval ?? existingConfig.DefoInterval;
                    existingConfig.ScatInterval = request.ScatInterval ?? existingConfig.ScatInterval;
                    existingConfig.DefoNumber = request.DefoNumber ?? existingConfig.DefoNumber;
                    existingConfig.ScatNumber = request.ScatNumber ?? existingConfig.ScatNumber;
                    
                    existingConfig.GenDefo = request.GenImageType?.Contains("0") == true || request.GenImageType == "02";
                    existingConfig.GenScat = request.GenImageType?.Contains("1") == true || request.GenImageType == "02";
                    existingConfig.UpdateTime = DateTime.Now;
                    
                    _logger.LogInformation("更新图像分析配置: GenImageType={Type}, DefoInterval={DefoInt}, ScatInterval={ScatInt}, DefoNumber={DefoNum}, ScatNumber={ScatNum}",
                        existingConfig.GenImageType, existingConfig.DefoInterval, existingConfig.ScatInterval, existingConfig.DefoNumber, existingConfig.ScatNumber);
                }
                else
                {
                    // 创建新配置
                    var newConfig = new ImageAnalysisConfigEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProjectId = request.ProjectId,
                        StandardImageSidePixel = 16384,
                        CompressImageSidePixel = 1024,
                        MatrixTileRngNum = 1203,
                        MatrixTileAngNum = 61,
                        GenDefo = request.GenImageType?.Contains("0") == true || request.GenImageType == "02",
                        GenScat = request.GenImageType?.Contains("1") == true || request.GenImageType == "02",
                        GenSpeed = false,
                        GenAcceleration = false,
                        // ✅ 使用独立字段
                        GenImageType = request.GenImageType ?? "01",
                        DefoInterval = request.DefoInterval ?? 60,
                        ScatInterval = request.ScatInterval ?? 60,
                        DefoNumber = request.DefoNumber ?? 10,
                        ScatNumber = request.ScatNumber ?? 10,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.ImageAnalysisConfigs.Add(newConfig);
                    existingConfig = newConfig;
                    
                    _logger.LogInformation("创建图像分析配置: GenImageType={Type}, DefoInterval={DefoInt}, ScatInterval={ScatInt}",
                        newConfig.GenImageType, newConfig.DefoInterval, newConfig.ScatInterval);
                }

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("图像分析配置更新成功: ProjectId={ProjectId}", request.ProjectId);
                return Ok(new 
                { 
                    code = 200, 
                    message = "图像分析配置更新成功",
                    data = new
                    {
                        projectId = existingConfig.ProjectId,
                        genDefo = existingConfig.GenDefo,
                        genScat = existingConfig.GenScat,
                        genImageType = existingConfig.GenImageType,
                        defoInterval = existingConfig.DefoInterval,
                        scatInterval = existingConfig.ScatInterval,
                        defoNumber = existingConfig.DefoNumber,
                        scatNumber = existingConfig.ScatNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新图像分析配置失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取项目图像分析配置
        /// GET /api/protocol/project/imageAnalysisConfig/{projectId}
        /// </summary>
        [HttpGet("project/imageAnalysisConfig/{projectId}")]
        public async Task<IActionResult> GetImageAnalysisConfig(string projectId)
        {
            try
            {
                var config = await _dbContext.ImageAnalysisConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId);
                
                if (config == null)
                {
                    return Ok(new { code = 404, message = "未找到配置" });
                }

                return Ok(new 
                { 
                    code = 200,
                    data = new
                    {
                        projectId = config.ProjectId,
                        genDefo = config.GenDefo,
                        genScat = config.GenScat,
                        genSpeed = config.GenSpeed,
                        genAcceleration = config.GenAcceleration,
                        // ✅ 返回独立字段
                        genImageType = config.GenImageType,
                        defoInterval = config.DefoInterval,
                        scatInterval = config.ScatInterval,
                        defoNumber = config.DefoNumber,
                        scatNumber = config.ScatNumber,
                        createTime = config.CreateTime,
                        updateTime = config.UpdateTime
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取图像分析配置失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新高程图颜色配置
        /// POST /api/protocol/update/terrainColor
        /// </summary>
        [HttpPost("update/terrainColor")]
        public async Task<IActionResult> UpdateTerrainColor([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                _logger.LogInformation("更新高程图颜色配置: ProjectId={ProjectId}", projectId);

                // 查找或创建配置
                var terrainConfig = await _dbContext.TerrainColorConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId);

                if (terrainConfig == null)
                {
                    terrainConfig = new TerrainColorConfigEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProjectId = projectId,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.TerrainColorConfigs.Add(terrainConfig);
                }

                // 更新字段
                if (body.TryGetProperty("colorSchemeType", out var colorSchemeType))
                    terrainConfig.ColorSchemeType = colorSchemeType.GetInt32();
                if (body.TryGetProperty("minValue", out var minValue))
                    terrainConfig.MinElevation = minValue.GetDouble();
                if (body.TryGetProperty("maxValue", out var maxValue))
                    terrainConfig.MaxElevation = maxValue.GetDouble();
                if (body.TryGetProperty("hslHStart", out var hslHStart))
                    terrainConfig.HslHStart = hslHStart.GetInt32();
                if (body.TryGetProperty("hslHEnd", out var hslHEnd))
                    terrainConfig.HslHEnd = hslHEnd.GetInt32();
                if (body.TryGetProperty("classCount", out var classCount))
                    terrainConfig.ClassCount = classCount.GetInt32();
                if (body.TryGetProperty("autoAdaptRange", out var autoAdaptRange))
                    terrainConfig.AutoAdaptRange = autoAdaptRange.GetBoolean();
                if (body.TryGetProperty("adaptBufferRatio", out var adaptBufferRatio))
                    terrainConfig.AdaptBufferRatio = adaptBufferRatio.GetDouble();
                if (body.TryGetProperty("customRanges", out var customRanges))
                    terrainConfig.CustomRanges = customRanges.GetString();

                terrainConfig.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "高程图颜色配置更新成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新高程图颜色配置失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取高程图颜色配置
        /// GET /api/protocol/query/terrainColor/{projectId}
        /// </summary>
        [HttpGet("query/terrainColor/{projectId}")]
        public async Task<IActionResult> GetTerrainColor(string projectId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                var terrainConfig = await _dbContext.TerrainColorConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId);

                if (terrainConfig == null)
                {
                    return Ok(new { code = 404, message = "未找到高程图配置" });
                }

                return Ok(new 
                { 
                    code = 200,
                    data = new
                    {
                        projectId = terrainConfig.ProjectId,
                        colorSchemeType = terrainConfig.ColorSchemeType,
                        minValue = terrainConfig.MinElevation,
                        maxValue = terrainConfig.MaxElevation,
                        hslHStart = terrainConfig.HslHStart,
                        hslHEnd = terrainConfig.HslHEnd,
                        classCount = terrainConfig.ClassCount,
                        autoAdaptRange = terrainConfig.AutoAdaptRange,
                        adaptBufferRatio = terrainConfig.AdaptBufferRatio,
                        customRanges = terrainConfig.CustomRanges,
                        enable = terrainConfig.Enable
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取高程图颜色配置失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新雷达基础参数（设备信息和雷达参数）
        /// POST /api/protocol/update/radar/param
        /// </summary>
        [HttpPost("update/radar/param")]
        public async Task<IActionResult> UpdateRadarParams([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                var deviceId = body.GetProperty("deviceId").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(deviceId))
                {
                    return Ok(new { code = 400, message = "项目ID和设备ID不能为空" });
                }

                _logger.LogInformation("更新雷达参数: ProjectId={ProjectId}, DeviceId={DeviceId}", projectId, deviceId);

                // 查找设备
                var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.ProjectId == projectId);
                if (device == null)
                {
                    return Ok(new { code = 404, message = "设备不存在" });
                }

                // 更新设备基础信息
                if (body.TryGetProperty("name", out var name))
                    device.DeviceName = name.GetString() ?? device.DeviceName;
                if (body.TryGetProperty("factoryId", out var factoryId))
                    device.FactoryId = factoryId.GetString() ?? device.FactoryId;
                if (body.TryGetProperty("longitude", out var longitude))
                    device.Longitude = longitude.GetDouble();
                if (body.TryGetProperty("latitude", out var latitude))
                    device.Latitude = latitude.GetDouble();
                if (body.TryGetProperty("height", out var height))
                    device.Elevation = height.GetDouble();
                if (body.TryGetProperty("orientation", out var orientation))
                    device.Orientation = orientation.GetDouble();
                if (body.TryGetProperty("radarOri", out var radarOri))
                    device.Orientation = radarOri.GetDouble();

                device.LastUpdateTime = DateTime.Now;
                device.UpdateTime = DateTime.Now;

                // ✅ 更新或创建雷达参数配置
                var radarParam = await _dbContext.RadarParams
                    .FirstOrDefaultAsync(rp => rp.ProjectId == projectId && rp.DeviceId == deviceId);
                
                if (radarParam == null)
                {
                    // 创建新的雷达参数记录
                    radarParam = new RadarParamEntity
                    {
                        ProjectId = projectId,
                        DeviceId = deviceId,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.RadarParams.Add(radarParam);
                }

                // 更新雷达参数字段
                if (body.TryGetProperty("ImgAngleStart", out var imgAngleStart))
                    radarParam.ImgAngleStart = imgAngleStart.GetDouble();
                if (body.TryGetProperty("ImgAngleEnd", out var imgAngleEnd))
                    radarParam.ImgAngleEnd = imgAngleEnd.GetDouble();
                if (body.TryGetProperty("RngMin", out var rngMin))
                    radarParam.RngMin = rngMin.GetDouble();
                if (body.TryGetProperty("RngMax", out var rngMax))
                    radarParam.RngMax = rngMax.GetDouble();
                if (body.TryGetProperty("FreqBand", out var freqBand))
                    radarParam.FreqBand = freqBand.GetString() ?? radarParam.FreqBand;
                if (body.TryGetProperty("AnteBeam_half", out var anteBeamHalf))
                    radarParam.AnteBeamHalf = anteBeamHalf.GetDouble();
                if (body.TryGetProperty("dataVersion", out var dataVersion))
                    radarParam.DataVersion = dataVersion.GetString() ?? radarParam.DataVersion;

                radarParam.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("雷达参数更新成功: DeviceId={DeviceId}", deviceId);
                return Ok(new 
                { 
                    code = 200, 
                    message = "雷达参数更新成功",
                    data = new
                    {
                        projectId = projectId,
                        deviceId = deviceId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新雷达参数失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新MIMO Lite雷达基础参数
        /// POST /api/protocol/update/radar/mimolite/param
        /// </summary>
        [HttpPost("update/radar/mimolite/param")]
        public async Task<IActionResult> UpdateMimoLiteRadarParams([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                var deviceId = body.GetProperty("deviceId").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(deviceId))
                {
                    return Ok(new { code = 400, message = "项目ID和设备ID不能为空" });
                }

                _logger.LogInformation("更新MIMO Lite雷达参数: ProjectId={ProjectId}, DeviceId={DeviceId}", projectId, deviceId);

                // 查找设备
                var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.ProjectId == projectId);
                if (device == null)
                {
                    return Ok(new { code = 404, message = "设备不存在" });
                }

                // 更新设备基础信息
                if (body.TryGetProperty("name", out var name))
                    device.DeviceName = name.GetString() ?? device.DeviceName;
                if (body.TryGetProperty("factoryId", out var factoryId))
                    device.FactoryId = factoryId.GetString() ?? device.FactoryId;
                if (body.TryGetProperty("longitude", out var longitude))
                    device.Longitude = longitude.GetDouble();
                if (body.TryGetProperty("latitude", out var latitude))
                    device.Latitude = latitude.GetDouble();
                if (body.TryGetProperty("height", out var height))
                    device.Elevation = height.GetDouble();
                if (body.TryGetProperty("orientation", out var orientation))
                    device.Orientation = orientation.GetDouble();
                if (body.TryGetProperty("radarOri", out var radarOri))
                    device.Orientation = radarOri.GetDouble();

                device.LastUpdateTime = DateTime.Now;
                device.UpdateTime = DateTime.Now;

                // ✅ 更新或创建雷达参数配置
                var radarParam = await _dbContext.RadarParams
                    .FirstOrDefaultAsync(rp => rp.ProjectId == projectId && rp.DeviceId == deviceId);
                
                if (radarParam == null)
                {
                    // 创建新的雷达参数记录
                    radarParam = new RadarParamEntity
                    {
                        ProjectId = projectId,
                        DeviceId = deviceId,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.RadarParams.Add(radarParam);
                }

                // 更新雷达参数字段
                if (body.TryGetProperty("ImgAngleStart", out var imgAngleStart))
                    radarParam.ImgAngleStart = imgAngleStart.GetDouble();
                if (body.TryGetProperty("ImgAngleEnd", out var imgAngleEnd))
                    radarParam.ImgAngleEnd = imgAngleEnd.GetDouble();
                if (body.TryGetProperty("RngMin", out var rngMin))
                    radarParam.RngMin = rngMin.GetDouble();
                if (body.TryGetProperty("RngMax", out var rngMax))
                    radarParam.RngMax = rngMax.GetDouble();
                if (body.TryGetProperty("FreqBand", out var freqBand))
                    radarParam.FreqBand = freqBand.GetString() ?? radarParam.FreqBand;
                if (body.TryGetProperty("AnteBeam_half", out var anteBeamHalf))
                    radarParam.AnteBeamHalf = anteBeamHalf.GetDouble();
                if (body.TryGetProperty("dataVersion", out var dataVersion))
                    radarParam.DataVersion = dataVersion.GetString() ?? radarParam.DataVersion;
                if (body.TryGetProperty("modelSelect", out var modelSelect))
                    radarParam.ModelSelect = modelSelect.GetString();

                radarParam.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("MIMO Lite雷达参数更新成功: DeviceId={DeviceId}", deviceId);
                return Ok(new 
                { 
                    code = 200, 
                    message = "雷达参数更新成功",
                    data = new
                    {
                        projectId = projectId,
                        deviceId = deviceId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新MIMO Lite雷达参数失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新雷达算法参数（通用雷达）
        /// POST /api/protocol/update/radar/algoparam
        /// </summary>
        [HttpPost("update/radar/algoparam")]
        public async Task<IActionResult> UpdateRadarAlgorithmParam([FromBody] UpdateAlgorithmParamRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProjectId) || string.IsNullOrWhiteSpace(request.DeviceId))
                {
                    return Ok(new { code = 400, message = "项目ID和设备ID不能为空" });
                }

                _logger.LogInformation("更新算法参数: ProjectId={ProjectId}, DeviceId={DeviceId}", 
                    request.ProjectId, request.DeviceId);

                // 查找或创建算法配置
                var algorithmConfig = await _dbContext.AlgorithmConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == request.ProjectId && c.DeviceId == request.DeviceId);

                if (algorithmConfig == null)
                {
                    algorithmConfig = new AlgorithmConfigEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProjectId = request.ProjectId,
                        DeviceId = request.DeviceId,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.AlgorithmConfigs.Add(algorithmConfig);
                }

                // ✅ 更新算法参数（32个字段）
                if (!string.IsNullOrEmpty(request.MonMode))
                    algorithmConfig.MonMode = request.MonMode;
                if (request.PhaFltTypeCtrl.HasValue)
                    algorithmConfig.PhaFltTypeCtrl = request.PhaFltTypeCtrl.Value;
                if (request.FltHalfWinLen.HasValue)
                    algorithmConfig.FltHalfWinLen = request.FltHalfWinLen.Value;
                if (request.AtmFltEn.HasValue)
                    algorithmConfig.AtmFltEn = request.AtmFltEn.Value;
                if (request.MeanWgt.HasValue)
                    algorithmConfig.MeanWgt = request.MeanWgt.Value;
                if (request.CmpDefThr.HasValue)
                    algorithmConfig.CmpDefThr = request.CmpDefThr.Value;
                if (request.CmpMult.HasValue)
                    algorithmConfig.CmpMult = request.CmpMult.Value;
                if (request.AmpDetThr.HasValue)
                    algorithmConfig.AmpDetThr = request.AmpDetThr.Value;
                if (request.AtmFltParaA.HasValue)
                    algorithmConfig.AtmFltParaA = request.AtmFltParaA.Value;
                if (request.AtmFltParaB.HasValue)
                    algorithmConfig.AtmFltParaB = request.AtmFltParaB.Value;
                if (request.AtmCorrThr2nd_1.HasValue)
                    algorithmConfig.AtmCorrThr2nd_1 = request.AtmCorrThr2nd_1.Value;
                if (request.AtmCompUpdPer.HasValue)
                    algorithmConfig.AtmCompUpdPer = request.AtmCompUpdPer.Value;
                if (request.AtmCorrThr2nd_2.HasValue)
                    algorithmConfig.AtmCorrThr2nd_2 = request.AtmCorrThr2nd_2.Value;
                if (!string.IsNullOrEmpty(request.DefImgDecim))
                    algorithmConfig.DefImgDecim = request.DefImgDecim;
                if (!string.IsNullOrEmpty(request.CplxImgDecim))
                    algorithmConfig.CplxImgDecim = request.CplxImgDecim;
                if (!string.IsNullOrEmpty(request.AtmCorrAlg))
                    algorithmConfig.AtmCorrAlg = request.AtmCorrAlg;
                if (request.AtmPhaErrEstDist_1.HasValue)
                    algorithmConfig.AtmPhaErrEstDist_1 = request.AtmPhaErrEstDist_1.Value;
                if (request.AtmPhaErrEstDist_2.HasValue)
                    algorithmConfig.AtmPhaErrEstDist_2 = request.AtmPhaErrEstDist_2.Value;
                if (request.StdDevWgt.HasValue)
                    algorithmConfig.StdDevWgt = request.StdDevWgt.Value;
                if (request.ShortDefAccPara.HasValue)
                    algorithmConfig.ShortDefAccPara = request.ShortDefAccPara.Value;
                if (request.DenoiseThr.HasValue)
                    algorithmConfig.DenoiseThr = request.DenoiseThr.Value;
                if (request.IsNoiseEq.HasValue)
                    algorithmConfig.IsNoiseEq = request.IsNoiseEq.Value;
                if (request.NoiseEqType.HasValue)
                    algorithmConfig.NoiseEqType = request.NoiseEqType.Value;
                if (request.AmpDevSelThrInit.HasValue)
                    algorithmConfig.AmpDevSelThrInit = request.AmpDevSelThrInit.Value;
                if (request.CohCoeThrInit.HasValue)
                    algorithmConfig.CohCoeThrInit = request.CohCoeThrInit.Value;
                if (request.CorrCoeffEffPSPts.HasValue)
                    algorithmConfig.CorrCoeffEffPSPts = request.CorrCoeffEffPSPts.Value;
                if (request.EffPSPts.HasValue)
                    algorithmConfig.EffPSPts = request.EffPSPts.Value;
                if (request.IfgPhaResThr.HasValue)
                    algorithmConfig.IfgPhaResThr = request.IfgPhaResThr.Value;
                if (request.SingPntThr.HasValue)
                    algorithmConfig.SingPntThr = request.SingPntThr.Value;
                if (request.PSPntSens.HasValue)
                    algorithmConfig.PSPntSens = request.PSPntSens.Value;
                if (request.PSThrAdjCoeff.HasValue)
                    algorithmConfig.PSThrAdjCoeff = request.PSThrAdjCoeff.Value;
                if (request.CohHalfWinLen.HasValue)
                    algorithmConfig.CohHalfWinLen = request.CohHalfWinLen.Value;

                algorithmConfig.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("算法参数更新成功");
                return Ok(new 
                { 
                    code = 200, 
                    message = "算法参数更新成功",
                    data = new
                    {
                        projectId = request.ProjectId,
                        deviceId = request.DeviceId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新算法参数失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新MIMO Lite雷达算法参数
        /// POST /api/protocol/update/radar/mimolite/algoparam
        /// </summary>
        [HttpPost("update/radar/mimolite/algoparam")]
        public async Task<IActionResult> UpdateMimoLiteAlgorithmParam([FromBody] UpdateAlgorithmParamRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProjectId) || string.IsNullOrWhiteSpace(request.DeviceId))
                {
                    return Ok(new { code = 400, message = "项目ID和设备ID不能为空" });
                }

                _logger.LogInformation("更新MIMO Lite算法参数: ProjectId={ProjectId}, DeviceId={DeviceId}", 
                    request.ProjectId, request.DeviceId);

                // 查找或创建算法配置
                var algorithmConfig = await _dbContext.AlgorithmConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == request.ProjectId && c.DeviceId == request.DeviceId);

                if (algorithmConfig == null)
                {
                    algorithmConfig = new AlgorithmConfigEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProjectId = request.ProjectId,
                        DeviceId = request.DeviceId,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.AlgorithmConfigs.Add(algorithmConfig);
                }

                // ✅ 更新算法参数（32个字段）
                if (!string.IsNullOrEmpty(request.MonMode))
                    algorithmConfig.MonMode = request.MonMode;
                if (request.PhaFltTypeCtrl.HasValue)
                    algorithmConfig.PhaFltTypeCtrl = request.PhaFltTypeCtrl.Value;
                if (request.FltHalfWinLen.HasValue)
                    algorithmConfig.FltHalfWinLen = request.FltHalfWinLen.Value;
                if (request.AtmFltEn.HasValue)
                    algorithmConfig.AtmFltEn = request.AtmFltEn.Value;
                if (request.MeanWgt.HasValue)
                    algorithmConfig.MeanWgt = request.MeanWgt.Value;
                if (request.CmpDefThr.HasValue)
                    algorithmConfig.CmpDefThr = request.CmpDefThr.Value;
                if (request.CmpMult.HasValue)
                    algorithmConfig.CmpMult = request.CmpMult.Value;
                if (request.AmpDetThr.HasValue)
                    algorithmConfig.AmpDetThr = request.AmpDetThr.Value;
                if (request.AtmFltParaA.HasValue)
                    algorithmConfig.AtmFltParaA = request.AtmFltParaA.Value;
                if (request.AtmFltParaB.HasValue)
                    algorithmConfig.AtmFltParaB = request.AtmFltParaB.Value;
                if (request.AtmCorrThr2nd_1.HasValue)
                    algorithmConfig.AtmCorrThr2nd_1 = request.AtmCorrThr2nd_1.Value;
                if (request.AtmCompUpdPer.HasValue)
                    algorithmConfig.AtmCompUpdPer = request.AtmCompUpdPer.Value;
                if (request.AtmCorrThr2nd_2.HasValue)
                    algorithmConfig.AtmCorrThr2nd_2 = request.AtmCorrThr2nd_2.Value;
                if (!string.IsNullOrEmpty(request.DefImgDecim))
                    algorithmConfig.DefImgDecim = request.DefImgDecim;
                if (!string.IsNullOrEmpty(request.CplxImgDecim))
                    algorithmConfig.CplxImgDecim = request.CplxImgDecim;
                if (!string.IsNullOrEmpty(request.AtmCorrAlg))
                    algorithmConfig.AtmCorrAlg = request.AtmCorrAlg;
                if (request.AtmPhaErrEstDist_1.HasValue)
                    algorithmConfig.AtmPhaErrEstDist_1 = request.AtmPhaErrEstDist_1.Value;
                if (request.AtmPhaErrEstDist_2.HasValue)
                    algorithmConfig.AtmPhaErrEstDist_2 = request.AtmPhaErrEstDist_2.Value;
                if (request.StdDevWgt.HasValue)
                    algorithmConfig.StdDevWgt = request.StdDevWgt.Value;
                if (request.ShortDefAccPara.HasValue)
                    algorithmConfig.ShortDefAccPara = request.ShortDefAccPara.Value;
                if (request.DenoiseThr.HasValue)
                    algorithmConfig.DenoiseThr = request.DenoiseThr.Value;
                if (request.IsNoiseEq.HasValue)
                    algorithmConfig.IsNoiseEq = request.IsNoiseEq.Value;
                if (request.NoiseEqType.HasValue)
                    algorithmConfig.NoiseEqType = request.NoiseEqType.Value;
                if (request.AmpDevSelThrInit.HasValue)
                    algorithmConfig.AmpDevSelThrInit = request.AmpDevSelThrInit.Value;
                if (request.CohCoeThrInit.HasValue)
                    algorithmConfig.CohCoeThrInit = request.CohCoeThrInit.Value;
                if (request.CorrCoeffEffPSPts.HasValue)
                    algorithmConfig.CorrCoeffEffPSPts = request.CorrCoeffEffPSPts.Value;
                if (request.EffPSPts.HasValue)
                    algorithmConfig.EffPSPts = request.EffPSPts.Value;
                if (request.IfgPhaResThr.HasValue)
                    algorithmConfig.IfgPhaResThr = request.IfgPhaResThr.Value;
                if (request.SingPntThr.HasValue)
                    algorithmConfig.SingPntThr = request.SingPntThr.Value;
                if (request.PSPntSens.HasValue)
                    algorithmConfig.PSPntSens = request.PSPntSens.Value;
                if (request.PSThrAdjCoeff.HasValue)
                    algorithmConfig.PSThrAdjCoeff = request.PSThrAdjCoeff.Value;
                if (request.CohHalfWinLen.HasValue)
                    algorithmConfig.CohHalfWinLen = request.CohHalfWinLen.Value;

                algorithmConfig.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("MIMO Lite算法参数更新成功");
                return Ok(new 
                { 
                    code = 200, 
                    message = "算法参数更新成功",
                    data = new
                    {
                        projectId = request.ProjectId,
                        deviceId = request.DeviceId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新MIMO Lite算法参数失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取算法参数配置
        /// GET /api/protocol/algorithm/{projectId}/{deviceId}
        /// </summary>
        [HttpGet("algorithm/{projectId}/{deviceId}")]
        public async Task<IActionResult> GetAlgorithmParam(string projectId, string deviceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(deviceId))
                {
                    return Ok(new { code = 400, message = "项目ID和设备ID不能为空" });
                }

                var algorithmConfig = await _dbContext.AlgorithmConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.DeviceId == deviceId);

                if (algorithmConfig == null)
                {
                    return Ok(new { code = 404, message = "未找到算法配置" });
                }

                return Ok(new 
                { 
                    code = 200,
                    data = new
                    {
                        projectId = algorithmConfig.ProjectId,
                        deviceId = algorithmConfig.DeviceId,
                        // ✅ 返回32个算法参数字段
                        MonMode = algorithmConfig.MonMode,
                        PhaFltTypeCtrl = algorithmConfig.PhaFltTypeCtrl,
                        FltHalfWinLen = algorithmConfig.FltHalfWinLen,
                        AtmFltEn = algorithmConfig.AtmFltEn,
                        MeanWgt = algorithmConfig.MeanWgt,
                        CmpDefThr = algorithmConfig.CmpDefThr,
                        CmpMult = algorithmConfig.CmpMult,
                        AmpDetThr = algorithmConfig.AmpDetThr,
                        AtmFltParaA = algorithmConfig.AtmFltParaA,
                        AtmFltParaB = algorithmConfig.AtmFltParaB,
                        AtmCorrThr2nd_1 = algorithmConfig.AtmCorrThr2nd_1,
                        AtmCompUpdPer = algorithmConfig.AtmCompUpdPer,
                        AtmCorrThr2nd_2 = algorithmConfig.AtmCorrThr2nd_2,
                        DefImgDecim = algorithmConfig.DefImgDecim,
                        CplxImgDecim = algorithmConfig.CplxImgDecim,
                        AtmCorrAlg = algorithmConfig.AtmCorrAlg,
                        AtmPhaErrEstDist_1 = algorithmConfig.AtmPhaErrEstDist_1,
                        AtmPhaErrEstDist_2 = algorithmConfig.AtmPhaErrEstDist_2,
                        StdDevWgt = algorithmConfig.StdDevWgt,
                        ShortDefAccPara = algorithmConfig.ShortDefAccPara,
                        DenoiseThr = algorithmConfig.DenoiseThr,
                        IsNoiseEq = algorithmConfig.IsNoiseEq,
                        NoiseEqType = algorithmConfig.NoiseEqType,
                        AmpDevSelThrInit = algorithmConfig.AmpDevSelThrInit,
                        CohCoeThrInit = algorithmConfig.CohCoeThrInit,
                        CorrCoeffEffPSPts = algorithmConfig.CorrCoeffEffPSPts,
                        EffPSPts = algorithmConfig.EffPSPts,
                        IfgPhaResThr = algorithmConfig.IfgPhaResThr,
                        SingPntThr = algorithmConfig.SingPntThr,
                        PSPntSens = algorithmConfig.PSPntSens,
                        PSThrAdjCoeff = algorithmConfig.PSThrAdjCoeff,
                        CohHalfWinLen = algorithmConfig.CohHalfWinLen
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取算法参数失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新速度指标配置
        /// POST /api/protocol/update/speed/target
        /// </summary>
        [HttpPost("update/speed/target")]
        public async Task<IActionResult> UpdateSpeedTarget([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                // 获取timeUnit数组
                var timeUnitArray = body.GetProperty("timeUnit").EnumerateArray().Select(x => x.GetString()).ToArray();
                
                _logger.LogInformation("更新速度指标配置: ProjectId={ProjectId}, TimeUnits={TimeUnits}", 
                    projectId, string.Join(",", timeUnitArray));

                // 查找或创建速度指标配置
                var speedIndex = await _dbContext.SpeedIndices
                    .FirstOrDefaultAsync(s => s.ProjectId == projectId);

                if (speedIndex == null)
                {
                    speedIndex = new SpeedIndexEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProjectId = projectId,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.SpeedIndices.Add(speedIndex);
                }

                // 更新时间单位（用逗号分隔）
                speedIndex.TimeUnits = string.Join(",", timeUnitArray);
                
                // ✅ 更新各个时间单位的启用状态（前端使用00-05）
                speedIndex.Enable30Min = timeUnitArray.Contains("00");
                speedIndex.Enable1Hour = timeUnitArray.Contains("01");
                speedIndex.Enable1Day = timeUnitArray.Contains("02");
                speedIndex.Enable3Day = timeUnitArray.Contains("03");
                speedIndex.Enable1Week = timeUnitArray.Contains("04");
                speedIndex.Enable1Month = timeUnitArray.Contains("05");
                
                // ✅ 更新速度图像自动生成配置
                if (body.TryGetProperty("autoGenSpeedImage", out var autoGenSpeed))
                    speedIndex.AutoGenSpeedImage = autoGenSpeed.GetBoolean();
                if (body.TryGetProperty("speedImageInterval", out var speedInterval))
                    speedIndex.SpeedImageInterval = speedInterval.GetInt32();
                if (body.TryGetProperty("autoGenAccelerationImage", out var autoGenAccel))
                    speedIndex.AutoGenAccelerationImage = autoGenAccel.GetBoolean();
                if (body.TryGetProperty("accelerationImageInterval", out var accelInterval))
                    speedIndex.AccelerationImageInterval = accelInterval.GetInt32();
                
                speedIndex.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "速度指标配置更新成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新速度指标配置失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取速度指标配置
        /// GET /api/protocol/query/speed/target/{projectId}
        /// </summary>
        [HttpGet("query/speed/target/{projectId}")]
        public async Task<IActionResult> GetSpeedTarget(string projectId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                var speedIndex = await _dbContext.SpeedIndices
                    .FirstOrDefaultAsync(s => s.ProjectId == projectId);

                if (speedIndex == null)
                {
                    return Ok(new { code = 404, message = "未找到速度配置" });
                }

                return Ok(new 
                { 
                    code = 200,
                    data = new
                    {
                        projectId = speedIndex.ProjectId,
                        timeUnits = speedIndex.TimeUnits,  // 逗号分隔的字符串
                        enable30Min = speedIndex.Enable30Min,
                        enable1Hour = speedIndex.Enable1Hour,
                        enable1Day = speedIndex.Enable1Day,
                        enable3Day = speedIndex.Enable3Day,
                        enable1Week = speedIndex.Enable1Week,
                        enable1Month = speedIndex.Enable1Month,
                        // ✅ 速度图像自动生成配置
                        autoGenSpeedImage = speedIndex.AutoGenSpeedImage,
                        speedImageInterval = speedIndex.SpeedImageInterval,
                        autoGenAccelerationImage = speedIndex.AutoGenAccelerationImage,
                        accelerationImageInterval = speedIndex.AccelerationImageInterval,
                        createTime = speedIndex.CreateTime,
                        updateTime = speedIndex.UpdateTime
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取速度指标配置失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新色条配置（位移/散射）
        /// POST /api/protocol/update/colorBar
        /// </summary>
        [HttpPost("update/colorBar")]
        public async Task<IActionResult> UpdateColorBar([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                var mode = body.GetProperty("mode").GetString(); // "defo" or "scat"
                
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(mode))
                {
                    return Ok(new { code = 400, message = "项目ID和模式不能为空" });
                }

                _logger.LogInformation("更新色条配置: ProjectId={ProjectId}, Mode={Mode}", projectId, mode);

                // 查找或创建色条配置
                var colorBarConfig = await _dbContext.ColorBarConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Mode == mode);

                if (colorBarConfig == null)
                {
                    colorBarConfig = new ColorBarConfigEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProjectId = projectId,
                        Mode = mode,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.ColorBarConfigs.Add(colorBarConfig);
                }

                // 更新字段
                if (body.TryGetProperty("colorSchemeType", out var colorSchemeType))
                    colorBarConfig.ColorSchemeType = colorSchemeType.GetInt32();
                if (body.TryGetProperty("minValue", out var minValue))
                    colorBarConfig.MinValue = minValue.GetDouble();
                if (body.TryGetProperty("maxValue", out var maxValue))
                    colorBarConfig.MaxValue = maxValue.GetDouble();
                if (body.TryGetProperty("hslHStart", out var hslHStart))
                    colorBarConfig.HslHStart = int.Parse(hslHStart.GetString() ?? "0");
                if (body.TryGetProperty("hslHEnd", out var hslHEnd))
                    colorBarConfig.HslHEnd = int.Parse(hslHEnd.GetString() ?? "240");
                if (body.TryGetProperty("filterAlpha", out var filterAlpha))
                    colorBarConfig.FilterAlpha = double.Parse(filterAlpha.GetString() ?? "0.8");
                if (body.TryGetProperty("filterMin", out var filterMin))
                    colorBarConfig.FilterMin = double.Parse(filterMin.GetString() ?? "-1000");
                if (body.TryGetProperty("filterMax", out var filterMax))
                    colorBarConfig.FilterMax = double.Parse(filterMax.GetString() ?? "1000");
                if (body.TryGetProperty("filterEnable", out var filterEnable))
                    colorBarConfig.FilterEnable = filterEnable.GetInt32();
                
                // ✅ 新增字段
                if (body.TryGetProperty("classCount", out var classCount))
                    colorBarConfig.ClassCount = classCount.GetInt32();
                if (body.TryGetProperty("autoAdaptRange", out var autoAdaptRange))
                    colorBarConfig.AutoAdaptRange = autoAdaptRange.GetBoolean();
                if (body.TryGetProperty("adaptBufferRatio", out var adaptBufferRatio))
                    colorBarConfig.AdaptBufferRatio = adaptBufferRatio.GetDouble();
                if (body.TryGetProperty("customRanges", out var customRanges))
                    colorBarConfig.CustomRanges = customRanges.GetString();
                
                // ✅ HSL完整支持
                if (body.TryGetProperty("hslDirection", out var hslDirection))
                    colorBarConfig.HslDirection = hslDirection.GetInt32();
                if (body.TryGetProperty("hslS", out var hslS))
                    colorBarConfig.HslS = hslS.GetDouble();
                if (body.TryGetProperty("hslL", out var hslL))
                    colorBarConfig.HslL = hslL.GetDouble();

                colorBarConfig.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "色条配置更新成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新色条配置失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取色条配置（位移/散射）
        /// GET /api/protocol/colorBar/{projectId}/{mode}
        /// </summary>
        [HttpGet("colorBar/{projectId}/{mode}")]
        public async Task<IActionResult> GetColorBar(string projectId, string mode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(mode))
                {
                    return Ok(new { code = 400, message = "项目ID和模式不能为空" });
                }

                var colorBarConfig = await _dbContext.ColorBarConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Mode == mode);

                if (colorBarConfig == null)
                {
                    return Ok(new { code = 404, message = "未找到色条配置" });
                }

                return Ok(new 
                { 
                    code = 200,
                    data = new
                    {
                        projectId = colorBarConfig.ProjectId,
                        mode = colorBarConfig.Mode,
                        colorSchemeType = colorBarConfig.ColorSchemeType,
                        minValue = colorBarConfig.MinValue,
                        maxValue = colorBarConfig.MaxValue,
                        hslHStart = colorBarConfig.HslHStart,
                        hslHEnd = colorBarConfig.HslHEnd,
                        hslDirection = colorBarConfig.HslDirection,  // ✅ 新增
                        hslS = colorBarConfig.HslS,                  // ✅ 新增
                        hslL = colorBarConfig.HslL,                  // ✅ 新增
                        filterAlpha = colorBarConfig.FilterAlpha,
                        filterMin = colorBarConfig.FilterMin,
                        filterMax = colorBarConfig.FilterMax,
                        filterEnable = colorBarConfig.FilterEnable,
                        classCount = colorBarConfig.ClassCount,
                        autoAdaptRange = colorBarConfig.AutoAdaptRange,
                        adaptBufferRatio = colorBarConfig.AdaptBufferRatio,
                        customRanges = colorBarConfig.CustomRanges
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取色条配置失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新隐患区域分析配置
        /// POST /api/protocol/update/hidden/analysis
        /// </summary>
        [HttpPost("update/hidden/analysis")]
        public async Task<IActionResult> UpdateHiddenAnalysis([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                _logger.LogInformation("更新隐患区域分析配置: ProjectId={ProjectId}", projectId);

                // 查找或创建隐患区域分析配置
                var hiddenAreaConfig = await _dbContext.HiddenAreaAnalysisConfigs
                    .FirstOrDefaultAsync(h => h.ProjectId == projectId);

                if (hiddenAreaConfig == null)
                {
                    hiddenAreaConfig = new HiddenAreaAnalysisConfigEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProjectId = projectId,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.HiddenAreaAnalysisConfigs.Add(hiddenAreaConfig);
                }

                // 更新独立字段
                if (body.TryGetProperty("threshold", out var threshold))
                    hiddenAreaConfig.Threshold = double.Parse(threshold.GetString() ?? "10.0");
                if (body.TryGetProperty("areaThreshold", out var areaThreshold))
                    hiddenAreaConfig.AreaThreshold = double.Parse(areaThreshold.GetString() ?? "1.0");
                if (body.TryGetProperty("analysisDec", out var analysisDec))
                    hiddenAreaConfig.AnalysisDec = int.Parse(analysisDec.GetString() ?? "1");
                if (body.TryGetProperty("autoAnalysisFlag", out var autoAnalysisFlag))
                    hiddenAreaConfig.AutoAnalysisFlag = autoAnalysisFlag.GetBoolean();
                
                hiddenAreaConfig.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "隐患区域分析配置更新成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新隐患区域分析配置失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        // ==================== 预警规则管理 ====================

        /// <summary>
        /// 添加预警规则（批量）
        /// POST /api/protocol/add/ruleBatch
        /// </summary>
        [HttpPost("add/ruleBatch")]
        public async Task<IActionResult> AddAlarmRuleBatch([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                var id = body.GetProperty("id").GetString();
                var ruleName = body.GetProperty("ruleName").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(id))
                {
                    return Ok(new { code = 400, message = "项目ID和规则ID不能为空" });
                }

                _logger.LogInformation("添加预警规则: ProjectId={ProjectId}, RuleId={Id}", projectId, id);

                var alarmRule = new AlarmRuleEntity
                {
                    Id = id,
                    ProjectId = projectId,
                    RuleName = ruleName ?? "未命名规则",
                    AlarmContent = body.TryGetProperty("alarmContent", out var ac) ? ac.GetString() : "",
                    Enable = body.TryGetProperty("enable", out var enable) ? enable.GetBoolean() : true,
                    Devices = body.TryGetProperty("devices", out var devices) ? devices.GetString() : "",
                    GeoMarkArray = body.TryGetProperty("geoMarkArray", out var geoMark) ? geoMark.ToString() : "",
                    DataSource = body.TryGetProperty("dataSource", out var dataSource) ? dataSource.GetString() : "10",
                    TargetFlag = body.TryGetProperty("targetFlag", out var targetFlag) ? targetFlag.GetBoolean() : false,
                    CreateTime = DateTime.Now
                };

                // 解析alarmTargetThresholds数组，提取各指标的阈值
                if (body.TryGetProperty("alarmTargetThresholds", out var thresholds))
                {
                    foreach (var levelThreshold in thresholds.EnumerateArray())
                    {
                        var level = levelThreshold.GetProperty("level").GetInt32();
                        var targetCheckbox = levelThreshold.GetProperty("targetCheckbox");

                        foreach (var target in targetCheckbox.EnumerateArray())
                        {
                            var targetName = target.GetProperty("target").GetString();
                            var targetValue = target.TryGetProperty("value", out var val) ? val.GetDouble() : 0.0;
                            var targetEnabled = target.TryGetProperty("flag", out var flag) ? flag.GetBoolean() : false;
                            var timeUnit = target.TryGetProperty("timeUnit", out var tu) ? tu.GetString() : "";

                            // 根据target类型和level设置对应字段
                            if (targetName == "displacement")
                            {
                                alarmRule.EnableDisplacement = targetEnabled;
                                switch (level)
                                {
                                    case 1: alarmRule.DisplacementBlue = targetValue; break;
                                    case 2: alarmRule.DisplacementYellow = targetValue; break;
                                    case 3: alarmRule.DisplacementOrange = targetValue; break;
                                    case 4: alarmRule.DisplacementRed = targetValue; break;
                                }
                            }
                            else if (targetName == "speed")
                            {
                                alarmRule.EnableSpeed = targetEnabled;
                                alarmRule.SpeedTimeUnit = timeUnit;
                                switch (level)
                                {
                                    case 1: alarmRule.SpeedBlue = targetValue; break;
                                    case 2: alarmRule.SpeedYellow = targetValue; break;
                                    case 3: alarmRule.SpeedOrange = targetValue; break;
                                    case 4: alarmRule.SpeedRed = targetValue; break;
                                }
                            }
                            else if (targetName == "acceleration")
                            {
                                alarmRule.EnableAcceleration = targetEnabled;
                                alarmRule.AccelerationTimeUnit = timeUnit;
                                switch (level)
                                {
                                    case 1: alarmRule.AccelerationBlue = targetValue; break;
                                    case 2: alarmRule.AccelerationYellow = targetValue; break;
                                    case 3: alarmRule.AccelerationOrange = targetValue; break;
                                    case 4: alarmRule.AccelerationRed = targetValue; break;
                                }
                            }
                        }
                    }
                }

                _dbContext.AlarmRules.Add(alarmRule);
                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "预警规则添加成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加预警规则失败");
                return Ok(new { code = 500, message = $"添加失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新预警规则（批量）
        /// POST /api/protocol/update/ruleBatch
        /// </summary>
        [HttpPost("update/ruleBatch")]
        public async Task<IActionResult> UpdateAlarmRuleBatch([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                var id = body.GetProperty("id").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(id))
                {
                    return Ok(new { code = 400, message = "项目ID和规则ID不能为空" });
                }

                _logger.LogInformation("更新预警规则: ProjectId={ProjectId}, RuleId={Id}", projectId, id);

                var alarmRule = await _dbContext.AlarmRules
                    .FirstOrDefaultAsync(r => r.Id == id && r.ProjectId == projectId);

                if (alarmRule == null)
                {
                    return Ok(new { code = 404, message = "预警规则不存在" });
                }

                // 更新基本信息
                if (body.TryGetProperty("ruleName", out var ruleName))
                    alarmRule.RuleName = ruleName.GetString() ?? alarmRule.RuleName;
                if (body.TryGetProperty("alarmContent", out var alarmContent))
                    alarmRule.AlarmContent = alarmContent.GetString();
                if (body.TryGetProperty("enable", out var enable))
                    alarmRule.Enable = enable.GetBoolean();
                if (body.TryGetProperty("devices", out var devices))
                    alarmRule.Devices = devices.GetString();
                if (body.TryGetProperty("geoMarkArray", out var geoMark))
                    alarmRule.GeoMarkArray = geoMark.ToString();
                if (body.TryGetProperty("dataSource", out var dataSource))
                    alarmRule.DataSource = dataSource.GetString();
                if (body.TryGetProperty("targetFlag", out var targetFlag))
                    alarmRule.TargetFlag = targetFlag.GetBoolean();

                // 解析alarmTargetThresholds数组，更新各指标的阈值
                if (body.TryGetProperty("alarmTargetThresholds", out var thresholds))
                {
                    foreach (var levelThreshold in thresholds.EnumerateArray())
                    {
                        var level = levelThreshold.GetProperty("level").GetInt32();
                        var targetCheckbox = levelThreshold.GetProperty("targetCheckbox");

                        foreach (var target in targetCheckbox.EnumerateArray())
                        {
                            var targetName = target.GetProperty("target").GetString();
                            var targetValue = target.TryGetProperty("value", out var val) ? val.GetDouble() : 0.0;
                            var targetEnabled = target.TryGetProperty("flag", out var flag) ? flag.GetBoolean() : false;
                            var timeUnit = target.TryGetProperty("timeUnit", out var tu) ? tu.GetString() : "";

                            // 根据target类型和level设置对应字段
                            if (targetName == "displacement")
                            {
                                alarmRule.EnableDisplacement = targetEnabled;
                                switch (level)
                                {
                                    case 1: alarmRule.DisplacementBlue = targetValue; break;
                                    case 2: alarmRule.DisplacementYellow = targetValue; break;
                                    case 3: alarmRule.DisplacementOrange = targetValue; break;
                                    case 4: alarmRule.DisplacementRed = targetValue; break;
                                }
                            }
                            else if (targetName == "speed")
                            {
                                alarmRule.EnableSpeed = targetEnabled;
                                alarmRule.SpeedTimeUnit = timeUnit;
                                switch (level)
                                {
                                    case 1: alarmRule.SpeedBlue = targetValue; break;
                                    case 2: alarmRule.SpeedYellow = targetValue; break;
                                    case 3: alarmRule.SpeedOrange = targetValue; break;
                                    case 4: alarmRule.SpeedRed = targetValue; break;
                                }
                            }
                            else if (targetName == "acceleration")
                            {
                                alarmRule.EnableAcceleration = targetEnabled;
                                alarmRule.AccelerationTimeUnit = timeUnit;
                                switch (level)
                                {
                                    case 1: alarmRule.AccelerationBlue = targetValue; break;
                                    case 2: alarmRule.AccelerationYellow = targetValue; break;
                                    case 3: alarmRule.AccelerationOrange = targetValue; break;
                                    case 4: alarmRule.AccelerationRed = targetValue; break;
                                }
                            }
                        }
                    }
                }

                alarmRule.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "预警规则更新成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新预警规则失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }
        /// <summary>
        /// 更新预警规则（批量）
        /// POST /api/protocol/update/ruleBatch
        /// </summary>

        /// <summary>
        /// 删除预警规则
        /// GET /api/protocol/remove/ruleBatch/{id}/{projectId}
        /// </summary>
        [HttpGet("remove/ruleBatch/{id}/{projectId}")]
        public async Task<IActionResult> RemoveAlarmRuleBatch(string id, string projectId)
        {
            try
            {
                _logger.LogInformation("删除预警规则: ProjectId={ProjectId}, RuleId={Id}", projectId, id);

                var alarmRule = await _dbContext.AlarmRules
                    .FirstOrDefaultAsync(r => r.Id == id && r.ProjectId == projectId);

                if (alarmRule == null)
                {
                    return Ok(new { code = 404, message = "预警规则不存在" });
                }

                alarmRule.IsDeleted = true;
                alarmRule.UpdateTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "预警规则删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除预警规则失败");
                return Ok(new { code = 500, message = $"删除失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 查询项目预警规则列表
        /// GET /api/protocol/query/rules/{projectId}
        /// </summary>
        [HttpGet("query/rules/{projectId}")]
        public async Task<IActionResult> QueryAlarmRules(string projectId)
        {
            try
            {
                _logger.LogInformation("查询预警规则: ProjectId={ProjectId}", projectId);

                var rules = await _dbContext.AlarmRules
                    .Where(r => r.ProjectId == projectId && !r.IsDeleted)
                    .OrderByDescending(r => r.CreateTime)
                    .ToListAsync();

                return Ok(new { code = 200, data = rules });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询预警规则失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }

        // ==================== 预警人员管理 ====================

        /// <summary>
        /// 添加预警联系人
        /// POST /api/protocol/add/contact
        /// </summary>
        [HttpPost("add/contact")]
        public async Task<IActionResult> AddAlarmContact([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                var id = body.GetProperty("id").GetString();
                var name = body.GetProperty("name").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(id))
                {
                    return Ok(new { code = 400, message = "项目ID和联系人ID不能为空" });
                }

                _logger.LogInformation("添加预警联系人: ProjectId={ProjectId}, ContactId={Id}", projectId, id);

                var contact = new AlarmContactEntity
                {
                    Id = id,
                    ProjectId = projectId,
                    Name = name ?? "未命名联系人",
                    Email = body.TryGetProperty("email", out var email) ? email.GetString() : "",
                    Phone = body.TryGetProperty("phone", out var phone) ? phone.GetString() : "",
                    AlarmLevel = body.TryGetProperty("alarmLevel", out var alarmLevel) ? alarmLevel.GetInt32() : 1,
                    Enable = body.TryGetProperty("enable", out var enable) ? enable.GetBoolean() : true,
                    CreateTime = DateTime.Now
                };

                _dbContext.AlarmContacts.Add(contact);
                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "联系人添加成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加联系人失败");
                return Ok(new { code = 500, message = $"添加失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新预警联系人
        /// POST /api/protocol/update/contact
        /// </summary>
        [HttpPost("update/contact")]
        public async Task<IActionResult> UpdateAlarmContact([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                var id = body.GetProperty("id").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(id))
                {
                    return Ok(new { code = 400, message = "项目ID和联系人ID不能为空" });
                }

                _logger.LogInformation("更新预警联系人: ProjectId={ProjectId}, ContactId={Id}", projectId, id);

                var contact = await _dbContext.AlarmContacts
                    .FirstOrDefaultAsync(c => c.Id == id && c.ProjectId == projectId);

                if (contact == null)
                {
                    return Ok(new { code = 404, message = "联系人不存在" });
                }

                if (body.TryGetProperty("name", out var name))
                    contact.Name = name.GetString() ?? contact.Name;
                if (body.TryGetProperty("email", out var email))
                    contact.Email = email.GetString();
                if (body.TryGetProperty("phone", out var phone))
                    contact.Phone = phone.GetString();
                if (body.TryGetProperty("alarmLevel", out var alarmLevel))
                    contact.AlarmLevel = alarmLevel.GetInt32();
                if (body.TryGetProperty("enable", out var enable))
                    contact.Enable = enable.GetBoolean();

                contact.UpdateTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "联系人更新成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新联系人失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 查询项目预警联系人列表
        /// GET /api/protocol/query/contact/{projectId}
        /// </summary>
        [HttpGet("query/contact/{projectId}")]
        public async Task<IActionResult> QueryAlarmContacts(string projectId)
        {
            try
            {
                _logger.LogInformation("查询预警联系人: ProjectId={ProjectId}", projectId);

                var contacts = await _dbContext.AlarmContacts
                    .Where(c => c.ProjectId == projectId && !c.IsDeleted)
                    .OrderByDescending(c => c.CreateTime)
                    .ToListAsync();

                return Ok(new { code = 200, data = contacts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询联系人失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 删除预警联系人
        /// GET /api/protocol/remove/contact/{id}/{projectId}
        /// </summary>
        [HttpGet("remove/contact/{id}/{projectId}")]
        public async Task<IActionResult> RemoveAlarmContact(string id, string projectId)
        {
            try
            {
                _logger.LogInformation("删除预警联系人: ProjectId={ProjectId}, ContactId={Id}", projectId, id);

                var contact = await _dbContext.AlarmContacts
                    .FirstOrDefaultAsync(c => c.Id == id && c.ProjectId == projectId);

                if (contact == null)
                {
                    return Ok(new { code = 404, message = "联系人不存在" });
                }

                contact.IsDeleted = true;
                contact.UpdateTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "联系人删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除联系人失败");
                return Ok(new { code = 500, message = $"删除失败: {ex.Message}" });
            }
        }

        // ==================== 短信模板配置 ====================

        /// <summary>
        /// 更新短信模板配置
        /// POST /api/protocol/update/smsConfig
        /// </summary>
        [HttpPost("update/smsConfig")]
        public async Task<IActionResult> UpdateSmsTemplate([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();
                
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                _logger.LogInformation("更新短信模板配置: ProjectId={ProjectId}", projectId);

                var smsConfig = await _dbContext.SmsConfigs
                    .FirstOrDefaultAsync(s => s.ProjectId == projectId);

                if (smsConfig == null)
                {
                    smsConfig = new SmsConfigEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProjectId = projectId,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.SmsConfigs.Add(smsConfig);
                }

                // 更新短信模板字段（对应前端smsNotifyConfig）
                if (body.TryGetProperty("enable", out var enable))
                    smsConfig.Enable = enable.GetBoolean();
                if (body.TryGetProperty("notifyChannel", out var notifyChannel))
                    smsConfig.NotifyChannel = notifyChannel.GetString() ?? "00";
                if (body.TryGetProperty("accessKeyId", out var accessKeyId))
                    smsConfig.AccessKeyId = accessKeyId.GetString();
                if (body.TryGetProperty("accessKeySecret", out var accessKeySecret))
                    smsConfig.AccessKeySecret = accessKeySecret.GetString();
                if (body.TryGetProperty("signName", out var signName))
                    smsConfig.SignName = signName.GetString();
                if (body.TryGetProperty("templateCode", out var templateCode))
                    smsConfig.TemplateCode = templateCode.GetString();

                smsConfig.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "短信模板配置更新成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新短信模板配置失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 查询短信模板配置
        /// GET /api/protocol/query/smsTemplate/{projectId}
        /// </summary>
        [HttpGet("query/smsTemplate/{projectId}")]
        public async Task<IActionResult> QuerySmsTemplate(string projectId)
        {
            try
            {
                _logger.LogInformation("查询短信模板配置: ProjectId={ProjectId}", projectId);

                var smsConfig = await _dbContext.SmsConfigs
                    .FirstOrDefaultAsync(s => s.ProjectId == projectId);

                if (smsConfig == null)
                {
                    return Ok(new { code = 200, data = new { } });
                }

                return Ok(new { code = 200, data = smsConfig });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询短信模板配置失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }

        // ==================== 授权用户管理 ====================

        /// <summary>
        /// 添加授权用户
        /// POST /api/protocol/add/user
        /// </summary>
        [HttpPost("add/user")]
        public async Task<IActionResult> AddUser([FromBody] JsonElement body)
        {
            try
            {
                var username = body.GetProperty("username").GetString();
                var projectId = body.TryGetProperty("projectId", out var pid) ? pid.GetString() : null;
                
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Ok(new { code = 400, message = "用户名不能为空" });
                }

                // 检查用户名是否已存在
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
                
                if (existingUser != null)
                {
                    return Ok(new { code = 400, message = "用户名已存在" });
                }

                _logger.LogInformation("添加授权用户: Username={Username}, ProjectId={ProjectId}", username, projectId);

                var user = new UserEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.TryGetProperty("password", out var pwd) ? pwd.GetString() : "123456"),
                    Email = body.TryGetProperty("email", out var email) ? email.GetString() : null,
                    Phone = body.TryGetProperty("phone", out var phone) ? phone.GetString() : null,
                    RealName = body.TryGetProperty("realName", out var realName) ? realName.GetString() : null,
                    Role = body.TryGetProperty("role", out var role) ? role.GetString() : "User",
                    ProjectId = projectId,
                    IsActive = true,
                    CreatedTime = DateTime.Now
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "用户添加成功", data = new { id = user.Id } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加用户失败");
                return Ok(new { code = 500, message = $"添加失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新授权用户
        /// POST /api/protocol/update/user
        /// </summary>
        [HttpPost("update/user")]
        public async Task<IActionResult> UpdateUser([FromBody] JsonElement body)
        {
            try
            {
                var id = body.GetProperty("id").GetString();
                
                if (string.IsNullOrWhiteSpace(id))
                {
                    return Ok(new { code = 400, message = "用户ID不能为空" });
                }

                _logger.LogInformation("更新授权用户: UserId={Id}", id);

                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

                if (user == null)
                {
                    return Ok(new { code = 404, message = "用户不存在" });
                }

                if (body.TryGetProperty("email", out var email))
                    user.Email = email.GetString();
                if (body.TryGetProperty("phone", out var phone))
                    user.Phone = phone.GetString();
                if (body.TryGetProperty("realName", out var realName))
                    user.RealName = realName.GetString();
                if (body.TryGetProperty("role", out var role))
                    user.Role = role.GetString();
                if (body.TryGetProperty("isActive", out var isActive))
                    user.IsActive = isActive.GetBoolean();
                if (body.TryGetProperty("password", out var password) && !string.IsNullOrWhiteSpace(password.GetString()))
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password.GetString());

                user.UpdatedTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "用户更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 删除授权用户
        /// GET /api/protocol/remove/user/{id}
        /// </summary>
        [HttpGet("remove/user/{id}")]
        public async Task<IActionResult> RemoveUser(string id)
        {
            try
            {
                _logger.LogInformation("删除授权用户: UserId={Id}", id);

                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

                if (user == null)
                {
                    return Ok(new { code = 404, message = "用户不存在" });
                }

                user.IsDeleted = true;
                user.UpdatedTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "用户删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户失败");
                return Ok(new { code = 500, message = $"删除失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 查询项目授权用户列表
        /// GET /api/protocol/query/users/{projectId}
        /// </summary>
        [HttpGet("query/users/{projectId}")]
        public async Task<IActionResult> QueryUsers(string projectId)
        {
            try
            {
                _logger.LogInformation("查询授权用户: ProjectId={ProjectId}", projectId);

                var users = await _dbContext.Users
                    .Where(u => u.ProjectId == projectId && !u.IsDeleted)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.Phone,
                        u.RealName,
                        u.Role,
                        u.IsActive,
                        u.LastLoginTime,
                        u.CreatedTime
                    })
                    .OrderByDescending(u => u.CreatedTime)
                    .ToListAsync();

                return Ok(new { code = 200, data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询用户失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 查询所有授权用户（不区分项目）
        /// GET /api/protocol/query/users
        /// </summary>
        [HttpGet("query/users")]
        public async Task<IActionResult> QueryAllUsers()
        {
            try
            {
                _logger.LogInformation("查询所有授权用户");

                var users = await _dbContext.Users
                    .Where(u => !u.IsDeleted)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.Phone,
                        u.RealName,
                        u.Role,
                        u.ProjectId,
                        u.IsActive,
                        u.LastLoginTime,
                        u.CreatedTime
                    })
                    .OrderByDescending(u => u.CreatedTime)
                    .ToListAsync();

                return Ok(new { code = 200, data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询用户失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }
    }
}

