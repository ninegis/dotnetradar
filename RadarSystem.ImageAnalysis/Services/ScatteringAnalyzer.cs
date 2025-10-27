using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RadarSystem.ImageAnalysis.Services
{
    /// <summary>
    /// 散射分析器 - 分析SAR图像的散射特性
    /// </summary>
    public class ScatteringAnalyzer
    {
        private readonly ILogger<ScatteringAnalyzer> _logger;
        
        public ScatteringAnalyzer(ILogger<ScatteringAnalyzer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 分析散射图像
        /// </summary>
        public async Task<ScatteringResult> AnalyzeScatteringAsync(
            float[,] amplitudeImage,
            ScatteringConfig config)
        {
            var startTime = DateTime.Now;
            var result = new ScatteringResult();
            
            try
            {
                _logger.LogInformation("开始散射分析: 尺寸={Width}x{Height}", 
                    amplitudeImage.GetLength(0), amplitudeImage.GetLength(1));
                
                int width = amplitudeImage.GetLength(0);
                int height = amplitudeImage.GetLength(1);
                
                // 1. 归一化幅度图像
                var normalizedImage = NormalizeImage(amplitudeImage);
                
                // 2. 计算后向散射系数（Sigma0）
                var sigma0Map = await CalculateSigma0Async(normalizedImage, config);
                
                // 3. 目标检测（强散射目标）
                var targets = DetectStrongScatterers(sigma0Map, config);
                
                // 4. 散射统计
                var statistics = CalculateScatteringStatistics(sigma0Map, targets);
                
                // 5. 分类散射类型
                var classification = ClassifyScatteringTypes(sigma0Map, config);
                
                result.Success = true;
                result.NormalizedImage = normalizedImage;
                result.Sigma0Map = sigma0Map;
                result.Targets = targets;
                result.Statistics = statistics;
                result.Classification = classification;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                
                _logger.LogInformation("散射分析完成: 检测到{Count}个强散射目标, 耗时{Time}ms", 
                    targets.Count, result.ProcessingTimeMs);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "散射分析失败");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                return result;
            }
        }
        
        /// <summary>
        /// 时序散射分析
        /// </summary>
        public async Task<TimeSeriesScatteringResult> AnalyzeTimeSeriesScatteringAsync(
            List<TimestampedScatteringImage> images,
            ScatteringConfig config)
        {
            var result = new TimeSeriesScatteringResult();
            
            try
            {
                _logger.LogInformation("开始时序散射分析: 图像数量={Count}", images.Count);
                
                // 分析每期散射
                var scatteringResults = new List<ScatteringResult>();
                foreach (var img in images)
                {
                    var scattering = await AnalyzeScatteringAsync(img.Image, config);
                    scattering.Timestamp = img.Timestamp;
                    scatteringResults.Add(scattering);
                }
                
                // 计算散射变化
                var changeMap = CalculateScatteringChange(scatteringResults);
                
                // 检测持久散射体（PS）
                var persistentScatterers = DetectPersistentScatterers(scatteringResults, config);
                
                result.Success = true;
                result.ScatteringResults = scatteringResults;
                result.ChangeMap = changeMap;
                result.PersistentScatterers = persistentScatterers;
                
                _logger.LogInformation("时序散射分析完成: 发现{Count}个持久散射体", 
                    persistentScatterers.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "时序散射分析失败");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
        
        #region 私有方法
        
        /// <summary>
        /// 归一化图像
        /// </summary>
        private float[,] NormalizeImage(float[,] image)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            var normalized = new float[width, height];
            
            // 找到最大最小值
            float min = float.MaxValue;
            float max = float.MinValue;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float val = image[x, y];
                    if (val < min) min = val;
                    if (val > max) max = val;
                }
            }
            
            // 归一化到[0, 1]
            float range = max - min;
            if (range > 0)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        normalized[x, y] = (image[x, y] - min) / range;
                    }
                }
            }
            
            return normalized;
        }
        
        /// <summary>
        /// 计算后向散射系数（Sigma0）
        /// </summary>
        private async Task<float[,]> CalculateSigma0Async(float[,] normalizedImage, ScatteringConfig config)
        {
            return await Task.Run(() =>
            {
                int width = normalizedImage.GetLength(0);
                int height = normalizedImage.GetLength(1);
                var sigma0 = new float[width, height];
                
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // 转换为dB
                        float amplitude = normalizedImage[x, y];
                        if (amplitude > 0)
                        {
                            sigma0[x, y] = 10 * (float)Math.Log10(amplitude * amplitude);
                        }
                        else
                        {
                            sigma0[x, y] = -50; // 最小值
                        }
                    }
                }
                
                return sigma0;
            });
        }
        
        /// <summary>
        /// 检测强散射目标
        /// </summary>
        private List<ScatteringTarget> DetectStrongScatterers(float[,] sigma0Map, ScatteringConfig config)
        {
            var targets = new List<ScatteringTarget>();
            int width = sigma0Map.GetLength(0);
            int height = sigma0Map.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float sigma0 = sigma0Map[x, y];
                    
                    if (sigma0 >= config.StrongScattererThreshold)
                    {
                        var target = new ScatteringTarget
                        {
                            X = x,
                            Y = y,
                            Sigma0 = sigma0,
                            Type = ClassifyTargetType(sigma0, config)
                        };
                        
                        targets.Add(target);
                    }
                }
            }
            
            return targets;
        }
        
        /// <summary>
        /// 分类目标类型
        /// </summary>
        private TargetType ClassifyTargetType(float sigma0, ScatteringConfig config)
        {
            if (sigma0 >= config.VeryStrongThreshold)
            {
                return TargetType.VeryStrong;
            }
            else if (sigma0 >= config.StrongScattererThreshold)
            {
                return TargetType.Strong;
            }
            else
            {
                return TargetType.Normal;
            }
        }
        
        /// <summary>
        /// 散射统计
        /// </summary>
        private ScatteringStatistics CalculateScatteringStatistics(
            float[,] sigma0Map,
            List<ScatteringTarget> targets)
        {
            var values = sigma0Map.Cast<float>().Where(v => v > -50).ToList();
            
            return new ScatteringStatistics
            {
                MaxSigma0 = values.Any() ? values.Max() : 0,
                MinSigma0 = values.Any() ? values.Min() : 0,
                MeanSigma0 = values.Any() ? values.Average() : 0,
                StdDeviation = values.Any() ? CalculateStdDev(values) : 0,
                StrongTargetCount = targets.Count,
                VeryStrongCount = targets.Count(t => t.Type == TargetType.VeryStrong),
                StrongCount = targets.Count(t => t.Type == TargetType.Strong)
            };
        }
        
        private double CalculateStdDev(List<float> values)
        {
            double mean = values.Average();
            double sumSquaredDiff = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumSquaredDiff / values.Count);
        }
        
        /// <summary>
        /// 分类散射类型
        /// </summary>
        private ScatteringClassification ClassifyScatteringTypes(float[,] sigma0Map, ScatteringConfig config)
        {
            int width = sigma0Map.GetLength(0);
            int height = sigma0Map.GetLength(1);
            var classification = new byte[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float sigma0 = sigma0Map[x, y];
                    
                    if (sigma0 >= config.VeryStrongThreshold)
                    {
                        classification[x, y] = 3; // 金属/角反射器
                    }
                    else if (sigma0 >= config.StrongScattererThreshold)
                    {
                        classification[x, y] = 2; // 建筑物
                    }
                    else if (sigma0 >= config.MediumThreshold)
                    {
                        classification[x, y] = 1; // 植被/土壤
                    }
                    else
                    {
                        classification[x, y] = 0; // 水体/阴影
                    }
                }
            }
            
            return new ScatteringClassification
            {
                ClassificationMap = classification,
                WaterShadowCount = CountPixels(classification, 0),
                VegetationCount = CountPixels(classification, 1),
                BuildingCount = CountPixels(classification, 2),
                MetalCount = CountPixels(classification, 3)
            };
        }
        
        private int CountPixels(byte[,] map, byte value)
        {
            int count = 0;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y] == value) count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// 计算散射变化
        /// </summary>
        private float[,] CalculateScatteringChange(List<ScatteringResult> results)
        {
            if (results.Count < 2)
            {
                return new float[0, 0];
            }
            
            var first = results.First().Sigma0Map;
            var last = results.Last().Sigma0Map;
            
            int width = first.GetLength(0);
            int height = first.GetLength(1);
            var change = new float[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    change[x, y] = last[x, y] - first[x, y];
                }
            }
            
            return change;
        }
        
        /// <summary>
        /// 检测持久散射体
        /// </summary>
        private List<PersistentScatterer> DetectPersistentScatterers(
            List<ScatteringResult> results,
            ScatteringConfig config)
        {
            var ps = new List<PersistentScatterer>();
            
            if (results.Count < 3)
            {
                return ps; // 需要至少3期数据
            }
            
            int width = results[0].Sigma0Map.GetLength(0);
            int height = results[0].Sigma0Map.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 收集该点所有时期的sigma0值
                    var values = results.Select(r => r.Sigma0Map[x, y]).ToList();
                    
                    // 计算该点的稳定性（标准差）
                    double mean = values.Average();
                    double stdDev = Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / values.Count);
                    
                    // 如果sigma0强且稳定，则认为是持久散射体
                    if (mean >= config.PSMinSigma0 && stdDev <= config.PSMaxStdDev)
                    {
                        ps.Add(new PersistentScatterer
                        {
                            X = x,
                            Y = y,
                            MeanSigma0 = (float)mean,
                            StdDeviation = (float)stdDev,
                            Stability = (float)(1.0 - stdDev / Math.Abs(mean))
                        });
                    }
                }
            }
            
            return ps;
        }
        
        #endregion
    }
    
    #region 数据模型
    
    public class ScatteringConfig
    {
        public float StrongScattererThreshold { get; set; } = -5.0f; // dB
        public float VeryStrongThreshold { get; set; } = 5.0f; // dB
        public float MediumThreshold { get; set; } = -15.0f; // dB
        public float PSMinSigma0 { get; set; } = -3.0f; // dB
        public float PSMaxStdDev { get; set; } = 2.0f; // dB
    }
    
    public class ScatteringResult
    {
        public bool Success { get; set; }
        public float[,] NormalizedImage { get; set; } = new float[0, 0];
        public float[,] Sigma0Map { get; set; } = new float[0, 0];
        public List<ScatteringTarget> Targets { get; set; } = new();
        public ScatteringStatistics Statistics { get; set; } = new();
        public ScatteringClassification Classification { get; set; } = new();
        public double ProcessingTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class ScatteringTarget
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Sigma0 { get; set; }
        public TargetType Type { get; set; }
    }
    
    public enum TargetType
    {
        Normal,
        Strong,
        VeryStrong
    }
    
    public class ScatteringStatistics
    {
        public float MaxSigma0 { get; set; }
        public float MinSigma0 { get; set; }
        public double MeanSigma0 { get; set; }
        public double StdDeviation { get; set; }
        public int StrongTargetCount { get; set; }
        public int VeryStrongCount { get; set; }
        public int StrongCount { get; set; }
    }
    
    public class ScatteringClassification
    {
        public byte[,] ClassificationMap { get; set; } = new byte[0, 0];
        public int WaterShadowCount { get; set; }
        public int VegetationCount { get; set; }
        public int BuildingCount { get; set; }
        public int MetalCount { get; set; }
    }
    
    public class TimeSeriesScatteringResult
    {
        public bool Success { get; set; }
        public List<ScatteringResult> ScatteringResults { get; set; } = new();
        public float[,] ChangeMap { get; set; } = new float[0, 0];
        public List<PersistentScatterer> PersistentScatterers { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
    
    public class PersistentScatterer
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float MeanSigma0 { get; set; }
        public float StdDeviation { get; set; }
        public float Stability { get; set; }
    }
    
    public class TimestampedScatteringImage
    {
        public float[,] Image { get; set; } = new float[0, 0];
        public DateTime Timestamp { get; set; }
    }
    
    #endregion
}

