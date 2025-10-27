using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.ImageAnalysis.Models;
using ColorMapUtility = RadarSystem.ImageAnalysis.Utilities.ColorMap;

namespace RadarSystem.ImageAnalysis.Services
{
    /// <summary>
    /// 图像切片生成器
    /// </summary>
    public class ImageTileGenerator
    {
        private readonly ILogger<ImageTileGenerator> _logger;
        
        public ImageTileGenerator(ILogger<ImageTileGenerator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 生成形变图像切片
        /// </summary>
        public async Task<TileGenerationResult> GenerateDeformationTilesAsync(
            float[,] deformationData,
            string outputPath,
            ColorMapConfig? colorMapConfig = null,
            int rngTileCount = 1203,
            int angTileCount = 61)
        {
            var startTime = DateTime.Now;
            var result = new TileGenerationResult { OutputPath = outputPath };
            
            try
            {
                _logger.LogInformation("开始生成形变图像切片: 输出路径={OutputPath}, 切片数={RngCount}x{AngCount}", 
                    outputPath, rngTileCount, angTileCount);
                
                // 验证数据
                ValidateImageData(deformationData);
                
                // 计算切片配置
                var tileConfig = CalculateTileConfiguration(
                    deformationData.GetLength(0),
                    deformationData.GetLength(1),
                    rngTileCount,
                    angTileCount
                );
                
                // 创建颜色映射
                var colorMap = colorMapConfig != null
                    ? new ColorMapUtility(colorMapConfig)
                    : ColorMapUtility.CreateDeformationColorMap();
                
                // 确保输出目录存在
                Directory.CreateDirectory(outputPath);
                
                // 生成切片
                int tileCount = 0;
                for (int rngIdx = 0; rngIdx < tileConfig.RngTileCount; rngIdx++)
                {
                    for (int angIdx = 0; angIdx < tileConfig.AngTileCount; angIdx++)
                    {
                        // 提取切片数据
                        var tileData = ExtractTileData(
                            deformationData,
                            rngIdx, angIdx,
                            tileConfig
                        );
                        
                        // 渲染切片图像
                        using var tileImage = RenderFloatTile(tileData, colorMap);
                        
                        // 保存切片
                        var tilePath = Path.Combine(outputPath, $"tile_{rngIdx}_{angIdx}.png");
                        await SaveTileImageAsync(tileImage, tilePath);
                        
                        tileCount++;
                        
                        // 每1000个切片记录一次进度
                        if (tileCount % 1000 == 0)
                        {
                            _logger.LogDebug("已生成 {Count}/{Total} 个切片", tileCount, tileConfig.TotalTileCount);
                        }
                    }
                }
                
                // 保存元数据
                await SaveTileMetadataAsync(outputPath, tileConfig, "deformation", colorMapConfig);
                
                result.Success = true;
                result.TileCount = tileCount;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                
                _logger.LogInformation("形变图像切片生成完成: 切片数={Count}, 耗时={Time}ms", 
                    tileCount, result.ProcessingTimeMs);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成形变图像切片失败");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                return result;
            }
        }
        
        /// <summary>
        /// 生成散射图像切片
        /// </summary>
        public async Task<TileGenerationResult> GenerateScatteringTilesAsync(
            byte[] scatteringData,
            int width,
            int height,
            string outputPath,
            int rngTileCount = 1203,
            int angTileCount = 61)
        {
            var startTime = DateTime.Now;
            var result = new TileGenerationResult { OutputPath = outputPath };
            
            try
            {
                _logger.LogInformation("开始生成散射图像切片: 尺寸={Width}x{Height}, 切片数={RngCount}x{AngCount}", 
                    width, height, rngTileCount, angTileCount);
                
                // 转换为2D数组
                var data2D = ConvertByteArrayToMatrix(scatteringData, width, height);
                
                // 计算切片配置
                var tileConfig = CalculateTileConfiguration(width, height, rngTileCount, angTileCount);
                
                // 创建颜色映射（散射使用灰度）
                var colorMap = ColorMapUtility.CreateScatteringColorMap();
                
                // 确保输出目录存在
                Directory.CreateDirectory(outputPath);
                
                // 生成切片
                int tileCount = 0;
                for (int rngIdx = 0; rngIdx < tileConfig.RngTileCount; rngIdx++)
                {
                    for (int angIdx = 0; angIdx < tileConfig.AngTileCount; angIdx++)
                    {
                        var tileData = ExtractTileData(data2D, rngIdx, angIdx, tileConfig);
                        using var tileImage = RenderFloatTile(tileData, colorMap);
                        
                        var tilePath = Path.Combine(outputPath, $"tile_{rngIdx}_{angIdx}.png");
                        await SaveTileImageAsync(tileImage, tilePath);
                        
                        tileCount++;
                        
                        if (tileCount % 1000 == 0)
                        {
                            _logger.LogDebug("已生成 {Count}/{Total} 个散射切片", tileCount, tileConfig.TotalTileCount);
                        }
                    }
                }
                
                // 保存元数据
                await SaveTileMetadataAsync(outputPath, tileConfig, "scattering", null);
                
                result.Success = true;
                result.TileCount = tileCount;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                
                _logger.LogInformation("散射图像切片生成完成: 切片数={Count}, 耗时={Time}ms", 
                    tileCount, result.ProcessingTimeMs);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成散射图像切片失败");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                return result;
            }
        }
        
        /// <summary>
        /// 生成速度图像切片
        /// </summary>
        public async Task<TileGenerationResult> GenerateVelocityTilesAsync(
            float[,] velocityData,
            string outputPath,
            ColorMapConfig? colorMapConfig = null,
            int rngTileCount = 1203,
            int angTileCount = 61)
        {
            var startTime = DateTime.Now;
            var result = new TileGenerationResult { OutputPath = outputPath };
            
            try
            {
                _logger.LogInformation("开始生成速度图像切片: 输出路径={OutputPath}", outputPath);
                
                ValidateImageData(velocityData);
                
                var tileConfig = CalculateTileConfiguration(
                    velocityData.GetLength(0),
                    velocityData.GetLength(1),
                    rngTileCount,
                    angTileCount
                );
                
                var colorMap = colorMapConfig != null
                    ? new ColorMapUtility(colorMapConfig)
                    : ColorMapUtility.CreateVelocityColorMap();
                
                Directory.CreateDirectory(outputPath);
                
                int tileCount = 0;
                for (int rngIdx = 0; rngIdx < tileConfig.RngTileCount; rngIdx++)
                {
                    for (int angIdx = 0; angIdx < tileConfig.AngTileCount; angIdx++)
                    {
                        var tileData = ExtractTileData(velocityData, rngIdx, angIdx, tileConfig);
                        using var tileImage = RenderFloatTile(tileData, colorMap);
                        
                        var tilePath = Path.Combine(outputPath, $"tile_{rngIdx}_{angIdx}.png");
                        await SaveTileImageAsync(tileImage, tilePath);
                        
                        tileCount++;
                    }
                }
                
                await SaveTileMetadataAsync(outputPath, tileConfig, "velocity", colorMapConfig);
                
                result.Success = true;
                result.TileCount = tileCount;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                
                _logger.LogInformation("速度图像切片生成完成: 切片数={Count}, 耗时={Time}ms", 
                    tileCount, result.ProcessingTimeMs);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成速度图像切片失败");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                return result;
            }
        }
        
        #region 私有辅助方法
        
        /// <summary>
        /// 计算切片配置
        /// </summary>
        private TileConfiguration CalculateTileConfiguration(
            int imageWidth,
            int imageHeight,
            int rngTileCount,
            int angTileCount)
        {
            return new TileConfiguration
            {
                ImageWidth = imageWidth,
                ImageHeight = imageHeight,
                RngTileCount = rngTileCount,
                AngTileCount = angTileCount,
                TileWidth = (int)Math.Ceiling((double)imageWidth / rngTileCount),
                TileHeight = (int)Math.Ceiling((double)imageHeight / angTileCount)
            };
        }
        
        /// <summary>
        /// 提取切片数据
        /// </summary>
        private float[,] ExtractTileData(
            float[,] fullImage,
            int rngIndex,
            int angIndex,
            TileConfiguration config)
        {
            int startX = rngIndex * config.TileWidth;
            int startY = angIndex * config.TileHeight;
            int endX = Math.Min(startX + config.TileWidth, config.ImageWidth);
            int endY = Math.Min(startY + config.TileHeight, config.ImageHeight);
            
            int tileW = endX - startX;
            int tileH = endY - startY;
            
            var tileData = new float[tileW, tileH];
            
            for (int x = 0; x < tileW; x++)
            {
                for (int y = 0; y < tileH; y++)
                {
                    tileData[x, y] = fullImage[startX + x, startY + y];
                }
            }
            
            return tileData;
        }
        
        /// <summary>
        /// 渲染浮点数切片为图像
        /// </summary>
        private Bitmap RenderFloatTile(float[,] tileData, ColorMapUtility colorMap)
        {
            int width = tileData.GetLength(0);
            int height = tileData.GetLength(1);
            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var value = tileData[x, y];
                    var color = colorMap.GetColor(value);
                    bitmap.SetPixel(x, y, color);
                }
            }
            
