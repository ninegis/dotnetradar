using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.WebAPI.Models;
using System.IO;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 数据存储控制器 - 磁盘空间查询
    /// </summary>
    [ApiController]
    [Route("api/datastorage")]
    [Authorize]
    public class DataStorageController : ControllerBase
    {
        private readonly ILogger<DataStorageController> _logger;

        public DataStorageController(ILogger<DataStorageController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 查询磁盘空间
        /// GET /api/datastorage/query/discSpace
        /// </summary>
        [HttpGet("query/discSpace")]
        public ApiResponse<DiskSpaceInfo> QueryDiskSpace()
        {
            try
            {
                // 获取系统所有驱动器信息
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .ToList();

                if (drives.Any())
                {
                    // 返回第一个固定磁盘的信息（通常是C盘）
                    var primaryDrive = drives.First();
                    
                    var diskInfo = new DiskSpaceInfo
                    {
                        DriveName = primaryDrive.Name,
                        TotalSize = primaryDrive.TotalSize,
                        TotalFreeSpace = primaryDrive.TotalFreeSpace,
                        AvailableFreeSpace = primaryDrive.AvailableFreeSpace,
                        UsedSpace = primaryDrive.TotalSize - primaryDrive.TotalFreeSpace,
                        UsedPercentage = (double)(primaryDrive.TotalSize - primaryDrive.TotalFreeSpace) / primaryDrive.TotalSize * 100,
                        FreePercentage = (double)primaryDrive.TotalFreeSpace / primaryDrive.TotalSize * 100
                    };

                    _logger.LogInformation("磁盘空间查询成功: {DriveName}, 已用: {UsedPercentage:F2}%", 
                        diskInfo.DriveName, diskInfo.UsedPercentage);

                    return ApiResponse<DiskSpaceInfo>.Ok(diskInfo);
                }

                return ApiResponse<DiskSpaceInfo>.Fail(404, "未找到可用的磁盘驱动器");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询磁盘空间失败");
                return ApiResponse<DiskSpaceInfo>.Fail(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询所有磁盘空间
        /// GET /api/datastorage/query/allDisks
        /// </summary>
        [HttpGet("query/allDisks")]
        public ApiResponse<List<DiskSpaceInfo>> QueryAllDisks()
        {
            try
            {
                var diskList = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .Select(drive => new DiskSpaceInfo
                    {
                        DriveName = drive.Name,
                        TotalSize = drive.TotalSize,
                        TotalFreeSpace = drive.TotalFreeSpace,
                        AvailableFreeSpace = drive.AvailableFreeSpace,
                        UsedSpace = drive.TotalSize - drive.TotalFreeSpace,
                        UsedPercentage = (double)(drive.TotalSize - drive.TotalFreeSpace) / drive.TotalSize * 100,
                        FreePercentage = (double)drive.TotalFreeSpace / drive.TotalSize * 100
                    })
                    .ToList();

                _logger.LogInformation("查询所有磁盘空间成功，共{Count}个磁盘", diskList.Count);

                return ApiResponse<List<DiskSpaceInfo>>.Ok(diskList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询所有磁盘空间失败");
                return ApiResponse<List<DiskSpaceInfo>>.Fail(500, ex.Message);
            }
        }
    }

    /// <summary>
    /// 磁盘空间信息
    /// </summary>
    public class DiskSpaceInfo
    {
        /// <summary>
        /// 驱动器名称
        /// </summary>
        public string DriveName { get; set; } = string.Empty;

        /// <summary>
        /// 总大小（字节）
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// 总空闲空间（字节）
        /// </summary>
        public long TotalFreeSpace { get; set; }

        /// <summary>
        /// 可用空闲空间（字节）
        /// </summary>
        public long AvailableFreeSpace { get; set; }

        /// <summary>
        /// 已用空间（字节）
        /// </summary>
        public long UsedSpace { get; set; }

        /// <summary>
        /// 已用百分比
        /// </summary>
        public double UsedPercentage { get; set; }

        /// <summary>
        /// 空闲百分比
        /// </summary>
        public double FreePercentage { get; set; }
    }
}

