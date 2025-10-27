using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 雷达数据处理器实现
    /// </summary>
    public class RadarDataProcessor : IRadarDataProcessor
    {
        private readonly ILogger<RadarDataProcessor> _logger;
        private readonly RadarConfiguration _configuration;

        public RadarDataProcessor(ILogger<RadarDataProcessor> logger, RadarConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<RadarData> ProcessRadarDataAsync(ReceivedRadarData receivedData)
        {
            try
            {
                _logger.LogInformation("开始处理雷达数据，设备ID: {DeviceId}", receivedData.DeviceId);

                var radarData = new RadarData
                {
                    DeviceId = receivedData.DeviceId,
                    Timestamp = receivedData.ReceiveTime,
                    DataType = receivedData.DataType,
                    ImageData = receivedData.ImageData,
                    FileName = receivedData.FileName
                };

                // 处理图像数据
                await ProcessImageDataAsync(radarData);

                _logger.LogInformation("雷达数据处理完成，设备ID: {DeviceId}", receivedData.DeviceId);
                return radarData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理雷达数据时发生错误，设备ID: {DeviceId}", receivedData.DeviceId);
                throw;
            }
        }

        public async Task<byte[]> CalculateDifferenceImageAsync(byte[] currentData, byte[] previousData)
        {
            try
            {
                _logger.LogDebug("开始计算差值图像");

                if (currentData.Length != previousData.Length)
                {
                    _logger.LogWarning("当前数据和前一帧数据长度不匹配");
                    return currentData;
                }

                var differenceData = new byte[currentData.Length];
                var size = currentData.Length / 2;

                await Task.Run(() =>
                {
                    for (int i = 0; i < size; i++)
                    {
                        float lastValue = BytesToFloat(previousData, 2 * i, 2 * i + 1);
                        float currentValue = BytesToFloat(currentData, 2 * i, 2 * i + 1);
                        float difference = currentValue - lastValue;

                        var float16Bytes = Float32ToFloat16Bytes(difference);
                        differenceData[2 * i] = float16Bytes[0];
                        differenceData[2 * i + 1] = float16Bytes[1];
                    }
                });

                _logger.LogDebug("差值图像计算完成");
                return differenceData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算差值图像时发生错误");
                throw;
            }
        }

        public async Task<byte[]> ProcessRadarImageDataAsync(byte[] imageData, int rangeNumber, int angleNumber, SarFileData sarFileData)
        {
            try
            {
                _logger.LogDebug("开始处理雷达图像数据，距离数量: {RangeNumber}, 角度数量: {AngleNumber}", rangeNumber, angleNumber);

                var radarImageData = new float[rangeNumber][];
                var rangeDistances = new double[rangeNumber];
                var angleDistances = new double[angleNumber];
                var floatArray = new float[rangeNumber * angleNumber];

                await Task.Run(() =>
                {
                    int index = 0;
                    for (int i = 0; i < rangeNumber; i++)
                    {
                        radarImageData[i] = new float[angleNumber];
                        for (int j = 0; j < angleNumber; j++)
                        {
                            if (i == 0)
                            {
                                angleDistances[j] = j * sarFileData.AngleResolution + sarFileData.AngleMin;
                            }
                            if (j == 0)
                            {
                                rangeDistances[i] = i * sarFileData.RangeResolution + sarFileData.RangeMin;
                            }

                            float value = BytesToFloat(imageData, 2 * index, 2 * index + 1);
                            floatArray[index] = value;
                            radarImageData[i][j] = value;
                            index++;
                        }
                    }

                    // 处理角度距离
                    ProcessAngleDistances(angleDistances);
                });

                var result = FloatArrayToByteArray(floatArray);
                _logger.LogDebug("雷达图像数据处理完成");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理雷达图像数据时发生错误");
                throw;
            }
        }

        public async Task<bool> ValidateRadarDataAsync(string dataType, byte[] imageData, string deviceId)
        {
            try
            {
                _logger.LogDebug("开始验证雷达数据，设备ID: {DeviceId}, 数据类型: {DataType}", deviceId, dataType);

                // 检查设备ID是否在配置列表中
                if (!Array.Exists(_configuration.DeviceIds, id => id == deviceId))
                {
                    _logger.LogWarning("设备ID {DeviceId} 不在配置列表中", deviceId);
                    return false;
                }

                // 检查数据类型
                if (dataType != "00")
                {
                    _logger.LogWarning("只接受雷达的原始数据，当前类型: {DataType}", dataType);
                    return false;
                }

                // 检查图像数据
                if (imageData == null || imageData.Length == 0)
                {
                    _logger.LogWarning("图像数据为空");
                    return false;
                }

                _logger.LogDebug("雷达数据验证通过");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证雷达数据时发生错误");
                return false;
            }
        }

        private async Task ProcessImageDataAsync(RadarData radarData)
        {
            // 处理图像数据的逻辑
            await Task.CompletedTask;
        }

        private float BytesToFloat(byte[] bytes, int startIndex, int endIndex)
        {
            // 实现字节到浮点数的转换
            // 这里需要根据实际的字节序和格式来实现
            return BitConverter.ToSingle(bytes, startIndex);
        }

        private byte[] Float32ToFloat16Bytes(float value)
        {
            // 实现32位浮点数到16位浮点数字节的转换
            // 这里需要根据实际需求来实现
            return BitConverter.GetBytes(value);
        }

        private byte[] FloatArrayToByteArray(float[] floatArray)
        {
            var result = new byte[floatArray.Length * 4];
            for (int i = 0; i < floatArray.Length; i++)
            {
                var bytes = BitConverter.GetBytes(floatArray[i]);
                Array.Copy(bytes, 0, result, i * 4, 4);
            }
            return result;
        }

        private void ProcessAngleDistances(double[] angleDistances)
        {
            if (angleDistances.Length == 0) return;

            double angleStart = angleDistances[0] * 180.0 / Math.PI;
            double angleEnd = angleDistances[angleDistances.Length - 1] * 180.0 / Math.PI;
            double radMiddle = ((angleEnd - angleStart) / 2.0 + angleStart) * Math.PI / 180.0;

            for (int i = 0; i < angleDistances.Length; i++)
            {
                angleDistances[i] -= radMiddle;
            }
        }
    }
}
