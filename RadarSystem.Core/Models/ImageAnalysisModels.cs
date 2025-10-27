using System;
using System.Collections.Generic;

namespace RadarSystem.Core.Models
{
    /// <summary>
    /// 图像分析结果
    /// </summary>
    public class ImageAnalysisResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime AnalysisTime { get; set; }
        public double ProcessingTimeMs { get; set; }
        public int TargetCount { get; set; }
        public List<DetectedTarget> Targets { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// 检测到的目标
    /// </summary>
    public class DetectedTarget
    {
        public int Id { get; set; }
        public double RangePosition { get; set; }
        public double AnglePosition { get; set; }
        public float Amplitude { get; set; }
        public float Confidence { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// 检测参数
    /// </summary>
    public class DetectionParameters
    {
        public float ThresholdAmplitude { get; set; } = 0.5f;
        public float MinConfidence { get; set; } = 0.6f;
        public int MinTargetSize { get; set; } = 3;
        public int MaxTargetSize { get; set; } = 100;
        public bool EnableFiltering { get; set; } = true;
        public string[] TargetTypes { get; set; } = Array.Empty<string>();
        public Dictionary<string, object> CustomParameters { get; set; } = new();
    }

    /// <summary>
    /// 目标检测结果
    /// </summary>
    public class TargetDetectionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime DetectionTime { get; set; }
        public double ProcessingTimeMs { get; set; }
        public int TotalTargets { get; set; }
        public List<DetectedTarget> Targets { get; set; } = new();
        public DetectionStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// 检测统计信息
    /// </summary>
    public class DetectionStatistics
    {
        public int TotalPixels { get; set; }
        public int ProcessedPixels { get; set; }
        public float AverageAmplitude { get; set; }
        public float MaxAmplitude { get; set; }
        public float MinAmplitude { get; set; }
        public Dictionary<string, int> TargetTypeCount { get; set; } = new();
    }
}

