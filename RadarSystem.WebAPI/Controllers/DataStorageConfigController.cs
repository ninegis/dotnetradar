using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;
using System.Text.Json;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 数据存储配置控制器
    /// </summary>
    [ApiController]
    [Route("api/storage")]
    public class DataStorageConfigController : ControllerBase
    {
        private readonly RadarDbContext _dbContext;
        private readonly ILogger<DataStorageConfigController> _logger;

        public DataStorageConfigController(RadarDbContext dbContext, ILogger<DataStorageConfigController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取数据存储配置
        /// GET /api/storage/config/{projectId}
        /// </summary>
        [HttpGet("config/{projectId}")]
        public async Task<IActionResult> GetStorageConfig(string projectId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                var config = await _dbContext.DataStorageConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId);

                if (config == null)
                {
                    return Ok(new { code = 404, message = "未找到存储配置" });
                }

                return Ok(new
                {
                    code = 200,
                    data = new
                    {
                        projectId = config.ProjectId,
                        autoCleanupEnable = config.AutoCleanupEnable,
                        diskThresholdPercent = config.DiskThresholdPercent,
                        dataRetentionDays = config.DataRetentionDays,
                        deleteRawData = config.DeleteRawData,
                        deleteImageData = config.DeleteImageData,
                        deleteAnalysisData = config.DeleteAnalysisData,
                        imageQuality = config.ImageQuality,
                        imageCompressionEnable = config.ImageCompressionEnable,
                        storagePath = config.StoragePath,
                        backupPath = config.BackupPath,
                        autoBackupEnable = config.AutoBackupEnable,
                        backupIntervalDays = config.BackupIntervalDays,
                        maxBackupCount = config.MaxBackupCount,
                        createTime = config.CreateTime,
                        updateTime = config.UpdateTime
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取存储配置失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新数据存储配置
        /// POST /api/storage/config
        /// </summary>
        [HttpPost("config")]
        public async Task<IActionResult> UpdateStorageConfig([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();

                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                _logger.LogInformation("更新数据存储配置: ProjectId={ProjectId}", projectId);

                // 查找或创建配置
                var config = await _dbContext.DataStorageConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId);

                if (config == null)
                {
                    config = new DataStorageConfigEntity
                    {
                        ProjectId = projectId,
                        CreateTime = DateTime.Now
                    };
                    _dbContext.DataStorageConfigs.Add(config);
                }

                // 更新字段
                if (body.TryGetProperty("autoCleanupEnable", out var autoCleanupEnable))
                    config.AutoCleanupEnable = autoCleanupEnable.GetBoolean();
                if (body.TryGetProperty("diskThresholdPercent", out var diskThreshold))
                    config.DiskThresholdPercent = diskThreshold.GetInt32();
                if (body.TryGetProperty("dataRetentionDays", out var retentionDays))
                    config.DataRetentionDays = retentionDays.GetInt32();
                if (body.TryGetProperty("deleteRawData", out var deleteRaw))
                    config.DeleteRawData = deleteRaw.GetBoolean();
                if (body.TryGetProperty("deleteImageData", out var deleteImage))
                    config.DeleteImageData = deleteImage.GetBoolean();
                if (body.TryGetProperty("deleteAnalysisData", out var deleteAnalysis))
                    config.DeleteAnalysisData = deleteAnalysis.GetBoolean();
                if (body.TryGetProperty("imageQuality", out var imageQuality))
                    config.ImageQuality = imageQuality.GetInt32();
                if (body.TryGetProperty("imageCompressionEnable", out var imageCompression))
                    config.ImageCompressionEnable = imageCompression.GetBoolean();
                if (body.TryGetProperty("storagePath", out var storagePath))
                    config.StoragePath = storagePath.GetString() ?? config.StoragePath;
                if (body.TryGetProperty("backupPath", out var backupPath))
                    config.BackupPath = backupPath.GetString();
                if (body.TryGetProperty("autoBackupEnable", out var autoBackup))
                    config.AutoBackupEnable = autoBackup.GetBoolean();
                if (body.TryGetProperty("backupIntervalDays", out var backupInterval))
                    config.BackupIntervalDays = backupInterval.GetInt32();
                if (body.TryGetProperty("maxBackupCount", out var maxBackup))
                    config.MaxBackupCount = maxBackup.GetInt32();

                config.UpdateTime = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(new { code = 200, message = "存储配置更新成功", data = "配置已保存" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新存储配置失败");
                return Ok(new { code = 500, message = $"更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取磁盘使用情况
        /// GET /api/storage/diskinfo/{projectId}
        /// </summary>
        [HttpGet("diskinfo/{projectId}")]
        public async Task<IActionResult> GetDiskInfo(string projectId)
        {
            try
            {
                var config = await _dbContext.DataStorageConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId);

                var storagePath = config?.StoragePath ?? "./Data";

                // 获取磁盘信息
                var driveInfo = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(storagePath)) ?? "C:\\");

                return Ok(new
                {
                    code = 200,
                    data = new
                    {
                        totalSpace = driveInfo.TotalSize,
                        freeSpace = driveInfo.AvailableFreeSpace,
                        usedSpace = driveInfo.TotalSize - driveInfo.AvailableFreeSpace,
                        usedPercent = (int)((driveInfo.TotalSize - driveInfo.AvailableFreeSpace) * 100 / driveInfo.TotalSize),
                        driveName = driveInfo.Name,
                        driveFormat = driveInfo.DriveFormat,
                        isReady = driveInfo.IsReady
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取磁盘信息失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 执行数据清理
        /// POST /api/storage/cleanup
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupData([FromBody] JsonElement body)
        {
            try
            {
                var projectId = body.GetProperty("projectId").GetString();

                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Ok(new { code = 400, message = "项目ID不能为空" });
                }

                _logger.LogInformation("执行数据清理: ProjectId={ProjectId}", projectId);

                var config = await _dbContext.DataStorageConfigs
                    .FirstOrDefaultAsync(c => c.ProjectId == projectId);

                if (config == null)
                {
                    return Ok(new { code = 404, message = "未找到存储配置" });
                }

                // 计算清理日期
                var cutoffDate = DateTime.Now.AddDays(-config.DataRetentionDays);
                int deletedCount = 0;

                // 清理原始数据
                if (config.DeleteRawData)
                {
                    var oldRawData = await _dbContext.RadarData
                        .Where(r => r.ProjectId == projectId && r.Timestamp < cutoffDate)
                        .ToListAsync();
                    _dbContext.RadarData.RemoveRange(oldRawData);
                    deletedCount += oldRawData.Count;
                }

                // 清理图像数据
                if (config.DeleteImageData)
                {
                    var oldImages = await _dbContext.RadarImages
                        .Where(i => i.ProjectId == projectId && i.CreateTime < cutoffDate)
                        .ToListAsync();
                    _dbContext.RadarImages.RemoveRange(oldImages);
                    deletedCount += oldImages.Count;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    code = 200,
                    message = "数据清理完成",
                    data = new
                    {
                        deletedCount = deletedCount,
                        cutoffDate = cutoffDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据清理失败");
                return Ok(new { code = 500, message = $"清理失败: {ex.Message}" });
            }
        }
    }
}

