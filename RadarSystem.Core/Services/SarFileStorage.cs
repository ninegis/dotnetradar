using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// SAR文件存储服务实现
    /// </summary>
    public class SarFileStorage : ISarFileStorage
    {
        private readonly ILogger<SarFileStorage> _logger;
        private readonly string _baseStoragePath;

        public SarFileStorage(ILogger<SarFileStorage> logger, string baseStoragePath)
        {
            _logger = logger;
            _baseStoragePath = baseStoragePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RadarSystem", "Storage");
            
            // 确保存储目录存在
            Directory.CreateDirectory(_baseStoragePath);
        }

        public async Task<string[]> SaveAsync(byte[] fileData, string fileName, string projectName, string deviceId, long timestamp)
        {
            try
            {
                _logger.LogInformation("开始保存SAR文件: {FileName}", fileName);

                // 创建项目目录结构
                var projectPath = Path.Combine(_baseStoragePath, projectName);
                var devicePath = Path.Combine(projectPath, deviceId);
                var datePath = Path.Combine(devicePath, DateTime.Now.ToString("yyyy-MM-dd"));

                Directory.CreateDirectory(datePath);

                // 生成文件路径
                var relativePath = Path.Combine(projectName, deviceId, DateTime.Now.ToString("yyyy-MM-dd"), fileName);
                var absolutePath = Path.Combine(_baseStoragePath, relativePath);

                // 保存文件
                await File.WriteAllBytesAsync(absolutePath, fileData);

                _logger.LogInformation("SAR文件保存成功: {AbsolutePath}", absolutePath);
                return new string[] { absolutePath, relativePath };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存SAR文件时发生错误: {FileName}", fileName);
                throw;
            }
        }

        public async Task<byte[]> ReadAsync(string filePath)
        {
            try
            {
                _logger.LogDebug("开始读取SAR文件: {FilePath}", filePath);

                var fullPath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(_baseStoragePath, filePath);
                
                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("SAR文件不存在: {FilePath}", fullPath);
                    return Array.Empty<byte>();
                }

                var fileData = await File.ReadAllBytesAsync(fullPath);

                _logger.LogDebug("SAR文件读取完成: {FilePath}", filePath);
                return fileData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取SAR文件时发生错误: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("开始删除SAR文件: {FilePath}", filePath);

                var fullPath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(_baseStoragePath, filePath);
                
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("SAR文件删除成功: {FilePath}", filePath);
                    return true;
                }
                else
                {
                    _logger.LogWarning("要删除的SAR文件不存在: {FilePath}", filePath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除SAR文件时发生错误: {FilePath}", filePath);
                return false;
            }
        }

        public bool Exists(string filePath)
        {
            try
            {
                var fullPath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(_baseStoragePath, filePath);
                return File.Exists(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查SAR文件是否存在时发生错误: {FilePath}", filePath);
                return false;
            }
        }
    }
}
