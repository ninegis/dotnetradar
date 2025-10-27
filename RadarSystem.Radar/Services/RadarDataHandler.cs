using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RadarSystem.Core.Models;
using RadarSystem.Core.Services;
using RadarSystem.Core.Utilities;
using RadarSystem.Core.Interfaces;

namespace RadarSystem.Radar.Services
{
    /// <summary>
    /// 雷达数据处理器 - 完整对应Java HandleRadarDataThread
    /// </summary>
    public class RadarDataHandler : BackgroundService
    {
        private readonly ILogger<RadarDataHandler> _logger;
        private readonly ISarFileDataProcessor _sarFileProcessor;
        private readonly ISarFileStorage _sarFileStorage;
        private readonly RadarConfiguration _configuration;
        private readonly Channel<ReceivedRadarData> _dataQueue;
        
        private SarFileData? _lastSarFileData;
        private long _lastProcessTime;
        private float[][]? _radarImageData;
        private double[]? _rangeDistances;
        private double[]? _angleDistances;

        public RadarDataHandler(
            ILogger<RadarDataHandler> logger,
            ISarFileDataProcessor sarFileProcessor,
            ISarFileStorage sarFileStorage,
            RadarConfiguration configuration,
            Channel<ReceivedRadarData> dataQueue)
        {
            _logger = logger;
            _sarFileProcessor = sarFileProcessor;
            _sarFileStorage = sarFileStorage;
            _configuration = configuration;
            _dataQueue = dataQueue;
            _lastProcessTime = 0;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("雷达数据处理线程启动...");

            try
            {
                await foreach (var receivedData in _dataQueue.Reader.ReadAllAsync(stoppingToken))
                {
                    await ProcessRadarDataAsync(receivedData, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("雷达数据处理线程正在停止...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "雷达数据处理线程发生未处理的异常");
            }
        }

        /// <summary>
        /// 处理雷达数据 - 对应Java的run()方法
        /// </summary>
        private async Task ProcessRadarDataAsync(ReceivedRadarData receivedData, CancellationToken cancellationToken)
        {
            byte[]? imageData = null;
            byte[]? diffImageData = null;
            SarFileData? currentSarData = null;

            try
            {
                // 数据过滤验证 - 对应Java的filter()方法
                if (!FilterRadarData(receivedData.DataType, receivedData.FileName, receivedData.DeviceId))
                {
                    return;
                }

                // 读取SAR文件头和数据
                var header = await _sarFileProcessor.ReadHeaderAsync(
                    receivedData.FileName, 
                    receivedData.DataType, 
                    12);

                var rawImageData = await _sarFileProcessor.ReadDataAsync(
                    receivedData.FileName, 
                    receivedData.DataType, 
                    12);

                currentSarData = new SarFileData
                {
                    Sequence = header.Sequence,
                    TaskId = header.TaskId,
                    TimeMillis = header.TimeMillis,
                    RangeResolution = header.RangeResolution,
                    RangeNumber = header.RangeNumber,
                    RangeMin = header.RangeMin,
                    AngleResolution = header.AngleResolution,
                    AngleNumber = header.AngleNumber,
                    AngleMin = header.AngleMin,
                    ImageData = rawImageData,
                    DataType = receivedData.DataType,
                    DeviceId = receivedData.DeviceId
                };

                // 判断是否需要计算差值 - 对应Java的逻辑判断
                if (_lastSarFileData == null || 
                    _lastSarFileData.Sequence > currentSarData.Sequence || 
                    _lastSarFileData.TaskId != currentSarData.TaskId)
                {
                    // 第一次获取数据，不进行差值计算
                    _lastSarFileData = CloneSarFileData(currentSarData);
                    _logger.LogInformation(
                        "第一次获取数据不进行差值计算 上一次TaskID:{LastTaskId}，当前TaskId:{CurrentTaskId}，上一次序号:{LastSeq}，当前序号:{CurrentSeq}",
                        _lastSarFileData.TaskId, currentSarData.TaskId, 
                        _lastSarFileData.Sequence, currentSarData.Sequence);
                }
                else
                {
                    // 计算差值图像 - 对应Java的diffImgData()方法
                    diffImageData = CalculateDifferenceImage(
                        currentSarData.ImageData, 
                        _lastSarFileData.ImageData);

                    // 处理雷达数据 - 对应Java的handleRadarData()方法
                    imageData = HandleRadarImageData(
                        diffImageData, 
                        currentSarData.RangeNumber, 
                        currentSarData.AngleNumber, 
                        currentSarData);

                    // 保存差值图像 - 对应Java的flush()方法
                    await FlushDifferenceImageAsync(currentSarData, receivedData.DeviceId, imageData);

                    // 更新上一帧数据
                    _lastSarFileData = CloneSarFileData(currentSarData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "转换雷达数据异常");
            }
            finally
            {
                // 清理资源
                currentSarData = null;
                _radarImageData = null;
                imageData = null;
                diffImageData = null;
            }
        }

        /// <summary>
        /// 计算差值图像 - 完整对应Java的diffImgData()方法
        /// </summary>
        private byte[] CalculateDifferenceImage(byte[] currentBytes, byte[] lastBytes)
        {
            var resultBytes = new byte[lastBytes.Length];
            var size = lastBytes.Length / 2;

            for (int i = 0; i < size; i++)
            {
                // 转换为半精度浮点数
                float lastValue = ByteConverter.ToHalfFloat(lastBytes, 2 * i, 2 * i + 1);
                float currentValue = ByteConverter.ToHalfFloat(currentBytes, 2 * i, 2 * i + 1);
                
                // 计算差值
                float difference = currentValue - lastValue;
                
                // 转换回16位浮点字节
                var float16Bytes = ByteConverter.Float32ToFloat16Bytes(difference);
                resultBytes[2 * i] = float16Bytes[0];
                resultBytes[2 * i + 1] = float16Bytes[1];
            }

            return resultBytes;
        }

        /// <summary>
        /// 处理雷达图像数据 - 完整对应Java的handleRadarData()方法
        /// </summary>
        private byte[] HandleRadarImageData(byte[] bytes, int rangeNum, int angleNum, SarFileData sarFileData)
        {
            float matrixRangeRes = sarFileData.RangeResolution;
            float matrixAngleRes = sarFileData.AngleResolution;
            int width = rangeNum;
            int height = angleNum;

            // 初始化数组
            _radarImageData = new float[width][];
            for (int i = 0; i < width; i++)
            {
                _radarImageData[i] = new float[height];
            }
            
            _rangeDistances = new double[width];
            _angleDistances = new double[height];
            var floatArray = new float[width * height];

            int index = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    // 计算角度距离（只在第一行计算）
                    if (i == 0)
                    {
                        _angleDistances[j] = j * matrixAngleRes + sarFileData.AngleMin;
                    }

                    // 计算距离（只在第一列计算）
                    if (j == 0)
                    {
                        _rangeDistances[i] = i * matrixRangeRes + sarFileData.RangeMin;
                    }

                    // 转换字节为浮点数
                    float value = ByteConverter.ToHalfFloat(bytes, 2 * index, 2 * index + 1);
                    floatArray[index] = value;
                    _radarImageData[i][j] = value;
                    index++;
                }
            }

            // 处理角度距离 - 对应Java的handleAngDistance()方法
            HandleAngleDistances();

            // 转换浮点数组为字节数组 - 对应Java的floatBufferToByteBuffer()方法
            return FloatArrayToByteArray(floatArray);
        }

        /// <summary>
        /// 处理角度距离 - 完整对应Java的handleAngDistance()方法
        /// </summary>
        private void HandleAngleDistances()
        {
            if (_angleDistances == null || _angleDistances.Length == 0)
                return;

            // 转换为度
            double angleStart = _angleDistances[0] * 180.0 / Math.PI;
            double angleEnd = _angleDistances[_angleDistances.Length - 1] * 180.0 / Math.PI;
            
            // 计算中间角度
            double radMiddle = ((angleEnd - angleStart) / 2.0 + angleStart) * Math.PI / 180.0;

            // 调整所有角度
            for (int i = 0; i < _angleDistances.Length; i++)
            {
                _angleDistances[i] -= radMiddle;
            }
        }

        /// <summary>
        /// 浮点数组转字节数组 - 完整对应Java的floatBufferToByteBuffer()方法
        /// </summary>
        private byte[] FloatArrayToByteArray(float[] floatArray)
        {
            var result = new byte[floatArray.Length * 4];
            
            for (int i = 0; i < floatArray.Length; i++)
            {
                var bytes = ByteConverter.SingleToBytes(floatArray[i]);
                result[4 * i] = bytes[0];
                result[4 * i + 1] = bytes[1];
                result[4 * i + 2] = bytes[2];
                result[4 * i + 3] = bytes[3];
            }

            return result;
        }

        /// <summary>
        /// 保存差值图像 - 完整对应Java的flush()方法
        /// </summary>
        private async Task FlushDifferenceImageAsync(SarFileData sarFileData, string deviceId, byte[] imageData)
        {
            try
            {
                // 生成文件名 - 对应Java的格式
                var timestamp = DateTimeOffset.FromUnixTimeSeconds(sarFileData.TimeMillis).DateTime;
                var fileName = timestamp.ToString("yyyy_MM_dd_HH_mm_ss") + ".DiffImage";

                // 保存文件
                var paths = await _sarFileStorage.SaveAsync(
                    sarFileData.ImageData, 
                    fileName, 
                    "RadarSystem", 
                    deviceId, 
                    DateTimeOffset.Now.ToUnixTimeMilliseconds());

                // 写入差值图像数据
                await _sarFileProcessor.WriteDataV1Async(
                    paths[1], 
                    sarFileData, 
                    _rangeDistances!, 
                    _angleDistances!, 
                    imageData);

                _logger.LogInformation("写入差值图像的文件 {FilePath}", paths[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存差值图像失败");
                throw;
            }
        }

        /// <summary>
        /// 数据过滤验证 - 完整对应Java的filter()方法
        /// </summary>
        private bool FilterRadarData(string dataType, string fileName, string deviceId)
        {
            // 检查设备ID是否在配置列表中
            if (!Array.Exists(_configuration.DeviceIds, id => id == deviceId))
            {
                _logger.LogInformation("当前雷达ID:{DeviceId}不在雷达列表内:{DeviceList}", 
                    deviceId, string.Join(",", _configuration.DeviceIds));
                return false;
            }

            // 只接受原始数据类型
            if (dataType != "00")
            {
                _logger.LogInformation("只接受雷达的原始数据，当前类型: {DataType}", dataType);
                return false;
            }

            // 检查时间间隔
            var currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var diffTimeMinutes = (currentTime - _lastProcessTime) / 1000 / 60;

            if (diffTimeMinutes < _configuration.GenerationTimeMinutes)
            {
                _logger.LogInformation(
                    "时间差{DiffTime}分钟未超过{RequiredTime}分钟 文件名{FileName}",
                    diffTimeMinutes, _configuration.GenerationTimeMinutes, fileName);
                return false;
            }

            // 更新最后处理时间
            _lastProcessTime = currentTime;
            return true;
        }

        /// <summary>
        /// 克隆SAR文件数据
        /// </summary>
        private SarFileData CloneSarFileData(SarFileData source)
        {
            return new SarFileData
            {
                Sequence = source.Sequence,
                TaskId = source.TaskId,
                TimeMillis = source.TimeMillis,
                RangeResolution = source.RangeResolution,
                RangeNumber = source.RangeNumber,
                RangeMin = source.RangeMin,
                AngleResolution = source.AngleResolution,
                AngleNumber = source.AngleNumber,
                AngleMin = source.AngleMin,
                ImageData = (byte[])source.ImageData.Clone(),
                DataType = source.DataType,
                DeviceId = source.DeviceId
            };
        }
    }
}
