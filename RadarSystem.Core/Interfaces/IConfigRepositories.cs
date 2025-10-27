using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 地理标记仓储接口
    /// </summary>
    public interface IGeoMarkRepository
    {
        Task<GeoMark> CreateAsync(GeoMark geoMark);
        Task<GeoMark?> GetByIdAsync(string id);
        Task<List<GeoMark>> GetByProjectIdAsync(string projectId, bool includeDeleted = false);
        Task<GeoMark> UpdateAsync(GeoMark geoMark);
        Task<bool> DeleteAsync(string id, bool hardDelete = false);
        Task<List<GeoMark>> SearchAsync(string projectId, string searchText);
        Task<int> GetCountByProjectIdAsync(string projectId);
    }

    /// <summary>
    /// 报警规则仓储接口
    /// </summary>
    public interface IAlarmRuleRepository
    {
        Task<AlarmRule> CreateAsync(AlarmRule alarmRule);
        Task<AlarmRule?> GetByIdAsync(string id);
        Task<List<AlarmRule>> GetByProjectIdAsync(string projectId, bool includeDeleted = false);
        Task<List<AlarmRule>> GetEnabledRulesByProjectIdAsync(string projectId);
        Task<AlarmRule> UpdateAsync(AlarmRule alarmRule);
        Task<bool> DeleteAsync(string id, bool hardDelete = false);
        Task<List<AlarmRule>> SearchAsync(string projectId, string searchText);
        Task<int> GetCountByProjectIdAsync(string projectId);
    }

    /// <summary>
    /// 颜色配置仓储接口
    /// </summary>
    public interface IColorSettingRepository
    {
        Task<ColorSetting> CreateAsync(ColorSetting colorSetting);
        Task<ColorSetting?> GetByIdAsync(string id);
        Task<ColorSetting?> GetByProjectIdAndTypeAsync(string projectId, string settingType);
        Task<List<ColorSetting>> GetByProjectIdAsync(string projectId);
        Task<ColorSetting> UpdateAsync(ColorSetting colorSetting);
        Task<bool> DeleteAsync(string id);
    }

    /// <summary>
    /// 面板配置仓储接口
    /// </summary>
    public interface IPanelConfigRepository
    {
        Task<PanelConfig> CreateAsync(PanelConfig panelConfig);
        Task<PanelConfig?> GetByIdAsync(string id);
        Task<PanelConfig?> GetByProjectIdAndTypeAsync(string projectId, string panelType);
        Task<List<PanelConfig>> GetByProjectIdAsync(string projectId);
        Task<PanelConfig> UpdateAsync(PanelConfig panelConfig);
        Task<bool> DeleteAsync(string id);
    }

    /// <summary>
    /// 图像标记仓储接口
    /// </summary>
    public interface IImageMarkRepository
    {
        Task<ImageMark> CreateAsync(ImageMark imageMark);
        Task<ImageMark?> GetByIdAsync(string id);
        Task<List<ImageMark>> GetByProjectIdAsync(string projectId, bool includeDeleted = false);
        Task<List<ImageMark>> GetByImageIdAsync(string imageId, bool includeDeleted = false);
        Task<ImageMark> UpdateAsync(ImageMark imageMark);
        Task<bool> DeleteAsync(string id, bool hardDelete = false);
        Task<List<ImageMark>> SearchAsync(string projectId, string searchText);
    }

    /// <summary>
    /// 图像分析配置仓储接口
    /// </summary>
    public interface IImageAnalysisConfigRepository
    {
        Task<ImageAnalysisConfig> CreateAsync(ImageAnalysisConfig config);
        Task<ImageAnalysisConfig?> GetByIdAsync(string id);
        Task<ImageAnalysisConfig?> GetByProjectIdAsync(string projectId);
        Task<ImageAnalysisConfig> UpdateAsync(ImageAnalysisConfig config);
        Task<bool> DeleteAsync(string id);
    }
}

