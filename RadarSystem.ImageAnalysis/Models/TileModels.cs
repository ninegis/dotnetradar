using System;
using System.Collections.Generic;
using System.Drawing;

namespace RadarSystem.ImageAnalysis.Models
{
    /// <summary>
    /// 切片配置
    /// </summary>
    public class TileConfiguration
    {
        /// <summary>
        /// 原始图像宽度
        /// </summary>
        public int ImageWidth { get; set; }
        
        /// <summary>
        /// 原始图像高度
        /// </summary>
        public int ImageHeight { get; set; }
        
        /// <summary>
        /// 距离向切片数量
        /// </summary>
        public int RngTileCount { get; set; }
        
        /// <summary>
        /// 角度向切片数量
        /// </summary>
        public int AngTileCount { get; set; }
        
        /// <summary>
        /// 每个切片的宽度
        /// </summary>
        public int TileWidth { get; set; }
        
        /// <summary>
        /// 每个切片的高度
        /// </summary>
        public int TileHeight { get; set; }
        
        /// <summary>
        /// 总切片数量
        /// </summary>
        public int TotalTileCount => RngTileCount * AngTileCount;
    }
    
    /// <summary>
    /// 切片生成结果
    /// </summary>
    public class TileGenerationResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 生成的切片数量
        /// </summary>
        public int TileCount { get; set; }
        
        /// <summary>
        /// 处理时间（毫秒）
        /// </summary>
        public double ProcessingTimeMs { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// 输出路径
        /// </summary>
        public string? OutputPath { get; set; }
        
        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
    
    /// <summary>
    /// 颜色映射配置
    /// </summary>
    public class ColorMapConfig
    {
        /// <summary>
        /// 最小值
        /// </summary>
        public double MinValue { get; set; }
        
        /// <summary>
        /// 最大值
        /// </summary>
        public double MaxValue { get; set; }
        
        /// <summary>
        /// HSL色相起始值 (0-360)
        /// </summary>
        public int HslHStart { get; set; }
        
        /// <summary>
        /// HSL色相结束值 (0-360)
        /// </summary>
        public int HslHEnd { get; set; }
        
        /// <summary>
        /// HSL饱和度 (0-1)
        /// </summary>
        public double HslS { get; set; } = 1.0;
        
        /// <summary>
        /// HSL亮度 (0-1)
        /// </summary>
        public double HslL { get; set; } = 0.5;
        
        /// <summary>
        /// 是否启用过滤
        /// </summary>
        public bool FilterEnable { get; set; }
        
        /// <summary>
        /// 过滤最小值
        /// </summary>
        public double FilterMin { get; set; }
        
        /// <summary>
        /// 过滤最大值
        /// </summary>
        public double FilterMax { get; set; }
        
        /// <summary>
        /// 过滤区域透明度 (0-255)
        /// </summary>
        public byte FilterAlpha { get; set; } = 0;
    }
    
    /// <summary>
    /// 切片元数据
    /// </summary>
    public class TileMetadata
    {
        /// <summary>
        /// 切片配置
        /// </summary>
        public TileConfiguration Configuration { get; set; } = new();
        
        /// <summary>
        /// 数据类型（deformation/scattering/velocity等）
        /// </summary>
        public string DataType { get; set; } = string.Empty;
        
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;
        
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 颜色映射配置
        /// </summary>
        public ColorMapConfig? ColorMap { get; set; }
        
        /// <summary>
        /// 扩展信息
        /// </summary>
        public Dictionary<string, object> ExtendedInfo { get; set; } = new();
    }
    
    /// <summary>
    /// 单个切片信息
    /// </summary>
    public class TileInfo
    {
        /// <summary>
        /// 距离向索引
        /// </summary>
        public int RngIndex { get; set; }
        
        /// <summary>
        /// 角度向索引
        /// </summary>
        public int AngIndex { get; set; }
        
        /// <summary>
        /// 切片文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// 切片数据范围 - X起始
        /// </summary>
        public int StartX { get; set; }
        
        /// <summary>
        /// 切片数据范围 - Y起始
        /// </summary>
        public int StartY { get; set; }
        
        /// <summary>
        /// 切片数据范围 - X结束
        /// </summary>
        public int EndX { get; set; }
        
        /// <summary>
        /// 切片数据范围 - Y结束
        /// </summary>
        public int EndY { get; set; }
        
        /// <summary>
        /// 切片宽度
        /// </summary>
        public int Width => EndX - StartX;
        
        /// <summary>
        /// 切片高度
        /// </summary>
        public int Height => EndY - StartY;
    }
}

