using System.ComponentModel.DataAnnotations;

namespace RadarSystem.WebAPI.Models
{
    #region 项目管理请求

    public class AddProjectRequest
    {
        /// <summary>
        /// 项目ID（可选，如果不提供则自动生成：KOT_日期_随机5位数）
        /// </summary>
        public string? ProjectId { get; set; }
        
        [Required]
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectDescribe { get; set; }
        public string? Contact { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public double? Lon { get; set; }
        public double? Lat { get; set; }
        public double? Alt { get; set; }
    }

    public class UpdateProjectInfoRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Contact { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class ProjectViewRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        public double Lon { get; set; }
        public double Lat { get; set; }
        public double Alt { get; set; }
        public double Heading { get; set; }
        public double Pitch { get; set; }
        public double Roll { get; set; }
    }

    public class ImageAnalysisConfigRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        public object? ImageDiffAnalysisConfig { get; set; }
        public object? ImageAnalysisConfig { get; set; }
    }

    /// <summary>
    /// 更新图像分析配置请求（对应前端 /api/protocol/update/project/imageAnalysisConfig）
    /// </summary>
    public class UpdateImageAnalysisConfigRequest
    {
        /// <summary>
        /// 项目ID
        /// </summary>
        [Required]
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// 图像生成类型 (例如: "01"=形变图, "02"=强度图, "03"=两者)
        /// </summary>
        public string? GenImageType { get; set; }

        /// <summary>
        /// 形变图间隔（分钟或小时）
        /// </summary>
        public int? DefoInterval { get; set; }

        /// <summary>
        /// 强度图间隔（分钟或小时）
        /// </summary>
        public int? ScatInterval { get; set; }

        /// <summary>
        /// 形变图生成数量
        /// </summary>
        public int? DefoNumber { get; set; }

        /// <summary>
        /// 强度图生成数量
        /// </summary>
        public int? ScatNumber { get; set; }
    }

    #endregion

    #region 设备管理请求

    public class AddDeviceRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceName { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public string? SlaveId { get; set; }
        public string? Ori { get; set; }
        public string? Type { get; set; }
        public double? Lon { get; set; }
        public double? Lat { get; set; }
        public double? Alt { get; set; }
        public string? Ipv4 { get; set; }
    }

    #endregion

    #region 监测位置请求

