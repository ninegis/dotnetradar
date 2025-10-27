using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 设备服务实现
    /// </summary>
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(
            IDeviceRepository deviceRepository,
            IProjectRepository projectRepository,
            ILogger<DeviceService> logger)
        {
            _deviceRepository = deviceRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, int DeviceId)> CreateDeviceAsync(CreateDeviceRequest request)
        {
            try
            {
                _logger.LogInformation("开始创建设备 - DeviceId: {DeviceId}, ProjectId: {ProjectId}, DeviceName: {DeviceName}", 
                    request.DeviceId, request.ProjectId, request.DeviceName);

                // 验证必填字段（先验证，避免空值查询）
                if (string.IsNullOrWhiteSpace(request.DeviceId))
                {
                    _logger.LogWarning("创建设备失败：设备ID为空");
                    return (false, "设备ID不能为空", 0);
                }

                if (string.IsNullOrWhiteSpace(request.DeviceName))
                {
                    _logger.LogWarning("创建设备失败：设备名称为空");
                    return (false, "设备名称不能为空", 0);
                }

                if (string.IsNullOrWhiteSpace(request.ProjectId))
                {
                    _logger.LogWarning("创建设备失败：项目ID为空");
                    return (false, "项目ID不能为空", 0);
                }

                // 验证设备ID是否已存在
                var deviceExists = await _deviceRepository.ExistsAsync(request.DeviceId);
                _logger.LogInformation("设备ID存在性检查：{DeviceId} = {Exists}", request.DeviceId, deviceExists);
                if (deviceExists)
                {
                    return (false, $"设备ID '{request.DeviceId}' 已存在", 0);
                }

                // 验证项目是否存在
                var projectExists = await _projectRepository.ExistsAsync(request.ProjectId);
                _logger.LogInformation("项目ID存在性检查：{ProjectId} = {Exists}", request.ProjectId, projectExists);
                if (!projectExists)
                {
                    return (false, $"项目 '{request.ProjectId}' 不存在，请先创建项目", 0);
                }

                var device = new Device
                {
                    DeviceId = request.DeviceId,
                    ProjectId = request.ProjectId,
                    DeviceName = request.DeviceName,
                    DeviceType = request.DeviceType,
                    DeviceTypeCode = request.DeviceTypeCode,
                    Status = "Offline",
                    // 地理位置信息
                    Longitude = request.Longitude,
                    Latitude = request.Latitude,
                    Elevation = request.Elevation,
                    Location = request.Location,
                    IpAddress = request.IpAddress,
                    Port = request.Port,
                    MqttTopic = request.MqttTopic,
                    // 雷达特有信息
                    FactoryId = request.FactoryId,
                    Orientation = request.Orientation,
                    Description = request.Description,
                    LastUpdateTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };

                var deviceId = await _deviceRepository.CreateAsync(device);

                _logger.LogInformation("设备创建成功: {DeviceId} - {DeviceName}", request.DeviceId, request.DeviceName);
                return (true, "设备创建成功", deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建设备失败: {DeviceId}", request.DeviceId);
                return (false, $"创建设备失败: {ex.Message}", 0);
            }
        }

        public async Task<(bool Success, string Message)> UpdateDeviceAsync(Device device)
        {
            try
            {
                // 验证设备是否存在
                var existingDevice = await _deviceRepository.GetByDeviceIdAsync(device.DeviceId);
                if (existingDevice == null)
                {
                    return (false, $"设备 '{device.DeviceId}' 不存在");
                }

                // 验证必填字段
                if (string.IsNullOrWhiteSpace(device.DeviceName))
                {
                    return (false, "设备名称不能为空");
                }

                var success = await _deviceRepository.UpdateAsync(device);

                if (success)
                {
                    _logger.LogInformation("设备更新成功: {DeviceId}", device.DeviceId);
                    return (true, "设备更新成功");
                }
                else
                {
                    return (false, "设备更新失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新设备失败: {DeviceId}", device.DeviceId);
                return (false, $"更新设备失败: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteDeviceAsync(string deviceId)
        {
            try
            {
                var success = await _deviceRepository.DeleteAsync(deviceId);

                if (success)
                {
                    _logger.LogInformation("设备删除成功: {DeviceId}", deviceId);
                    return (true, "设备删除成功");
                }
                else
                {
                    return (false, "设备不存在或已被删除");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除设备失败: {DeviceId}", deviceId);
                return (false, $"删除设备失败: {ex.Message}");
            }
        }

        public async Task<Device?> GetDeviceAsync(string deviceId)
        {
            try
            {
                return await _deviceRepository.GetByDeviceIdAsync(deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备失败: {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<(List<Device> Devices, int TotalCount)> QueryDevicesAsync(DeviceQueryRequest request)
        {
            try
            {
                var devices = await _deviceRepository.QueryAsync(request);
                var totalCount = await _deviceRepository.CountAsync(request);

                return (devices, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询设备列表失败");
                throw;
            }
        }

        public async Task<List<Device>> GetProjectDevicesAsync(string projectId)
        {
            try
            {
                return await _deviceRepository.GetByProjectIdAsync(projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取项目设备列表失败: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<bool> UpdateDeviceStatusAsync(string deviceId, string status)
        {
            try
            {
                return await _deviceRepository.UpdateStatusAsync(deviceId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新设备状态失败: {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<bool> IsDeviceIdAvailableAsync(string deviceId)
        {
            try
            {
                return !await _deviceRepository.ExistsAsync(deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证设备ID失败: {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<string> AddDeviceAsync(object request)
        {
            _logger.LogInformation("添加设备 (新接口)");
            // TODO: 实现添加设备逻辑，对应前端API调用
            await Task.CompletedTask;
            return Guid.NewGuid().ToString();
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            _logger.LogInformation("删除设备: {DeviceId} (新接口)", deviceId);
            // TODO: 实现删除设备逻辑，对应前端API调用
            await _deviceRepository.DeleteAsync(deviceId);
        }

        public async Task<object> GetRadarOnlineStatusByTimeAsync(string deviceId, string datetime)
        {
            _logger.LogInformation("获取雷达在线状态: {DeviceId}, 时间: {DateTime}", deviceId, datetime);
            // TODO: 实现获取雷达在线状态逻辑
            await Task.CompletedTask;
            return new { online = true, datetime };
        }

        public async Task<object> GetRadarLastHeartbeatAsync(string deviceId)
        {
            _logger.LogInformation("获取雷达最后心跳时间: {DeviceId}", deviceId);
            // TODO: 实现获取雷达心跳时间逻辑
            await Task.CompletedTask;
            return new { deviceId, lastHeartbeat = DateTime.UtcNow };
        }
    }
}

