namespace RadarSystem.WebAPI.Models
{
    /// <summary>
    /// 统一API响应格式
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? RequestId { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Code = 200,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(int code, string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Code = code,
                Message = message
            };
        }
    }

    /// <summary>
    /// 分页响应
    /// </summary>
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    }

    // ==================== 具体响应模型 ====================

    /// <summary>
    /// 图像切片生成结果
    /// </summary>
    public class TileGenerationResult
    {
        public string TaskId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public int TileCount { get; set; }
    }

    /// <summary>
    /// 图像信息
    /// </summary>
    public class ImageInfo
    {
        public string ImageId { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; }
        public long FileSize { get; set; }
    }

    /// <summary>
    /// 设备类型信息
    /// </summary>
    public class DeviceTypeInfo
    {
        public string TypeCode { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// 雷达数据记录
    /// </summary>
    public class RadarDataRecord
    {
        public string RecordId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, double> Values { get; set; } = new();
    }

    /// <summary>
    /// 数据统计结果
    /// </summary>
    public class DataStatistics
    {
        public long TotalCount { get; set; }
        public DateTime? FirstDataTime { get; set; }
        public DateTime? LastDataTime { get; set; }
        public Dictionary<string, double> Averages { get; set; } = new();
        public Dictionary<string, double> MaxValues { get; set; } = new();
        public Dictionary<string, double> MinValues { get; set; } = new();
    }

    /// <summary>
    /// 数据质量报告
    /// </summary>
    public class DataQualityReport
    {
        public string DeviceId { get; set; } = string.Empty;
        public DateTime ReportTime { get; set; }
        public double DataCompleteness { get; set; }
        public double DataAccuracy { get; set; }
        public int AnomalyCount { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    /// <summary>
    /// 分析结果
    /// </summary>
    public class AnalysisResult
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime AnalysisTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, double> Metrics { get; set; } = new();
        public string? ImagePath { get; set; }
    }

    /// <summary>
    /// 报表信息
    /// </summary>
    public class ReportInfo
    {
        public string ReportId { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public DateTime GenerateTime { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    /// <summary>
    /// 报表生成结果
    /// </summary>
    public class ReportGenerationResult
    {
        public string ReportId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// 报表模板信息
    /// </summary>
    public class ReportTemplate
    {
        public string TemplateId { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 颜色映射配置
    /// </summary>
    public class ColorMapConfig
    {
        public string MapType { get; set; } = string.Empty;
        public List<ColorStop> Colors { get; set; } = new();
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }

    /// <summary>
    /// 颜色停止点
    /// </summary>
    public class ColorStop
    {
        public double Position { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// 系统参数配置
    /// </summary>
    public class SystemParameters
    {
        public Dictionary<string, string> Settings { get; set; } = new();
    }

    /// <summary>
    /// 设备参数配置
    /// </summary>
    public class DeviceParameters
    {
        public string DeviceId { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new();
    }

    /// <summary>
    /// 算法参数配置
    /// </summary>
    public class AlgorithmParameters
    {
        public string AlgorithmType { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new();
    }

    /// <summary>
    /// 登出响应
    /// </summary>
    public class LogoutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 密码修改响应
    /// </summary>
    public class PasswordChangeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

