using System;
using System.Collections.Generic;

namespace RadarSystem.Core.Models
{
    /// <summary>
    /// 雷达数据模型
    /// </summary>
    public class RadarData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string DataType { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TaskId { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public float[][] RadarImageMatrix { get; set; } = Array.Empty<float[]>();
        public double[] RangeDistances { get; set; } = Array.Empty<double>();
        public double[] AngleDistances { get; set; } = Array.Empty<double>();
        public float RangeResolution { get; set; }
        public float AngleResolution { get; set; }
        public float RangeMin { get; set; }
        public float AngleMin { get; set; }
    }

    /// <summary>
    /// 雷达原始数据接收模型
    /// </summary>
    public class ReceivedRadarData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public DateTime ReceiveTime { get; set; }
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// SAR文件数据模型 - 完整对应Java SarFileData
    /// </summary>
    public class SarFileData
    {
        public string FilePath { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public string DataType { get; set; } = string.Empty;
        public int OffsetByte { get; set; }
        public int Sequence { get; set; }
        public bool Md5CheckResult { get; set; }
        public int TaskId { get; set; }
        public string Date { get; set; } = string.Empty;
        public int TimeMillis { get; set; }
        public float RangeResolution { get; set; }
        public int RangeNumber { get; set; }
        public float RangeMin { get; set; }
        public float AngleResolution { get; set; }
        public int AngleNumber { get; set; }
        public float AngleMin { get; set; }
        public int SarDataType { get; set; }
        public int DataSize { get; set; }
        public float ImageMaxAmplitude { get; set; }
        public float ImageMinAmplitude { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }
        public float NorthAngle { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string DeviceId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 雷达配置模型
    /// </summary>
    public class RadarConfiguration
    {
        public string[] DeviceIds { get; set; } = Array.Empty<string>();
        public int GenerationTimeMinutes { get; set; }
        public string ProjectId { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public bool EnableDifferenceCalculation { get; set; } = true;
        public int MaxQueueSize { get; set; } = 1000;
    }
}
