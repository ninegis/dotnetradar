using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.Core.Models;

namespace RadarSystem.ImageAnalysis.Services
{
    /// <summary>
    /// 形变分析器 - 基于SAR图像的形变分析算法
    /// </summary>
    public class DeformationAnalyzer
    {
        private readonly ILogger<DeformationAnalyzer> _logger;
        
        public DeformationAnalyzer(ILogger<DeformationAnalyzer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 计算形变图（当前图像与参考图像的差分）
        /// </summary>
        public async Task<DeformationResult> AnalyzeDeformationAsync(
            float[,] currentImage,
            float[,] referenceImage,
            DeformationConfig config)
        {
            var startTime = DateTime.Now;
            var result = new DeformationResult();
            
            try
            {
                _logger.LogInformation("开始形变分析: 尺寸={Width}x{Height}", 
                    currentImage.GetLength(0), currentImage.GetLength(1));
                
                // 验证输入
                ValidateImages(currentImage, referenceImage);
                
                int width = currentImage.GetLength(0);
                int height = currentImage.GetLength(1);
                
                // 1. 计算差分图像
                var deformationMap = CalculateDifference(currentImage, referenceImage);
                
                // 2. 相干性分析（可选）
                float[,]? coherenceMap = null;
                if (config.EnableCoherenceAnalysis)
                {
                    coherenceMap = await CalculateCoherenceAsync(currentImage, referenceImage);
                }
                
                // 3. 检测形变点
                var deformationPoints = DetectDeformationPoints(deformationMap, coherenceMap, config);
                
                // 4. 统计分析
                var statistics = CalculateStatistics(deformationMap, deformationPoints);
                
                // 5. 报警判断
                var alarms = GenerateAlarms(deformationPoints, config);
                
                result.Success = true;
                result.DeformationMap = deformationMap;
                result.CoherenceMap = coherenceMap;
                result.DeformationPoints = deformationPoints;
                result.Statistics = statistics;
                result.Alarms = alarms;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                
                _logger.LogInformation("形变分析完成: 检测到{Count}个形变点, 耗时{Time}ms", 
                    deformationPoints.Count, result.ProcessingTimeMs);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "形变分析失败");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                return result;
            }
        }
        
        /// <summary>
        /// 计算时序形变（多期图像分析）
        /// </summary>
        public async Task<TimeSeriesDeformationResult> AnalyzeTimeSeriesAsync(
            List<TimestampedImage> images,
            DeformationConfig config)
        {
            var result = new TimeSeriesDeformationResult();
            
            try
            {
                _logger.LogInformation("开始时序形变分析: 图像数量={Count}", images.Count);
                
                if (images.Count < 2)
                {
                    throw new ArgumentException("时序分析至少需要2期图像");
                }
                
                // 使用第一期作为参考
                var referenceImage = images[0];
                
                // 计算每期相对于参考的形变
                var deformations = new List<DeformationResult>();
                
                for (int i = 1; i < images.Count; i++)
                {
                    var deformation = await AnalyzeDeformationAsync(
                        images[i].Image, 
                        referenceImage.Image, 
                        config);
                    
                    deformation.Timestamp = images[i].Timestamp;
                    deformations.Add(deformation);
                }
                
                // 计算累积形变
                var cumulativeDeformation = CalculateCumulativeDeformation(deformations);
                
                // 计算形变速率
                var velocityMap = CalculateDeformationVelocity(deformations, images);
                
                result.Success = true;
                result.Deformations = deformations;
                result.CumulativeDeformation = cumulativeDeformation;
                result.VelocityMap = velocityMap;
                result.ReferenceTimestamp = referenceImage.Timestamp;
                
                _logger.LogInformation("时序形变分析完成");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "时序形变分析失败");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
        
        #region 私有方法
        
        /// <summary>
        /// 验证图像
        /// </summary>
        private void ValidateImages(float[,] img1, float[,] img2)
        {
            if (img1 == null || img2 == null)
            {
                throw new ArgumentNullException("图像不能为空");
            }
            
            if (img1.GetLength(0) != img2.GetLength(0) || img1.GetLength(1) != img2.GetLength(1))
            {
                throw new ArgumentException("图像尺寸不匹配");
            }
        }
        
        /// <summary>
        /// 计算差分图像
        /// </summary>
        private float[,] CalculateDifference(float[,] current, float[,] reference)
        {
            int width = current.GetLength(0);
            int height = current.GetLength(1);
            var diff = new float[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    diff[x, y] = current[x, y] - reference[x, y];
                }
            }
            
            return diff;
        }
        
