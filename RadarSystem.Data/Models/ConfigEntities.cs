using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 地理标记实体
    /// </summary>
    [Table("geo_marks")]
    public class GeoMarkEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [Column("type")]
        [MaxLength(32)]
        public string Type { get; set; } = string.Empty; // Point/Line/Polygon

        [Column("coordinates_json")]
        public string? CoordinatesJson { get; set; } // JSON 格式存储坐标

        [Column("description")]
        public string? Description { get; set; }

        [Column("color")]
        [MaxLength(32)]
        public string? Color { get; set; }

        [Column("icon")]
        [MaxLength(128)]
        public string? Icon { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // 导航属性（外键关系在 RadarDbContext 中配置）
        public virtual ProjectEntity? Project { get; set; }
    }

    /// <summary>
    /// 预警规则实体（完整字段版本）
    /// </summary>
    [Table("alarm_rules")]
    public class AlarmRuleEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("rule_name")]
        [MaxLength(256)]
        public string RuleName { get; set; } = string.Empty;

        [Column("alarm_content")]
        public string? AlarmContent { get; set; }

        [Column("enable")]
        public bool Enable { get; set; } = true;

        // 设备ID列表（逗号分隔）
        [Column("devices")]
        public string? Devices { get; set; }

        // 地理标记数组（逗号分隔）
        [Column("geo_mark_array")]
        public string? GeoMarkArray { get; set; }

        // 数据来源（10:连续形变, 00:原始形变）
        [Column("data_source")]
        [MaxLength(16)]
        public string DataSource { get; set; } = "10";

        // 数据值是否为绝对值
        [Column("target_flag")]
        public bool TargetFlag { get; set; } = false;

        // ========== 位移指标启用状态和阈值 ==========
        [Column("enable_displacement")]
        public bool EnableDisplacement { get; set; } = true;

        [Column("displacement_blue")]
        public double? DisplacementBlue { get; set; }

        [Column("displacement_yellow")]
        public double? DisplacementYellow { get; set; }

        [Column("displacement_orange")]
        public double? DisplacementOrange { get; set; }

        [Column("displacement_red")]
        public double? DisplacementRed { get; set; }

        // ========== 速度指标启用状态和阈值 ==========
        [Column("enable_speed")]
        public bool EnableSpeed { get; set; } = false;

        [Column("speed_time_unit")]
        [MaxLength(16)]
        public string? SpeedTimeUnit { get; set; } // 02:30分钟, 03:1小时, 04:1天, 05:1周, 06:1月

        [Column("speed_blue")]
        public double? SpeedBlue { get; set; }

        [Column("speed_yellow")]
        public double? SpeedYellow { get; set; }

        [Column("speed_orange")]
        public double? SpeedOrange { get; set; }

        [Column("speed_red")]
        public double? SpeedRed { get; set; }

        // ========== 加速度指标启用状态和阈值 ==========
        [Column("enable_acceleration")]
        public bool EnableAcceleration { get; set; } = false;

        [Column("acceleration_time_unit")]
        [MaxLength(16)]
        public string? AccelerationTimeUnit { get; set; }

        [Column("acceleration_blue")]
        public double? AccelerationBlue { get; set; }

        [Column("acceleration_yellow")]
        public double? AccelerationYellow { get; set; }

        [Column("acceleration_orange")]
        public double? AccelerationOrange { get; set; }

        [Column("acceleration_red")]
        public double? AccelerationRed { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
    }

    /// <summary>
    /// 颜色配置实体
    /// </summary>
    [Table("color_settings")]
    public class ColorSettingEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("setting_type")]
        [MaxLength(32)]
        public string SettingType { get; set; } = string.Empty; // terrain/defo/scat

        [Column("type")]
        public int Type { get; set; }

        [Column("min_value")]
        public double MinValue { get; set; }

        [Column("max_value")]
        public double MaxValue { get; set; }

        [Column("hsl_h_start")]
        public int HslHStart { get; set; }

        [Column("hsl_h_end")]
        public int HslHEnd { get; set; }

        [Column("hsl_direction")]
        public int HslDirection { get; set; }

        [Column("filter_enable")]
        public bool FilterEnable { get; set; }

        [Column("filter_min")]
        public double? FilterMin { get; set; }

        [Column("filter_max")]
        public double? FilterMax { get; set; }

        [Column("filter_alpha")]
        public double? FilterAlpha { get; set; }

        [Column("hsl_s")]
        public double HslS { get; set; } = 1.0;

        [Column("hsl_l")]
        public double HslL { get; set; } = 0.5;

        [Column("value_array_json")]
        public string? ValueArrayJson { get; set; } // JSON 数组

        [Column("color_array_json")]
        public string? ColorArrayJson { get; set; } // JSON 数组

        [Column("auto_mode")]
        public bool AutoMode { get; set; } = false;

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性（外键关系在 RadarDbContext 中配置）
        public virtual ProjectEntity? Project { get; set; }
    }

    /// <summary>
    /// 面板配置实体
    /// </summary>
    [Table("panel_configs")]
    public class PanelConfigEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("panel_type")]
        [MaxLength(64)]
        public string PanelType { get; set; } = string.Empty; // target/event/sarimage/alarm/mimo

        [Required]
        [Column("config_json")]
        public string ConfigJson { get; set; } = "{}"; // 完整 JSON 配置

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性（外键关系在 RadarDbContext 中配置）
        public virtual ProjectEntity? Project { get; set; }
    }

    /// <summary>
    /// 图像标记实体
    /// </summary>
    [Table("image_marks")]
    public class ImageMarkEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Column("image_id")]
        [MaxLength(64)]
        public string? ImageId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [Column("mark_type")]
        [MaxLength(32)]
        public string MarkType { get; set; } = string.Empty; // Point/Line/Polygon/Text

        [Column("coordinates_json")]
        public string? CoordinatesJson { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("color")]
        [MaxLength(32)]
        public string? Color { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // 导航属性（外键关系在 RadarDbContext 中配置）
        public virtual ProjectEntity? Project { get; set; }
    }

    /// <summary>
    /// 图像分析配置实体
    /// </summary>
    [Table("image_analysis_configs")]
    public class ImageAnalysisConfigEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Column("standard_image_side_pixel")]
        public int StandardImageSidePixel { get; set; } = 16384;

        [Column("compress_image_side_pixel")]
        public int CompressImageSidePixel { get; set; } = 1024;

        [Column("matrix_tile_rng_num")]
        public int MatrixTileRngNum { get; set; } = 1203;

        [Column("matrix_tile_ang_num")]
        public int MatrixTileAngNum { get; set; } = 61;

        [Column("gen_defo")]
        public bool GenDefo { get; set; } = false;

        [Column("gen_scat")]
        public bool GenScat { get; set; } = true;

        [Column("gen_speed")]
        public bool GenSpeed { get; set; } = false;

        [Column("gen_acceleration")]
        public bool GenAcceleration { get; set; } = false;

        /// <summary>
        /// 图像生成类型 (01:按帧号, 02:按时间间隔)
        /// </summary>
        [Column("gen_image_type")]
        [MaxLength(8)]
        public string GenImageType { get; set; } = "01";

        /// <summary>
        /// 形变图生成间隔（分钟）- 按时间生成时使用
        /// </summary>
        [Column("defo_interval")]
        public int DefoInterval { get; set; } = 60;

        /// <summary>
        /// 散射图生成间隔（分钟）- 按时间生成时使用
        /// </summary>
        [Column("scat_interval")]
        public int ScatInterval { get; set; } = 60;

        /// <summary>
        /// 形变图生成帧数 - 按帧号生成时使用（转几圈生成一次）
        /// </summary>
        [Column("defo_number")]
        public int DefoNumber { get; set; } = 10;

        /// <summary>
        /// 散射图生成帧数 - 按帧号生成时使用
        /// </summary>
        [Column("scat_number")]
        public int ScatNumber { get; set; } = 10;

        [Column("config_json")]
        public string? ConfigJson { get; set; } // 保留用于扩展配置

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性（外键关系在 RadarDbContext 中配置）
        public virtual ProjectEntity? Project { get; set; }
    }

    /// <summary>
    /// 项目完整配置（替代原JSON配置文件）
    /// </summary>
    [Table("project_configurations")]
    public class ProjectConfigurationEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        // 基本信息（原本在JSON中）
        [Column("project_name")]
        [MaxLength(256)]
        public string? ProjectName { get; set; }

        [Column("description")]
        [MaxLength(2000)]
        public string? Description { get; set; }

        [Column("contact")]
        [MaxLength(128)]
        public string? Contact { get; set; }

        [Column("phone")]
        [MaxLength(32)]
        public string? Phone { get; set; }

        [Column("email")]
        [MaxLength(128)]
        public string? Email { get; set; }

        // 相机初始化参数
        [Column("camera_longitude")]
        public double? CameraLongitude { get; set; }

        [Column("camera_latitude")]
        public double? CameraLatitude { get; set; }

        [Column("camera_altitude")]
        public double? CameraAltitude { get; set; }

        [Column("camera_heading")]
        public double? CameraHeading { get; set; }

        [Column("camera_pitch")]
        public double? CameraPitch { get; set; }

        [Column("camera_roll")]
        public double? CameraRoll { get; set; }

        // 项目范围
        [Column("min_longitude")]
        public double? MinLongitude { get; set; }

        [Column("max_longitude")]
        public double? MaxLongitude { get; set; }

        [Column("min_latitude")]
        public double? MinLatitude { get; set; }

        [Column("max_latitude")]
        public double? MaxLatitude { get; set; }

        [Column("min_elevation")]
        public double? MinElevation { get; set; }

        [Column("max_elevation")]
        public double? MaxElevation { get; set; }

        // 其他配置（原本在JSON中）
        [Column("extra_config_json")]
        public string? ExtraConfigJson { get; set; }

        // 时间戳
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
    }

    /// <summary>
    /// 图像差分分析配置（原本在JSON中）
    /// </summary>
    [Table("image_diff_analysis_configs")]
    public class ImageDiffAnalysisConfigEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("device_id")]
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;

        // 差分方法
        [Column("diff_method")]
        [MaxLength(32)]
        public string? DiffMethod { get; set; }

        // 参考图像ID
        [Column("reference_image_id")]
        [MaxLength(64)]
        public string? ReferenceImageId { get; set; }

        // 差分阈值
        [Column("diff_threshold")]
        public double DiffThreshold { get; set; } = 10.0;

        // 噪声过滤
        [Column("noise_filter")]
        public bool NoiseFilter { get; set; } = true;

        // 边缘检测
        [Column("edge_detection")]
        public bool EdgeDetection { get; set; } = false;

        // 时间窗口（小时）
        [Column("time_window_hours")]
        public int TimeWindowHours { get; set; } = 24;

        // 启用状态
        [Column("enable")]
        public bool Enable { get; set; } = false;

        // 完整配置JSON
        [Column("config_json")]
        public string? ConfigJson { get; set; }

        // 时间戳
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
        public virtual DeviceEntity? Device { get; set; }
    }

    /// <summary>
    /// 隐患区域分析配置（对应前端autoAnalysisHiddenAreaConfig）
    /// </summary>
    [Table("hidden_area_analysis_configs")]
    public class HiddenAreaAnalysisConfigEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// 隐患点阈值（mm）- 点的位移值超过阈值将视为隐患点
        /// </summary>
        [Column("threshold")]
        public double Threshold { get; set; } = 10.0;

        /// <summary>
        /// 隐患区域面积阈值（m²）- 面积小于阈值的区域将被过滤
        /// </summary>
        [Column("area_threshold")]
        public double AreaThreshold { get; set; } = 1.0;

        /// <summary>
        /// 隐患生成设置（间隔帧）
        /// </summary>
        [Column("analysis_dec")]
        public int AnalysisDec { get; set; } = 1;

        /// <summary>
        /// 是否开启隐患区域分析
        /// </summary>
        [Column("auto_analysis_flag")]
        public bool AutoAnalysisFlag { get; set; } = false;

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
    }

    /// <summary>
    /// 俯仰电机配置（原本在JSON中）
    /// </summary>
    [Table("tilt_motor_configs")]
    public class TiltMotorConfigEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("device_id")]
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;

        // 当前俯仰角（度）
        [Column("current_pitch")]
        public double CurrentPitch { get; set; } = 0.0;

        // 目标俯仰角（度）
        [Column("target_pitch")]
        public double? TargetPitch { get; set; }

        // 最小角度
        [Column("min_pitch")]
        public double MinPitch { get; set; } = -90.0;

        // 最大角度
        [Column("max_pitch")]
        public double MaxPitch { get; set; } = 90.0;

        // 步进角度
        [Column("step_angle")]
        public double StepAngle { get; set; } = 1.0;

        // 转速（度/秒）
        [Column("speed")]
        public double Speed { get; set; } = 10.0;

        // 运动状态
        [Column("is_moving")]
        public bool IsMoving { get; set; } = false;

        // 校准状态
        [Column("is_calibrated")]
        public bool IsCalibrated { get; set; } = false;

        // 完整配置JSON
        [Column("config_json")]
        public string? ConfigJson { get; set; }

        // 时间戳
        [Column("last_move_time")]
        public DateTime? LastMoveTime { get; set; }

        [Column("calibration_time")]
        public DateTime? CalibrationTime { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
        public virtual DeviceEntity? Device { get; set; }
    }
}