            return bitmap;
        }
        
        /// <summary>
        /// 保存切片图像
        /// </summary>
        private async Task SaveTileImageAsync(Bitmap image, string filePath)
        {
            await Task.Run(() =>
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                image.Save(filePath, ImageFormat.Png);
            });
        }
        
        /// <summary>
        /// 转换字节数组为矩阵
        /// </summary>
        private float[,] ConvertByteArrayToMatrix(byte[] data, int width, int height)
        {
            var matrix = new float[width, height];
            int index = 0;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (index < data.Length)
                    {
                        matrix[x, y] = data[index++] / 255.0f;
                    }
                }
            }
            
            return matrix;
        }
        
        /// <summary>
        /// 验证图像数据
        /// </summary>
        private void ValidateImageData(float[,] data)
        {
            if (data == null || data.GetLength(0) == 0 || data.GetLength(1) == 0)
            {
                throw new ArgumentException("图像数据无效或为空");
            }
        }
        
        /// <summary>
        /// 保存切片元数据
        /// </summary>
        private async Task SaveTileMetadataAsync(
            string outputPath,
            TileConfiguration tileConfig,
            string dataType,
            ColorMapConfig? colorMapConfig)
        {
            try
            {
                var metadata = new TileMetadata
                {
                    Configuration = tileConfig,
                    DataType = dataType,
                    Timestamp = DateTime.Now,
                    ColorMap = colorMapConfig
                };
                
                var metadataPath = Path.Combine(outputPath, "tile_metadata.json");
                var json = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                await File.WriteAllTextAsync(metadataPath, json);
                
                _logger.LogDebug("切片元数据已保存: {Path}", metadataPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "保存切片元数据失败，但不影响切片生成");
            }
        }
        
        #endregion
    }
}

