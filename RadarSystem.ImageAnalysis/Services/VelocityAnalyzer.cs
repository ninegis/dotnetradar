using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RadarSystem.ImageAnalysis.Services
{
    /// <summary>
    /// 速度场分析器 - 计算形变速度场
    /// </summary>
    public class VelocityAnalyzer
    {
        private readonly ILogger<VelocityAnalyzer> _logger;
        
        public VelocityAnalyzer(ILogger<VelocityAnalyzer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 计算速度场
        /// </summary>
        public async Task<VelocityResult> CalculateVelocityAsync(
            List<TimestampedImage> images,
            VelocityConfig config)
        {
            var startTime = DateTime.Now;
            var result = new VelocityResult();
            
            try
            {
                _logger.LogInformation("开始速度场计算: 图像数量={Count}", images.Count);
                
                if (images.Count < 2)
                {
                    throw new ArgumentException("速度场计算至少需要2期图像");
                }
                
                // 按时间排序
                var sortedImages = images.OrderBy(img => img.Timestamp).ToList();
                
                // 计算逐期速度
                var incrementalVelocities = await CalculateIncrementalVelocitiesAsync(sortedImages);
                
                // 计算平均速度场
                var averageVelocity = CalculateAverageVelocity(incrementalVelocities);
                
                // 检测加速区域
                var accelerationZones = DetectAccelerationZones(incrementalVelocities, config);
                
                // 计算速度统计
                var statistics = CalculateVelocityStatistics(averageVelocity, accelerationZones);
                
                // 生成报警
                var alarms = GenerateVelocityAlarms(accelerationZones, statistics, config);
                
                result.Success = true;
                result.IncrementalVelocities = incrementalVelocities;
                result.AverageVelocityMap = averageVelocity;
                result.AccelerationZones = accelerationZones;
                result.Statistics = statistics;
                result.Alarms = alarms;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                
                _logger.LogInformation("速度场计算完成: 发现{Count}个加速区域, 耗时{Time}ms", 
                    accelerationZones.Count, result.ProcessingTimeMs);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "速度场计算失败");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                return result;
            }
        }
        
        /// <summary>
        /// 计算方向速度场（2D矢量场）
        /// </summary>
        public async Task<DirectionalVelocityResult> CalculateDirectionalVelocityAsync(
            List<TimestampedImage> rangeImages,
            List<TimestampedImage> azimuthImages,
            VelocityConfig config)
        {
            var result = new DirectionalVelocityResult();
            
            try
            {
                _logger.LogInformation("开始方向速度场计算");
                
                // 分别计算距离向和方位向速度
                var rangeVelocity = await CalculateVelocityAsync(rangeImages, config);
                var azimuthVelocity = await CalculateVelocityAsync(azimuthImages, config);
                
                // 合成速度矢量
                var velocityVectors = CalculateVelocityVectors(
                    rangeVelocity.AverageVelocityMap,
                    azimuthVelocity.AverageVelocityMap);
                
                result.Success = true;
                result.RangeVelocity = rangeVelocity;
                result.AzimuthVelocity = azimuthVelocity;
                result.VelocityVectors = velocityVectors;
                
                _logger.LogInformation("方向速度场计算完成");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "方向速度场计算失败");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
        
        #region 私有方法
        
        /// <summary>
        /// 计算逐期速度
        /// </summary>
        private async Task<List<VelocityField>> CalculateIncrementalVelocitiesAsync(
            List<TimestampedImage> sortedImages)
        {
            return await Task.Run(() =>
            {
                var velocities = new List<VelocityField>();
                
                for (int i = 1; i < sortedImages.Count; i++)
                {
                    var img1 = sortedImages[i - 1];
                    var img2 = sortedImages[i];
                    
                    // 计算时间间隔（天）
                    double timeDelta = (img2.Timestamp - img1.Timestamp).TotalDays;
                    
                    if (timeDelta > 0)
                    {
                        // 计算形变
                        var deformation = CalculateDifference(img2.Image, img1.Image);
                        
                        // 计算速度 (mm/day)
                        var velocity = DivideByScalar(deformation, (float)timeDelta);
                        
                        velocities.Add(new VelocityField
                        {
                            VelocityMap = velocity,
                            StartTime = img1.Timestamp,
                            EndTime = img2.Timestamp,
                            TimeDeltaDays = timeDelta
                        });
                    }
                }
                
                return velocities;
            });
        }
        
        private float[,] CalculateDifference(float[,] img1, float[,] img2)
        {
            int width = img1.GetLength(0);
            int height = img1.GetLength(1);
            var diff = new float[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    diff[x, y] = img1[x, y] - img2[x, y];
                }
            }
            
            return diff;
        }
        
        private float[,] DivideByScalar(float[,] matrix, float scalar)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            var result = new float[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[x, y] = matrix[x, y] / scalar;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 计算平均速度场
        /// </summary>
        private float[,] CalculateAverageVelocity(List<VelocityField> velocities)
        {
            if (!velocities.Any())
            {
                return new float[0, 0];
            }
            
            var first = velocities[0].VelocityMap;
            int width = first.GetLength(0);
            int height = first.GetLength(1);
            var average = new float[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float sum = 0;
                    foreach (var vf in velocities)
                    {
                        sum += vf.VelocityMap[x, y];
                    }
                    average[x, y] = sum / velocities.Count;
                }
            }
            
            return average;
        }
        
        /// <summary>
        /// 检测加速区域
        /// </summary>
        private List<AccelerationZone> DetectAccelerationZones(
            List<VelocityField> velocities,
            VelocityConfig config)
        {
            var zones = new List<AccelerationZone>();
            
            if (velocities.Count < 2)
            {
                return zones; // 需要至少2期速度
            }
            
            int width = velocities[0].VelocityMap.GetLength(0);
            int height = velocities[0].VelocityMap.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 计算速度变化（加速度）
                    var velocityValues = velocities.Select(v => v.VelocityMap[x, y]).ToList();
                    
                    // 简单的线性拟合计算加速度
                    double acceleration = CalculateAcceleration(velocityValues);
                    
                    if (Math.Abs(acceleration) >= config.AccelerationThreshold)
                    {
                        zones.Add(new AccelerationZone
                        {
                            X = x,
                            Y = y,
                            Acceleration = (float)acceleration,
                            InitialVelocity = velocityValues.First(),
                            FinalVelocity = velocityValues.Last(),
                            Level = ClassifyAccelerationLevel(Math.Abs((float)acceleration), config)
                        });
                    }
                }
            }
            
            return zones;
        }
        
        /// <summary>
        /// 计算加速度（简单线性拟合）
        /// </summary>
        private double CalculateAcceleration(List<float> velocityValues)
        {
            if (velocityValues.Count < 2)
            {
                return 0;
            }
            
            // 简单的加速度 = (末速度 - 初速度) / 期数
            return (velocityValues.Last() - velocityValues.First()) / (velocityValues.Count - 1);
        }
        
        /// <summary>
        /// 分类加速度等级
        /// </summary>
        private AccelerationLevel ClassifyAccelerationLevel(float absAcceleration, VelocityConfig config)
        {
            if (absAcceleration >= config.CriticalAcceleration)
            {
                return AccelerationLevel.Critical;
            }
            else if (absAcceleration >= config.WarningAcceleration)
            {
                return AccelerationLevel.Warning;
            }
            else
            {
                return AccelerationLevel.Normal;
            }
        }
        
        /// <summary>
        /// 统计速度
        /// </summary>
        private VelocityStatistics CalculateVelocityStatistics(
            float[,] averageVelocity,
            List<AccelerationZone> accelerationZones)
        {
            var values = averageVelocity.Cast<float>().ToList();
            
            return new VelocityStatistics
            {
                MaxVelocity = values.Max(),
                MinVelocity = values.Min(),
                MeanVelocity = values.Average(),
                StdDeviation = CalculateStdDev(values),
                AccelerationZoneCount = accelerationZones.Count,
                NormalCount = accelerationZones.Count(z => z.Level == AccelerationLevel.Normal),
                WarningCount = accelerationZones.Count(z => z.Level == AccelerationLevel.Warning),
                CriticalCount = accelerationZones.Count(z => z.Level == AccelerationLevel.Critical)
            };
        }
        
        private double CalculateStdDev(List<float> values)
        {
            double mean = values.Average();
            double sumSquaredDiff = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumSquaredDiff / values.Count);
        }
        
        /// <summary>
        /// 生成速度报警
        /// </summary>
        private List<VelocityAlarm> GenerateVelocityAlarms(
            List<AccelerationZone> zones,
            VelocityStatistics statistics,
            VelocityConfig config)
        {
            var alarms = new List<VelocityAlarm>();
            
            var criticalZones = zones.Where(z => z.Level == AccelerationLevel.Critical).ToList();
            var warningZones = zones.Where(z => z.Level == AccelerationLevel.Warning).ToList();
            
            if (criticalZones.Any())
            {
                alarms.Add(new VelocityAlarm
                {
                    Level = AlarmLevel.Critical,
                    Message = $"检测到{criticalZones.Count}个严重加速区域",
                    Zones = criticalZones,
                    Timestamp = DateTime.Now
                });
            }
            
            if (warningZones.Any())
            {
                alarms.Add(new VelocityAlarm
                {
                    Level = AlarmLevel.Warning,
                    Message = $"检测到{warningZones.Count}个警告级加速区域",
                    Zones = warningZones,
                    Timestamp = DateTime.Now
                });
            }
            
            return alarms;
        }
        
        /// <summary>
        /// 计算速度矢量
        /// </summary>
        private VelocityVector[,] CalculateVelocityVectors(
            float[,] rangeVelocity,
            float[,] azimuthVelocity)
        {
            int width = rangeVelocity.GetLength(0);
            int height = rangeVelocity.GetLength(1);
            var vectors = new VelocityVector[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float vr = rangeVelocity[x, y];
                    float va = azimuthVelocity[x, y];
                    
                    vectors[x, y] = new VelocityVector
                    {
                        RangeComponent = vr,
                        AzimuthComponent = va,
                        Magnitude = (float)Math.Sqrt(vr * vr + va * va),
                        Direction = (float)(Math.Atan2(va, vr) * 180 / Math.PI)
                    };
                }
            }
            
            return vectors;
        }
        
        #endregion
    }
    
    #region 数据模型
    
    public class VelocityConfig
    {
        public float AccelerationThreshold { get; set; } = 0.5f; // mm/day²
        public float WarningAcceleration { get; set; } = 1.0f; // mm/day²
        public float CriticalAcceleration { get; set; } = 2.0f; // mm/day²
        public float MinVelocity { get; set; } = 0.1f; // mm/day
    }
    
    public class VelocityResult
    {
        public bool Success { get; set; }
        public List<VelocityField> IncrementalVelocities { get; set; } = new();
        public float[,] AverageVelocityMap { get; set; } = new float[0, 0];
        public List<AccelerationZone> AccelerationZones { get; set; } = new();
        public VelocityStatistics Statistics { get; set; } = new();
        public List<VelocityAlarm> Alarms { get; set; } = new();
        public double ProcessingTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }
    
    public class VelocityField
    {
        public float[,] VelocityMap { get; set; } = new float[0, 0];
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double TimeDeltaDays { get; set; }
    }
    
    public class AccelerationZone
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Acceleration { get; set; }
        public float InitialVelocity { get; set; }
        public float FinalVelocity { get; set; }
        public AccelerationLevel Level { get; set; }
    }
    
    public enum AccelerationLevel
    {
        Normal,
        Warning,
        Critical
    }
    
    public class VelocityStatistics
    {
        public float MaxVelocity { get; set; }
        public float MinVelocity { get; set; }
        public double MeanVelocity { get; set; }
        public double StdDeviation { get; set; }
        public int AccelerationZoneCount { get; set; }
        public int NormalCount { get; set; }
        public int WarningCount { get; set; }
        public int CriticalCount { get; set; }
    }
    
    public class VelocityAlarm
    {
        public AlarmLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AccelerationZone> Zones { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
    
    // TimestampedImage 已在 DeformationAnalyzer.cs 中定义，共享使用
    
    public enum AlarmLevel
    {
        Normal,
        Warning,
        Critical
    }
    
    public class DirectionalVelocityResult
    {
        public bool Success { get; set; }
        public VelocityResult? RangeVelocity { get; set; }
        public VelocityResult? AzimuthVelocity { get; set; }
        public VelocityVector[,] VelocityVectors { get; set; } = new VelocityVector[0, 0];
        public string? ErrorMessage { get; set; }
    }
    
    public class VelocityVector
    {
        public float RangeComponent { get; set; }
        public float AzimuthComponent { get; set; }
        public float Magnitude { get; set; }
        public float Direction { get; set; } // degrees
    }
    
    #endregion
}

