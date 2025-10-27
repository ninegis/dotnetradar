using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 地理标记服务接口
    /// </summary>
    public interface IGeoService
    {
        Task<GeoMark> CreateGeoMarkAsync(CreateGeoMarkRequest request);
        Task<GeoMark> UpdateGeoMarkAsync(string id, UpdateGeoMarkRequest request);
        Task<bool> DeleteGeoMarkAsync(string id, bool hardDelete = false);
        Task<GeoMark?> GetGeoMarkAsync(string id);
        Task<List<GeoMark>> GetProjectGeoMarksAsync(string projectId, bool includeDeleted = false);
        Task<List<GeoMark>> SearchGeoMarksAsync(string projectId, string searchText);
        Task<int> GetGeoMarkCountAsync(string projectId);
        Task<bool> ValidateGeoMarkAsync(CreateGeoMarkRequest request);
    }


    /// <summary>
    /// 颜色配置服务接口
    /// </summary>
    public interface IColorSettingService
    {
        Task<ColorSetting> CreateOrUpdateColorSettingAsync(CreateColorSettingRequest request);
        Task<ColorSetting?> GetColorSettingAsync(string projectId, string settingType);
        Task<List<ColorSetting>> GetProjectColorSettingsAsync(string projectId);
        Task<bool> DeleteColorSettingAsync(string projectId, string settingType);
        Task<ColorSetting> ResetColorSettingAsync(string projectId, string settingType);
    }

    /// <summary>
    /// 面板配置服务接口
    /// </summary>
    public interface IPanelConfigService
    {
        Task<PanelConfig> CreateOrUpdatePanelConfigAsync(CreatePanelConfigRequest request);
        Task<PanelConfig?> GetPanelConfigAsync(string projectId, string panelType);
        Task<List<PanelConfig>> GetProjectPanelConfigsAsync(string projectId);
        Task<bool> DeletePanelConfigAsync(string projectId, string panelType);
        Task<bool> ValidatePanelConfigJsonAsync(string configJson);
    }

    /// <summary>
    /// 图像标记服务接口
    /// </summary>
    public interface IImageMarkService
    {
        Task<ImageMark> CreateImageMarkAsync(CreateImageMarkRequest request);
        Task<ImageMark> UpdateImageMarkAsync(string id, CreateImageMarkRequest request);
        Task<bool> DeleteImageMarkAsync(string id, bool hardDelete = false);
        Task<ImageMark?> GetImageMarkAsync(string id);
        Task<List<ImageMark>> GetProjectImageMarksAsync(string projectId, bool includeDeleted = false);
        Task<List<ImageMark>> GetImageMarksAsync(string imageId, bool includeDeleted = false);
        Task<List<ImageMark>> SearchImageMarksAsync(string projectId, string searchText);
    }

    /// <summary>
    /// 图像分析配置服务接口
    /// </summary>
    public interface IImageAnalysisConfigService
    {
        Task<ImageAnalysisConfig> CreateOrUpdateConfigAsync(CreateImageAnalysisConfigRequest request);
        Task<ImageAnalysisConfig?> GetConfigAsync(string projectId);
        Task<bool> DeleteConfigAsync(string projectId);
        Task<ImageAnalysisConfig> GetOrCreateDefaultConfigAsync(string projectId);
    }
}

