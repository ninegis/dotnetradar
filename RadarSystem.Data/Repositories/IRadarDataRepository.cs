using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    /// <summary>
    /// 雷达数据仓库接口
    /// </summary>
    public interface IRadarDataRepository
    {
        /// <summary>
        /// 添加雷达数据
        /// </summary>
        Task<bool> AddRadarDataAsync(RadarDataEntity radarData);

        /// <summary>
        /// 根据ID获取雷达数据
        /// </summary>
        Task<RadarDataEntity?> GetRadarDataByIdAsync(int id);

        /// <summary>
        /// 根据设备ID和时间范围获取雷达数据
        /// </summary>
        Task<List<RadarDataEntity>> GetRadarDataByDeviceAndTimeAsync(string deviceId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 根据项目ID获取雷达数据
        /// </summary>
        Task<List<RadarDataEntity>> GetRadarDataByProjectAsync(string projectId, int pageIndex, int pageSize);

        /// <summary>
        /// 更新雷达数据
        /// </summary>
        Task<bool> UpdateRadarDataAsync(RadarDataEntity radarData);

        /// <summary>
        /// 删除雷达数据
        /// </summary>
        Task<bool> DeleteRadarDataAsync(int id);

        /// <summary>
        /// 获取雷达数据总数
        /// </summary>
        Task<int> GetRadarDataCountAsync(string? projectId = null, string? deviceId = null);

        /// <summary>
        /// 根据文件名获取雷达数据
        /// </summary>
        Task<RadarDataEntity?> GetRadarDataByFileNameAsync(string fileName);
    }
}
