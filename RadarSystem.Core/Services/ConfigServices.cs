using Microsoft.Extensions.Logging;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 颜色配置服务实现
    /// </summary>
    public class ColorSettingService : IColorSettingService
    {
        private readonly IColorSettingRepository _colorSettingRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ColorSettingService> _logger;

        public ColorSettingService(
            IColorSettingRepository colorSettingRepository,
            IProjectRepository projectRepository,
            ILogger<ColorSettingService> logger)
        {
            _colorSettingRepository = colorSettingRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<ColorSetting> CreateOrUpdateColorSettingAsync(CreateColorSettingRequest request)
        {
            try
            {
                // 检查项目是否存在
                var project = await _projectRepository.GetByProjectIdAsync(request.ProjectId);
                if (project == null)
                {
                    throw new ArgumentException($"项目不存在: {request.ProjectId}");
                }

                // 检查是否已存在
                var existing = await _colorSettingRepository.GetByProjectIdAndTypeAsync(
                    request.ProjectId, request.SettingType);

                if (existing != null)
                {
                    // 更新现有配置
                    existing.Type = request.Type;
                    existing.MinValue = request.MinValue;
                    existing.MaxValue = request.MaxValue;
                    existing.HslHStart = request.HslHStart;
                    existing.HslHEnd = request.HslHEnd;
                    existing.HslDirection = request.HslDirection;
                    existing.FilterEnable = request.FilterEnable;
                    existing.FilterMin = request.FilterMin;
                    existing.FilterMax = request.FilterMax;
                    existing.FilterAlpha = request.FilterAlpha;
                    existing.HslS = request.HslS;
                    existing.HslL = request.HslL;
                    existing.ValueArrayJson = request.ValueArrayJson;
                    existing.ColorArrayJson = request.ColorArrayJson;
                    existing.AutoMode = request.AutoMode;
                    existing.UpdateTime = DateTime.Now;

                    var result = await _colorSettingRepository.UpdateAsync(existing);
                    _logger.LogInformation($"更新颜色配置成功: {request.SettingType}");
                    return result;
                }
                else
                {
                    // 创建新配置
                    var colorSetting = new ColorSetting
                    {
                        Id = $"COLOR_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
                        ProjectId = request.ProjectId,
                        SettingType = request.SettingType,
                        Type = request.Type,
                        MinValue = request.MinValue,
                        MaxValue = request.MaxValue,
                        HslHStart = request.HslHStart,
                        HslHEnd = request.HslHEnd,
                        HslDirection = request.HslDirection,
                        FilterEnable = request.FilterEnable,
                        FilterMin = request.FilterMin,
                        FilterMax = request.FilterMax,
                        FilterAlpha = request.FilterAlpha,
                        HslS = request.HslS,
                        HslL = request.HslL,
                        ValueArrayJson = request.ValueArrayJson,
                        ColorArrayJson = request.ColorArrayJson,
                        AutoMode = request.AutoMode,
                        CreateTime = DateTime.Now
                    };

                    var result = await _colorSettingRepository.CreateAsync(colorSetting);
                    _logger.LogInformation($"创建颜色配置成功: {request.SettingType}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建或更新颜色配置失败: {request.SettingType}");
                throw;
            }
        }

        public async Task<ColorSetting?> GetColorSettingAsync(string projectId, string settingType)
        {
            return await _colorSettingRepository.GetByProjectIdAndTypeAsync(projectId, settingType);
        }

        public async Task<List<ColorSetting>> GetProjectColorSettingsAsync(string projectId)
        {
            return await _colorSettingRepository.GetByProjectIdAsync(projectId);
        }

        public async Task<bool> DeleteColorSettingAsync(string projectId, string settingType)
        {
            try
            {
                var setting = await _colorSettingRepository.GetByProjectIdAndTypeAsync(projectId, settingType);
                if (setting == null)
                {
                    return false;
                }

                var result = await _colorSettingRepository.DeleteAsync(setting.Id);
                
                if (result)
                {
                    _logger.LogInformation($"删除颜色配置成功: {settingType}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除颜色配置失败: {settingType}");
                throw;
            }
        }

        public async Task<ColorSetting> ResetColorSettingAsync(string projectId, string settingType)
        {
            // 删除现有配置
            await DeleteColorSettingAsync(projectId, settingType);

            // 创建默认配置
            var defaultRequest = GetDefaultColorSetting(projectId, settingType);
            return await CreateOrUpdateColorSettingAsync(defaultRequest);
        }

        private CreateColorSettingRequest GetDefaultColorSetting(string projectId, string settingType)
        {
            return new CreateColorSettingRequest
            {
                ProjectId = projectId,
                SettingType = settingType,
                Type = 0,
                MinValue = settingType == "scat" ? 0.3 : -30.0,
                MaxValue = settingType == "scat" ? 0.98 : 30.0,
                HslHStart = 240,
                HslHEnd = 0,
                HslDirection = settingType == "scat" ? 1 : -1,
                FilterEnable = false,
                HslS = 1.0,
                HslL = 0.5,
                AutoMode = false
            };
        }
    }

    /// <summary>
    /// 面板配置服务实现
    /// </summary>
    public class PanelConfigService : IPanelConfigService
    {
        private readonly IPanelConfigRepository _panelConfigRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<PanelConfigService> _logger;

        public PanelConfigService(
            IPanelConfigRepository panelConfigRepository,
            IProjectRepository projectRepository,
            ILogger<PanelConfigService> logger)
        {
            _panelConfigRepository = panelConfigRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<PanelConfig> CreateOrUpdatePanelConfigAsync(CreatePanelConfigRequest request)
        {
            try
            {
                // 验证 JSON
                if (!await ValidatePanelConfigJsonAsync(request.ConfigJson))
                {
                    throw new ArgumentException("无效的配置 JSON");
                }

                // 检查项目是否存在
                var project = await _projectRepository.GetByProjectIdAsync(request.ProjectId);
                if (project == null)
                {
                    throw new ArgumentException($"项目不存在: {request.ProjectId}");
                }

                // 检查是否已存在
                var existing = await _panelConfigRepository.GetByProjectIdAndTypeAsync(
                    request.ProjectId, request.PanelType);

                if (existing != null)
                {
                    // 更新现有配置
                    existing.ConfigJson = request.ConfigJson;
                    existing.UpdateTime = DateTime.Now;

                    var result = await _panelConfigRepository.UpdateAsync(existing);
                    _logger.LogInformation($"更新面板配置成功: {request.PanelType}");
                    return result;
                }
                else
                {
                    // 创建新配置
                    var panelConfig = new PanelConfig
                    {
                        Id = $"PANEL_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
                        ProjectId = request.ProjectId,
                        PanelType = request.PanelType,
                        ConfigJson = request.ConfigJson,
                        CreateTime = DateTime.Now
                    };

                    var result = await _panelConfigRepository.CreateAsync(panelConfig);
                    _logger.LogInformation($"创建面板配置成功: {request.PanelType}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建或更新面板配置失败: {request.PanelType}");
                throw;
            }
        }

        public async Task<PanelConfig?> GetPanelConfigAsync(string projectId, string panelType)
        {
            return await _panelConfigRepository.GetByProjectIdAndTypeAsync(projectId, panelType);
        }

        public async Task<List<PanelConfig>> GetProjectPanelConfigsAsync(string projectId)
        {
            return await _panelConfigRepository.GetByProjectIdAsync(projectId);
        }

        public async Task<bool> DeletePanelConfigAsync(string projectId, string panelType)
        {
            try
            {
                var config = await _panelConfigRepository.GetByProjectIdAndTypeAsync(projectId, panelType);
                if (config == null)
                {
                    return false;
                }

                var result = await _panelConfigRepository.DeleteAsync(config.Id);
                
                if (result)
                {
                    _logger.LogInformation($"删除面板配置成功: {panelType}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除面板配置失败: {panelType}");
                throw;
            }
        }

        public async Task<bool> ValidatePanelConfigJsonAsync(string configJson)
        {
            if (string.IsNullOrWhiteSpace(configJson))
            {
                return false;
            }

            try
            {
                JsonDocument.Parse(configJson);
                return await Task.FromResult(true);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "无效的 JSON 配置");
                return false;
            }
        }
    }

    /// <summary>
    /// 图像标记服务实现
    /// </summary>
    public class ImageMarkService : IImageMarkService
    {
        private readonly IImageMarkRepository _imageMarkRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ImageMarkService> _logger;

        public ImageMarkService(
            IImageMarkRepository imageMarkRepository,
            IProjectRepository projectRepository,
            ILogger<ImageMarkService> logger)
        {
            _imageMarkRepository = imageMarkRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<ImageMark> CreateImageMarkAsync(CreateImageMarkRequest request)
        {
            try
            {
                // 检查项目是否存在
                var project = await _projectRepository.GetByProjectIdAsync(request.ProjectId);
                if (project == null)
                {
                    throw new ArgumentException($"项目不存在: {request.ProjectId}");
                }

                var imageMark = new ImageMark
                {
                    Id = $"IMGMARK_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
                    ProjectId = request.ProjectId,
                    ImageId = request.ImageId,
                    Name = request.Name,
                    MarkType = request.MarkType,
                    CoordinatesJson = request.CoordinatesJson,
                    Description = request.Description,
                    Color = request.Color ?? "#FF0000",
                    CreateTime = DateTime.Now,
                    IsDeleted = false
                };

                var result = await _imageMarkRepository.CreateAsync(imageMark);
                _logger.LogInformation($"创建图像标记成功: {result.Name} (ID: {result.Id})");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建图像标记失败: {request.Name}");
                throw;
            }
        }

        public async Task<ImageMark> UpdateImageMarkAsync(string id, CreateImageMarkRequest request)
        {
            try
            {
                var existing = await _imageMarkRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    throw new ArgumentException($"图像标记不存在: {id}");
                }

                existing.Name = request.Name;
                existing.MarkType = request.MarkType;
                existing.CoordinatesJson = request.CoordinatesJson;
                existing.Description = request.Description;
                existing.Color = request.Color ?? existing.Color;
                existing.UpdateTime = DateTime.Now;

                var result = await _imageMarkRepository.UpdateAsync(existing);
                _logger.LogInformation($"更新图像标记成功: {result.Name} (ID: {result.Id})");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新图像标记失败: {id}");
                throw;
            }
        }

        public async Task<bool> DeleteImageMarkAsync(string id, bool hardDelete = false)
        {
            try
            {
                var result = await _imageMarkRepository.DeleteAsync(id, hardDelete);
                
                if (result)
                {
                    _logger.LogInformation($"删除图像标记成功: ID={id}, HardDelete={hardDelete}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除图像标记失败: {id}");
                throw;
            }
        }

        public async Task<ImageMark?> GetImageMarkAsync(string id)
        {
            return await _imageMarkRepository.GetByIdAsync(id);
        }

        public async Task<List<ImageMark>> GetProjectImageMarksAsync(string projectId, bool includeDeleted = false)
        {
            return await _imageMarkRepository.GetByProjectIdAsync(projectId, includeDeleted);
        }

        public async Task<List<ImageMark>> GetImageMarksAsync(string imageId, bool includeDeleted = false)
        {
            return await _imageMarkRepository.GetByImageIdAsync(imageId, includeDeleted);
        }

        public async Task<List<ImageMark>> SearchImageMarksAsync(string projectId, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetProjectImageMarksAsync(projectId);
            }
            
            return await _imageMarkRepository.SearchAsync(projectId, searchText);
        }
    }

    /// <summary>
    /// 图像分析配置服务实现
    /// </summary>
    public class ImageAnalysisConfigService : IImageAnalysisConfigService
    {
        private readonly IImageAnalysisConfigRepository _imageAnalysisConfigRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ImageAnalysisConfigService> _logger;

        public ImageAnalysisConfigService(
            IImageAnalysisConfigRepository imageAnalysisConfigRepository,
            IProjectRepository projectRepository,
            ILogger<ImageAnalysisConfigService> logger)
        {
            _imageAnalysisConfigRepository = imageAnalysisConfigRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<ImageAnalysisConfig> CreateOrUpdateConfigAsync(CreateImageAnalysisConfigRequest request)
        {
            try
            {
                // 检查项目是否存在
                var project = await _projectRepository.GetByProjectIdAsync(request.ProjectId);
                if (project == null)
                {
                    throw new ArgumentException($"项目不存在: {request.ProjectId}");
                }

                // 检查是否已存在
                var existing = await _imageAnalysisConfigRepository.GetByProjectIdAsync(request.ProjectId);

                if (existing != null)
                {
                    // 更新现有配置
                    existing.StandardImageSidePixel = request.StandardImageSidePixel;
                    existing.CompressImageSidePixel = request.CompressImageSidePixel;
                    existing.MatrixTileRngNum = request.MatrixTileRngNum;
                    existing.MatrixTileAngNum = request.MatrixTileAngNum;
                    existing.GenDefo = request.GenDefo;
                    existing.GenScat = request.GenScat;
                    existing.GenSpeed = request.GenSpeed;
                    existing.GenAcceleration = request.GenAcceleration;
                    existing.ConfigJson = request.ConfigJson;
                    existing.UpdateTime = DateTime.Now;

                    var result = await _imageAnalysisConfigRepository.UpdateAsync(existing);
                    _logger.LogInformation($"更新图像分析配置成功 (ProjectId: {request.ProjectId})");
                    return result;
                }
                else
                {
                    // 创建新配置
                    var config = new ImageAnalysisConfig
                    {
                        Id = $"IMGCFG_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
                        ProjectId = request.ProjectId,
                        StandardImageSidePixel = request.StandardImageSidePixel,
                        CompressImageSidePixel = request.CompressImageSidePixel,
                        MatrixTileRngNum = request.MatrixTileRngNum,
                        MatrixTileAngNum = request.MatrixTileAngNum,
                        GenDefo = request.GenDefo,
                        GenScat = request.GenScat,
                        GenSpeed = request.GenSpeed,
                        GenAcceleration = request.GenAcceleration,
                        ConfigJson = request.ConfigJson,
                        CreateTime = DateTime.Now
                    };

                    var result = await _imageAnalysisConfigRepository.CreateAsync(config);
                    _logger.LogInformation($"创建图像分析配置成功 (ProjectId: {request.ProjectId})");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建或更新图像分析配置失败 (ProjectId: {request.ProjectId})");
                throw;
            }
        }

        public async Task<ImageAnalysisConfig?> GetConfigAsync(string projectId)
        {
            return await _imageAnalysisConfigRepository.GetByProjectIdAsync(projectId);
        }

        public async Task<bool> DeleteConfigAsync(string projectId)
        {
            try
            {
                var config = await _imageAnalysisConfigRepository.GetByProjectIdAsync(projectId);
                if (config == null)
                {
                    return false;
                }

                var result = await _imageAnalysisConfigRepository.DeleteAsync(config.Id);
                
                if (result)
                {
                    _logger.LogInformation($"删除图像分析配置成功 (ProjectId: {projectId})");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除图像分析配置失败 (ProjectId: {projectId})");
                throw;
            }
        }

        public async Task<ImageAnalysisConfig> GetOrCreateDefaultConfigAsync(string projectId)
        {
            var existing = await _imageAnalysisConfigRepository.GetByProjectIdAsync(projectId);
            
            if (existing != null)
            {
                return existing;
            }

            // 创建默认配置
            var defaultRequest = new CreateImageAnalysisConfigRequest
            {
                ProjectId = projectId,
                StandardImageSidePixel = 16384,
                CompressImageSidePixel = 1024,
                MatrixTileRngNum = 1203,
                MatrixTileAngNum = 61,
                GenDefo = false,
                GenScat = true,
                GenSpeed = false,
                GenAcceleration = false
            };

            return await CreateOrUpdateConfigAsync(defaultRequest);
        }
    }
}

