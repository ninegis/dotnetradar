using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.Core.Models;
using RadarSystem.Core.Utilities;
using Snappy;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// SAR文件数据处理器 - 完整实现Java SarFileData所有功能
    /// </summary>
    public class SarFileDataProcessor : ISarFileDataProcessor, IDisposable
    {
        private readonly ILogger<SarFileDataProcessor> _logger;
        private bool _disposed = false;

        public SarFileDataProcessor(ILogger<SarFileDataProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 异步读取SAR文件头
        /// </summary>
        public async Task<SarFileHeader> ReadHeaderAsync(string filePath, string dataType, int offsetByte)
        {
            try
            {
                _logger.LogDebug("开始读取SAR文件头: {FilePath}", filePath);

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);

                var start = offsetByte;
                var header = new SarFileHeader
                {
                    Sequence = ByteConverter.ToInt32LittleEndian(buffer, start, start + 3),
                    TimeMillis = ByteConverter.ToInt32LittleEndian(buffer, start + 20, start + 23),
                    TaskId = ByteConverter.ToInt32LittleEndian(buffer, start + 24, start + 27),
                    RangeResolution = ByteConverter.ToSingleLittleEndian(buffer, start + 28, start + 31),
                    RangeNumber = ByteConverter.ToInt32LittleEndian(buffer, start + 32, start + 35),
                    RangeMin = ByteConverter.ToSingleLittleEndian(buffer, start + 36, start + 39),
                    AngleResolution = ByteConverter.ToSingleLittleEndian(buffer, start + 40, start + 43),
                    AngleNumber = ByteConverter.ToInt32LittleEndian(buffer, start + 44, start + 47),
                    AngleMin = ByteConverter.ToSingleLittleEndian(buffer, start + 48, start + 51),
                    SarDataType = ByteConverter.ToInt32LittleEndian(buffer, start + 52, start + 55),
                    DataSize = ByteConverter.ToInt32LittleEndian(buffer, start + 56, start + 59),
                    DataType = dataType,
                    FilePath = filePath
                };

                // MD5校验
                header.Md5CheckResult = await ValidateMD5Async(buffer, offsetByte);

                // 根据数据类型读取额外字段
                if (dataType == "01") // 散射
                {
                    header.ImageMaxAmplitude = ByteConverter.ToSingleLittleEndian(buffer, start + 60, start + 63);
                }
                else if (dataType == "11" || dataType == "60" || dataType == "70") // 形变相关
                {
                    header.Longitude = ByteConverter.ToDoubleLittleEndian(buffer, start + 60, start + 67);
                    header.Latitude = ByteConverter.ToDoubleLittleEndian(buffer, start + 68, start + 75);
                    header.Altitude = ByteConverter.ToDoubleLittleEndian(buffer, start + 76, start + 83);
                    header.NorthAngle = ByteConverter.ToSingleLittleEndian(buffer, start + 84, start + 87);
                }
                else if (dataType == "61" || dataType == "71") // MIMO散射
                {
                    header.ImageMaxAmplitude = ByteConverter.ToSingleLittleEndian(buffer, start + 60, start + 63);
                    header.ImageMinAmplitude = ByteConverter.ToSingleLittleEndian(buffer, start + 64, start + 67);
                }

                // 转换时间戳
                header.Date = DateTimeOffset.FromUnixTimeSeconds(header.TimeMillis).DateTime;

                _logger.LogDebug("SAR文件头读取完成: Seq={Sequence}, TaskId={TaskId}", header.Sequence, header.TaskId);
                return header;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取SAR文件头异常: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// 异步读取SAR文件数据
        /// </summary>
        public async Task<byte[]> ReadDataAsync(string filePath, string dataType, int offsetByte)
        {
            try
            {
                _logger.LogDebug("开始读取SAR文件数据: {FilePath}, 类型: {DataType}", filePath, dataType);

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);

                byte[] imageData;
                var start = offsetByte;

                // 根据数据类型处理数据
                switch (dataType)
                {
                    case "00": // 形变
                    case "20": // 速度
                    case "07": // 报警
                    case "30": // 加速度
                    case "40": // 高度
                    case "02": // 置信度
                    case "03": // 速度形变
                    case "04": // 速度断点
                    case "08": // 速度反演
                    case "09": // 敏感
                    case "05": // 速度手动
                    case "60": // MIMO形变
                    case "70": // Build2D形变
                        // Snappy解压缩
                        var compressed = buffer.Skip(start + 60).ToArray();
                        imageData = await Task.Run(() => SnappyCodec.Uncompress(compressed));
                        break;

                    case "01": // 散射
                        // 直接读取
                        imageData = buffer.Skip(start + 64).ToArray();
                        break;

                    case "10": // 断点形变
                        compressed = buffer.Skip(start + 60).ToArray();
                        imageData = await Task.Run(() => SnappyCodec.Uncompress(compressed));
                        break;

                    case "11": // 差值形变
                        compressed = buffer.Skip(start + 88).ToArray();
                        imageData = await Task.Run(() => SnappyCodec.Uncompress(compressed));
                        break;

                    case "61": // MIMO散射
                    case "71": // Build2D散射
                        imageData = buffer.Skip(start + 68).ToArray();
                        break;

                    default:
                        throw new NotSupportedException($"不支持的数据类型: {dataType}");
                }

                _logger.LogDebug("SAR文件数据读取完成，数据长度: {Length}", imageData.Length);
                return imageData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取SAR文件数据异常: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// 异步写入SAR文件数据（V1版本 - 差值图像）
        /// </summary>
        public async Task WriteDataV1Async(string filePath, SarFileData sarData, double[] rangeDistances, double[] angleDistances, byte[] imageData)
        {
            try
            {
                _logger.LogDebug("开始写入SAR文件数据V1: {FilePath}", filePath);

                if (sarData.RangeNumber <= 0 || sarData.AngleNumber <= 0)
                {
                    _logger.LogWarning("文件角度、距离量为0，跳过写入");
                    return;
                }

                using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                using var writer = new BinaryWriter(stream);

                // 写入距离和角度数量
                writer.Write(sarData.RangeNumber);
                writer.Write(sarData.AngleNumber);

                // 写入距离数组
                foreach (var distance in rangeDistances)
                {
                    writer.Write(distance);
                }

                // 写入角度数组
                foreach (var angle in angleDistances)
                {
                    writer.Write(angle);
                }

                // 写入图像数据
                await stream.WriteAsync(imageData, 0, imageData.Length);
                await stream.FlushAsync();

                _logger.LogDebug("SAR文件数据V1写入完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入SAR文件数据V1异常: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// 异步写入SAR文件数据（完整版本）
        /// </summary>
        public async Task WriteDataAsync(string filePath, SarFileData sarData, int offsetByte)
        {
            try
            {
                _logger.LogDebug("开始写入SAR文件数据: {FilePath}", filePath);

                using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                
                // 构建文件头
                var headerBytes = new byte[offsetByte];
                var dataBytes = ByteConverter.Concat(
                    headerBytes,
                    ByteConverter.Int32ToBytes(sarData.Sequence),
                    new byte[16], // MD5占位符
                    ByteConverter.Int32ToBytes(sarData.TimeMillis),
                    ByteConverter.Int32ToBytes(sarData.TaskId),
                    ByteConverter.SingleToBytes(sarData.RangeResolution),
                    ByteConverter.Int32ToBytes(sarData.RangeNumber),
                    ByteConverter.SingleToBytes(sarData.RangeMin),
                    ByteConverter.SingleToBytes(sarData.AngleResolution),
                    ByteConverter.Int32ToBytes(sarData.AngleNumber),
                    ByteConverter.SingleToBytes(sarData.AngleMin),
                    ByteConverter.Int32ToBytes(sarData.SarDataType),
                    ByteConverter.Int32ToBytes(sarData.DataSize)
                );

                // 根据数据类型添加数据
                byte[] compressedData;
                switch (sarData.DataType)
                {
                    case "00":
                    case "20":
                    case "07":
                    case "30":
                    case "40":
                    case "02":
                    case "03":
                    case "04":
                    case "08":
                    case "09":
                    case "05":
                        compressedData = await Task.Run(() => SnappyCodec.Compress(sarData.ImageData));
                        dataBytes = ByteConverter.Concat(dataBytes, compressedData);
                        break;

                    case "01":
                        dataBytes = ByteConverter.Concat(
                            dataBytes,
                            ByteConverter.SingleToBytes(sarData.ImageMaxAmplitude),
                            sarData.ImageData
                        );
                        break;

                    case "11":
                        dataBytes = ByteConverter.Concat(
                            dataBytes,
                            ByteConverter.DoubleToBytes(sarData.Longitude),
                            ByteConverter.DoubleToBytes(sarData.Latitude),
                            ByteConverter.DoubleToBytes(sarData.Altitude),
                            ByteConverter.SingleToBytes(sarData.NorthAngle)
                        );
                        compressedData = await Task.Run(() => SnappyCodec.Compress(sarData.ImageData));
                        dataBytes = ByteConverter.Concat(dataBytes, compressedData);
                        break;

                    case "61":
                    case "71":
                        dataBytes = ByteConverter.Concat(
                            dataBytes,
                            ByteConverter.SingleToBytes(sarData.ImageMaxAmplitude),
                            ByteConverter.SingleToBytes(sarData.ImageMinAmplitude),
                            sarData.ImageData
                        );
                        break;
                }

                // 生成并设置MD5
                var md5Bytes = await GenerateMD5Async(dataBytes, offsetByte);
                Array.Copy(md5Bytes, 0, dataBytes, offsetByte + 4, 16);

                // 写入文件
                await stream.WriteAsync(dataBytes, 0, dataBytes.Length);
                await stream.FlushAsync();

                _logger.LogDebug("SAR文件数据写入完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入SAR文件数据异常: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// MD5校验
        /// </summary>
        private async Task<bool> ValidateMD5Async(byte[] data, int offsetByte)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var md5 = MD5.Create();
                    
                    // 提取存储的MD5
                    var storedMd5 = data.Skip(offsetByte + 4).Take(16).ToArray();
                    
                    // 计算数据的MD5
                    var dataToCheck = data.Skip(offsetByte + 20).ToArray();
                    var calculatedMd5 = md5.ComputeHash(dataToCheck);
                    
                    return storedMd5.SequenceEqual(calculatedMd5);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MD5校验异常");
                    return false;
                }
            });
        }

        /// <summary>
        /// 生成MD5
        /// </summary>
        private async Task<byte[]> GenerateMD5Async(byte[] data, int offsetByte)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var md5 = MD5.Create();
                    var dataToHash = data.Skip(offsetByte + 20).ToArray();
                    return md5.ComputeHash(dataToHash);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "生成MD5异常");
                    return new byte[16];
                }
            });
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// SAR文件头信息
    /// </summary>
    public class SarFileHeader
    {
        public int Sequence { get; set; }
        public bool Md5CheckResult { get; set; }
        public int TaskId { get; set; }
        public int TimeMillis { get; set; }
        public DateTime Date { get; set; }
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
        public string DataType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// SAR文件数据处理器接口
    /// </summary>
    public interface ISarFileDataProcessor
    {
        Task<SarFileHeader> ReadHeaderAsync(string filePath, string dataType, int offsetByte);
        Task<byte[]> ReadDataAsync(string filePath, string dataType, int offsetByte);
        Task WriteDataV1Async(string filePath, SarFileData sarData, double[] rangeDistances, double[] angleDistances, byte[] imageData);
        Task WriteDataAsync(string filePath, SarFileData sarData, int offsetByte);
    }
}
