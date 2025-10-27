namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 配置管理服务接口（替代JSON配置文件）
    /// </summary>
    public interface IConfigurationManagementService
    {
        // ========== 项目配置 ==========
        /// <summary>
        /// 获取项目完整配置
        /// </summary>
        Task<object?> GetProjectConfigurationAsync(string projectId);

        /// <summary>
        /// 更新项目配置
        /// </summary>
        Task UpdateProjectConfigurationAsync(string projectId, object config);

        // ========== 图像分析配置 ==========
        /// <summary>
        /// 获取图像差分分析配置
        /// </summary>
        Task<object?> GetImageDiffAnalysisConfigAsync(string projectId, string deviceId);

        /// <summary>
        /// 更新图像差分分析配置
        /// </summary>
        Task UpdateImageDiffAnalysisConfigAsync(string projectId, string deviceId, object config);

        // ========== 隐患区域分析配置 ==========
        /// <summary>
        /// 获取隐患区域分析配置
        /// </summary>
        Task<object?> GetHiddenAreaAnalysisConfigAsync(string projectId, string deviceId);

        /// <summary>
        /// 更新隐患区域分析配置
        /// </summary>
        Task UpdateHiddenAreaAnalysisConfigAsync(string projectId, string deviceId, object config);

        // ========== 俯仰电机配置 ==========
        /// <summary>
        /// 获取俯仰电机配置
        /// </summary>
        Task<object?> GetTiltMotorConfigAsync(string deviceId);

        /// <summary>
        /// 更新俯仰电机配置
        /// </summary>
        Task UpdateTiltMotorConfigAsync(string deviceId, object config);

        // ========== 色条配置 ==========
        /// <summary>
        /// 获取色条配置（按类型）
        /// </summary>
        Task<object?> GetColorBarSettingAsync(string projectId, string deviceId, string barType);

        /// <summary>
        /// 更新色条配置
        /// </summary>
        Task UpdateColorBarSettingAsync(string projectId, string deviceId, string barType, object config);

        // ========== 综合配置获取（兼容原有API） ==========
        /// <summary>
        /// 获取项目的所有配置（兼容原有getRadarData接口）
        /// </summary>
        Task<object> GetAllConfigurationsAsync(string projectId);
    }
}