        /// <summary>
        /// 计算相干性图
        /// </summary>
        private async Task<float[,]> CalculateCoherenceAsync(float[,] img1, float[,] img2)
        {
            return await Task.Run(() =>
            {
                int width = img1.GetLength(0);
                int height = img1.GetLength(1);
                var coherence = new float[width, height];
                
                int windowSize = 5; // 相干性计算窗口大小
                int halfWindow = windowSize / 2;
                
                for (int x = halfWindow; x < width - halfWindow; x++)
                {
                    for (int y = halfWindow; y < height - halfWindow; y++)
                    {
                        coherence[x, y] = CalculateLocalCoherence(img1, img2, x, y, windowSize);
                    }
                }
                
                return coherence;
            });
        }
        
        /// <summary>
        /// 计算局部相干性
        /// </summary>
        private float CalculateLocalCoherence(float[,] img1, float[,] img2, int cx, int cy, int windowSize)
        {
            int halfWindow = windowSize / 2;
            double sumProduct = 0;
            double sum1Squared = 0;
            double sum2Squared = 0;
            
            for (int dx = -halfWindow; dx <= halfWindow; dx++)
            {
                for (int dy = -halfWindow; dy <= halfWindow; dy++)
                {
                    float val1 = img1[cx + dx, cy + dy];
                    float val2 = img2[cx + dx, cy + dy];
                    
                    sumProduct += val1 * val2;
                    sum1Squared += val1 * val1;
                    sum2Squared += val2 * val2;
                }
            }
            
            if (sum1Squared == 0 || sum2Squared == 0)
            {
                return 0;
            }
            
            return (float)(sumProduct / Math.Sqrt(sum1Squared * sum2Squared));
        }
        
        /// <summary>
        /// 检测形变点
        /// </summary>
        private List<DeformationPoint> DetectDeformationPoints(
            float[,] deformationMap, 
            float[,]? coherenceMap,
            DeformationConfig config)
        {
            var points = new List<DeformationPoint>();
            int width = deformationMap.GetLength(0);
            int height = deformationMap.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float deformation = deformationMap[x, y];
                    float absDeformation = Math.Abs(deformation);
                    
                    // 检查是否超过阈值
                    if (absDeformation >= config.DeformationThreshold)
                    {
                        // 检查相干性（如果可用）
                        if (coherenceMap != null)
                        {
                            float coherence = coherenceMap[x, y];
                            if (coherence < config.MinCoherence)
                            {
                                continue; // 相干性太低，跳过
                            }
                        }
                        
                        var point = new DeformationPoint
                        {
                            X = x,
                            Y = y,
                            Deformation = deformation,
                            Coherence = coherenceMap?[x, y] ?? 1.0f,
                            Level = ClassifyDeformationLevel(absDeformation, config)
                        };
                        
                        points.Add(point);
                    }
                }
            }
            
