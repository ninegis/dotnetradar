using System;
using System.Collections.Generic;

namespace RadarSystem.Core.Models
{
    /// <summary>
    /// 地理标记模型
    /// </summary>
    public class GeoMark
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Point/Line/Polygon
        public string? CoordinatesJson { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 颜色配置模型
    /// </summary>
    public class ColorSetting
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string SettingType { get; set; } = string.Empty; // terrain/defo/scat
        public int Type { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public int HslHStart { get; set; }
        public int HslHEnd { get; set; }
        public int HslDirection { get; set; }
        public bool FilterEnable { get; set; }
        public double? FilterMin { get; set; }
        public double? FilterMax { get; set; }
        public double? FilterAlpha { get; set; }
        public double HslS { get; set; } = 1.0;
        public double HslL { get; set; } = 0.5;
        public string? ValueArrayJson { get; set; }
        public string? ColorArrayJson { get; set; }
        public bool AutoMode { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 面板配置模型
    /// </summary>
    public class PanelConfig
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string PanelType { get; set; } = string.Empty; // target/event/sarimage/alarm/mimo
        public string ConfigJson { get; set; } = "{}";
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 图像标记模型
    /// </summary>
    public class ImageMark
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string? ImageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MarkType { get; set; } = string.Empty; // Point/Line/Polygon/Text
        public string? CoordinatesJson { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 图像分析配置模型
    /// </summary>
    public class ImageAnalysisConfig
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public int StandardImageSidePixel { get; set; } = 16384;
        public int CompressImageSidePixel { get; set; } = 1024;
        public int MatrixTileRngNum { get; set; } = 1203;
        public int MatrixTileAngNum { get; set; } = 61;
        public bool GenDefo { get; set; }
        public bool GenScat { get; set; } = true;
        public bool GenSpeed { get; set; }
        public bool GenAcceleration { get; set; }
        public string? ConfigJson { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }

    // ===== Request DTOs =====

    /// <summary>
    /// 创建地理标记请求
    /// </summary>
    public class CreateGeoMarkRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? CoordinatesJson { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }

    /// <summary>
    /// 更新地理标记请求
    /// </summary>
    public class UpdateGeoMarkRequest
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? CoordinatesJson { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }

    /// <summary>
    /// 创建报警规则请求
    /// </summary>
    public class CreateAlarmRuleRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string? RuleDescription { get; set; }
        public string? AlarmContent { get; set; }
        public string RuleOperator { get; set; } = ">";
        public int AlarmLevel { get; set; } = 1;
        public bool Enable { get; set; } = true;
        public double AlarmThreshold { get; set; }
        public string? DevicesJson { get; set; }
        public string? GeoMarkArrayJson { get; set; }
        public string? DataSource { get; set; }
        public string? TargetType { get; set; }
        public string? Mode { get; set; }
    }

    /// <summary>
    /// 更新报警规则请求
    /// </summary>
    public class UpdateAlarmRuleRequest
    {
        public string? RuleName { get; set; }
        public string? RuleDescription { get; set; }
        public string? AlarmContent { get; set; }
        public string? RuleOperator { get; set; }
        public int? AlarmLevel { get; set; }
        public bool? Enable { get; set; }
        public double? AlarmThreshold { get; set; }
        public string? DevicesJson { get; set; }
        public string? GeoMarkArrayJson { get; set; }
        public string? DataSource { get; set; }
        public string? TargetType { get; set; }
        public string? Mode { get; set; }
    }

    /// <summary>
    /// 创建颜色配置请求
    /// </summary>
    public class CreateColorSettingRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string SettingType { get; set; } = string.Empty;
        public int Type { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public int HslHStart { get; set; }
        public int HslHEnd { get; set; }
        public int HslDirection { get; set; }
        public bool FilterEnable { get; set; }
        public double? FilterMin { get; set; }
        public double? FilterMax { get; set; }
        public double? FilterAlpha { get; set; }
        public double HslS { get; set; } = 1.0;
        public double HslL { get; set; } = 0.5;
        public string? ValueArrayJson { get; set; }
        public string? ColorArrayJson { get; set; }
        public bool AutoMode { get; set; }
    }

    /// <summary>
    /// 创建面板配置请求
    /// </summary>
    public class CreatePanelConfigRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string PanelType { get; set; } = string.Empty;
        public string ConfigJson { get; set; } = "{}";
    }

    /// <summary>
    /// 创建图像标记请求
    /// </summary>
    public class CreateImageMarkRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string? ImageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MarkType { get; set; } = string.Empty;
        public string? CoordinatesJson { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
    }

    /// <summary>
    /// 创建图像分析配置请求
    /// </summary>
    public class CreateImageAnalysisConfigRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public int StandardImageSidePixel { get; set; } = 16384;
        public int CompressImageSidePixel { get; set; } = 1024;
        public int MatrixTileRngNum { get; set; } = 1203;
        public int MatrixTileAngNum { get; set; } = 61;
        public bool GenDefo { get; set; }
        public bool GenScat { get; set; } = true;
        public bool GenSpeed { get; set; }
        public bool GenAcceleration { get; set; }
        public string? ConfigJson { get; set; }
    }
}

