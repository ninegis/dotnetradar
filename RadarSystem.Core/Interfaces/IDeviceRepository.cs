using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 设备数据仓储接口
    /// </summary>
    public interface IDeviceRepository
    {
        /// <summary>
        /// 创建设备
        /// </summary>
        Task<int> CreateAsync(Device device);

        /// <summary>
        /// 更新设备
        /// </summary>
        Task<bool> UpdateAsync(Device device);

        /// <summary>
        /// 删除设备（软删除）
        /// </summary>
        Task<bool> DeleteAsync(string deviceId);

        /// <summary>
        /// 根据ID获取设备
        /// </summary>
        Task<Device?> GetByIdAsync(int id);

        /// <summary>
        /// 根据设备ID获取设备
        /// </summary>
        Task<Device?> GetByDeviceIdAsync(string deviceId);

        /// <summary>
        /// 查询设备列表
        /// </summary>
        Task<List<Device>> QueryAsync(DeviceQueryRequest request);

        /// <summary>
        /// 获取设备总数
        /// </summary>
        Task<int> CountAsync(DeviceQueryRequest request);

        /// <summary>
        /// 根据项目ID获取设备列表
        /// </summary>
        Task<List<Device>> GetByProjectIdAsync(string projectId);

        /// <summary>
        /// 更新设备状态
        /// </summary>
        Task<bool> UpdateStatusAsync(string deviceId, string status);

        /// <summary>
        /// 检查设备ID是否存在
        /// </summary>
        Task<bool> ExistsAsync(string deviceId);
    }
}

