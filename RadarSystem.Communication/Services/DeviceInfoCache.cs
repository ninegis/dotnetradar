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
        
        // SlaveId → DeviceId 映射
        private static readonly ConcurrentDictionary<string, string> _factoryIdMapping = new();

        /// <summary>
        /// 添加设备
        /// </summary>
        public static void AddDevice(string deviceId, string factoryId, string projectId, string deviceName, string deviceType)
        {
            var device = new DeviceInfo
            {
                DeviceId = deviceId,
                SlaveId = factoryId,
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
        /// 通过SlaveId获取DeviceId
        /// </summary>
        public static string? GetDeviceIdBySlaveId(string factoryId)
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
        /// 生成文件路径：Data/ProjectId_DeviceId_SlaveId/yyyyMMdd/HHmmss.dat
        /// </summary>
        public static string GenerateFilePath(string basePath, string factoryId, string dataType)
        {
            var deviceId = GetDeviceIdBySlaveId(factoryId) ?? factoryId;
            var device = GetDevice(deviceId);
            var projectId = device?.ProjectId ?? "PROJECT001";
            var slaveId = device?.SlaveId ?? factoryId;
            
            // 目录结构：ProjectId_DeviceId_SlaveId
            string deviceFolder = $"{projectId}_{deviceId}_{slaveId}";
            
            string dateFolder = DateTime.Now.ToString("yyyyMMdd");
            string fileName = DateTime.Now.ToString("HHmmss") + ".dat";
            
            return Path.Combine(basePath, deviceFolder, dateFolder, fileName);
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
        /// 获取所有SlaveId映射（用于调试）
        /// </summary>
        public static Dictionary<string, string> GetAllSlaveIdMappings()
        {
            return new Dictionary<string, string>(_factoryIdMapping);
        }

        /// <summary>
        /// 输出所有设备映射（用于调试）
        /// </summary>
        public static void PrintAllMappings()
        {
            Console.WriteLine("[DeviceInfoCache] === 所有设备映射 ===");
            Console.WriteLine($"[DeviceInfoCache] 设备总数: {_deviceCache.Count}");
            Console.WriteLine($"[DeviceInfoCache] SlaveId映射总数: {_factoryIdMapping.Count}");
            
            foreach (var device in _deviceCache.Values)
            {
                Console.WriteLine($"[DeviceInfoCache]   DeviceId={device.DeviceId}, SlaveId={device.SlaveId}, ProjectId={device.ProjectId}");
            }
            
            Console.WriteLine("[DeviceInfoCache] === SlaveId → DeviceId 映射 ===");
            foreach (var kvp in _factoryIdMapping)
            {
                Console.WriteLine($"[DeviceInfoCache]   SlaveId={kvp.Key} → DeviceId={kvp.Value}");
            }
            Console.WriteLine("[DeviceInfoCache] ========================================");
        }

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
        public string SlaveId { get; set; } = string.Empty;
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

