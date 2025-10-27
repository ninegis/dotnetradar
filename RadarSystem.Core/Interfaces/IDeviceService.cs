using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 设备服务接口
    /// </summary>
    public interface IDeviceService
    {
        /// <summary>
        /// 创建设备
        /// </summary>
        Task<(bool Success, string Message, int DeviceId)> CreateDeviceAsync(CreateDeviceRequest request);

        /// <summary>
        /// 更新设备
        /// </summary>
        Task<(bool Success, string Message)> UpdateDeviceAsync(Device device);

        /// <summary>
        /// 删除设备
        /// </summary>
        Task<(bool Success, string Message)> DeleteDeviceAsync(string deviceId);

        /// <summary>
        /// 获取设备详情
        /// </summary>
        Task<Device?> GetDeviceAsync(string deviceId);

        /// <summary>
        /// 查询设备列表
        /// </summary>
        Task<(List<Device> Devices, int TotalCount)> QueryDevicesAsync(DeviceQueryRequest request);

        /// <summary>
        /// 获取项目下的所有设备
        /// </summary>
        Task<List<Device>> GetProjectDevicesAsync(string projectId);

        /// <summary>
        /// 更新设备状态
        /// </summary>
        Task<bool> UpdateDeviceStatusAsync(string deviceId, string status);

        /// <summary>
        /// 验证设备ID是否可用
        /// </summary>
        Task<bool> IsDeviceIdAvailableAsync(string deviceId);

        // 新增接口，对应前端API调用
        Task<string> AddDeviceAsync(object request);
        Task RemoveDeviceAsync(string deviceId);
        Task<object> GetRadarOnlineStatusByTimeAsync(string deviceId, string datetime);
        Task<object> GetRadarLastHeartbeatAsync(string deviceId);
    }
}