    public class AddGeoMarkRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Type { get; set; } // "point" or "area"
        public double? Lon { get; set; }
        public double? Lat { get; set; }
        public double? Alt { get; set; }
        public string? Coordinates { get; set; } // JSON格式的坐标数组
        public bool EnableShieldArea { get; set; }
    }

    #endregion

    #region 告警规则请求

    public class AddAlarmRuleRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public string? GeoMarkId { get; set; }
        public string? RuleName { get; set; }
        public string? AlarmType { get; set; }
        public double? Threshold { get; set; }
        public int? Level { get; set; }
        public bool Enable { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateAlarmRuleRequest : AddAlarmRuleRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
    }

    #endregion

    #region 告警联系人请求

    public class AddContactRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? AlarmLevel { get; set; }
        public bool Enable { get; set; }
    }

    public class UpdateContactRequest : AddContactRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
    }

    public class UpdateSmsConfigRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        public bool EnableSms { get; set; }
        public string? SmsProvider { get; set; }
        public string? SmsApiKey { get; set; }
        public string? SmsTemplate { get; set; }
    }

    #endregion

    #region 雷达控制请求

    public class RadarControlRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        [Required]
        public string Command { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class SetParamControlRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
    }

    public class UpdateTiltMotorRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public double Pitch { get; set; }
    }

    #endregion

    #region 雷达参数请求

    public class UpdateRadarParamRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class UpdateAlgoParamRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public Dictionary<string, object>? AlgorithmParameters { get; set; }
    }

    /// <summary>
    /// 更新算法参数请求（对应前端 /api/protocol/update/radar/algoparam）
    /// </summary>
    public class UpdateAlgorithmParamRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        
        // ========== 通用算法参数 ==========
        /// <summary>
        /// 灵敏度系数 (1-9)
        /// </summary>
        public int? SensCoef { get; set; }
        
        /// <summary>
        /// 形变图像抽取倍数
        /// </summary>
        public string? DefoImageDec { get; set; }
        
        /// <summary>
        /// 散射图像抽取倍数
        /// </summary>
        public string? ScatImageDec { get; set; }
        
        /// <summary>
        /// 大气相位误差估计功能开关 (0=距离模式, 1=临近模式, 2=高程模式)
        /// </summary>
        public string? AtmPhaErrEstFuncSwitch { get; set; }
        
        // ========== MIMO Lite 专有参数 ==========
        /// <summary>
        /// 滤波类型 (0=启用, 1=关闭)
        /// </summary>
        public int? FilterType { get; set; }
        
        /// <summary>
        /// Alpha滤波参数 (1-3)
        /// </summary>
        public int? AlphaFilter { get; set; }
        
        /// <summary>
        /// Beta滤波参数 (2-10)
        /// </summary>
        public int? BetaFilter { get; set; }
        
        /// <summary>
        /// 去噪阈值 (0-100)
        /// </summary>
        public int? DeNoiseThread { get; set; }
        
        /// <summary>
        /// 窗口相干 (1-5)
        /// </summary>
        public int? WinCoheren { get; set; }
        
        /// <summary>
        /// 滤波宽度 (1-5)
        /// </summary>
        public int? FilterWidth { get; set; }
        
        /// <summary>
        /// 监测模式 (0=Z, 1=B, 2=S)
        /// </summary>
        public string? MonitorMode { get; set; }
        
        /// <summary>
        /// 设备IP地址
        /// </summary>
        public string? Ipv4 { get; set; }
        
        /// <summary>
        /// Slave ID（MIMO Lite专用）
        /// </summary>
        public string? SlaveId { get; set; }
        
        /// <summary>
        /// 指令编号（用于MQTT发布）
        /// </summary>
        public string? Command { get; set; }
    }

    public class UpdateSpeedTargetRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        public string? TimeUnit { get; set; }
    }

    public class UpdateColorBarRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        public string? ColorBarConfig { get; set; }
    }

    public class UpdateHiddenAnalysisRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        public string? HiddenAreaConfig { get; set; }
    }

    #endregion

    #region 雷达图像请求

    public class GenerateImageRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string? FileName { get; set; }
        public int Sequence { get; set; }
        public string? Status { get; set; }
        public string? TimeUnit { get; set; }
        public long Ts { get; set; }
        public string? Type { get; set; }
        // 新增字段（用于Service调用）
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? ImageType { get; set; }
    }

    #endregion

    #region 数据管理请求

    public class DataRestoreRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public string? GeoMaskId { get; set; }
        public string? GeoMaskType { get; set; }
        [Required]
        public string StartTime { get; set; } = string.Empty;
        [Required]
        public string EndTime { get; set; } = string.Empty;
    }

    public class DataGenerateRequest
    {
        [Required]
        public string Url { get; set; } = string.Empty;
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        [Required]
        public string StartTime { get; set; } = string.Empty;
        [Required]
        public string EndTime { get; set; } = string.Empty;
        public int Interval { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public string? MarkId { get; set; }
        public string? GeoMarkId { get; set; }  // 别名，与MarkId相同
        public string? Target { get; set; }
        public double CurrentValue { get; set; }
    }

    #endregion

    #region 系统配置请求

    public class UpdateDiskStorageRequest
    {
        public double DiscSpacePercentage { get; set; }
        public bool DeleteFile { get; set; }
    }

    #endregion

    #region 告警记录请求

    public class AlarmRecordQueryRequest
    {
        public string? ProjectId { get; set; }
        public string? DeviceId { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int? Level { get; set; }
        public string? Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AddAlarmMessageRequest
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public string? AlarmType { get; set; }
        public int? Level { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }

    #endregion

    #region 图层管理请求

    public class AddLayerRequest
    {
        [Required]
        public string Oid { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Url { get; set; }
        public string? UserId { get; set; }
        public string? PostId { get; set; }
        public string? DivisionId { get; set; }
        public string? OrgId { get; set; }
        public string? TreeId { get; set; }
    }

    public class EnableLayerRequest
    {
        [Required]
        public string Oid { get; set; } = string.Empty;
        public bool Enable { get; set; }
    }

    public class ShowLayerRequest
    {
        [Required]
        public string Oid { get; set; } = string.Empty;
        public bool Show { get; set; }
    }

    #endregion

    #region 系统日志请求

    public class AddRadarLogRequest
    {
        [Required]
        public string OperateContent { get; set; } = string.Empty;
        [Required]
        public string OperateUsername { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
    }

    #endregion
}

