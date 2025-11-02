using System.Collections.Concurrent;

namespace RadarSystem.Communication.Services
{
    /// <summary>
    /// 设备信息缓存 - 运行时内存缓存
    /// </summary>
    public class DeviceInfoCache
    {
        private static readonly ConcurrentDictionary<string, DeviceInfo> _deviceCache = new();
        private static readonly ConcurrentDictionary<string, ProjectInfo> _projectCache = new();
        
        // FactoryId → DeviceId 映射
        private static readonly ConcurrentDictionary<string, string> _factoryIdMapping = new();

        /// <summary>
        /// 添加设备
        /// </summary>
        public static void AddDevice(string deviceId, string factoryId, string projectId, string deviceName, string deviceType)
        {
            var device = new DeviceInfo
            {
                DeviceId = deviceId,
                FactoryId = factoryId,
                ProjectId = projectId,
                DeviceName = deviceName,
                DeviceType = deviceType
            };
            
            _deviceCache[deviceId] = device;
            
            if (!string.IsNullOrEmpty(factoryId))
            {
                _factoryIdMapping[factoryId] = deviceId;
            }
        }

        /// <summary>
        /// 添加项目
        /// </summary>
        public static void AddProject(string projectId, string projectName)
        {
            _projectCache[projectId] = new ProjectInfo
            {
                ProjectId = projectId,
                ProjectName = projectName
            };
        }

        /// <summary>
        /// 通过FactoryId获取DeviceId
        /// </summary>
        public static string? GetDeviceIdByFactoryId(string factoryId)
        {
            return _factoryIdMapping.TryGetValue(factoryId, out var deviceId) ? deviceId : null;
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        public static DeviceInfo? GetDevice(string deviceId)
        {
            return _deviceCache.TryGetValue(deviceId, out var device) ? device : null;
        }

        /// <summary>
        /// 获取项目信息
        /// </summary>
        public static ProjectInfo? GetProject(string projectId)
        {
            return _projectCache.TryGetValue(projectId, out var project) ? project : null;
        }

        /// <summary>
        /// 生成文件路径：ProjectId/DeviceId_FactoryId/dataType/yyyyMMdd/HHmmss.dat
        /// </summary>
        public static string GenerateFilePath(string basePath, string factoryId, string dataType)
        {
            var deviceId = GetDeviceIdByFactoryId(factoryId) ?? factoryId;
            var device = GetDevice(deviceId);
            var projectId = device?.ProjectId ?? "PROJECT001";
            
            // 目录结构：ProjectId/DeviceId_FactoryId
            string deviceFolder = string.IsNullOrEmpty(device?.FactoryId) 
                ? deviceId 
                : $"{deviceId}_{device.FactoryId}";
            
            string dateFolder = DateTime.Now.ToString("yyyyMMdd");
            string fileName = DateTime.Now.ToString("HHmmss") + ".dat";
            
            return Path.Combine(basePath, projectId, deviceFolder, dataType, dateFolder, fileName);
        }

        /// <summary>
        /// 获取所有设备数量
        /// </summary>
        public static int GetDeviceCount() => _deviceCache.Count;

        /// <summary>
        /// 获取所有项目数量
        /// </summary>
        public static int GetProjectCount() => _projectCache.Count;

        /// <summary>
        /// 清空缓存
        /// </summary>
        public static void Clear()
        {
            _deviceCache.Clear();
            _projectCache.Clear();
            _factoryIdMapping.Clear();
        }
    }

    public class DeviceInfo
    {
        public string DeviceId { get; set; } = string.Empty;
        public string FactoryId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
    }

    public class ProjectInfo
    {
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }
}

