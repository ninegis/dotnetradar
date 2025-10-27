using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Interfaces;
using RadarSystem.Communication.Models;

namespace RadarSystem.Communication.Services
{
    /// <summary>
    /// 雷达数据文件存储服务
    /// 按设备ID和日期组织文件：/{projectId}/radar/{deviceId}/{yyyyMMdd}/{Type}{UUID}
    /// </summary>
    public class RadarFileStorageService : IRadarFileStorage
    {
        private readonly string _baseDataPath;
        private readonly ILogger<RadarFileStorageService> _logger;

        public RadarFileStorageService(
            IConfiguration configuration,
            ILogger<RadarFileStorageService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // 从配置中读取基础路径，默认为 ./data
            _baseDataPath = configuration["RadarDataReceiver:DataPath"] ?? "./data";
            
            // 确保基础目录存在
            if (!Directory.Exists(_baseDataPath))
            {
                Directory.CreateDirectory(_baseDataPath);
                _logger.LogInformation("创建数据根目录: {Path}", _baseDataPath);
            }
        }

        /// <summary>
        /// 生成文件路径
        /// 格式：{baseDataPath}/project/{projectId}/radar/{deviceId}/{yyyyMMdd}/{Type}{UUID}
        /// </summary>
        public string GenerateFilePath(RadarDataPacket packet)
        {
            try
            {
                // 数据类型前缀
                string dataTypePrefix = packet.GetDataTypePrefix();
                
                // 按日期创建目录
                string datePath = packet.ReceiveTime.ToString("yyyyMMdd");
                
                // 生成UUID文件名
                string uuid = Guid.NewGuid().ToString("N");  // 32位无分隔符
                
                // 获取设备标识（优先DeviceId，否则用SlaveId）
                string deviceIdentifier = packet.GetDeviceIdentifier();
                
                // 完整路径
                string fullPath = Path.Combine(
                    _baseDataPath,
                    "project",
                    packet.ProjectId,
                    "radar",
                    deviceIdentifier,
                    datePath,
                    $"{dataTypePrefix}{uuid}"
                );
                
                _logger.LogDebug("生成文件路径: {Path}", fullPath);
                return fullPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成文件路径失败: {Packet}", packet);
                throw;
            }
        }

        /// <summary>
        /// 保存雷达数据到文件
        /// </summary>
        public async Task SaveRadarDataAsync(string filePath, byte[] data)
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath) 
                    ?? throw new InvalidOperationException("无效的文件路径");
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug("创建目录: {Directory}", directory);
                }
                
                // 写入文件
                await File.WriteAllBytesAsync(filePath, data);
                
                _logger.LogInformation("文件保存成功: {FilePath}, Size={Size} bytes", 
                    filePath, data.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存文件失败: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// 读取雷达数据文件
        /// </summary>
        public async Task<byte[]> ReadRadarDataAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"文件不存在: {filePath}");
                }
                
                byte[] data = await File.ReadAllBytesAsync(filePath);
                _logger.LogDebug("读取文件: {FilePath}, Size={Size} bytes", filePath, data.Length);
                
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取文件失败: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 获取设备的数据文件列表
        /// </summary>
        public async Task<string[]> GetDeviceDataFilesAsync(
            string projectId, 
            string deviceId, 
            string? date = null)
        {
            try
            {
                string devicePath = Path.Combine(
                    _baseDataPath,
                    "project",
                    projectId,
                    "radar",
                    deviceId
                );
                
                if (!Directory.Exists(devicePath))
                {
                    _logger.LogWarning("设备数据目录不存在: {Path}", devicePath);
                    return Array.Empty<string>();
                }
                
                // 如果指定了日期，只查询该日期的文件
                if (!string.IsNullOrEmpty(date))
                {
                    string datePath = Path.Combine(devicePath, date);
                    if (Directory.Exists(datePath))
                    {
                        return await Task.FromResult(Directory.GetFiles(datePath));
                    }
                    return Array.Empty<string>();
                }
                
                // 否则查询所有文件
                return await Task.FromResult(
                    Directory.GetFiles(devicePath, "*", SearchOption.AllDirectories)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备数据文件列表失败: ProjectId={ProjectId}, DeviceId={DeviceId}", 
                    projectId, deviceId);
                return Array.Empty<string>();
            }
        }
    }
}