            return points;
        }
        
        /// <summary>
        /// 分类形变等级
        /// </summary>
        private DeformationLevel ClassifyDeformationLevel(float absDeformation, DeformationConfig config)
        {
            if (absDeformation >= config.CriticalThreshold)
            {
                return DeformationLevel.Critical;
            }
            else if (absDeformation >= config.WarningThreshold)
            {
                return DeformationLevel.Warning;
            }
            else
            {
                return DeformationLevel.Normal;
            }
        }
        
        /// <summary>
        /// 统计分析
        /// </summary>
        private DeformationStatistics CalculateStatistics(
            float[,] deformationMap,
            List<DeformationPoint> points)
        {
            var values = deformationMap.Cast<float>().ToList();
            
            return new DeformationStatistics
            {
                MaxDeformation = values.Max(),
                MinDeformation = values.Min(),
                MeanDeformation = values.Average(),
                StdDeviation = CalculateStdDev(values),
                DeformationPointCount = points.Count,
                NormalCount = points.Count(p => p.Level == DeformationLevel.Normal),
                WarningCount = points.Count(p => p.Level == DeformationLevel.Warning),
                CriticalCount = points.Count(p => p.Level == DeformationLevel.Critical)
            };
        }
        
        /// <summary>
        /// 计算标准差
        /// </summary>
        private double CalculateStdDev(List<float> values)
        {
            double mean = values.Average();
            double sumSquaredDiff = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumSquaredDiff / values.Count);
        }
        
        /// <summary>
        /// 生成报警信息
        /// </summary>
        private List<DeformationAlarm> GenerateAlarms(
            List<DeformationPoint> points,
            DeformationConfig config)
        {
            var alarms = new List<DeformationAlarm>();
            
            var criticalPoints = points.Where(p => p.Level == DeformationLevel.Critical).ToList();
            var warningPoints = points.Where(p => p.Level == DeformationLevel.Warning).ToList();
            
            if (criticalPoints.Any())
            {
                alarms.Add(new DeformationAlarm
                {
                    Level = AlarmLevel.Critical,
                    Message = $"检测到{criticalPoints.Count}个严重形变点",
                    Points = criticalPoints,
                    Timestamp = DateTime.Now
                });
            }
            
            if (warningPoints.Any())
            {
                alarms.Add(new DeformationAlarm
                {
                    Level = AlarmLevel.Warning,
                    Message = $"检测到{warningPoints.Count}个警告级形变点",
                    Points = warningPoints,
                    Timestamp = DateTime.Now
                });
            }
            
            return alarms;
        }
        
        /// <summary>
        /// 计算累积形变
        /// </summary>
        private float[,] CalculateCumulativeDeformation(List<DeformationResult> deformations)
        {
            if (!deformations.Any())
            {
                return new float[0, 0];
            }
            
            var firstMap = deformations[0].DeformationMap;
            int width = firstMap.GetLength(0);
            int height = firstMap.GetLength(1);
            var cumulative = new float[width, height];
            
            foreach (var deformation in deformations)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        cumulative[x, y] += deformation.DeformationMap[x, y];
                    }
                }
            }
            
            return cumulative;
        }
        
        /// <summary>
        /// 计算形变速率
        /// </summary>
        private float[,] CalculateDeformationVelocity(
            List<DeformationResult> deformations,
            List<TimestampedImage> images)
        {
            if (deformations.Count < 2)
            {
                return new float[0, 0];
            }
            
            var firstMap = deformations[0].DeformationMap;
            int width = firstMap.GetLength(0);
            int height = firstMap.GetLength(1);
            var velocity = new float[width, height];
            
            // 计算总时间跨度（天）
            double totalDays = (images.Last().Timestamp - images.First().Timestamp).TotalDays;
            
            if (totalDays > 0)
            {
                var totalDeformation = CalculateCumulativeDeformation(deformations);
                
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        velocity[x, y] = (float)(totalDeformation[x, y] / totalDays); // mm/day
                    }
                }
            }
            
            return velocity;
        }
        
        #endregion
    }
    
    #region 数据模型
    
    /// <summary>
    /// 形变配置
    /// </summary>
    public class DeformationConfig
    {
        public float DeformationThreshold { get; set; } = 2.0f; // mm
        public float WarningThreshold { get; set; } = 5.0f; // mm
        public float CriticalThreshold { get; set; } = 10.0f; // mm
        public float MinCoherence { get; set; } = 0.3f;
        public bool EnableCoherenceAnalysis { get; set; } = true;
    }
    
    /// <summary>
    /// 形变结果
    /// </summary>
    public class DeformationResult
    {
        public bool Success { get; set; }
        public float[,] DeformationMap { get; set; } = new float[0, 0];
        public float[,]? CoherenceMap { get; set; }
        public List<DeformationPoint> DeformationPoints { get; set; } = new();
        public DeformationStatistics Statistics { get; set; } = new();
        public List<DeformationAlarm> Alarms { get; set; } = new();
        public double ProcessingTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// 形变点
    /// </summary>
    public class DeformationPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Deformation { get; set; }
        public float Coherence { get; set; }
        public DeformationLevel Level { get; set; }
    }
    
    /// <summary>
    /// 形变等级
    /// </summary>
    public enum DeformationLevel
    {
        Normal,
        Warning,
        Critical
    }
    
    /// <summary>
    /// 形变统计
    /// </summary>
    public class DeformationStatistics
    {
        public float MaxDeformation { get; set; }
        public float MinDeformation { get; set; }
        public double MeanDeformation { get; set; }
        public double StdDeviation { get; set; }
        public int DeformationPointCount { get; set; }
        public int NormalCount { get; set; }
        public int WarningCount { get; set; }
        public int CriticalCount { get; set; }
    }
    
    /// <summary>
    /// 形变报警
    /// </summary>
    public class DeformationAlarm
    {
        public AlarmLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<DeformationPoint> Points { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// 时序形变结果
    /// </summary>
    public class TimeSeriesDeformationResult
    {
        public bool Success { get; set; }
        public List<DeformationResult> Deformations { get; set; } = new();
        public float[,] CumulativeDeformation { get; set; } = new float[0, 0];
        public float[,] VelocityMap { get; set; } = new float[0, 0];
        public DateTime ReferenceTimestamp { get; set; }
        public string? ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// 带时间戳的图像
    /// </summary>
    public class TimestampedImage
    {
        public float[,] Image { get; set; } = new float[0, 0];
        public DateTime Timestamp { get; set; }
    }
    
    #endregion
}

