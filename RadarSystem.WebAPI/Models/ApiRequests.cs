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
        
        /// <summary>
        /// Slave ID（MIMO Lite专用）
        /// </summary>
        public string? SlaveId { get; set; }
        
        // ========== 算法参数（32个字段） ==========
        /// <summary>
        /// 监测模式 (Z/B/S)
        /// </summary>
        public string? MonMode { get; set; }
        
        /// <summary>
        /// 相位滤波类型选择控制变量 (0/1)
        /// </summary>
        public int? PhaFltTypeCtrl { get; set; }
        
        /// <summary>
        /// 滤波半窗长
        /// </summary>
        public int? FltHalfWinLen { get; set; }
        
        /// <summary>
        /// 大气滤波使能
        /// </summary>
        public double? AtmFltEn { get; set; }
        
        /// <summary>
        /// 均值加权
        /// </summary>
        public double? MeanWgt { get; set; }
        
        /// <summary>
        /// 压缩形变阈值
        /// </summary>
        public int? CmpDefThr { get; set; }
        
        /// <summary>
        /// 压缩倍数
        /// </summary>
        public int? CmpMult { get; set; }
        
        /// <summary>
        /// 幅度检测门限
        /// </summary>
        public double? AmpDetThr { get; set; }
        
        /// <summary>
        /// 大气滤波参数 A
        /// </summary>
        public double? AtmFltParaA { get; set; }
        
        /// <summary>
        /// 大气滤波参数 B
        /// </summary>
        public double? AtmFltParaB { get; set; }
        
        /// <summary>
        /// 第二阶段大气校正门限 1
        /// </summary>
        public double? AtmCorrThr2nd_1 { get; set; }
        
        /// <summary>
        /// 二次大气补偿更新周期
        /// </summary>
        public double? AtmCompUpdPer { get; set; }
        
        /// <summary>
        /// 第二阶段大气校正门限 2
        /// </summary>
        public double? AtmCorrThr2nd_2 { get; set; }
        
        /// <summary>
        /// 形变图像抽帧
        /// </summary>
        public string? DefImgDecim { get; set; }
        
        /// <summary>
        /// 复数图图像抽帧
        /// </summary>
        public string? CplxImgDecim { get; set; }
        
        /// <summary>
        /// 大气校正算法
        /// </summary>
        public string? AtmCorrAlg { get; set; }
        
        /// <summary>
        /// 大气相位误差估计距离 1
        /// </summary>
        public double? AtmPhaErrEstDist_1 { get; set; }
        
        /// <summary>
        /// 大气相位误差估计距离 2
        /// </summary>
        public double? AtmPhaErrEstDist_2 { get; set; }
        
        /// <summary>
        /// 标准差加权
        /// </summary>
        public double? StdDevWgt { get; set; }
        
        /// <summary>
        /// 短时形变量积参数
        /// </summary>
        public double? ShortDefAccPara { get; set; }
        
        /// <summary>
        /// 去噪门限
        /// </summary>
        public int? DenoiseThr { get; set; }
        
        /// <summary>
        /// 噪声均衡使能
        /// </summary>
        public double? IsNoiseEq { get; set; }
        
        /// <summary>
        /// 噪声均衡类型
        /// </summary>
        public double? NoiseEqType { get; set; }
        
        /// <summary>
        /// 幅度偏差选择门限初始值
        /// </summary>
        public double? AmpDevSelThrInit { get; set; }
        
        /// <summary>
        /// 相干系数门限初始值
        /// </summary>
        public double? CohCoeThrInit { get; set; }
        
        /// <summary>
        /// 有效PS点相关系数
        /// </summary>
        public double? CorrCoeffEffPSPts { get; set; }
        
        /// <summary>
        /// 有效PS点数
        /// </summary>
        public double? EffPSPts { get; set; }
        
        /// <summary>
        /// 干涉相位残差门限
        /// </summary>
        public double? IfgPhaResThr { get; set; }
        
        /// <summary>
        /// 单点门限
        /// </summary>
        public double? SingPntThr { get; set; }
        
        /// <summary>
        /// PS点灵敏度
        /// </summary>
        public int? PSPntSens { get; set; }
        
        /// <summary>
        /// PS门限调整系数
        /// </summary>
        public double? PSThrAdjCoeff { get; set; }
        
        /// <summary>
        /// 相干半窗长
        /// </summary>
        public int? CohHalfWinLen { get; set; }
        
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

