using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;

namespace RadarSystem.ImageAnalysis.Services
{
    /// <summary>
    /// 图像分析服务
    /// </summary>
    public class ImageAnalysisService : IImageAnalysisService
    {
        private readonly ILogger<ImageAnalysisService> _logger;

        public ImageAnalysisService(ILogger<ImageAnalysisService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 分析雷达图像
        /// </summary>
        public async Task<ImageAnalysisResult> AnalyzeRadarImageAsync(byte[] imageData, RadarData radarData)
        {
            var startTime = DateTime.Now;
            try
            {
                _logger.LogInformation("开始分析雷达图像，设备ID: {DeviceId}", radarData.DeviceId);

                var result = new ImageAnalysisResult
                {
                    Success = true,
                    AnalysisTime = DateTime.Now
                };

                // 执行图像分析
                await Task.Run(() =>
                {
                    // 简单的图像分析逻辑
                    result.TargetCount = 0;
                    result.Metadata["DeviceId"] = radarData.DeviceId;
                    result.Metadata["Timestamp"] = radarData.Timestamp;
                });

                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                _logger.LogInformation("雷达图像分析完成，设备ID: {DeviceId}", radarData.DeviceId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分析雷达图像时发生错误，设备ID: {DeviceId}", radarData.DeviceId);
                return new ImageAnalysisResult
                {
                    Success = false,
                    Message = ex.Message,
                    AnalysisTime = DateTime.Now,
                    ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds
                };
            }
        }

        /// <summary>
        /// 计算差值图像
        /// </summary>
        public async Task<byte[]> CalculateDifferenceImageAsync(byte[] currentImage, byte[] previousImage)
        {
            try
            {
                _logger.LogDebug("开始计算差值图像");

                using var current = ByteArrayToImage(currentImage);
                using var previous = ByteArrayToImage(previousImage);

                if (current == null || previous == null)
                {
                    _logger.LogWarning("无法处理图像数据");
                    return currentImage;
                }

                // 确保图像尺寸相同
                if (current.Width != previous.Width || current.Height != previous.Height)
                {
                    _logger.LogWarning("图像尺寸不匹配，无法计算差值");
                    return currentImage;
                }

                var differenceImage = new Bitmap(current.Width, current.Height);

                await Task.Run(() =>
                {
                    for (int x = 0; x < current.Width; x++)
                    {
                        for (int y = 0; y < current.Height; y++)
                        {
                            var currentPixel = current.GetPixel(x, y);
                            var previousPixel = previous.GetPixel(x, y);

                            // 计算差值
                            var diffR = Math.Abs(currentPixel.R - previousPixel.R);
                            var diffG = Math.Abs(currentPixel.G - previousPixel.G);
                            var diffB = Math.Abs(currentPixel.B - previousPixel.B);

                            var differencePixel = Color.FromArgb(diffR, diffG, diffB);
                            differenceImage.SetPixel(x, y, differencePixel);
                        }
                    }
                });

                var result = ImageToByteArray(differenceImage);
                _logger.LogDebug("差值图像计算完成");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算差值图像时发生错误");
                return currentImage;
            }
        }

        /// <summary>
        /// 检测目标
        /// </summary>
        public async Task<TargetDetectionResult> DetectTargetsAsync(byte[] imageData, DetectionParameters parameters)
        {
            var startTime = DateTime.Now;
            try
            {
                _logger.LogInformation("开始目标检测");

                using var image = ByteArrayToImage(imageData);
                if (image == null)
                {
                    _logger.LogWarning("无法处理图像数据");
                    return new TargetDetectionResult 
                    { 
                        Success = false, 
                        Message = "图像数据无效",
                        DetectionTime = DateTime.Now,
                        ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds
                    };
                }

                var result = new TargetDetectionResult
                {
                    Success = true,
                    DetectionTime = DateTime.Now,
                    Targets = new List<DetectedTarget>()
                };

                // 执行目标检测算法
                await DetectTargetsInImageAsync(image, result, parameters);

                result.TotalTargets = result.Targets.Count;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                
                _logger.LogInformation("目标检测完成，检测到 {Count} 个目标", result.TotalTargets);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "目标检测时发生错误");
                return new TargetDetectionResult
                {
                    Success = false,
                    Message = ex.Message,
                    DetectionTime = DateTime.Now,
                    ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds
                };
            }
        }

        private async Task DetectTargetsInImageAsync(Bitmap image, TargetDetectionResult result, DetectionParameters parameters)
        {
            await Task.Run(() =>
            {
                var targets = new List<DetectedTarget>();
                var threshold = parameters.ThresholdAmplitude;
                var statistics = new DetectionStatistics
                {
                    TotalPixels = image.Width * image.Height
                };

                // 简单的目标检测算法
                int targetId = 1;
                for (int x = 0; x < image.Width; x += 10)
                {
                    for (int y = 0; y < image.Height; y += 10)
                    {
                        var pixel = image.GetPixel(x, y);
                        var brightness = (pixel.R + pixel.G + pixel.B) / (3.0f * 255.0f);

                        if (brightness > threshold)
                        {
                            targets.Add(new DetectedTarget
                            {
                                Id = targetId++,
                                RangePosition = x,
                                AnglePosition = y,
                                Amplitude = brightness,
                                Confidence = Math.Min(brightness / threshold, 1.0f),
                                TargetType = "Unknown"
                            });
                        }

                        statistics.ProcessedPixels++;
                        statistics.AverageAmplitude += brightness;
                        statistics.MaxAmplitude = Math.Max(statistics.MaxAmplitude, brightness);
                        statistics.MinAmplitude = Math.Min(statistics.MinAmplitude, brightness);
                    }
                }

                if (statistics.ProcessedPixels > 0)
                {
                    statistics.AverageAmplitude /= statistics.ProcessedPixels;
                }

                result.Targets = targets;
                result.Statistics = statistics;
            });
        }

        private Bitmap? ByteArrayToImage(byte[] byteArray)
        {
            try
            {
                using var ms = new MemoryStream(byteArray);
                return new Bitmap(ms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "将字节数组转换为图像时发生错误");
                return null;
            }
        }

        private byte[] ImageToByteArray(Bitmap image)
        {
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
